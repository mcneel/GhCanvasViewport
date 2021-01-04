using System;
using System.Windows.Forms;
using Grasshopper;
using Rhino.Display;
using Rhino.Geometry;

namespace GhCanvasViewport
{
    class CanvasViewportControl : RhinoWindows.Forms.Controls.ViewportControl
    {
        IniFile MyIni = new IniFile(Folders.AppDataFolder + "GhCanvasViewport.ini");
        public CanvasViewportControl()
        {
            // stupid hack to get GH to draw preview geometry in this control
            this.Viewport.Name = "GH_HACK";

            if (!MyIni.KeyExists("view"))
            { MyIni.Write("view", "Perspective"); }

            if (!MyIni.KeyExists("gridtoggle"))
            { MyIni.Write("gridtoggle", "true"); }
            var gridtoggle = bool.Parse(MyIni.Read("gridtoggle"));
            Viewport.ConstructionGridVisible = gridtoggle;

            if (!MyIni.KeyExists("axestoggle"))
            { MyIni.Write("axestoggle", "true"); }
            var axestoggle = bool.Parse(MyIni.Read("axestoggle"));
            Viewport.ConstructionAxesVisible = axestoggle;

            if (!MyIni.KeyExists("worldtoggle"))
            { MyIni.Write("worldtoggle", "true"); }
            var worldtoggle = bool.Parse(MyIni.Read("worldtoggle"));
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
                bool locked1 = bool.Parse(MyIni.Read("locked1"));
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
                bool locked2 = bool.Parse(MyIni.Read("locked2"));
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
            bool locked1 = bool.Parse(MyIni.Read("locked1"));
            lockMenu1.Click += (s, e) => { locked1 = !locked1; MyIni.Write("locked1", locked1.ToString()); CanvasViewport.UpdateViewport(true); };
            lockMenu1.Checked = locked1;
            lockMenuMain.MenuItems.Add(lockMenu1);
            contextMenu.MenuItems.Add(lockMenuMain);

            var lockMenu2 = new MenuItem("Lock Dragging");
            bool locked2 = bool.Parse(MyIni.Read("locked2"));
            lockMenu2.Click += (s, e) => { locked2 = !locked2; MyIni.Write("locked2", locked2.ToString()); CanvasViewport.UpdateViewport(true); };
            lockMenu2.Checked = locked2;
            lockMenuMain.MenuItems.Add(lockMenu2);
            contextMenu.MenuItems.Add(lockMenuMain);

            var iconToggle = new MenuItem("Disable Lock Icons");
            bool icontoggle = bool.Parse(MyIni.Read("icontoggle"));
            iconToggle.Click += (s, e) => { icontoggle = !icontoggle; MyIni.Write("icontoggle", icontoggle.ToString()); CanvasViewport.UpdateViewport(true); };
            iconToggle.Checked = icontoggle;
            lockMenuMain.MenuItems.Add(iconToggle);

            lockMenuMain.MenuItems.Add("-");

            var dockiconsMenu = new MenuItem("Dock Icons");
            var dockicons = MyIni.Read("dockicons");
            var dockiconsMenuItem = new MenuItem("Top Left");
            dockiconsMenuItem.RadioCheck = true;
            dockiconsMenuItem.Click += (s, e) => { dockicons = "topleft"; MyIni.Write("dockicons", dockicons); CanvasViewport.UpdateViewport(true); };
            dockiconsMenuItem.Checked = (dockicons == "topleft");
            dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
            dockiconsMenuItem = new MenuItem("Top Right");
            dockiconsMenuItem.RadioCheck = true;
            dockiconsMenuItem.Click += (s, e) => { dockicons = "topright"; MyIni.Write("dockicons", dockicons); CanvasViewport.UpdateViewport(true); };
            dockiconsMenuItem.Checked = (dockicons == "topright");
            dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
            dockiconsMenuItem = new MenuItem("Bottom Left");
            dockiconsMenuItem.RadioCheck = true;
            dockiconsMenuItem.Click += (s, e) => { dockicons = "bottomleft"; MyIni.Write("dockicons", dockicons); CanvasViewport.UpdateViewport(true); };
            dockiconsMenuItem.Checked = (dockicons == "bottomleft");
            dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
            dockiconsMenuItem = new MenuItem("Bottom Right");
            dockiconsMenuItem.RadioCheck = true;
            dockiconsMenuItem.Click += (s, e) => { dockicons = "bottomright"; MyIni.Write("dockicons", dockicons); CanvasViewport.UpdateViewport(true); };
            dockiconsMenuItem.Checked = (dockicons == "bottomright");
            dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
            lockMenuMain.MenuItems.Add(dockiconsMenu);

            var styleMenu = new MenuItem("Icon Style");
            var iconstyle = MyIni.Read("iconstyle");
            var styleMenuItem = new MenuItem("Colored");
            styleMenuItem.RadioCheck = true;
            styleMenuItem.Click += (s, e) => { iconstyle = "colored"; MyIni.Write("iconstyle", iconstyle); CanvasViewport.UpdateViewport(true); };
            styleMenuItem.Checked = (iconstyle == "colored");
            styleMenu.MenuItems.Add(styleMenuItem);
            styleMenuItem = new MenuItem("Simple");
            styleMenuItem.RadioCheck = true;
            styleMenuItem.Click += (s, e) => { iconstyle = "simple"; MyIni.Write("iconstyle", iconstyle); CanvasViewport.UpdateViewport(true); };
            styleMenuItem.Checked = (iconstyle == "simple");
            styleMenu.MenuItems.Add(styleMenuItem);
            lockMenuMain.MenuItems.Add(styleMenu);

            contextMenu.MenuItems.Add(lockMenuMain);

            var wireframe = DisplayModeDescription.FindByName("Wireframe");
            var displayModeMenu = new MenuItem("Display Mode");
            if (!MyIni.KeyExists("displaymode"))
            { MyIni.Write("displaymode", wireframe.Id.ToString()/*Guid.Empty.ToString()*/); }
            var displaymode = Guid.Parse(MyIni.Read("displaymode"));
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
                    MyIni.Write("displaymode", mode.Id.ToString());
                    CanvasViewport.UpdateViewport(true);
                    Invalidate();
                };
                displayModeMenu.MenuItems.Add(modeMenuItem);
                displayModeMenu.Tag = mode.Id;
            }
            contextMenu.MenuItems.Add(displayModeMenu);

