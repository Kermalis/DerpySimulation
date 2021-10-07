using DerpySimulation.Render.Shaders;
using Silk.NET.OpenGL;
using System.Collections;
using System.Collections.Generic;

namespace DerpySimulation.Render.Meshes
{
    internal sealed class TexturedModel : IReadOnlyList<TexturedMesh>
    {
        private readonly IReadOnlyList<TexturedMesh> _meshes;

        public int Count => _meshes.Count;
        public TexturedMesh this[int index] => _meshes[index];

        public TexturedModel(IReadOnlyList<TexturedMesh> meshes)
        {
            _meshes = meshes;
        }

        public IEnumerator<TexturedMesh> GetEnumerator()
        {
            return _meshes.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _meshes.GetEnumerator();
        }

        public void Render(GL gl, GLShader shader)
        {
            for (int i = 0; i < _meshes.Count; i++)
            {
                _meshes[i].Render(gl, shader);
            }
        }
        public void RenderInstanced(GL gl, GLShader shader, uint numInstances)
        {
            for (int i = 0; i < _meshes.Count; i++)
            {
                _meshes[i].RenderInstanced(gl, shader, numInstances);
            }
        }

        public void Delete(GL gl)
        {
            for (int i = 0; i < _meshes.Count; i++)
            {
                _meshes[i].Delete(gl);
            }
        }
    }
}
