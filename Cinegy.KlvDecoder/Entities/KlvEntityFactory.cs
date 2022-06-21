using System;
using System.Collections.Generic;
using Cinegy.KlvDecoder.TransportStream;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.KlvDecoder.Entities
{
    public class KlvEntityFactory
    {
        private readonly MetadataAccessUnitFactory _accessUnitFactory = new MetadataAccessUnitFactory();

        public KlvEntityFactory()
        {
            _accessUnitFactory.MetadataReady += _accessUnitFactory_MetadataReady;
        }


        public void AddPes(Pes pes, PesHdr tsPacketPesHeader)
        {   
            if (pes.PacketStartCodePrefix != Pes.DefaultPacketStartCodePrefix ||
                pes.PesPacketLength <= 0) return;

            const ushort klvMinimumSize = 6;
            var startOfKlvData = klvMinimumSize;
            
            if (pes.OptionalPesHeader.MarkerBits == 2) //optional PES header exists - minimum length is 3
            {
                startOfKlvData += (ushort)(3 + pes.OptionalPesHeader.PesHeaderLength);
            }

            var dataBuf = new byte[pes.PesPacketLength - startOfKlvData + klvMinimumSize];
            
            Buffer.BlockCopy(pes.Data, startOfKlvData, dataBuf, 0, dataBuf.Length);

            if (pes.StreamId == 0xFC)
            {
                //this is metadata framed within Metadata Access Units - see IEC13818 2.12.4
                _accessUnitFactory.AddData(dataBuf);
            }
            else
            {
                var entities = GetEntitiesFromData(dataBuf);
                OnKlvReady(entities);
            }
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
        
        public event KlvEntitiesReadyEventHandler KlvEntitiesReady;
        
        private void OnKlvReady(List<KlvEntity> klvEntities)
        {
            KlvEntitiesReady?.Invoke(this, new KlvEntityReadyEventArgs(klvEntities));
        }

        private void _accessUnitFactory_MetadataReady(object sender, MetadataAccessDataEventArgs args)
        {
            var entities = GetEntitiesFromData(args.AccessUnit.Data);
            OnKlvReady(entities);
        }
    }
}
