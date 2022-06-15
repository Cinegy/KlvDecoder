using System;
using System.Collections.Generic;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.Klv.Entities
{
    public static class KlvEntityFactory
    {
        public static List<KlvEntity> GetEntitiesFromPes(Pes pes, PesHdr tsPacketPesHeader)
        {   
            if (pes.PacketStartCodePrefix != Pes.DefaultPacketStartCodePrefix || pes.StreamId != (byte)PesStreamTypes.PrivateStream1 ||
                pes.PesPacketLength <= 0) return null;

            const ushort klvMinimumSize = 6;
            var startOfKlvData = klvMinimumSize;
            
            if (pes.OptionalPesHeader.MarkerBits == 2) //optional PES header exists - minimum length is 3
            {
                startOfKlvData += (ushort)(3 + pes.OptionalPesHeader.PesHeaderLength);
            }

            var dataBuf = new byte[pes.PesPacketLength - startOfKlvData + klvMinimumSize];
            
            Buffer.BlockCopy(pes.Data, startOfKlvData, dataBuf, 0, dataBuf.Length);

            return GetEntitiesFromData(dataBuf);
        }
        
        public static List<KlvEntity> GetEntitiesFromData(byte[] sourceData)
        {
            var klvMetadataList = new List<KlvEntity>();
            var sourceDataPos = 0;
            while (sourceDataPos < sourceData.Length)
            {
                var klvMetadata = new UniversalLabelKlvEntity(sourceData, sourceDataPos);
                sourceDataPos += klvMetadata.ReadBytes;
                klvMetadataList.Add(klvMetadata);
            }

            return klvMetadataList;
        }

        public static List<LocalSetKlvEntity> GetLocalSetEntitiesFromData(byte[] sourceData, int offset = 0)
        {
            var metadataList = new List<LocalSetKlvEntity>();
            var sourceDataPos = offset;
            while (sourceDataPos < sourceData.Length)
            {
                var klvMetadata = new LocalSetKlvEntity(sourceData, sourceDataPos);
                sourceDataPos += klvMetadata.ReadBytes;
                metadataList.Add(klvMetadata);
            }

            return metadataList;
        }
    }
}
