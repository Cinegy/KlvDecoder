using System;
using System.IO;

namespace Cinegy.KlvDecoder.Entities
{
    public class LocalSetKlvEntity : KlvEntity
    {
        public int TagId
        {
            get;
        }

        public LocalSetKlvEntity(byte[] data, int offset)
        {
            var dataPos = offset;
        
            if ((data[dataPos] & 0x80) != 0x80)
            {
                //BER-OID one-byte tag
                Key = new[] { data[dataPos] };
                TagId = data[dataPos++];
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
    }
}