namespace Cinegy.KlvDecoder.TransportStream
{
    public class MetadataAccessUnit
    {
        public byte ServiceId { get; set; }
        
        public byte SequenceNum { get; set; }

        public FragmentationIndicator Fragmentation { get; set; }

        public bool DecoderConfigFlag { get; set; }

        public bool RandomAccessFlag { get; set; }

        public ushort DataLength { get; set; } 

        public byte[] Data { get; set; }
        
        public enum FragmentationIndicator
        {
            MiddleCell = 0,
            LastCell = 1,
            FirstCell = 2,
            Complete = 3
        }
    }
}