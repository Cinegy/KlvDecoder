using System;

namespace Cinegy.KlvDecoder.TransportStream
{
    public class MetadataAccessDataEventArgs : EventArgs
    {
        public int TsPid { get; set; }

        public MetadataAccessUnit AccessUnit { get; set; }
    }
}