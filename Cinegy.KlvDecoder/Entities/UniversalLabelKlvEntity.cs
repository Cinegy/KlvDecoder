using System;
using System.IO;

namespace Cinegy.Klv.Entities
{
    public class UniversalLabelKlvEntity : KlvEntity
    {
        public UniversalLabelKlvEntity(byte[] data) : this(data,0) { }

        public UniversalLabelKlvEntity(byte[] data, int offset)
        {
            if (data.Length - offset < 18)
                throw new InvalidDataException("Provided KLV data does not meet minimum data size of at least 18");

            var dataPos = offset;
            Key = new byte[16];
            Buffer.BlockCopy(data, dataPos, Key, 0, Key.Length);

            dataPos += 16;

            int length;
            
            if((data[dataPos] & 0x80) == 0x80)
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