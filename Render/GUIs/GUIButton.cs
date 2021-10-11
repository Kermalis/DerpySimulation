using DerpySimulation.Input;
using Silk.NET.OpenGL;

namespace DerpySimulation.Render.GUIs
{
    internal abstract class GUIButton : GUIComponent
    {
        public delegate void MouseOverDelegate(GUIButton button, bool mouseOver);
        public delegate void ClickedDelegate(GUIButton button, bool clicked);

        private bool _mouseOver;

        private readonly bool _disabled;

        private readonly MouseOverDelegate? OnMouseOver;
        private readonly ClickedDelegate OnClicked;

        protected GUIButton(ClickedDelegate onClicked, MouseOverDelegate? onMouseOver = null, bool disabled = false)
        {
            OnClicked = onClicked;
            OnMouseOver = onMouseOver;
            _disabled = disabled;
        }

        private void UpdateMouseOver()
        {
            bool mouseOver = IsMouseOver();
            if (mouseOver == _mouseOver)
            {
                return;
            }
            _mouseOver = mouseOver;
            // TODO: Start effect
            OnMouseOver?.Invoke(this, mouseOver);
        }
        protected override void Update(GL gl, float delta)
        {
            if (_disabled)
            {
                return;
            }
            UpdateMouseOver();
            if (!_mouseOver)
            {
                return;
            }

            if (Mouse.JustPressed(MouseButton.Left))
            {
                OnClicked(this, true);
            }
            else if (Mouse.JustReleased(MouseButton.Left))
            {
                OnClicked(this, false);
            }
        }
    }
}
