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

using System.Collections.Generic;
using Cinegy.KlvDecoder.Entities;
using Cinegy.TsDecoder.TransportStream;
using Cinegy.TsDecoder.Descriptors;

namespace Cinegy.KlvDecoder.TransportStream
{
    public class KlvTsService
    {
        private readonly KlvEntityFactory _klvEntityFactory;

        public KlvTsService(bool preserveSourceData = true)
        {
            _klvEntityFactory = new KlvEntityFactory(preserveSourceData);
            _klvEntityFactory.KlvEntitiesReady  += KlvEntityFactoryOnKlvEntitiesReady;
        }

        private void KlvEntityFactoryOnKlvEntitiesReady(object sender, KlvEntityReadyEventArgs args)
        {
             OnKlvEntitiesReady(args.EntityList);
        }
        
        /// <summary>
        /// Reference PTS, used to calculate and display relative time offsets for data within stream
        /// </summary>
        public long ReferencePts { get; set; }

        /// <summary>
        /// The TS Packet ID that has been selected as the elementary stream containing KLV data
        /// </summary>
        public ushort? KlvPid { get; set; } = null;

        /// <summary>
        /// The Program Number ID to which the selected KLV PID belongs, if any
        /// </summary>
        public ushort ProgramNumber { get; set; } = 0;

        /// <summary>
        /// The associated Descriptor for the service, if any
        /// </summary>
        public Descriptor AssociatedDescriptor { get; set; }
 
        public void AddData(Pes pes, PesHdr tsPacketPesHeader)
        {
            //update / store any reference PTS for displaying easy relative values
            if (ReferencePts == 0) ReferencePts = tsPacketPesHeader.Pts;
            if (ReferencePts > 0 && tsPacketPesHeader.Pts < ReferencePts) ReferencePts = tsPacketPesHeader.Pts;
            
            _klvEntityFactory.AddPes(pes, tsPacketPesHeader);
            
        }
        
        public event KlvEntitiesReadyEventHandler KlvEntitiesReady;

        internal virtual void OnKlvEntitiesReady(List<KlvEntity> metadata)
        {
            KlvEntitiesReady?.Invoke(this, new KlvEntityReadyEventArgs(metadata));
        }
    }
}