            var viewMenu = new MenuItem("Set View");

            if (!MyIni.KeyExists("view"))
            { MyIni.Write("view", "Perspective"); }
            var view = MyIni.Read("view");

            //var viewMenuItem = new MenuItem("None");
            //viewMenuItem.RadioCheck = true;
            //viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.None, "None", true); view = "0"; MyIni.Write("view", view); Invalidate(); };
            //viewMenuItem.Checked = (view == "0");
            //viewMenu.MenuItems.Add(viewMenuItem);
            var viewMenuItem = new MenuItem("Top");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Top, "Top", true); view = "Top"; MyIni.Write("view", view); Invalidate(); };
            viewMenuItem.Checked = (view == "Top");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Bottom");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Bottom, "Bottom", true); view = "Bottom"; MyIni.Write("view", view); Invalidate(); };
            viewMenuItem.Checked = (view == "Bottom");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Left");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Left, "Left", true); view = "Left"; MyIni.Write("view", view); Invalidate(); };
            viewMenuItem.Checked = (view == "Left");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Right");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Right, "Right", true); view = "Right"; MyIni.Write("view", view); Invalidate(); };
            viewMenuItem.Checked = (view == "Right");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Front");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Front, "Front", true); view = "Front"; MyIni.Write("view", view); Invalidate(); };
            viewMenuItem.Checked = (view == "Front");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Back");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Back, "Back", true); view = "Back"; MyIni.Write("view", view); Invalidate(); };
            viewMenuItem.Checked = (view == "Back");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("Perspective");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Perspective, "Perspective", true); view = "Perspective"; MyIni.Write("view", view); Invalidate(); };
            viewMenuItem.Checked = (view == "Perspective");
            viewMenu.MenuItems.Add(viewMenuItem);
            viewMenuItem = new MenuItem("TwoPointPerspective");
            viewMenuItem.RadioCheck = true;
            viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.TwoPointPerspective, "TwoPointPerspective", true); view = "TwoPointPerspective"; MyIni.Write("view", view); Invalidate(); };
            viewMenuItem.Checked = (view == "TwoPointPerspective");
            viewMenu.MenuItems.Add(viewMenuItem);
            contextMenu.MenuItems.Add(viewMenu);

            var dock = MyIni.Read("dock");
            var dockMenu = new MenuItem("Dock");
            var mnu = new MenuItem("Top Left");
            mnu.RadioCheck = true;
            mnu.Checked = (dock == "topleft");
            mnu.Click += (s, args) => { CanvasViewport.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Left); dock = "topleft"; MyIni.Write("dock", dock); };
            dockMenu.MenuItems.Add(mnu);
            mnu = new MenuItem("Top Right");
            mnu.Checked = (dock == "topright");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => { CanvasViewport.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Right); dock = "topright"; MyIni.Write("dock", dock); };
            dockMenu.MenuItems.Add(mnu);
            mnu = new MenuItem("Bottom Left");
            mnu.Checked = (dock == "bottomleft");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => { CanvasViewport.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Left); dock = "bottomleft"; MyIni.Write("dock", dock); };
            dockMenu.MenuItems.Add(mnu);
            mnu = new MenuItem("Bottom Right");
            mnu.Checked = (dock == "bottomright");
            mnu.RadioCheck = true;
            mnu.Click += (s, args) => { CanvasViewport.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Right); dock = "bottomright"; MyIni.Write("dock", dock); };
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

            var gridtoggle = bool.Parse(MyIni.Read("gridtoggle"));
            var grid = new MenuItem("Toggle Grid");
            grid.Checked = (gridtoggle);
            grid.Click += (s, args) => { Viewport.ConstructionGridVisible = !gridtoggle; gridtoggle = !gridtoggle; MyIni.Write("gridtoggle", gridtoggle.ToString()); Invalidate(); };
            contextMenu.MenuItems.Add(grid);

            var axestoggle = bool.Parse(MyIni.Read("axestoggle"));
            var gridaxes = new MenuItem("Toggle Grid Axes");
            gridaxes.Checked = (axestoggle);
            gridaxes.Click += (s, args) => { Viewport.ConstructionAxesVisible = !axestoggle; axestoggle = !axestoggle; MyIni.Write("axestoggle", axestoggle.ToString()); Invalidate(); };
            contextMenu.MenuItems.Add(gridaxes);

            var worldtoggle = bool.Parse(MyIni.Read("worldtoggle"));
            var worldaxes = new MenuItem("Toggle World Axes");
            worldaxes.Checked = (worldtoggle);
            worldaxes.Click += (s, args) => { Viewport.WorldAxesVisible = !worldtoggle; worldtoggle = !worldtoggle; MyIni.Write("worldtoggle", worldtoggle.ToString()); Invalidate(); };
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
                MyIni.Write("displaymode", wireframe.Id.ToString()/*Guid.Empty.ToString()*/);
                Viewport.DisplayMode = wireframe;
                MyIni.Write("worldtoggle", "true");
                Viewport.WorldAxesVisible = true;
                MyIni.Write("axestoggle", "true");
                Viewport.ConstructionAxesVisible = true;
                MyIni.Write("gridtoggle", "true");
                Viewport.ConstructionGridVisible = true;
                MyIni.Write("width", "400");
                Parent.Width = 400;
                MyIni.Write("height", "300");
                Parent.Height = 300;
                MyIni.Write("view", "Perspective");
                Viewport.SetProjection(DefinedViewportProjection.Perspective, "Perspective", true);
                ViewReset();
                MyIni.Write("dock", "topleft");
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
                MyIni.Write("state", "false");
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
            var view = MyIni.Read("view");
            //if (view == "0")
            //{ Viewport.SetProjection(DefinedViewportProjection.None, "None", true); }
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
