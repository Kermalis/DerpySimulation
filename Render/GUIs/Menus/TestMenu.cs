﻿using DerpySimulation.Core;
using DerpySimulation.Render.GUIs.Fonts;
using DerpySimulation.Render.GUIs.Positioning;
using DerpySimulation.Render.Renderers;
using DerpySimulation.Render.Shaders;
using DerpySimulation.World;
using Silk.NET.OpenGL;
using System.Collections.Generic;

namespace DerpySimulation.Render.GUIs.Menus
{
    internal sealed class TestMenu : GUIComponent, IGUIVisual
    {
        private const float BUTTONS_RELX = 0.075f;
        private const float BUTTONS_RELY_START = 0.4f;
        private const float BUTTONS_RELY_GAP = 0.1f;
        private const float BUTTONS_RELW = 0.25f;
        private const float BUTTONS_RELH = 0.075f;

        public TestMenu(GL gl)
        {
            InitAsTopComponent(gl);
            GUIRenderer.Instance.SetTopComponentCallbacks();
        }

        protected override void Init(GL gl)
        {
            int i = 0;
            AddButton(gl, i++, "Begin", OnBeginClicked);
            AddButton(gl, i++, "Quit", OnQuitClicked);
            AddButton(gl, i++, "Test", OnTestClicked);
        }

        private MenuButton AddButton(GL gl, int index, string text, GUIButton.ClickedDelegate clicked)
        {
            var str = new GUIString(text, Font.Instance, FontColors.White_I);
            var button = new MenuButton(str, clicked);
            var cons = new GUIConstraints(
                new RelativeConstraint(BUTTONS_RELX),
                new RelativeConstraint((BUTTONS_RELY_GAP * index) + BUTTONS_RELY_START),
                new RelativeConstraint(BUTTONS_RELW),
                new RelativeConstraint(BUTTONS_RELH)
                );
            AddChild(gl, button, cons);
            return button;
        }
        private static void OnBeginClicked(GUIButton button, bool clicked)
        {
            _ = new SimulationCreator(SimulationCreationSettings.CreatePreset(2));
        }
        private static void OnQuitClicked(GUIButton button, bool clicked)
        {
            ProgramMain.QuitRequested = true;
        }
        private static void OnTestClicked(GUIButton button, bool clicked)
        {
            button.IsInvisible = true;
        }

        protected override void Update(GL gl, float delta)
        {
            //
        }

        public void Render(GL gl, float delta)
        {
            StarNestShader.Instance.Render(gl, delta);
        }

        protected override IEnumerable<IGUIVisual> GetVisuals()
        {
            yield return this;
            foreach (IGUIVisual v in base.GetVisuals())
            {
                yield return v;
            }
        }
    }
}
