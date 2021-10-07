using DerpySimulation.Core;
using DerpySimulation.Render.Data;
using Silk.NET.Assimp;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DerpySimulation.Render.Meshes
{
    internal static unsafe class AssimpLoader
    {
        private static readonly Assimp _assimp = Assimp.GetApi();

        public static Model ImportModel(GL gl, string asset)
        {
            Scene* scene = BeginImport(gl, asset, out string dir);
            var meshes = new List<Mesh>();
            ProcessNode(gl, meshes, scene->MRootNode, scene, dir);
            return new Model(meshes);
        }
        public static TexturedModel ImportTexturedModel(GL gl, string asset)
        {
            Scene* scene = BeginImport(gl, asset, out string dir);
            var meshes = new List<TexturedMesh>();
            var loadedTextures = new List<MeshTexture>();
            ProcessNodeTextured(gl, meshes, scene->MRootNode, scene, dir, loadedTextures);
            return new TexturedModel(meshes);
        }
        private static Scene* BeginImport(GL gl, string asset, out string dir)
        {
            asset = AssetLoader.GetPath(asset);
            Scene* scene = _assimp.ImportFile(asset, (uint)(PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.FlipUVs));

            // Check for errors
            if (scene is null || scene->MFlags == (uint)SceneFlags.Incomplete || scene->MRootNode is null)
            {
                throw new InvalidDataException(_assimp.GetErrorStringS());
            }

            dir = Path.GetDirectoryName(asset);
            return scene;
        }

        private static void ProcessNode(GL gl, List<Mesh> meshes, Node* node, Scene* scene, string dir)
        {
            for (uint i = 0; i < node->MNumMeshes; i++)
            {
                Silk.NET.Assimp.Mesh* mesh = scene->MMeshes[node->MMeshes[i]];
                meshes.Add(ProcessMesh(gl, mesh, scene, dir));
            }

            for (uint i = 0; i < node->MNumChildren; i++)
            {
                ProcessNode(gl, meshes, node->MChildren[i], scene, dir);
            }
        }
        private static void ProcessNodeTextured(GL gl, List<TexturedMesh> meshes, Node* node, Scene* scene, string dir, List<MeshTexture> loadedTextures)
        {
            for (uint i = 0; i < node->MNumMeshes; i++)
            {
                Silk.NET.Assimp.Mesh* mesh = scene->MMeshes[node->MMeshes[i]];
                meshes.Add(ProcessMeshTextured(gl, mesh, scene, dir, loadedTextures));
            }

            for (uint i = 0; i < node->MNumChildren; i++)
            {
                ProcessNodeTextured(gl, meshes, node->MChildren[i], scene, dir, loadedTextures);
            }
        }

        private static Mesh ProcessMesh(GL gl, Silk.NET.Assimp.Mesh* mesh, Scene* scene, string dir)
        {
            var vertices = new VBOData_PosNormal[mesh->MNumVertices];

            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                vertices[i] = new VBOData_PosNormal(mesh->MVertices[i], mesh->MNormals[i]);
            }

            // Get vertex indices from mesh faces (triangles)
            var indices = new List<uint>();
            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                Face f = mesh->MFaces[i];
                for (uint j = 0; j < f.MNumIndices; j++)
                {
                    indices.Add(f.MIndices[j]);
                }
            }

            return CreateMesh(gl, vertices, indices);
        }
        private static TexturedMesh ProcessMeshTextured(GL gl, Silk.NET.Assimp.Mesh* mesh, Scene* scene, string dir, List<MeshTexture> loadedTextures)
        {
            var vertices = new VBOData_PosNormalUV[mesh->MNumVertices];

            for (uint i = 0; i < mesh->MNumVertices; i++)
            {
                // UVs
                Vector2 uv;
                if (mesh->MTextureCoords[0] is not null)
                {
                    Vector3 v = mesh->MTextureCoords[0][i];
                    uv = new Vector2(v.X, v.Y);
                }
                else
                {
                    uv = default; // 0,0
                }

                vertices[i] = new VBOData_PosNormalUV(mesh->MVertices[i], mesh->MNormals[i], uv);
            }

            // Get vertex indices from mesh faces (triangles)
            var indices = new List<uint>();
            for (uint i = 0; i < mesh->MNumFaces; i++)
            {
                Face f = mesh->MFaces[i];
                for (uint j = 0; j < f.MNumIndices; j++)
                {
                    indices.Add(f.MIndices[j]);
                }
            }

            // Materials
            var textures = new List<MeshTexture>();
            if (mesh->MMaterialIndex >= 0)
            {
                Material* material = scene->MMaterials[mesh->MMaterialIndex];

                // Diffuse map
                textures.AddRange(LoadMaterialTextures(gl, material, TextureType.TextureTypeDiffuse, TexturedMesh.DIFFUSE_STR, dir, loadedTextures));
                // Specular map
                textures.AddRange(LoadMaterialTextures(gl, material, TextureType.TextureTypeSpecular, TexturedMesh.SPECULAR_STR, dir, loadedTextures));
                // Not supporting other textures (for now)
            }

            return CreateMeshTextured(gl, vertices, indices, textures);
        }

        private static Mesh CreateMesh(GL gl, VBOData_PosNormal[] vertices, List<uint> indices)
        {
            uint elementCount = (uint)indices.Count;

            // Create vao
            uint _vao = gl.GenVertexArray();
            gl.BindVertexArray(_vao);

            // Create ebo
            uint ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            fixed (void* d = CollectionsMarshal.AsSpan(indices))
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, sizeof(uint) * elementCount, d, BufferUsageARB.StaticDraw);
            }

            // Create vbo
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* d = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, VBOData_PosNormalUV.SizeOf * (uint)vertices.Length, d, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VBOData_PosNormal.SizeOf, (void*)VBOData_PosNormal.OffsetOfPos);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VBOData_PosNormal.SizeOf, (void*)VBOData_PosNormal.OffsetOfNormal);

            gl.BindVertexArray(0);

            return new Mesh(_vao, elementCount, false, ebo, vbo);
        }
        private static TexturedMesh CreateMeshTextured(GL gl, VBOData_PosNormalUV[] vertices, List<uint> indices, List<MeshTexture> textures)
        {
            uint elementCount = (uint)indices.Count;

            // Create vao
            uint vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);

            // Create ebo
            uint ebo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            fixed (void* d = CollectionsMarshal.AsSpan(indices))
            {
                gl.BufferData(BufferTargetARB.ElementArrayBuffer, sizeof(uint) * elementCount, d, BufferUsageARB.StaticDraw);
            }

            // Create vbo
            uint vbo = gl.GenBuffer();
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            fixed (void* d = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, VBOData_PosNormalUV.SizeOf * (uint)vertices.Length, d, BufferUsageARB.StaticDraw);
            }

            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VBOData_PosNormalUV.SizeOf, (void*)VBOData_PosNormalUV.OffsetOfPos);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, VBOData_PosNormalUV.SizeOf, (void*)VBOData_PosNormalUV.OffsetOfNormal);
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, VBOData_PosNormalUV.SizeOf, (void*)VBOData_PosNormalUV.OffsetOfUV);

            gl.BindVertexArray(0);

            return new TexturedMesh(new Mesh(vao, elementCount, false, ebo, vbo), textures);
        }

        private static List<MeshTexture> LoadMaterialTextures(GL gl, Material* mat, TextureType type, string ttype, string dir, List<MeshTexture> loaded)
        {
            var textures = new List<MeshTexture>();

            uint count = _assimp.GetMaterialTextureCount(mat, type);
            for (uint i = 0; i < count; i++)
            {
                AssimpString path = default;
                if (_assimp.GetMaterialTexture(mat, type, i, &path, null, null, null, null, null, null) != Return.ReturnSuccess)
                {
                    throw new Exception("Error loading material textures");
                }

                string sPath = path.AsString;

                for (int j = 0; j < loaded.Count; j++)
                {
                    MeshTexture l = loaded[j];
                    if (l.Path == sPath)
                    {
                        textures.Add(l);
                        goto dontload; // break out of this loop and continue on the parent loop
                    }
                }

                // Load it
                gl.ActiveTexture(TextureUnit.Texture0);
                uint tex = gl.GenTexture();
                gl.BindTexture(TextureTarget.Texture2D, tex);
                RenderUtils.LoadTextureFromFile(gl, Path.Combine(dir, sPath));

                var texture = new MeshTexture(sPath, tex, ttype);
                textures.Add(texture);
                loaded.Add(texture);

            dontload:
                ;
            }

            return textures;
        }

        public static void Quit()
        {
            _assimp.Dispose();
        }
    }
}
