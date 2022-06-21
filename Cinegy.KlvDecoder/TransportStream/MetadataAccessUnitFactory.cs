using System;

namespace Cinegy.KlvDecoder.TransportStream
{
    public class MetadataAccessUnitFactory
    {
        public void AddData(byte[] data, int offset = 0)
        {
            var position = offset;
            while (position < data.Length)
            {
                var au = new MetadataAccessUnit
                {
                    ServiceId = data[position],
                    SequenceNum = data[position + 1],
                    Fragmentation = (MetadataAccessUnit.FragmentationIndicator) ((data[position + 2] & 0b11000000) >> 6),
                    DecoderConfigFlag = (data[position + 2] & 0b00100000) > 0,
                    RandomAccessFlag = (data[position + 2] & 0b00010000) > 0,
                    DataLength = (ushort) ((data[position + 3] << 8) + data[position + 4])
                };

                au.Data = new byte[au.DataLength];

                Buffer.BlockCopy(data, position + 5, au.Data, 0, au.DataLength);
                position += 5 + au.DataLength;
                OnAccessUnitReady(au);
            }
        }

        public delegate void MetadataAccessUnitEventHandler(object sender, MetadataAccessDataEventArgs args);
        
        public event MetadataAccessUnitEventHandler MetadataReady;

        protected void OnAccessUnitReady(MetadataAccessUnit metadataAccessUnit)
        {
            var handler = MetadataReady;
            if (handler == null) return;

            var args = new MetadataAccessDataEventArgs { TsPid = 1, AccessUnit = metadataAccessUnit };
            handler(this, args);
        }

    }

}