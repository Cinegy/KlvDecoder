using System;
using System.Linq;
using System.Text.Json.Serialization;
using Cinegy.KlvDecoder.Interfaces;

namespace Cinegy.KlvDecoder.Entities
{
    public abstract class KlvEntity : IKlvEntity
    {
        [JsonIgnore]
        public byte[] Key { get; set; }

        [JsonIgnore]
        public byte[] Value { get; set; }

        [JsonIgnore]
        public byte[] ValueNative => BitConverter.IsLittleEndian ? Value.Reverse().ToArray() : Value;

        [JsonIgnore]
        public int ReadBytes { get; internal set; }
     
        public string KeyHexString => BitConverter.ToString(Key, 0, Key.Length);
        
        public string ValueHexString => Value != null ? BitConverter.ToString(Value, 0, Value.Length) : string.Empty;

    }
}