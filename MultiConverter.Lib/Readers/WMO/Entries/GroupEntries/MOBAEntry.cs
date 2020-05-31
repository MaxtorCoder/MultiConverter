namespace MultiConverter.Lib.Readers.WMO.Entries.GroupEntries
{
    public struct MOBAEntry
    {
        public short PossibleBox1_1;
        public short PossibleBox1_2;
        public short PossibleBox1_3;
        public short PossibleBox2_1;
        public short PossibleBox2_2;
        public short PossibleBox2_3;
        public uint FirstFace;
        public ushort NumFaces;
        public ushort FirstVertex;
        public ushort LastVertex;
        public byte Flags;
        public byte MaterialId;
    }
}
