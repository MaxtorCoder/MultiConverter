using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MultiConverter.Lib.Converters.Base;

namespace MultiConverter.Lib.Converters
{
    public class AdtConverter : IConverter
    {
        private string file;
        private ChunkedWowFile adt;
        private ChunkedWowFile obj;
        private ChunkedWowFile tex;
        private bool water;
        private bool models;
        private int textureCount = 0;
        int currentPos = 0;

        public AdtConverter(string path, bool h2o, bool model)
        {
            file = path;
            water = h2o;
            models = model;

            adt = new ChunkedWowFile(path);
            obj = new ChunkedWowFile(path.Replace(".adt", "_obj0.adt"));
            tex = new ChunkedWowFile(path.Replace(".adt", "_tex0.adt"));

            // clean the folder
            Utils.DeleteFile(path.Replace(".adt", "_obj1.adt"));
            Utils.DeleteFile(path.Replace(".adt", "_lod.adt"));
        }

        public bool Fix()
        {
            //                                           already converted
            if (!adt.Valid || !obj.Valid || !tex.Valid || adt.ReadUInt(0x18) > 0)
            {
                return false;
            }

            CreateMCIN();
            RetrieveMTEX();
            RetrieveMMDX();
            RetrieveMMID();
            RetrieveMWMO();
            RetrieveMWID();
            RetrieveMDDF();
            RetrieveMODF();
            CheckMH2O();

            for (int i = 0; i < 256; ++i)
            {
                RetrieveMCNK(i);
            }

            CheckMFBO();

            RetrieveMTXF();

            return true;
        }


        private void CreateMCIN()
        {
            adt.AddEmptyBytes(0x54, 0x1008);
            adt.WriteInt(0x54, ChunkedWowFile.MagicToInt("MCIN"));
            adt.WriteInt(0x58, 0x1000); // chunk size
            adt.WriteInt(0x18, 0x40);

            // after mcin chunk
            currentPos = 0x1008 + 0x54;
        }

        private void RetrieveMTEX()
        {
            // skip MAMP chunk      
            int start_mtex = tex.IsChunk(0xC, ChunkedWowFile.MagicToInt("MAMP")) ? tex.ReadInt(0x10) + 0x14 : 0xC;

            // real size = size + 0x8 (header size)
            int size = tex.ReadInt(start_mtex + 4) + 0x8;

            // start MTEX
            adt.AddEmptyBytes(currentPos, size);

            tex.BlockCopy(start_mtex, adt, currentPos, size);
            // remove unecessary data
            tex.RemoveBytes(0x0, start_mtex + size);

            adt.WriteInt(0x1C, currentPos - 0x14);

            for (int i = currentPos + 0x8; i < currentPos + size; ++i)
            {
                if (adt.Data[i] == 0)
                {
                    textureCount++;
                }
            }

            currentPos += size;
        }

        private void RetrieveMMDX()
        {
            int size = 0x8;

            if (models)
            {
                size += obj.ReadInt(0x10);

                adt.AddEmptyBytes(currentPos, size);
                obj.BlockCopy(0xC, adt, currentPos, size);
                obj.RemoveBytes(0, size + 0xC);

            }
            else
            {
                adt.AddEmptyBytes(currentPos, size);
                adt.WriteHeaderMagic(currentPos, "MMDX");
                obj.RemoveBytes(0, obj.ReadInt(0x10) + 0xC + 0x8);
            }

            adt.WriteInt(0x20, currentPos - 0x14);
            currentPos += size;
        }

        private void RetrieveMMID()
        {
            int size = 0x8;

            if (models)
            {
                size += obj.ReadInt(0x4);
                adt.AddEmptyBytes(currentPos, size);
                obj.BlockCopy(0, adt, currentPos, size);
                obj.RemoveBytes(0, size);
            }
            else
            {
                adt.AddEmptyBytes(currentPos, size);
                adt.WriteHeaderMagic(currentPos, "MMID");
                obj.RemoveBytes(0, obj.ReadInt(0x4) + 0x8);
            }


            adt.WriteInt(0x24, currentPos - 0x14);
            currentPos += size;
        }

