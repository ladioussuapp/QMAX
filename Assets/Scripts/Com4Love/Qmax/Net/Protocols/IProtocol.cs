using System.IO;

namespace Com4Love.Qmax.Net.Protocols
{
    public interface IProtocol
    {
        byte[] Serialize(bool isLittleEndian = true);
        int Serialize(Stream stream, bool isLittleEndian = true);

        int Deserialize(byte[] bytes, int startIndex, bool isLittleEndian = true);
        int Deserialize(Stream stream, bool isLittleEndian = true);
    }
}
