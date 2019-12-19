using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultiConverter.Lib.Interface
{
    public class IWowFile
    {
        public string Path { get; }
        public bool Valid { get; } = false;

        public BinaryReader Reader { get; protected set; }
        public BinaryWriter Writer { get; protected set; }
        public MemoryStream Stream { get; protected set; }

        public int Size => Stream.ToArray().Length;

        private uint LastPos = 0;

        public IWowFile(string file)
        {
            Path = file;

            if (File.Exists(Path))
            {
                Valid   = true;
                NewStream(File.ReadAllBytes(Path));
            }
        }

        private void NewStream(byte[] data)
        {
            Stream = new MemoryStream(data);
            Reader = new BinaryReader(Stream);
            Writer = new BinaryWriter(Stream);
        }


        public T Read<T>(int pos = -1)
        {
            var type = typeof(T);
            var finalType = type.IsEnum ? type.GetEnumUnderlyingType() : type;

            if (pos != -1)
                Reader.BaseStream.Position = pos;

            return (T)Extension.ReadValue[finalType](Reader);
        }

        public void Write<T>(T value, int pos = 0)
        {
            var type = typeof(T);
            var finalType = type.IsEnum ? type.GetEnumUnderlyingType() : type;

            Writer.BaseStream.Position = pos;
            Extension.WriteValue[finalType](Writer, value);
        }

        #region Edit Bytes
        public void RemoveBytes(int start, int count)
        {
            // Todo: Exceptions
            if (Size < start + count || count <= 0)
                return;

            var data = Stream.ToArray();
            byte[] tmp = new byte[Size - count];
            if (start > 0)
                Buffer.BlockCopy(data, 0, tmp, 0, start);

            Buffer.BlockCopy(data, start + count, tmp, start, Size - count - start);
            data = tmp;

            NewStream(data);
        }

        public void AddEmptyBytes(int start, int count)
        {
            var data = Stream.ToArray();

            byte[] tmp = new byte[Size + count];
            Buffer.BlockCopy(data, 0, tmp, 0, start);
            Buffer.BlockCopy(data, start, tmp, start + count, Size - start);
            data = tmp;

            NewStream(data);
        }

        public void InsertBytes(int start, byte[] bytes)
        {
            var data = Stream.ToArray();

            AddEmptyBytes(start, bytes.Length);
            Buffer.BlockCopy(bytes, 0, data, start, bytes.Length);

            NewStream(data);
        }

        public void BlockCopy(int source_ofs, IWowFile dest, int dest_ofs, int count)
        {
            var data = Stream.ToArray();
            Buffer.BlockCopy(data, source_ofs, dest.Stream.ToArray(), dest_ofs, count);

            NewStream(data);
        }

        public void BlockCopy(int source_ofs, Array dest, int dest_ofs, int count)
        {
            var data = Stream.ToArray();
            Buffer.BlockCopy(data, source_ofs, dest, dest_ofs, count);

            NewStream(data);
        }
        #endregion

        public void RemoveOnDisk()
        {
            if (Valid)
                File.Delete(Path);
        }


        public virtual void Save()
        {
            if (Valid)
                File.WriteAllBytes(Path, Stream.ToArray());
        }
    }
}
