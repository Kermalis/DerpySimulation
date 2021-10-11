using DerpySimulation.Input;
using DerpySimulation.Render.GUIs.Positioning;
using DerpySimulation.Render.Renderers;
using Silk.NET.OpenGL;
using System.Collections.Generic;

namespace DerpySimulation.Render.GUIs
{
    internal abstract class GUIComponent
    {
        protected GUIRect RelPos;
        private GUIConstraints _constraints = null!; // Set in SetConstraints()

        public bool IsInvisible;

        /// <summary>The parent of this component. If this component is the top component, it will be its own parent.
        /// Will be set either in <see cref="AddChild(GUIComponent, GUIConstraints)"/> (called by <see cref="Init"/>) or in <see cref="InitAsTopComponent"/>.</summary>
        public GUIComponent Parent { get; private set; } = null!;
        private GUIComponent? _firstChild;
        private GUIComponent? _nextSibling;

        /// <summary>Init is called to create and add children.</summary>
        protected abstract void Init(GL gl);
        /// <summary>Sets this component to be the top component for rendering, then calls <see cref="Init"/>. It will fill the whole screen.</summary>
        public void InitAsTopComponent(GL gl)
        {
            GUIRenderer.Instance.TopComponent = this;
            Parent = this;
            SetConstraints(RelativeConstraint.CreateFillConstraints());
            RelPos = new GUIRect { W = 1, H = 1 };
            Init(gl);
        }

        private void SetConstraints(GUIConstraints cons)
        {
            _constraints = cons;
            cons.OnAttached(this);
        }

        protected void AddChild(GL gl, GUIComponent child, GUIConstraints cons)
        {
            child.Parent = this;
            child.SetConstraints(cons);
            GUIComponent? oldFirst = _firstChild;
            _firstChild = child;
            child._nextSibling = oldFirst;
            child.CalculateRelPos(true);
            child.Init(gl);
        }

        /// <summary>Updates this component. Does not update children.</summary>
        protected abstract void Update(GL gl, float delta);
        /// <summary>Updates this component, then all of its children.</summary>
        public void UpdateAllVisible(GL gl, float delta)
        {
            if (IsInvisible)
            {
                return;
            }
            Update(gl, delta);
            for (GUIComponent? c = _firstChild; c is not null; c = c._nextSibling)
            {
                c.UpdateAllVisible(gl, delta);
            }
        }

        /// <summary>Handles this component being deleted. Does not handle children.</summary>
        protected virtual void Delete(GL gl)
        {
            //
        }
        /// <summary>Handles this component being deleted, then all of its children.</summary>
        public void DeleteAll(GL gl)
        {
            Delete(gl);
            for (GUIComponent? c = _firstChild; c is not null; c = c._nextSibling)
            {
                c.Delete(gl);
            }
        }

        /// <summary>Finds all visuals to render for this component and its children.</summary>
        public IEnumerable<IGUIVisual> GetVisualsCheckVisible()
        {
            if (IsInvisible)
            {
                yield break;
            }
            foreach (IGUIVisual v in GetVisuals())
            {
                yield return v;
            }
        }
        protected virtual IEnumerable<IGUIVisual> GetVisuals()
        {
            for (GUIComponent? c = _firstChild; c is not null; c = c._nextSibling)
            {
                foreach (IGUIVisual v in c.GetVisualsCheckVisible())
                {
                    yield return v;
                }
            }
        }

        /// <summary>Re-calculates the screen position of this component and its children.</summary>
        public void OnDisplayResized()
        {
            CalculateRelPos(true);
            for (GUIComponent? c = _firstChild; c is not null; c = c._nextSibling)
            {
                c.OnDisplayResized();
            }
        }
        public float GetAbsWidth()
        {
            return RelPos.W * Display.CurrentWidth;
        }
        public float GetAbsHeight()
        {
            return RelPos.H * Display.CurrentHeight;
        }
        private void CalculateRelPos(bool calcSize)
        {
            // TODO: ANIMATIONS
            ref GUIRect parent = ref Parent.RelPos;
            RelPos.X = (_constraints.X.GetRelativeValue() * parent.W) + parent.X;
            RelPos.Y = (_constraints.Y.GetRelativeValue() * parent.H) + parent.Y;
            // Don't need to calculate size for most animations
            if (calcSize)
            {
                RelPos.W = _constraints.W.GetRelativeValue() * parent.W;
                RelPos.H = _constraints.H.GetRelativeValue() * parent.H;
            }
        }

        public bool IsMouseOver()
        {
            if (IsInvisible)
            {
                return false;
            }
            float x = (float)Mouse.X / Display.CurrentWidth;
            float y = (float)Mouse.Y / Display.CurrentHeight;
            return RelPos.Contains(x, y);
        }
    }
}