        private void RetrieveMWMO()
        {
            int size = 0x8;

            if (models)
            {
                size += obj.ReadInt(0x4);
                adt.AddEmptyBytes(currentPos, size);
                obj.BlockCopy(0, adt, currentPos, size);
                obj.RemoveBytes(0, size);
            }
            else
            {
                adt.AddEmptyBytes(currentPos, size);
                adt.WriteHeaderMagic(currentPos, "MWMO");
                obj.RemoveBytes(0, obj.ReadInt(0x4) + 0x8);
            }


            adt.WriteInt(0x28, currentPos - 0x14);
            currentPos += size;
        }

        private void RetrieveMWID()
        {
            int size = 0x8;

            if (models)
            {
                size += obj.ReadInt(0x4);
                adt.AddEmptyBytes(currentPos, size);
                obj.BlockCopy(0, adt, currentPos, size);
                obj.RemoveBytes(0, size);
            }
            else
            {
                adt.AddEmptyBytes(currentPos, size);
                adt.WriteHeaderMagic(currentPos, "MWID");
                obj.RemoveBytes(0, obj.ReadInt(0x4) + 0x8);
            }

            adt.WriteInt(0x2C, currentPos - 0x14);
            currentPos += size;
        }

        private void RetrieveMDDF()
        {
            int size = 0x8;

            if (models)
            {
                size += obj.ReadInt(0x4);
                adt.AddEmptyBytes(currentPos, size);
                obj.BlockCopy(0, adt, currentPos, size);
                obj.RemoveBytes(0, size);
            }
            else
            {
                adt.AddEmptyBytes(currentPos, size);
                adt.WriteHeaderMagic(currentPos, "MDDF");
                obj.RemoveBytes(0, obj.ReadInt(0x4) + 0x8);
            }

            adt.WriteInt(0x30, currentPos - 0x14);
            currentPos += size;
        }

        private void RetrieveMODF()
        {
            int size = 0x8;

            if (models)
            {
                size += obj.ReadInt(0x4);
                adt.AddEmptyBytes(currentPos, size);
                obj.BlockCopy(0, adt, currentPos, size);
                obj.RemoveBytes(0, size);
            }
            else
            {
                adt.AddEmptyBytes(currentPos, size);
                adt.WriteHeaderMagic(currentPos, "MODF");
                obj.RemoveBytes(0, obj.ReadInt(0x4) + 0x8);
            }

            adt.WriteInt(0x34, currentPos - 0x14);
            currentPos += size;
        }

        private void Fix_MH2O_Info(int pos)
        {
            ushort id = adt.ReadUShort(pos);
            ushort type = adt.ReadUShort(pos + 0x2);

            if (id > 100)
            {
                switch (id)
                {
                    case 181:
                    case 221:
                    case 301:
                    case 406:
                    case 407:
                    case 411:
                    case 689:
                    case 733:
                    case 750:
                    case 751:
                    case 752:
                    case 760:
                    case 761:
                    case 763:
                    case 764:
                    case 765:
                    case 787:
                    case 805:
                    case 806:
                    case 807:
                    case 808:
                    case 809:
                    case 812:
                    case 814:
                    case 834:
                    case 837:
                    case 839:
                    case 844:
                    case 848:
                    case 849:
                    case 850:
                    case 851:
                    case 852:
                    case 853:
                    case 855:
                    case 864:
                    case 865:
                    case 866:
                    case 872:
                    case 880:
                    case 881:
                    case 884:
                    case 885:
                    case 886:
                    case 887:
                    case 888:
                    case 892:
                    case 894:
                    default: // water for default
                        id = 5;
                        break;
                    case 101:
                    case 321:
                    case 324:
                    case 350:
                    case 412:
                    case 868:
                    case 890:
                    case 891:
                    case 896:
                        id = 2;
                        break;
                    case 121:
                    case 141:
                    case 302:
                    case 303:
                    case 397:
                    case 404:
                    case 671:
                    case 739:
                    case 859:
                    case 860:
                    case 869:
                    case 870:
                    case 873:
                    case 874:
                    case 875:
                    case 876:
                    case 877:
                    case 878:
                    case 879:
                        id = 7;
                        break;
                    case 586:
                        id = 4;
                        break;

                }
            }

            switch (id)
            {
                // ocean
                case 2:
                case 6:
                case 10:
                case 14:
                case 100:
                    type = 2;
                    break;
                // lava
                case 3:
                case 7:
                case 11:
                case 15:
                    type = 1;
                    break;
                // water and slime (no data found for this one that's why)
                default:
                    type = 0;
                    break;
            }
            adt.WriteUShort(pos, id);
            adt.WriteUShort(pos + 0x2, type);
        }

