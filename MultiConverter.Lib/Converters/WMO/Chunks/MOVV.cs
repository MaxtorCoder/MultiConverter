using MultiConverter.Lib.Converters.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOVV : IChunk
    {
        public string Signature => "MOVV";
        public uint Length => (uint)Write().Length;
        public uint Order => 10;

        public void Read(byte[] inData) { }

        public byte[] Write() => new byte[0];
    }
}
