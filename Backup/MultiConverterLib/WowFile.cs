using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MultiConverterLib
{
    public class WowFile
    {
        public byte[] Data { get; protected set; }
        public string Path { get; }
        public bool Valid { get; } = false;

        public WowFile(string file)
        {
            Path = file;

            if (File.Exists(Path))
            {
                Valid = true;
                Data = File.ReadAllBytes(Path);
            }
        }

        #region read

        public char ReadChar(int pos)
        {
            return BitConverter.ToChar(Data, pos);
        }
        public short ReadShort(int pos)
        {
            return BitConverter.ToInt16(Data, pos);
        }
        public ushort ReadUShort(int pos)
        {
            return BitConverter.ToUInt16(Data, pos);
        }
        public int ReadInt(int pos)
        {
            return BitConverter.ToInt32(Data, pos);
        }
        public uint ReadUInt(int pos)
        {
            return BitConverter.ToUInt32(Data, pos);
        }
        public ulong ReadULong(int pos)
        {
            return BitConverter.ToUInt64(Data, pos);
        }
        public float ReadFloat(int pos)
        {
            return BitConverter.ToSingle(Data, pos);
        }


        #endregion

        #region write

        public void WriteChar(int pos, char value)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Data, pos, 2);
        }
        public void WriteShort(int pos, short value)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Data, pos, 2);
        }
        public void WriteUShort(int pos, ushort value)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Data, pos, 2);
        }
        public void WriteInt(int pos, int value)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Data, pos, 4);
        }
        public void WriteUInt(int pos, uint value)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Data, pos, 4);
        }
        public void WriteFloat(int pos, float value)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, Data, pos, 4);
        }

        #endregion


        #region edit

        public void RemoveBytes(int start, int count)
        {
            int size = Data.Length;

            if (size < start + count || count <= 0)
            {
                // todo: exceptions
                return;
            }

            byte[] tmp = new byte[size - count];
            if (start > 0)
            {
                Buffer.BlockCopy(Data, 0, tmp, 0, start);
            }
            Buffer.BlockCopy(Data, start + count, tmp, start, size - count - start);
            Data = tmp;
        }

        public void AddEmptyBytes(int start, int count)
        {
            int size = Data.Length;
            byte[] tmp = new byte[size + count];
            Buffer.BlockCopy(Data, 0, tmp, 0, start);
            Buffer.BlockCopy(Data, start, tmp, start + count, size - start);
            Data = tmp;
        }

        public void InsertBytes(int start, byte[] bytes)
        {
            AddEmptyBytes(start, bytes.Length);
            Buffer.BlockCopy(bytes, 0, Data, start, bytes.Length);
        }

        public void BlockCopy(int source_ofs, WowFile dest, int dest_ofs, int count)
        {
            Buffer.BlockCopy(Data, source_ofs, dest.Data, dest_ofs, count);
        }

        public void BlockCopy(int source_ofs, Array dest, int dest_ofs, int count)
        {
            Buffer.BlockCopy(Data, source_ofs, dest, dest_ofs, count);
        }

        #endregion

        public int Size()
        {
            return Data.Length;
        }

        public void RemoveOnDisk()
        {
            if (Valid)
            {
                File.Delete(Path);
            }
        }

        public virtual void Save()
        {
            if (Valid)
            {
                File.WriteAllBytes(Path, Data);
            }
        }
    };
}
