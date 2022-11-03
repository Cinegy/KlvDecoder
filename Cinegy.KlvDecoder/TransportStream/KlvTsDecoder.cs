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

using System;
using Cinegy.TsDecoder.TransportStream;

namespace Cinegy.KlvDecoder.TransportStream
{
    public class KlvTsDecoder
    {
        private Pes _currentKlvPes;

        public long LastPts { get; private set; }

        public KlvTsService TsService { get; private set; }

        public Descriptor CurrentKlvDescriptor { get; private set; }

        /// <summary>
        /// The Program Number of the service that is used as source for KLV data - can be set by constructor only, otherwise default program will be used.
        /// </summary>
        public ushort ProgramNumber { get; private set; }

        public ushort StreamType { get; private set; } = 0x6;

        public ushort DescriptorTag { get; private set; }

        public bool PreserveSourceData { get; private set; }

        public KlvTsDecoder() { }

        public KlvTsDecoder(int streamType, int descriptorTag, ushort programNumber = 0, bool preserveSourceData = false)
        {
            StreamType = (ushort)streamType;
            DescriptorTag = (ushort)descriptorTag;
            ProgramNumber = programNumber;
            PreserveSourceData = preserveSourceData;
            TsService = new KlvTsService(preserveSourceData);
        }

        public bool FindKlvService(TsDecoder.TransportStream.TsDecoder tsDecoder, out EsInfo esStreamInfo, out Descriptor klvDescriptor)
        {
            if (tsDecoder == null) throw new InvalidOperationException("Null reference to TS Decoder");

            esStreamInfo = null;
            klvDescriptor = null;

            lock (tsDecoder)
            {
                if (ProgramNumber == 0)
                {
                    var pmt = tsDecoder.GetSelectedPmt(ProgramNumber);
                    if (pmt != null)
                    {
                        ProgramNumber = pmt.ProgramNumber;
                    }
                }

                if (ProgramNumber == 0) return false;

                TsService.ProgramNumber = ProgramNumber;

                esStreamInfo = tsDecoder.GetEsStreamForProgramNumberByTag(ProgramNumber, StreamType, DescriptorTag);
                
                klvDescriptor = tsDecoder.GetDescriptorForProgramNumberByTag<Descriptor>(ProgramNumber, StreamType, DescriptorTag, true);
                
                return klvDescriptor != null;
            }
        }

        private void Setup(TsDecoder.TransportStream.TsDecoder tsDecoder)
        {
            if (FindKlvService(tsDecoder, out var esStreamInfo, out var klvDesc))
            {
                Setup(klvDesc, esStreamInfo.ElementaryPid);
            }
        }

        public void Setup(Descriptor klvDescriptor, short klvPid)
        {
            CurrentKlvDescriptor = klvDescriptor;

            TsService.AssociatedDescriptor = klvDescriptor;

            Setup(klvPid);
        }

        public void Setup(short klvPid)
        {
            TsService.KlvPid = klvPid;
        }

        public void AddPacket(TsPacket tsPacket, TsDecoder.TransportStream.TsDecoder tsDecoder = null)
        {
            if (TsService == null || TsService.KlvPid == -1)
            {
                Setup(tsDecoder);
            }

            if (tsPacket.Pid != TsService?.KlvPid) return;
            
            if (tsPacket.PayloadUnitStartIndicator)
            {
                if (tsPacket.PesHeader.Pts > -1)
                    LastPts = tsPacket.PesHeader.Pts;

                if (_currentKlvPes == null)
                {
                    _currentKlvPes = new Pes(tsPacket);
                }
            }
            else
            {
                _currentKlvPes?.Add(tsPacket);
            }

            if (_currentKlvPes?.HasAllBytes() != true) return;

            _currentKlvPes.Decode();

            TsService.AddData(_currentKlvPes, tsPacket.PesHeader);

            _currentKlvPes = null;
        }
    }
}
