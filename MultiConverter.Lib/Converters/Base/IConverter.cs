namespace MultiConverter.Lib.Converters.Base
{
    public interface IConverter
    {
        void Read(byte[] inData);
        void Write(string filename);
    }
}
