/* Copyright 2022 Cinegy GmbH.

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
using Cinegy.Klv.Entities;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.Klv.TransportStream
{
    public class KlvTsService
    {
        /// <summary>
        /// Reference PTS, used to calculate and display relative time offsets for data within stream
        /// </summary>
        public long ReferencePts { get; set; }

        /// <summary>
        /// The TS Packet ID that has been selected as the elementary stream containing KLV data
        /// </summary>
        public short KlvPid { get; set; } = -1;

        /// <summary>
        /// The Program Number ID to which the selected KLV PID belongs, if any
        /// </summary>
        public ushort ProgramNumber { get; set; } = 0;

        /// <summary>
        /// The associated RegistrationDescriptor for the service, if any
        /// </summary>
        public RegistrationDescriptor AssociatedDescriptor { get; set; }
 
        public void AddData(Pes pes, PesHdr tsPacketPesHeader)
        {
            //update / store any reference PTS for displaying easy relative values
            if (ReferencePts == 0) ReferencePts = tsPacketPesHeader.Pts;
            if (ReferencePts > 0 && tsPacketPesHeader.Pts < ReferencePts) ReferencePts = tsPacketPesHeader.Pts;

            var klvEntities = KlvEntityFactory.GetEntitiesFromPes(pes, tsPacketPesHeader);
            
            if (klvEntities == null) return;

            OnKlvEntitiesReady(klvEntities);
            
        }
        
        public event KlvEntitiesReadyEventHandler KlvEntitiesReady;

        internal virtual void OnKlvEntitiesReady(List<KlvEntity> metadata)
        {
            KlvEntitiesReady?.Invoke(this, new KlvEntityReadyEventArgs(metadata));
        }

    }

    public delegate void KlvEntitiesReadyEventHandler(object sender, KlvEntityReadyEventArgs args);
}