        private void FixMH2O(int start)
        {
            int pos = start;

            for (int i = 0; i < 256; ++i)
            {
                int ofs = adt.ReadInt(pos);
                if (ofs > 0)
                {
                    Fix_MH2O_Info(start + ofs);
                }
                pos += 0xC;
            }
        }

        private void CheckMH2O()
        {
            if (adt.IsChunk(currentPos, "MH2O"))
            {
                if (water)
                {
                    FixMH2O(currentPos + 0x8);
                    int size = adt.ReadInt(currentPos + 4) + 0x8;
                    adt.WriteInt(0x3C, currentPos - 0x14);
                    currentPos += size;
                }
                else
                {
                    adt.RemoveBytes(currentPos, adt.ReadInt(currentPos + 4) + 0x8);
                    adt.WriteInt(0x3C, 0);
                }
            }
            else // no MH2O
            {
                adt.WriteInt(0x3C, 0);
            }
        }

        private ushort HighToLowResHole(ulong hole)
        {
            if (hole == 0)
            {
                return 0;
            }

            ushort low = 0x0000;

            for (int i = 0; i < 64; ++i)
            {
                if (((hole >> i) & 0x1) != 0)
                {
                    int x = (i % 8) / 2, y = i / 16;
                    low |= (ushort)(1 << (x + y * 4));
                }
            }

            return low;
        }

        enum chunks : int
        {
            MCNK = 1296256587,
            MCCV = 1296253782,
            MCNR = 1296256594,
            MCLY = 1296256089,
            MCRF = 1296257606,
            MCSH = 1296257864,
            MCAL = 1296253260,
            MCSE = 1296257861,
            // cata+
            MCRD = 1296257604,
            MCRW = 1296257623,
            MCLV = 1296256086,
        }

