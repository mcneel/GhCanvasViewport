using System;
using System.Collections.Generic;
using System.Linq;
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
            var contextMenu = new ContextMenuStrip();

            var displayModeMenu = new ToolStripMenuItem("Display Mode");
            var modes = Rhino.Display.DisplayModeDescription.GetDisplayModes();
            var currentModeId = Guid.Empty;
            if (Viewport.DisplayMode != null)
                currentModeId = Viewport.DisplayMode.Id;

            foreach (var mode in modes)
            {
                var modeMenuItem = new ToolStripMenuItem(mode.LocalName);
                modeMenuItem.Checked = true;
                modeMenuItem.Checked = (currentModeId == mode.Id);
                modeMenuItem.Click += (s, e) => 
                {
                    Viewport.DisplayMode = mode;
                    Invalidate();
                };
                displayModeMenu.DropDownItems.Add(modeMenuItem);
                displayModeMenu.Tag = mode.Id;
            }
            contextMenu.Items.Add(displayModeMenu);

            var dockMenu = new ToolStripMenuItem("Dock");
            var mnu = new ToolStripRadioMenuItem("Top Left");
            mnu.Checked = true;
            mnu.Click += (s, args) => CanvasViewport.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Left);
            dockMenu.DropDownItems.Add(mnu);
            mnu = new ToolStripRadioMenuItem("Top Right");
            mnu.Checked = true;
            mnu.Click += (s, args) => CanvasViewport.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Right);
            dockMenu.DropDownItems.Add(mnu);
            mnu = new ToolStripRadioMenuItem("Bottom Left");
            mnu.Checked = true;
            mnu.Click += (s, args) => CanvasViewport.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Left);
            dockMenu.DropDownItems.Add(mnu);
            mnu = new ToolStripRadioMenuItem("Bottom Right");
            mnu.Checked = true;
            mnu.Click += (s, args) => CanvasViewport.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Right);
            dockMenu.DropDownItems.Add(mnu);
            contextMenu.Items.Add(dockMenu);
            dockMenu.DropDownOpening += (s, args) =>
            {
                var anchor = this.Parent.Anchor;
                
                ((ToolStripMenuItem)dockMenu.DropDownItems[0]).Checked = (anchor == (AnchorStyles.Top | AnchorStyles.Left));
                ((ToolStripMenuItem)dockMenu.DropDownItems[1]).Checked = (anchor == (AnchorStyles.Bottom | AnchorStyles.Left));
                ((ToolStripMenuItem)dockMenu.DropDownItems[2]).Checked = (anchor == (AnchorStyles.Top | AnchorStyles.Right));
                ((ToolStripMenuItem)dockMenu.DropDownItems[3]).Checked = (anchor == (AnchorStyles.Bottom | AnchorStyles.Right));
            };

            contextMenu.Items.Add("Zoom Extents", null, (s, e) =>
            {
                Viewport.Camera35mmLensLength = 50;
                Viewport.ZoomExtents();
                Refresh();
            });
            contextMenu.Items.Add("Hide", null, (s, e) =>
            {
                this.Parent.Hide();
            });
            contextMenu.Show(this, location);

        }
    }

    class ToolStripRadioMenuItem : ToolStripMenuItem
    {
        public ToolStripRadioMenuItem(string text) : base(text)
        {
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = true;
            if (OwnerItem is ToolStripMenuItem parent)
            {
                foreach (var item in parent.DropDownItems.OfType<ToolStripRadioMenuItem>())
                {
                    if (ReferenceEquals(this, item))
                        continue;
                        
                    item.Checked = false;
                }
            }
            
            base.OnClick(e);
        }
    }
}
