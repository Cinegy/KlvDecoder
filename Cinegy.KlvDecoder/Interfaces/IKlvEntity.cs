namespace Cinegy.Klv.Interfaces
{
    public interface IKlvEntity
    {
        string KeyHexString { get; }
        byte[] Key { get; set; }
        byte[] Value { get; set; }
        byte[] ValueNative { get; }
        string ValueHexString { get; }
        int ReadBytes { get; }
    }
}