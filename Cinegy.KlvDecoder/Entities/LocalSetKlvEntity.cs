/* Copyright 2022-2023 Cinegy GmbH.

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

using System;
using System.Buffers;
using System.IO;

namespace Cinegy.KlvDecoder.Entities
{
    public class LocalSetKlvEntity : KlvEntity
    {
        public int TagId
        {
            get
            {
                if(Key.Length == 1)
                {
                    return Key[0];
                }

                throw new NotImplementedException("KLV entity has > 1-byte tag; this is not currently supported");
            }
        }

        public LocalSetKlvEntity() { }

        public LocalSetKlvEntity(int tagId, byte[] data)
        {
            if(tagId > 255)
            {
                throw new NotImplementedException("KLV tags > 255 are not yet supported");
            }

            var encodedKey = GetBerLengthData(tagId); //TODO: this is wrong - but works for tags < 127 (probably)
            var encodedLength = GetBerLengthData(data.Length);
            var encodedArraySize = encodedKey.Length + encodedLength.Length + data.Length;
            var encodedArray = ArrayPool<byte>.Shared.Rent(encodedArraySize);
            Buffer.BlockCopy(encodedKey, 0, encodedArray, 0, encodedKey.Length);
            Buffer.BlockCopy(encodedLength, 0, encodedArray, encodedKey.Length, encodedLength.Length);
            Buffer.BlockCopy(data, 0, encodedArray, encodedKey.Length + encodedLength.Length, data.Length);

            Init(encodedArray, 0);
            ArrayPool<byte>.Shared.Return(encodedArray);
        }

        public LocalSetKlvEntity(byte[] data, int offset)
        {
            Init(data, offset);
        }

        private void Init(byte[] data, int offset)
        {
            var dataPos = offset;

            if ((data[dataPos] & 0x80) != 0x80)
            {
                //BER-OID one-byte tag
                Key = new[] { data[dataPos++] };
            }
            else
            {
                //BER-OID multi-byte tag
                throw new NotImplementedException("Multi-byte BER-OID tag decoding not yet implement");
            }

            int length;

            if ((data[dataPos] & 0x80) == 0x80)
            {
                //flag indicated long-form BER length encoding
                var lenBytes = data[dataPos] & 0x7F;
                dataPos++;
                //'int' length is largest we will support (nobody should really have more than 4GB of data in a single KLV!)
                switch (lenBytes)
                {
                    case 1:
                        length = data[dataPos];
                        break;
                    case 2:
                        length = (data[dataPos] << 8) + data[dataPos + 1];
                        break;
                    case 3:
                        length = (data[dataPos] << 16) + (data[dataPos + 1] << 8) + data[dataPos + 2];
                        break;
                    case 4:
                        length = (data[dataPos] << 24) + (data[dataPos + 1] << 16) + (data[dataPos + 2] << 8) + data[dataPos + 3];
                        break;
                    default:
                        throw new InvalidDataException("Unsupported KLV length field");
                }
                dataPos += lenBytes;
            }
            else
            {
                //flag indicated short-form BER length encoding - so the bytes of length is just the 7 lower-order bits
                length = data[dataPos++] & 0x7F;
            }

            Value = new byte[length];
            Buffer.BlockCopy(data, dataPos, Value, 0, length);

            dataPos += length;
            ReadBytes = dataPos - offset;
        }

        public byte[] Encode()
        {
            var encodedLength = GetBerLengthData(Value.Length);
            var encodedArray = new byte[Key.Length + encodedLength.Length + Value.Length];
            Buffer.BlockCopy(Key, 0, encodedArray, 0, Key.Length);
            Buffer.BlockCopy(encodedLength, 0, encodedArray, Key.Length, encodedLength.Length);
            Buffer.BlockCopy(Value, 0, encodedArray, Key.Length + encodedLength.Length, Value.Length);
            return encodedArray;
        }

        private byte[] GetBerLengthData(int dataLength)
        {
            if (dataLength > 254)
            {
                throw new NotSupportedException("Encoding KLV with length > 255 is not yet supported");
            }

            //TODO: This is not right
            return new[] { (byte)dataLength };
        }

    }
}