        private void RetrieveMCNK(int id = 0)
        {
            adt.RemoveUnwantedChunksUntil(currentPos, (int)chunks.MCNK);
            tex.RemoveUnwantedChunksUntil(0, (int)chunks.MCNK);
            obj.RemoveUnwantedChunksUntil(0, (int)chunks.MCNK);

            int size_root_mcnk = adt.ReadInt(currentPos + 0x4);
            int size_tex_mcnk = tex.ReadInt(0x4);
            int size_obj_mcnk = obj.ReadInt(0x4);
            ChunkedWowFile root_mcnk = new ChunkedWowFile(adt.Data, currentPos, size_root_mcnk + 0x8);
            ChunkedWowFile tex_mcnk = new ChunkedWowFile(tex.Data, 8, size_tex_mcnk);
            ChunkedWowFile obj_mcnk = new ChunkedWowFile(obj.Data, 8, size_obj_mcnk);

            // remove MCNK in split files
            tex.RemoveBytes(0, size_tex_mcnk + 8);
            obj.RemoveBytes(0, size_obj_mcnk + 8);

            Dictionary<int, int> adt_chunks = root_mcnk.ChunksOfs(0x88, (int)chunks.MCNK);
            Dictionary<int, int> tex_chunks = tex_mcnk.ChunksOfs(0, (int)chunks.MCNK);
            Dictionary<int, int> obj_chunks = obj_mcnk.ChunksOfs(0, (int)chunks.MCNK);

            int pos = 0;
            int ofsMCVT = 0x88;
            int sizeMCVT = (9 * 9 + 8 * 8) * 4;

            root_mcnk.WriteInt(pos + 0x3C, 0);

            uint flags = root_mcnk.ReadUInt(pos + 0x8);

            // fix high res hole
            if ((flags & 0x10000) != 0)
            {
                root_mcnk.WriteUShort(pos + 0x3C + 0x8, HighToLowResHole(root_mcnk.ReadULong(pos + 0x14 + 0x8)));
            }

            root_mcnk.WriteUInt(pos + 0x8, flags & 0xFFFF);

            pos += ofsMCVT + 0x8 + sizeMCVT;

            int ofsMCCV = 0;
            int sizeMCCV = 0;

            if (adt_chunks.ContainsKey((int)chunks.MCCV))
            {
                ofsMCCV = pos;
                sizeMCCV = root_mcnk.ReadInt(ofsMCCV + 0x4) + 0x8;
                pos += sizeMCCV;
            }

            // remove MCLV
            if (adt_chunks.ContainsKey((int)chunks.MCLV))
            {
                root_mcnk.RemoveUnwantedChunksUntil(pos, (int)chunks.MCNR);
            }

            int ofsMCNR = pos;
            pos += 448 + 0x8;
            root_mcnk.WriteInt(ofsMCNR + 0x4, 435);

            int ofsMCLY = 0;
            int sizeMCLY = 0;

            int nLayer = 0;

            List<int> mcal_offsets = new List<int>();

            if (tex_chunks.ContainsKey((int)chunks.MCLY))
            {
                tex_mcnk.RemoveUnwantedChunksUntil(0, (int)chunks.MCLY);
                ofsMCLY = pos;
                int size = tex_mcnk.ReadInt(0x4);
                sizeMCLY = size + 0x8;
                root_mcnk.AddEmptyBytes(pos, sizeMCLY);
                tex_mcnk.BlockCopy(0, root_mcnk, pos, sizeMCLY);
                tex_mcnk.RemoveBytes(0, sizeMCLY);

                nLayer = size / 0x10;

                int layer_pos = pos + 0x8;
                for (int i = 0; i < nLayer; ++i)
                {
                    uint groundEffect = root_mcnk.ReadUInt(layer_pos + 0xC);
                    if (groundEffect > 73186) // max wotlk id in GroundEffectTexture
                    {
                        root_mcnk.WriteInt(layer_pos + 0xC, 0);
                    }
                    root_mcnk.WriteUInt(layer_pos + 0x4, root_mcnk.ReadUInt(layer_pos + 0x4) & 0x7FF);
                    mcal_offsets.Add(root_mcnk.ReadInt(layer_pos + 0x8));

                    layer_pos += 0x10;
                }

                if (nLayer > 4)
                {
                    root_mcnk.RemoveBytes(pos + 0x8 + 64, 16 * (nLayer - 4));
                    sizeMCLY = 64 + 0x8;
                    root_mcnk.WriteInt(pos + 0x4, 64);
                    nLayer = 4;
                }
                pos += sizeMCLY;
            }

            int ofsMCRF = pos;
            int sizeMCRF = 0x8;

            int nDoodads = 0, nMapObjRefs = 0;

            root_mcnk.AddEmptyBytes(pos, 8);
            root_mcnk.WriteInt(pos, ChunkedWowFile.MagicToInt("MCRF"));
            pos += 0x8;

            if (models)
            {
                if (obj_chunks.ContainsKey((int)chunks.MCRD))
                {
                    obj_mcnk.RemoveUnwantedChunksUntil(0, (int)chunks.MCRD);
                    int sizeMCRD = obj_mcnk.ReadInt(0x4);
                    root_mcnk.AddEmptyBytes(pos, sizeMCRD);
                    obj_mcnk.BlockCopy(0x8, root_mcnk, pos, sizeMCRD);
                    pos += sizeMCRD;
                    nDoodads = sizeMCRD / 4;
                    sizeMCRF += sizeMCRD;
                    obj_mcnk.RemoveBytes(0, 0x8 + sizeMCRD);
                }

                if (obj_chunks.ContainsKey((int)chunks.MCRW))
                {
                    obj_mcnk.RemoveUnwantedChunksUntil(0, (int)chunks.MCRW);
                    int sizeMCRW = obj_mcnk.ReadInt(0x4);
                    root_mcnk.AddEmptyBytes(pos, sizeMCRW);
                    obj_mcnk.BlockCopy(0x8, root_mcnk, pos, sizeMCRW);
                    pos += sizeMCRW;
                    nMapObjRefs = sizeMCRW / 4;
                    sizeMCRF += sizeMCRW;
                    obj_mcnk.RemoveBytes(0, 0x8 + sizeMCRW);
                }
            }

            // update MCRF size
            root_mcnk.WriteInt(ofsMCRF + 0x4, sizeMCRF - 0x8);

            // MCSH
            int ofsMCSH = 0;
            int sizeMCSH = 0;

            if (tex_chunks.ContainsKey((int)chunks.MCSH))
            {
                ofsMCSH = pos;
                tex_mcnk.RemoveUnwantedChunksUntil(0, (int)chunks.MCSH);
                sizeMCSH = tex_mcnk.ReadInt(0x4) + 0x8;
                root_mcnk.AddEmptyBytes(pos, sizeMCSH);
                tex_mcnk.BlockCopy(0, root_mcnk, pos, sizeMCSH);
                pos += sizeMCSH;
                tex_mcnk.RemoveBytes(0, sizeMCSH);
            }


            // MCAL
            int ofsMCAL = 0;
            int sizeMCAL = 0;
            ofsMCAL = pos;

            if (tex_chunks.ContainsKey((int)chunks.MCAL))
            {
                tex_mcnk.RemoveUnwantedChunksUntil(0, (int)chunks.MCAL);
                sizeMCAL = tex_mcnk.ReadInt(0x4) + 0x8;
                root_mcnk.AddEmptyBytes(pos, sizeMCAL);
                tex_mcnk.BlockCopy(0, root_mcnk, pos, sizeMCAL);
                tex_mcnk.RemoveBytes(0, sizeMCAL);

                if (mcal_offsets.Count() > 4)
                {
                    int size = sizeMCAL - 0x8;
                    int target = mcal_offsets[4];
                    root_mcnk.RemoveBytes(pos + 0x8 + target, size - target);
                    sizeMCAL = target + 0x8;
                    root_mcnk.WriteInt(pos + 0x4, target);
                }

                pos += sizeMCAL;

            }
            if (sizeMCAL == 0)
            {
                root_mcnk.AddEmptyBytes(pos, 8);
                root_mcnk.WriteInt(pos, (int)chunks.MCAL);
                sizeMCAL = 0x8;
                pos += 8;
            }

            // MCSE
            int ofsMCSE = pos;
            int sizeMCSE = 0;
            int nSoundEmitter = 0;

            if (adt_chunks.ContainsKey((int)chunks.MCSE))
            {
                root_mcnk.RemoveUnwantedChunksUntil(pos, (int)chunks.MCSE);
                sizeMCSE = root_mcnk.ReadInt(pos + 0x4) + 0x8;
                nSoundEmitter = (sizeMCSE - 8) / 0x1C;
                pos += sizeMCSE;
            }
            else
            {
                ofsMCSE = 0;
            }



            adt.AddEmptyBytes(currentPos, root_mcnk.Size() - size_root_mcnk - 8);
            root_mcnk.BlockCopy(0, adt.Data, currentPos, root_mcnk.Size());

            FillMCIN(id, currentPos, root_mcnk.Size());

            adt.WriteInt(currentPos, ChunkedWowFile.MagicToInt("MCNK")); // should not be necessary
            adt.WriteInt(currentPos + 0x4, root_mcnk.Size() - 0x8);

            int ofsPos = currentPos + 0x8; // MCNK header for offsets

            // Update headers
            adt.WriteInt(ofsPos + 0x4, (id % 16));
            adt.WriteInt(ofsPos + 0x8, (id / 16));
            // nLayer
            adt.WriteInt(ofsPos + 0xC, nLayer);
            // nDoodads
            adt.WriteInt(ofsPos + 0x10, nDoodads);
            adt.WriteInt(ofsPos + 0x14, ofsMCVT);
            adt.WriteInt(ofsPos + 0x18, ofsMCNR);
            adt.WriteInt(ofsPos + 0x1C, ofsMCLY);
            adt.WriteInt(ofsPos + 0x20, ofsMCRF);
            adt.WriteInt(ofsPos + 0x24, ofsMCAL);
            // sizeAlpha
            adt.WriteInt(ofsPos + 0x28, sizeMCAL);
            adt.WriteInt(ofsPos + 0x2C, ofsMCSH);
            // size shadow
            adt.WriteInt(ofsPos + 0x30, sizeMCSH);
            // area id
            adt.WriteInt(ofsPos + 0x34, 0);
            // nMapObjRefs
            adt.WriteInt(ofsPos + 0x38, nMapObjRefs);
            adt.WriteInt(ofsPos + 0x50, 0);
            adt.WriteInt(ofsPos + 0x58, ofsMCSE);
            // nSoundEmitter
            adt.WriteInt(ofsPos + 0x5C, nSoundEmitter);
            adt.WriteInt(ofsPos + 0x60, 0); // MCLQ
            adt.WriteInt(ofsPos + 0x64, 0); // size Liquid
            adt.WriteInt(ofsPos + 0x74, ofsMCCV);
            adt.WriteInt(ofsPos + 0x78, 0);
            adt.WriteInt(ofsPos + 0x7C, 0);

            currentPos += root_mcnk.Size();
        }

