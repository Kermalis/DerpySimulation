using DerpySimulation.Render.GUIs.Fonts;
using DerpySimulation.Render.GUIs.Positioning;
using Silk.NET.OpenGL;
using System.Collections.Generic;

namespace DerpySimulation.Render.GUIs.Menus
{
    internal sealed class MenuButton : GUIButton
    {
        private static readonly Color4 TEMP_COLOR = new(Colors.Red, 1f);

        private GUIBlock _background = null!; // Set in Init()
        private readonly GUIString _str;

        public MenuButton(GUIString str, ClickedDelegate onClicked)
            : base(onClicked)
        {
            _str = str;
        }

        protected override void Init(GL gl)
        {
            // Background
            _background = new GUIBlock(TEMP_COLOR);
            AddChild(gl, _background, RelativeConstraint.CreateFillConstraints());
            // Str
            AddChild(gl, _str, _str.CreateCenterConstraints());
        }

        protected override IEnumerable<IGUIVisual> GetVisuals()
        {
            yield return _background;
            yield return _str;
            foreach (GUIBlock b in base.GetVisuals())
            {
                yield return b;
            }
        }

        protected override void Delete(GL gl)
        {
            base.Delete(gl);
        }
    }
}
