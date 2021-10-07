using DerpySimulation.Render.Shaders;
using Silk.NET.OpenGL;
using System.Collections.Generic;

namespace DerpySimulation.Render.Meshes
{
    internal struct MeshTexture
    {
        public readonly string Path;
        public readonly uint Name;
        public readonly string Type;

        public MeshTexture(string path, uint name, string type)
        {
            Path = path;
            Name = name;
            Type = type;
        }
    }
    internal sealed class TexturedMesh
    {
        public const string DIFFUSE_STR = "texture_diffuse";
        public const string SPECULAR_STR = "texture_specular";

        private readonly Mesh _mesh;
        private readonly IReadOnlyList<MeshTexture> _textures;

        public TexturedMesh(Mesh mesh, IReadOnlyList<MeshTexture> textures)
        {
            _mesh = mesh;
            _textures = textures;
        }

        public void Render(GL gl, GLShader shader)
        {
            BeginRender(gl, shader);
            _mesh.Render(gl);
            EndRender(gl);
        }
        public void RenderInstanced(GL gl, GLShader shader, uint numInstances)
        {
            BeginRender(gl, shader);
            _mesh.RenderInstanced(gl, numInstances);
            EndRender(gl);
        }
        private void BeginRender(GL gl, GLShader shader)
        {
            // Activate all textures
            for (int i = 0; i < _textures.Count; i++)
            {
                // Set texture unit in the uniform
                int loc = shader.GetUniformLocation(gl, _textures[i].Type + (i + 1), throwIfNotExists: false);
                if (loc != -1)
                {
                    gl.Uniform1(loc, i);
                }

                gl.ActiveTexture(i.ToTextureUnit());
                gl.BindTexture(TextureTarget.Texture2D, _textures[i].Name);
            }
        }
        private void EndRender(GL gl)
        {
            // Deactivate all textures
            for (int i = 0; i < _textures.Count; i++)
            {
                gl.ActiveTexture(i.ToTextureUnit());
                gl.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        public void Delete(GL gl)
        {
            _mesh.Delete(gl);
            for (int i = 0; i < _textures.Count; i++)
            {
                gl.DeleteTexture(_textures[i].Name);
            }
        }
    }
}
