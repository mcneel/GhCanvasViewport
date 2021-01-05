using System;
using System.Windows.Forms;
using Rhino.Display;
using Rhino.Geometry;

namespace GhCanvasViewport
{
    class CanvasViewportControl : RhinoWindows.Forms.Controls.ViewportControl
    {
        public CanvasViewportControl()
        {
            // stupid hack to get GH to draw preview geometry in this control
            this.Viewport.Name = "GH_HACK";

            bool gridtoggle = CanvasViewport.settings.GetValue("gridtoggle", true);
            bool axestoggle = CanvasViewport.settings.GetValue("axestoggle", true);
            bool worldtoggle = CanvasViewport.settings.GetValue("worldtoggle", true);

            Viewport.ConstructionGridVisible = gridtoggle;
            Viewport.ConstructionAxesVisible = axestoggle;
            Viewport.WorldAxesVisible = worldtoggle;

            ViewReset();
            CamFetcher();
        }

        System.Drawing.Point RightMouseDownLocation { get; set; }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RightMouseDownLocation = e.Location;
            }
            else
            { RightMouseDownLocation = System.Drawing.Point.Empty; }
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                bool locked1 = CanvasViewport.settings.GetValue("locked1", false);
                if (!locked1)
                {
                    var vec = new Rhino.Geometry.Vector2d(e.X - RightMouseDownLocation.X, e.Y - RightMouseDownLocation.Y);
                    if (vec.Length > 10)
                    { RightMouseDownLocation = System.Drawing.Point.Empty; }
                    base.OnMouseMove(e);
                }
            }
            else
            {
                bool locked2 = CanvasViewport.settings.GetValue("locked2", false);
                if (!locked2)
                {
                    RightMouseDownLocation = System.Drawing.Point.Empty;
                    base.OnMouseMove(e);
                }
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right)
            {
                var vec = new Rhino.Geometry.Vector2d(e.X - RightMouseDownLocation.X, e.Y - RightMouseDownLocation.Y);
                if (vec.Length < 10)
                { ShowContextMenu(e.Location); }
            }
            RightMouseDownLocation = System.Drawing.Point.Empty;
        }

        void ShowContextMenu(System.Drawing.Point location)
        {
            var contextMenu = new ContextMenu();

            var lockMenuMain = new MenuItem("Locking Options");

            var lockMenu1 = new MenuItem("Lock Rotation");
            bool locked1 = CanvasViewport.settings.GetValue("locked1", false);
            lockMenu1.Click += (s, e) => { locked1 = !locked1; CanvasViewport.settings.SetValue("locked1", locked1); CanvasViewport.settings.WritePersistentSettings(); CanvasViewport.UpdateViewport(true); };
            lockMenu1.Checked = locked1;
            lockMenuMain.MenuItems.Add(lockMenu1);
            contextMenu.MenuItems.Add(lockMenuMain);

            var lockMenu2 = new MenuItem("Lock Dragging");
            bool locked2 = CanvasViewport.settings.GetValue("locked2", false);
            lockMenu2.Click += (s, e) => { locked2 = !locked2; CanvasViewport.settings.SetValue("locked2", locked2); CanvasViewport.settings.WritePersistentSettings(); CanvasViewport.UpdateViewport(true); };
            lockMenu2.Checked = locked2;
            lockMenuMain.MenuItems.Add(lockMenu2);
            contextMenu.MenuItems.Add(lockMenuMain);

            var iconToggle = new MenuItem("Disable Lock Icons");
            bool icontoggle = CanvasViewport.settings.GetValue("icontoggle", false);
            iconToggle.Click += (s, e) => { icontoggle = !icontoggle; CanvasViewport.settings.SetValue("icontoggle", icontoggle); CanvasViewport.settings.WritePersistentSettings(); CanvasViewport.UpdateViewport(true); };
            iconToggle.Checked = icontoggle;
            lockMenuMain.MenuItems.Add(iconToggle);

            lockMenuMain.MenuItems.Add("-");

            var dockiconsMenu = new MenuItem("Dock Icons");
            string dockicons = CanvasViewport.settings.GetValue("dockicons", "topleft");
            var dockiconsMenuItem = new MenuItem("Top Left");
            dockiconsMenuItem.RadioCheck = true;
            dockiconsMenuItem.Click += (s, e) => { dockicons = "topleft"; CanvasViewport.settings.SetValue("dockicons", dockicons); CanvasViewport.settings.WritePersistentSettings(); CanvasViewport.UpdateViewport(true); };
            dockiconsMenuItem.Checked = (dockicons == "topleft");
            dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
            dockiconsMenuItem = new MenuItem("Top Right");
            dockiconsMenuItem.RadioCheck = true;
            dockiconsMenuItem.Click += (s, e) => { dockicons = "topright"; CanvasViewport.settings.SetValue("dockicons", dockicons); CanvasViewport.settings.WritePersistentSettings(); CanvasViewport.UpdateViewport(true); };
            dockiconsMenuItem.Checked = (dockicons == "topright");
            dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
            dockiconsMenuItem = new MenuItem("Bottom Left");
            dockiconsMenuItem.RadioCheck = true;
            dockiconsMenuItem.Click += (s, e) => { dockicons = "bottomleft"; CanvasViewport.settings.SetValue("dockicons", dockicons); CanvasViewport.settings.WritePersistentSettings(); CanvasViewport.UpdateViewport(true); };
            dockiconsMenuItem.Checked = (dockicons == "bottomleft");
            dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
            dockiconsMenuItem = new MenuItem("Bottom Right");
            dockiconsMenuItem.RadioCheck = true;
            dockiconsMenuItem.Click += (s, e) => { dockicons = "bottomright"; CanvasViewport.settings.SetValue("dockicons", dockicons); CanvasViewport.settings.WritePersistentSettings(); CanvasViewport.UpdateViewport(true); };
            dockiconsMenuItem.Checked = (dockicons == "bottomright");
            dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
            lockMenuMain.MenuItems.Add(dockiconsMenu);

            var styleMenu = new MenuItem("Icon Style");
            string iconstyle = CanvasViewport.settings.GetValue("iconstyle", "colored");

            var styleMenuItem = new MenuItem("Colored");
            styleMenuItem.RadioCheck = true;
            styleMenuItem.Click += (s, e) => { iconstyle = "colored"; CanvasViewport.settings.SetValue("iconstyle", iconstyle); CanvasViewport.settings.WritePersistentSettings(); CanvasViewport.UpdateViewport(true); };
            styleMenuItem.Checked = (iconstyle == "colored");
            styleMenu.MenuItems.Add(styleMenuItem);
            styleMenuItem = new MenuItem("Simple");
            styleMenuItem.RadioCheck = true;
            styleMenuItem.Click += (s, e) => { iconstyle = "simple"; CanvasViewport.settings.SetValue("iconstyle", iconstyle); CanvasViewport.settings.WritePersistentSettings(); CanvasViewport.UpdateViewport(true); };
            styleMenuItem.Checked = (iconstyle == "simple");
            styleMenu.MenuItems.Add(styleMenuItem);
            lockMenuMain.MenuItems.Add(styleMenu);

            contextMenu.MenuItems.Add(lockMenuMain);

            var wireframe = DisplayModeDescription.FindByName("Wireframe");
            var displayModeMenu = new MenuItem("Display Mode");

            var displaymode = Guid.Parse(CanvasViewport.settings.GetValue("displaymode", wireframe.Id.ToString()));
            var modes = DisplayModeDescription.GetDisplayModes();
            var currentModeId = displaymode;
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
                    CanvasViewport.settings.SetValue("displaymode", mode.Id.ToString());
                    CanvasViewport.settings.WritePersistentSettings();
                    CanvasViewport.UpdateViewport(true);
                    Invalidate();
                };
                displayModeMenu.MenuItems.Add(modeMenuItem);
                displayModeMenu.Tag = mode.Id;
            }
            contextMenu.MenuItems.Add(displayModeMenu);

            var viewMenu = new MenuItem("Set View");
            string view = CanvasViewport.settings.GetValue("view", "Perspective");
            var viewMenuItem = new MenuItem("Top");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Top, "Top", true); view = "Top"; CanvasViewport.settings.SetValue("view", view); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            viewMenuItem.Checked = (view == "Top");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Bottom");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Bottom, "Bottom", true); view = "Bottom"; CanvasViewport.settings.SetValue("view", view); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            viewMenuItem.Checked = (view == "Bottom");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Left");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Left, "Left", true); view = "Left"; CanvasViewport.settings.SetValue("view", view); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            viewMenuItem.Checked = (view == "Left");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Right");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Right, "Right", true); view = "Right"; CanvasViewport.settings.SetValue("view", view); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            viewMenuItem.Checked = (view == "Right");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Front");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Front, "Front", true); view = "Front"; CanvasViewport.settings.SetValue("view", view); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            viewMenuItem.Checked = (view == "Front");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Back");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Back, "Back", true); view = "Back"; CanvasViewport.settings.SetValue("view", view); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            viewMenuItem.Checked = (view == "Back");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Perspective");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Perspective, "Perspective", true); view = "Perspective"; CanvasViewport.settings.SetValue("view", view); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            viewMenuItem.Checked = (view == "Perspective");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("TwoPointPerspective");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.TwoPointPerspective, "TwoPointPerspective", true); view = "TwoPointPerspective"; CanvasViewport.settings.SetValue("view", view); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            viewMenuItem.Checked = (view == "TwoPointPerspective");
            viewMenu.MenuItems.Add(viewMenuItem);
            contextMenu.MenuItems.Add(viewMenu);

            string dock = CanvasViewport.settings.GetValue("dock", "topleft");
            var dockMenu = new MenuItem("Dock");
            var mnu = new MenuItem("Top Left");
            mnu.RadioCheck = true;
            mnu.Checked = (dock == "topleft");
            mnu.Click += (s, args) => { CanvasViewport.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Left); dock = "topleft"; CanvasViewport.settings.SetValue("dock", dock); CanvasViewport.settings.WritePersistentSettings(); };
            dockMenu.MenuItems.Add(mnu);
            mnu = new MenuItem("Top Right");
            mnu.Checked = (dock == "topright");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => { CanvasViewport.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Right); dock = "topright"; CanvasViewport.settings.SetValue("dock", dock); CanvasViewport.settings.WritePersistentSettings(); };
            dockMenu.MenuItems.Add(mnu);
            mnu = new MenuItem("Bottom Left");
            mnu.Checked = (dock == "bottomleft");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => { CanvasViewport.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Left); dock = "bottomleft"; CanvasViewport.settings.SetValue("dock", dock); CanvasViewport.settings.WritePersistentSettings(); };
            dockMenu.MenuItems.Add(mnu);
            mnu = new MenuItem("Bottom Right");
            mnu.Checked = (dock == "bottomright");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => { CanvasViewport.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Right); dock = "bottomright"; CanvasViewport.settings.SetValue("dock", dock); CanvasViewport.settings.WritePersistentSettings(); };
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

            contextMenu.MenuItems.Add("-");

            bool gridtoggle = CanvasViewport.settings.GetValue("gridtoggle", true);
            var grid = new MenuItem("Toggle Grid");
            grid.Checked = (gridtoggle);
            grid.Click += (s, args) => { Viewport.ConstructionGridVisible = !gridtoggle; gridtoggle = !gridtoggle; CanvasViewport.settings.SetValue("gridtoggle", gridtoggle); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            contextMenu.MenuItems.Add(grid);

            bool axestoggle = CanvasViewport.settings.GetValue("axestoggle", true);
            var gridaxes = new MenuItem("Toggle Grid Axes");
            gridaxes.Checked = (axestoggle);
            gridaxes.Click += (s, args) => { Viewport.ConstructionAxesVisible = !axestoggle; axestoggle = !axestoggle; CanvasViewport.settings.SetValue("axestoggle", axestoggle); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            contextMenu.MenuItems.Add(gridaxes);

            bool worldtoggle = CanvasViewport.settings.GetValue("worldtoggle", true);
            var worldaxes = new MenuItem("Toggle World Axes");
            worldaxes.Checked = (worldtoggle);
            worldaxes.Click += (s, args) => { Viewport.WorldAxesVisible = !worldtoggle; worldtoggle = !worldtoggle; CanvasViewport.settings.SetValue("worldtoggle", worldtoggle); CanvasViewport.settings.WritePersistentSettings(); Invalidate(); };
            contextMenu.MenuItems.Add(worldaxes);

            contextMenu.MenuItems.Add("Reset Camera", (s, e) =>
            {
                Viewport.SetCameraLocation(camLocation, true);
                Viewport.SetCameraTarget(camTarget, true);
                Refresh();
            });
            contextMenu.MenuItems.Add("Reset View", (s, e) =>
            {
                ViewReset();
                Viewport.SetCameraLocation(camLocation, true);
                Viewport.SetCameraTarget(camTarget, true);
                Viewport.Camera35mmLensLength = 50;
                Viewport.ZoomExtents();
                Refresh();
            });
            contextMenu.MenuItems.Add("Zoom Extents", (s, e) =>
            {
                Viewport.Camera35mmLensLength = 50;
                Viewport.ZoomExtents();
                Refresh();
            });

            contextMenu.MenuItems.Add("-");

            contextMenu.MenuItems.Add("Restore Defaults", (s, e) =>
            {
                CanvasViewport.settings.SetValue("width", 400);
                CanvasViewport.settings.SetValue("height", 300);
                CanvasViewport.settings.SetValue("dock", "topleft");
                CanvasViewport.settings.SetValue("locked1", false);
                CanvasViewport.settings.SetValue("locked2", false);
                CanvasViewport.settings.SetValue("icontoggle", false);
                CanvasViewport.settings.SetValue("dockicons", "topleft");
                CanvasViewport.settings.SetValue("iconstyle", "colored");
                CanvasViewport.settings.SetValue("view", "Perspective");
                CanvasViewport.settings.SetValue("displaymode", wireframe.Id.ToString());
                CanvasViewport.settings.SetValue("gridtoggle", true);
                CanvasViewport.settings.SetValue("axestoggle", true);
                CanvasViewport.settings.SetValue("worldtoggle", true);
                CanvasViewport.settings.WritePersistentSettings();
                CanvasViewport.UpdateViewport(true);
                Viewport.DisplayMode = wireframe;
                Viewport.WorldAxesVisible = true;
                Viewport.ConstructionAxesVisible = true;
                Viewport.ConstructionGridVisible = true;
                Parent.Width = 400;
                Parent.Height = 300;
                Viewport.SetProjection(DefinedViewportProjection.Perspective, "Perspective", true);
                ViewReset();
                Viewport.SetCameraLocation(camLocation, true);
                Viewport.SetCameraTarget(camTarget, true);
                Viewport.Camera35mmLensLength = 50;
                Viewport.ZoomExtents();
                CanvasViewport.DockPanel(Parent, base.Anchor);
                Invalidate();
            });
            contextMenu.MenuItems.Add("Close Viewport", (s, e) =>
            {
                CanvasViewport.viewportMenuItem.Checked = false;
                CanvasViewport.settings.SetValue("state", false);
                CanvasViewport.settings.WritePersistentSettings();
                CanvasViewport.viewportMenuItem.CheckedChanged += CanvasViewport.ViewportMenuItem_CheckedChanged;
            });
            contextMenu.Show(this, location);
        }

        public Point3d camLocation;
        public Point3d camTarget;
        public void CamFetcher()
        {
            camLocation = Viewport.CameraLocation;
            camTarget = Viewport.CameraTarget;
        }

        public void ViewReset()
        {
            string view = CanvasViewport.settings.GetValue("view", "Perspective");
            if (view == "Top")
            { Viewport.SetProjection(DefinedViewportProjection.Top, "Top", true); }
            if (view == "Bottom")
            { Viewport.SetProjection(DefinedViewportProjection.Bottom, "Bottom", true); }
            if (view == "Left")
            { Viewport.SetProjection(DefinedViewportProjection.Left, "Left", true); }
            if (view == "Right")
            { Viewport.SetProjection(DefinedViewportProjection.Right, "Right", true); }
            if (view == "Front")
            { Viewport.SetProjection(DefinedViewportProjection.Front, "Front", true); }
            if (view == "Back")
            { Viewport.SetProjection(DefinedViewportProjection.Back, "Back", true); }
            if (view == "Perspective")
            { Viewport.SetProjection(DefinedViewportProjection.Perspective, "Perspective", true); }
            if (view == "TwoPointPerspective")
            { Viewport.SetProjection(DefinedViewportProjection.TwoPointPerspective, "TwoPointPerspective", true); }
        }
    }
}
