namespace MultiConverter.Lib.Converters.Base
{
    public interface IChunk
    {
        /// <summary>
        /// Chunk Magic
        /// </summary>
        string Signature { get; }

        /// <summary>
        /// Chunk Length
        /// </summary>
        uint Length { get; }

        /// <summary>
        /// Order 'id' of the chunk
        /// and where to place it.
        /// </summary>
        uint Order { get; }

        /// <summary>
        /// Read the chunk data.
        /// </summary>
        /// <param name="inData"></param>
        void Read(byte[] inData);

        /// <summary>
        /// Write the chunk data.
        /// </summary>
        /// <returns></returns>
        byte[] Write();
    }
}
