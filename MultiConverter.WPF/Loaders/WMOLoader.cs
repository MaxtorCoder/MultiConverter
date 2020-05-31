using MultiConverter.Lib.Readers.WMO;
using MultiConverter.Lib.Readers.WMO.Chunks;
using MultiConverter.Lib.Readers.WMO.Chunks.GroupChunks;
using MultiConverter.Lib.RenderingObject;
using MultiConverter.Lib.RenderingObject.Structures;
using MultiConverter.WPF.Util;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiConverter.WPF.Loaders
{
    public class WMOLoader : WorldObject
    {
        public List<WMOGroupFile> GroupFiles;

        public void ReadWMO(string filename, int shaderProgram)
        {
            var wmoFile = new WMOFile();
            wmoFile.ReadWMO(CASC.CascHandler, filename);

            var momt = wmoFile.GetChunk("MOMT") as MOMT;
            var mohd = wmoFile.GetChunk("MOHD") as MOHD;
            GroupFiles = new List<WMOGroupFile>((int)mohd.MOHDEntry.GroupCount);

            var gfid = wmoFile.GetChunk("GFID") as GFID;
            if (gfid.GroupFileDataIds.Count > 0)
            {
                for (var i = 0; i < mohd.MOHDEntry.GroupCount; ++i)
                {
                    if (CASC.CascHandler.FileExists((int)gfid.GroupFileDataIds[i]))
                    {
                        using (var stream = CASC.CascHandler.OpenFile((int)gfid.GroupFileDataIds[i]))
                        {
                            var groupFile = new WMOGroupFile();
                            groupFile.Read(stream);
                            GroupFiles.Add(groupFile);
                        }
                    }
                    else
                        Console.WriteLine($"{gfid.GroupFileDataIds[i]} does not exist!");
                }
            }

            WorldModel = new WorldModel
            {
                GroupBatches    = new List<GroupBatch>(),
                Materials       = new List<Material>(),
                Batches         = new List<RenderBatch>()
            };

            GL.Enable(EnableCap.Texture2D);

            for (var i = 0; i < mohd.MOHDEntry.MaterialCount; ++i)
            {
                var material = new Material
                {
                    Texture1 = momt.MOMTs[i].TextureId1,
                    Texture2 = momt.MOMTs[i].TextureId2,
                    Texture3 = momt.MOMTs[i].TextureId3,
                };

                material.TextureId1 = BLPLoader.LoadTexture(momt.MOMTs[i].TextureId1);
                material.TextureId2 = BLPLoader.LoadTexture(momt.MOMTs[i].TextureId2);
                material.TextureId3 = BLPLoader.LoadTexture(momt.MOMTs[i].TextureId3);

                WorldModel.Materials.Add(material);
            }

            foreach (var groupFile in GroupFiles)
            {
                var mogp = groupFile.GetChunk("MOGP") as MOGP;
                if (mogp.MOGPEntry.Indices == null)
                    return;

                var groupBatch = new GroupBatch
                {
                    Vao = GL.GenVertexArray(),
                    VertexBuffer = GL.GenBuffer(),
                    IndiceBuffer = GL.GenBuffer()
                };

                GL.BindVertexArray(groupBatch.Vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, groupBatch.VertexBuffer);

                var wmoVertices = new List<Vertex>();
                for (var i = 0; i < mogp.MOGPEntry.Vertices.Length; ++i)
                {
                    var vertex = new Vertex
                    {
                        Position = new Vector3(mogp.MOGPEntry.Vertices[i].Vector.X, mogp.MOGPEntry.Vertices[i].Vector.Y, mogp.MOGPEntry.Vertices[i].Vector.Z),
                        Normal = new Vector3(mogp.MOGPEntry.Normals[i].Vector.X, mogp.MOGPEntry.Normals[i].Vector.Y, mogp.MOGPEntry.Normals[i].Vector.Z)
                    };

                    if (mogp.MOGPEntry.TextureCoords[0] == null)
                        vertex.TextureCoord = new Vector2(0.0f, 0.0f);
                    else
                        vertex.TextureCoord = new Vector2(mogp.MOGPEntry.TextureCoords[0][i].X, mogp.MOGPEntry.TextureCoords[0][i].Y);

                    wmoVertices.Add(vertex);
                }

                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(wmoVertices.Count * 8 * sizeof(float)), wmoVertices.ToArray(), BufferUsageHint.StaticDraw);

                var texCoordAttrib = GL.GetAttribLocation(shaderProgram, "texCoord");
                GL.EnableVertexAttribArray(texCoordAttrib);
                GL.VertexAttribPointer(texCoordAttrib, 2, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 3);

                var posAttrib = GL.GetAttribLocation(shaderProgram, "position");
                GL.EnableVertexAttribArray(posAttrib);
                GL.VertexAttribPointer(posAttrib, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, sizeof(float) * 5);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, groupBatch.IndiceBuffer);

                var indiceList = new List<uint>();
                for (var j = 0; j < mogp.MOGPEntry.Indices.Length; j++)
                    indiceList.Add(mogp.MOGPEntry.Indices[j].Indice);

                groupBatch.Indices = indiceList.ToArray();
                WorldModel.GroupBatches.Add(groupBatch);

                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(groupBatch.Indices.Length * 4), groupBatch.Indices, BufferUsageHint.StaticDraw);

                for (var i = 0; i < mogp.MOGPEntry.RenderBatches.Length; ++i)
                {
                    var batch = new RenderBatch
                    {
                        FirstFace = mogp.MOGPEntry.RenderBatches[i].FirstFace,
                        NumFaces = mogp.MOGPEntry.RenderBatches[i].NumFaces,
                        GroupId = (uint)GroupFiles.IndexOf(groupFile),
                        MaterialId = new int[3]
                    };

                    var materialId = 0;
                    if (mogp.MOGPEntry.RenderBatches[i].Flags == 2)
                        materialId = mogp.MOGPEntry.RenderBatches[i].PossibleBox2_3;
                    else
                        materialId = mogp.MOGPEntry.RenderBatches[i].MaterialId;

                    batch.BlendType = momt.MOMTs[materialId].BlendMode;

                    for (var j = 0; j < momt.MOMTs.Count; ++j)
                    {
                        if (WorldModel.Materials[materialId].Texture1 == momt.MOMTs[j].TextureId1)
                        {
                            batch.MaterialId[0] = WorldModel.Materials[materialId].TextureId1;
                        }

                        if (WorldModel.Materials[materialId].Texture2 == momt.MOMTs[j].TextureId2)
                        {
                            batch.MaterialId[1] = WorldModel.Materials[materialId].TextureId1;
                        }

                        if (WorldModel.Materials[materialId].Texture3 == momt.MOMTs[j].TextureId3)
                        {
                            batch.MaterialId[2] = WorldModel.Materials[materialId].TextureId1;
                        }
                    }

                    WorldModel.Batches.Add(batch);
                }
            }
        }
    }
}
