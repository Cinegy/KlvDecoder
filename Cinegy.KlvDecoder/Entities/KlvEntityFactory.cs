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

using Cinegy.KlvDecoder.TransportStream;
using Cinegy.TsDecoder.TransportStream;
using System;
using System.Buffers;
using System.Collections.Generic;

namespace Cinegy.KlvDecoder.Entities
{
    public class KlvEntityFactory
    {
        private readonly MetadataAccessUnitFactory _accessUnitFactory = new();
        private readonly bool _preserveSourceData;

        public KlvEntityFactory(bool preserveSourceData = false)
        {
            _preserveSourceData = preserveSourceData;
            _accessUnitFactory.MetadataReady += _accessUnitFactory_MetadataReady;
        }
        
        public void AddPes(Pes pes, PesHdr tsPacketPesHeader)
        {   
            if (pes.PacketStartCodePrefix != Pes.DefaultPacketStartCodePrefix ||
                pes.PesPacketLength <= 0) return;

            const ushort klvMinimumSize = 6;
            var startOfKlvData = klvMinimumSize;
            
            if (pes.OptionalPesHeader?.MarkerBits == 2) //optional PES header exists - minimum length is 3
            {
                startOfKlvData += (ushort)(3 + pes.OptionalPesHeader.PesHeaderLength);
            }

            var dataBufSize = pes.PesPacketLength - startOfKlvData + klvMinimumSize;
            var dataBuf = ArrayPool<byte>.Shared.Rent(dataBufSize);

            Buffer.BlockCopy(pes.Data, startOfKlvData, dataBuf, 0, dataBufSize);

            if (pes.StreamId == 0xFC)
            {
                //this is metadata framed within Metadata Access Units - see IEC13818 2.12.4
                _accessUnitFactory.AddData(dataBuf);
            }
            else
            {
                var entities = GetEntitiesFromData(dataBuf, dataBufSize, _preserveSourceData);
                OnKlvReady(entities);
            }
            ArrayPool<byte>.Shared.Return(dataBuf);
        }
        
        public static List<KlvEntity> GetEntitiesFromData(byte[] sourceData, int dataLen, bool preserveSourceData = true)
        {
            var klvMetadataList = new List<KlvEntity>();
            var sourceDataPos = 0;
            while (sourceDataPos < dataLen)
            {
                var klvMetadata = new UniversalLabelKlvEntity(sourceData, sourceDataPos, dataLen, preserveSourceData);
                sourceDataPos += klvMetadata.ReadBytes;
                klvMetadataList.Add(klvMetadata);
            }

            return klvMetadataList;
        }

        public static List<LocalSetKlvEntity> GetLocalSetEntitiesFromData(byte[] sourceData, int dataLen = 0, int offset = 0)
        {
            if(dataLen == 0) dataLen = sourceData.Length;

            var metadataList = new List<LocalSetKlvEntity>();
            var sourceDataPos = offset;
            while (sourceDataPos < dataLen)
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
            var entities = GetEntitiesFromData(args.AccessUnit.Data, args.AccessUnit.Data.Length, _preserveSourceData);
            OnKlvReady(entities);
        }
    }
}
