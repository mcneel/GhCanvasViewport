using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace GhCanvasViewport
{
    class CanvasViewportControl : RhinoWindows.Forms.Controls.ViewportControl
    {
        public CanvasViewportControl()
        {
            // stupid hack to get GH to draw preview geometry in this control
            this.Viewport.Name = "GH_HACK";
        }

        System.Drawing.Point RightMouseDownLocation { get; set; }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                RightMouseDownLocation = e.Location;
            else
                RightMouseDownLocation = System.Drawing.Point.Empty;
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var vec = new Rhino.Geometry.Vector2d(e.X - RightMouseDownLocation.X, e.Y - RightMouseDownLocation.Y);
                if (vec.Length > 10)
                    RightMouseDownLocation = System.Drawing.Point.Empty;
            }
            else
            {
                RightMouseDownLocation = System.Drawing.Point.Empty;
            }
            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right)
            {
                var vec = new Rhino.Geometry.Vector2d(e.X - RightMouseDownLocation.X, e.Y - RightMouseDownLocation.Y);
                if (vec.Length < 10)
                {
                    ShowContextMenu(e.Location);
                }
            }
            RightMouseDownLocation = System.Drawing.Point.Empty;
        }

        void ShowContextMenu(System.Drawing.Point location)
        {
            var contextMenu = new ContextMenu();

            var displayModeMenu = new MenuItem("Display Mode");
            var modes = Rhino.Display.DisplayModeDescription.GetDisplayModes();
            var currentModeId = Guid.Empty;
            if (Viewport.DisplayMode != null)
                currentModeId = Viewport.DisplayMode.Id;

            foreach (var mode in modes)
            {
                var modeMenuItem = new MenuItem(mode.LocalName);
                modeMenuItem.RadioCheck = true;
                modeMenuItem.Checked = (currentModeId == mode.Id);
                modeMenuItem.Click += (s, e) => 
                {
                    Viewport.DisplayMode = mode;
                    Invalidate();
                };
                displayModeMenu.MenuItems.Add(modeMenuItem);
                displayModeMenu.Tag = mode.Id;
            }
            contextMenu.MenuItems.Add(displayModeMenu);

            var dockMenu = new MenuItem("Dock");
            var mnu = new MenuItem("Top Left");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => CanvasViewport.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Left);
            dockMenu.MenuItems.Add(mnu);
            mnu = new MenuItem("Top Right");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => CanvasViewport.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Right);
            dockMenu.MenuItems.Add(mnu);
            mnu = new MenuItem("Bottom Left");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => CanvasViewport.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Left);
            dockMenu.MenuItems.Add(mnu);
            mnu = new MenuItem("Bottom Right");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => CanvasViewport.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Right);
            dockMenu.MenuItems.Add(mnu);
            contextMenu.MenuItems.Add(dockMenu);
            dockMenu.Popup += (s, args) =>
            {
                var anchor = this.Parent.Anchor;
                dockMenu.MenuItems[0].Checked = (anchor == (AnchorStyles.Top | AnchorStyles.Left));
                dockMenu.MenuItems[1].Checked = (anchor == (AnchorStyles.Bottom | AnchorStyles.Left));
                dockMenu.MenuItems[2].Checked = (anchor == (AnchorStyles.Top | AnchorStyles.Right));
                dockMenu.MenuItems[3].Checked = (anchor == (AnchorStyles.Bottom | AnchorStyles.Right));
            };

            contextMenu.MenuItems.Add("Zoom Extents", (s, e) =>
            {
                Viewport.Camera35mmLensLength = 50;
                Viewport.ZoomExtents();
                Refresh();
            });
            contextMenu.MenuItems.Add("Hide", (s, e) =>
            {
                this.Parent.Hide();
            });
            contextMenu.Show(this, location);

        }
    }
}
