using System.Collections.Generic;

namespace MultiConverter.Lib.RenderingObject.Structures
{
    public struct WorldModel
    {
        public List<GroupBatch> GroupBatches;
        public List<Material> Materials;
        public List<RenderBatch> Batches;
        public string[] DoodadSets;
    }
}
