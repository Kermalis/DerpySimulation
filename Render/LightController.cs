using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace DerpySimulation.Render
{
    internal sealed class LightController : IReadOnlyList<PointLight>
    {
        public const int MAX_LIGHTS = 4;

        public static LightController Instance { get; private set; } = null!; // Initialized in ProgramMain

        public int Count { get; private set; }
        public PointLight Sun { get; }
        private readonly List<PointLight> _nonSunLights;

        public PointLight this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return Sun;
                }
                return _nonSunLights[index - 1];
            }
        }

        public LightController()
        {
            Sun = new PointLight(new Vector3(0, 20000, 0), new Vector3(1.25f, 1.125f, 1f));
            Count = 1;
            _nonSunLights = new List<PointLight>();

            Instance = this;
        }

        public void Add(PointLight light)
        {
            if (Count >= MAX_LIGHTS)
            {
                throw new InvalidOperationException("Too many lights");
            }
            _nonSunLights.Add(light);
            Count++;
        }

        public IEnumerator<PointLight> GetEnumerator()
        {
            yield return Sun;
            for (int i = 0; i < _nonSunLights.Count; i++)
            {
                yield return _nonSunLights[i];
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PointLight>)this).GetEnumerator();
        }
    }
}
