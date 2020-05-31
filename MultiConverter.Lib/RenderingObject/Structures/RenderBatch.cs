using OpenTK;

namespace MultiConverter.Lib.RenderingObject.Structures
{
    public struct RenderBatch
    {
        public uint FirstFace;
        public uint NumFaces;
        public int[] MaterialId;

        /* WMO ONLY */
        public uint GroupId;
        public uint BlendType;

        /* ADT ONLY */
        public int[] AlphaMaterialId;
        public float[] Scales;
        public int[] HeightMaterialIds;

        public Vector4 HeightScales;
        public Vector4 HeightOffsets;
    }
}