        private void FillMCIN(int id, int ofs, int size)
        {
            int pos = 0x5C + 0x10 * id;
            adt.WriteInt(pos, ofs);
            adt.WriteInt(pos + 0x4, size);
        }

        private void CheckMFBO()
        {
            adt.RemoveUnwantedChunksUntil(currentPos, "MFBO");

            if (currentPos < adt.Size() - 4 && adt.ReadInt(currentPos) == ChunkedWowFile.MagicToInt("MFBO"))
            {
                adt.WriteInt(0x38, currentPos - 0x14);
                currentPos += adt.ReadInt(currentPos + 0x4) + 0x8;
            }
            else
            {
                adt.WriteInt(0x38, 0x0);
            }
        }


        private void RetrieveMTXF()
        {
            adt.WriteInt(0x40, currentPos - 0x14);

            int posMTXF = 0, posMTXP = 0;
            int i = 0;
            for (i = 0; i < tex.Size() - 8; i += tex.ReadInt(i + 4))
            {
                if (tex.IsChunk(i, "MTXF"))
                {
                    posMTXF = i;
                    break;
                }
                else if (tex.IsChunk(i, "MTXP"))
                {
                    posMTXP = i;
                    break;
                }
            }

            if (posMTXF > 0)
            {
                int size = tex.ReadInt(i + 0x4);
                adt.AddEmptyBytes(currentPos, 0x8 + size);
                adt.WriteInt(currentPos, ChunkedWowFile.MagicToInt("MTXF"));
                adt.WriteInt(currentPos + 0x4, size);
                currentPos += 0x8;

                for (int k = posMTXF + 0x8; k < posMTXF + 0x8 + size; k += 0x4)
                {
                    // only flag used in wotlk
                    adt.WriteInt(currentPos, tex.Data[k] & 0x1);
                    currentPos += 0x4;
                }
            }
            else if (posMTXP > 0)
            {
                int size = tex.ReadInt(i + 0x4);
                int mtxf_size = size / 4;

                adt.AddEmptyBytes(currentPos, 0x8 + mtxf_size);
                adt.WriteInt(currentPos, ChunkedWowFile.MagicToInt("MTXF"));
                adt.WriteInt(currentPos + 0x4, mtxf_size);
                currentPos += 0x8;

                for (int k = posMTXP + 0x8; k < posMTXP + 0x8 + size; k += 0x10)
                {
                    // only flag used in wotlk
                    adt.WriteInt(currentPos, tex.Data[k] & 0x1);
                    currentPos += 0x4;
                }
            }
            else
            {
                adt.AddEmptyBytes(currentPos, 0x8 + textureCount * 4);
                adt.WriteInt(currentPos, ChunkedWowFile.MagicToInt("MTXF"));
                adt.WriteInt(currentPos + 0x4, textureCount * 4);
                currentPos += 0x8 + textureCount * 4;
            }
        }

        public void Save()
        {
            adt.Save();

            // Cleanup
            obj.RemoveOnDisk();
            tex.RemoveOnDisk();
            Utils.DeleteFile(file.Replace(".adt", ".tex"));
            Utils.DeleteFile(file.Replace(".adt", "_occ.wdt"));
            Utils.DeleteFile(file.Replace(".adt", "_lgt.wdt"));
        }
    }
}
