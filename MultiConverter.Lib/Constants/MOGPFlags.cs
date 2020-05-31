using System;

namespace MultiConverter.Lib.Constants
{
    [Flags]
    public enum MOGPFlags
    {
        Flag_HasBoundingBoxes               = 0x1,          // Has MOBN and MOBR chunk.
        Flag_0x2                            = 0x2,
        Flag_HasVertexColors                = 0x4,          // Has vertex colors (MOCV chunk)
        Flag_IsOutdoor                      = 0x8,          // Outdoor
        Flag_0x10                           = 0x10,
        Flag_0x20                           = 0x20,
        Flag_0x40                           = 0x40,
        Flag_0x80                           = 0x80,
        Flag_0x100                          = 0x100,
        Flag_HasLights                      = 0x200,        // Has lights  (MOLR chunk)
        Flag_HasMPBChunks                   = 0x400,        // Has MPBV, MPBP, MPBI, MPBG chunks.
        Flag_HasDoodads                     = 0x800,        // Has doodads (MODR chunk)
        Flag_HasWater                       = 0x1000,       // Has water   (MLIQ chunk)
        Flag_IsIndoor                       = 0x2000,       // Indoor
        Flag_0x8000                         = 0x8000,
        Flag_0x10000                        = 0x10000,
        Flag_HasTriangleStrips              = 0x20000,      // Has MORI and MORB chunks.
        Flag_HasSkybox                      = 0x40000,      // Show skybox
        Flag_IsNotOcean                     = 0x80000,      // isNotOcean, LiquidType related, see below in the MLIQ chunk.
        Flag_0x100000                       = 0x100000,
        Flag_0x200000                       = 0x200000,
        Flag_0x400000                       = 0x400000,
        Flag_0x800000                       = 0x800000,
        Flag_0x1000000                      = 0x1000000,    // SMOGroup::CVERTS2: Has two MOCV chunks: Just add two or don't set 0x4 to only use cverts2.
        Flag_0x2000000                      = 0x2000000,    // SMOGroup::TVERTS2: Has two MOTV chunks: Just add two.
        Flag_0x40000000                     = 0x40000000,   // SMOGroup::TVERTS3: Has three MOTV chunks, eg. for MOMT with shader 18.
    }
}
