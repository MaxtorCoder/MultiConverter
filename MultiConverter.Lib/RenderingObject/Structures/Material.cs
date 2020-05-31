namespace MultiConverter.Lib.RenderingObject.Structures
{
    public struct Material
    {
        public string Filename;
        public string FileDataId;

        // M2
        public int TextureId;

        // WMO
        public int TextureId1;
        public int TextureId2;
        public int TextureId3;
        public uint Texture1;
        public uint Texture2;
        public uint Texture3;

        // ADT
        public float Scale;
        public float HeightScale;
        public float HeightOffset;
        public int HeightTexture;

        public uint BlendMode;
        public uint TextureFlags;
    }
}
