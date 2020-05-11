using MultiConverter.Lib.Converters.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace MultiConverter.Lib.Converters.WMO.Chunks
{
    public class MOVB : IChunk
    {
        public string Signature => "MOVB";
        public uint Length => (uint)Write().Length;
        public uint Order => 11;

        public void Read(byte[] inData) { }

        public byte[] Write() => new byte[0];
    }
}
