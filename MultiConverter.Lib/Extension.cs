using MultiConverter.Lib.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace MultiConverter.Lib
{
    public static class Extension
    {
        #region BinaryReader/BinaryWriter Definitions
        public static Dictionary<Type, Func<BinaryReader, object>> ReadValue = new Dictionary<Type, Func<BinaryReader, object>>()
        {
            { typeof(bool),      br => br.ReadBoolean() },
            { typeof(sbyte),     br => br.ReadSByte()   },
            { typeof(byte),      br => br.ReadByte()    },
            { typeof(char),      br => br.ReadChar()    },
            { typeof(short),     br => br.ReadInt16()   },
            { typeof(ushort),    br => br.ReadUInt16()  },
            { typeof(int),       br => br.ReadInt32()   },
            { typeof(uint),      br => br.ReadUInt32()  },
            { typeof(float),     br => br.ReadSingle()  },
            { typeof(long),      br => br.ReadInt64()   },
            { typeof(ulong),     br => br.ReadUInt64()  },
            { typeof(double),    br => br.ReadDouble()  },
            { typeof(M2Array),   br => br.ReadM2Array() },
        };

        public static Dictionary<Type, Action<BinaryWriter, object>> WriteValue = new Dictionary<Type, Action<BinaryWriter, object>>()
        {
            { typeof(bool),      (bw, val) => bw.Write((bool)val)   },
            { typeof(sbyte),     (bw, val) => bw.Write((sbyte)val)  },
            { typeof(byte),      (bw, val) => bw.Write((byte)val)   },
            { typeof(short),     (bw, val) => bw.Write((short)val)  },
            { typeof(ushort),    (bw, val) => bw.Write((ushort)val) },
            { typeof(int),       (bw, val) => bw.Write((int)val)    },
            { typeof(uint),      (bw, val) => bw.Write((uint)val)   },
            { typeof(float),     (bw, val) => bw.Write((float)val)  },
            { typeof(long),      (bw, val) => bw.Write((long)val)   },
            { typeof(ulong),     (bw, val) => bw.Write((ulong)val)  },
            { typeof(double),    (bw, val) => bw.Write((double)val) },
            { typeof(byte[]),    (bw, val) => bw.Write((byte[])val) },
            { typeof(M2Array),   (bw, val) =>
                {
                    var array = (M2Array)val;
                    bw.Write((uint)array.Size);
                    bw.Write((uint)array.Offset);
                }
            },
        };
        #endregion

        public static M2Array ReadM2Array(this BinaryReader br)
        {
            return new M2Array
            {
                Size    = br.ReadUInt32(),
                Offset  = br.ReadUInt32()
            };
        }
    }
}
