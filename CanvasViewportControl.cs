using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rhino.Runtime;

namespace GhCanvasViewport
{
    public class CanvasViewportControl : RhinoWindows.Forms.Controls.ViewportControl
    {
        //private int viewId = 0; // used when we choose add viewports (line 107)

        public Rhino.Display.RhinoViewport RhinoViewport
        {
            get { return Viewport; }
        }

        public CanvasViewportControl()
        {
            // stupid hack to get GH to draw preview geometry in this control
            this.Viewport.Name = "GH_HACK";

            // this used when we choose add viewports (line 107)
            //var views = Rhino.RhinoDoc.ActiveDoc.Views.ToList();   
            //Viewport.Name = views[viewId].ActiveViewport.Name;
        }

        static Icon customIcon = Properties.Resources.rot;
        Cursor customCursor = new Cursor(customIcon.Handle);

        System.Drawing.Point RightMouseDownLocation { get; set; }
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Cursor.Current = customCursor;
                RightMouseDownLocation = e.Location;   
            }
            else
            {
                Cursor.Current = Cursors.Hand;
                RightMouseDownLocation = System.Drawing.Point.Empty;
            }
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
            if (e.Button == MouseButtons.Right)
            {
                var vec = new Rhino.Geometry.Vector2d(e.X - RightMouseDownLocation.X, e.Y - RightMouseDownLocation.Y);
                if (vec.Length < 10)
                {
                    ShowContextMenu(e.Location);
                }
            }
            RightMouseDownLocation = System.Drawing.Point.Empty;
            base.OnMouseUp(e);
        }

        void ShowContextMenu(System.Drawing.Point location)
        {
            var contextMenu = new ContextMenuStrip();

            var displayModeMenu = new ToolStripMenuItem("Display Mode");
            var modes = Rhino.Display.DisplayModeDescription.GetDisplayModes();
            var currentModeId = Guid.Empty;
            if (Viewport.DisplayMode != null)
                currentModeId = Viewport.DisplayMode.Id;

            foreach (Rhino.Display.DisplayModeDescription mode in modes)
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

            // in case the user prefer this instead of combobox
            // add viewports
            /*/
            var viewsMenu = new ToolStripMenuItem("View");
            var views = Rhino.RhinoDoc.ActiveDoc.Views;
            var currentViewId = Viewport.Name;
            //var projection = Rhino.Display.DefinedViewportProjection.Perspective;
            foreach (Rhino.Display.RhinoView view in views)
            {
                var viewMenuItem = new ToolStripMenuItem(view.ActiveViewport.Name);
                viewMenuItem.Checked = true;
                viewMenuItem.Checked = (currentViewId == view.ActiveViewport.Name);
                viewMenuItem.Click += (s, e) =>
                {
                    if (view.ActiveViewport.Name == "Top")
                    { projection = Rhino.Display.DefinedViewportProjection.Top; viewId = 1; }
                    else if (view.ActiveViewport.Name == "Front")
                    { projection = Rhino.Display.DefinedViewportProjection.Front; viewId = 2; }
                    else if (view.ActiveViewport.Name == "Right")
                    { projection = Rhino.Display.DefinedViewportProjection.Right; viewId = 3; }
                    Viewport.SetProjection(projection, view.ActiveViewport.Name, true);     
                    Invalidate();
                };
                viewsMenu.DropDownItems.Add(viewMenuItem);
                //viewsMenu.Tag = view.ActiveViewportID;
            }
            contextMenu.Items.Add(viewsMenu);
            /*/

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
