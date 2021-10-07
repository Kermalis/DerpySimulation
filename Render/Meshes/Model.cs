using Silk.NET.OpenGL;
using System.Collections;
using System.Collections.Generic;

namespace DerpySimulation.Render.Meshes
{
    internal sealed class Model : IReadOnlyList<Mesh>
    {
        private readonly IReadOnlyList<Mesh> _meshes;

        public int Count => _meshes.Count;
        public Mesh this[int index] => _meshes[index];

        public Model(IReadOnlyList<Mesh> meshes)
        {
            _meshes = meshes;
        }

        public IEnumerator<Mesh> GetEnumerator()
        {
            return _meshes.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _meshes.GetEnumerator();
        }

        public void Render(GL gl)
        {
            for (int i = 0; i < _meshes.Count; i++)
            {
                _meshes[i].Render(gl);
            }
        }
        public void RenderInstanced(GL gl, uint numInstances)
        {
            for (int i = 0; i < _meshes.Count; i++)
            {
                _meshes[i].RenderInstanced(gl, numInstances);
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
