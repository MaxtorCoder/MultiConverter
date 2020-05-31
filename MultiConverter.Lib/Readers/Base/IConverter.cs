using System.IO;

namespace MultiConverter.Lib.Readers.Base
{
    public interface IConverter
    {
        void Read(Stream inData);
        void Write(string filename);
        ChunkBase GetChunk(string name);
    }
}
