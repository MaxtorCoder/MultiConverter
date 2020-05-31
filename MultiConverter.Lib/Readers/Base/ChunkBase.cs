namespace MultiConverter.Lib.Readers.Base
{
    public abstract class ChunkBase
    {
        /// <summary>
        /// Chunk Magic
        /// </summary>
        public virtual string Signature { get; private set; }

        /// <summary>
        /// Chunk Length
        /// </summary>
        public virtual uint Length { get; private set; }

        /// <summary>
        /// Order 'id' of the chunk
        /// and where to place it.
        /// </summary>
        public virtual uint Order { get; set; }

        /// <summary>
        /// Read the chunk data.
        /// </summary>
        /// <param name="inData"></param>
        public abstract void Read(byte[] inData);

        /// <summary>
        /// Write the chunk data.
        /// </summary>
        /// <returns></returns>
        public abstract byte[] Write();
    }
}
