using System;
using System.Drawing;
using System.Windows.Forms;
using GhCanvasViewport.Properties;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Display;
using RhinoWindows.Forms.Controls;

namespace GhCanvasViewport
{
    class CanvasViewport
    {
        Eto.Forms.UITimer _timer;
        static Panel _viewportControlPanel;
        static ViewportControl ctrl;
        static PictureBox lockedPic1;
        static PictureBox lockedPic2;
        public static ToolStripMenuItem viewportMenuItem;
        public static GH_SettingsServer settings = new GH_SettingsServer("GhCanvasViewport", true);

        public void AddToMenu()
        {
            if (_timer != null)
                return;
            _timer = new Eto.Forms.UITimer();
            _timer.Interval = 1;
            _timer.Elapsed += SetupMenu;
            _timer.Start();
        }

        public void SetupMenu(object sender, EventArgs e)
        {
            if (!settings.ConstainsEntry("state"))
            { DefaultSettings(); }
            bool state = settings.GetValue("state", false);
            int width = settings.GetValue("width", 400);
            int height = settings.GetValue("height", 300);
            string dock = settings.GetValue("dock", "topleft");

            var editor = Instances.DocumentEditor;
            if (null == editor || editor.Handle == IntPtr.Zero)
                return;

            var controls = editor.Controls;
            if (null == controls || controls.Count == 0)
                return;

            _timer.Stop();
            foreach (var ctrl2 in controls)
            {
                var menu = ctrl2 as Grasshopper.GUI.GH_MenuStrip;
                if (menu == null)
                    continue;
                for (int i = 0; i < menu.Items.Count; i++)
                {
                    var menuitem = menu.Items[i] as ToolStripMenuItem;
                    if (menuitem != null && menuitem.Text == "Display")
                    {
                        for (int j = 0; j < menuitem.DropDownItems.Count; j++)
                        {
                            if (menuitem.DropDownItems[j].Text.StartsWith("canvas widgets", StringComparison.OrdinalIgnoreCase))
                            {
                                viewportMenuItem = new ToolStripMenuItem("Canvas Viewport", Resources.picture, new EventHandler(OnToggle));
                                viewportMenuItem.ToolTipText = "Opens a docked window in Grasshopper that displays Rhino viewports.\r\n - Use the right-click menu to change display modes, views, and other settings.";
                                viewportMenuItem.CheckOnClick = true;
                                viewportMenuItem.Checked = state;

                                if (state)
                                {
                                    if (_viewportControlPanel == null)
                                    {
                                        _viewportControlPanel = new ViewportContainerPanel();
                                        _viewportControlPanel.Size = new Size(width, height);
                                        _viewportControlPanel.MinimumSize = new Size(50, 50);
                                        _viewportControlPanel.Padding = new Padding(10);
                                        ctrl = new CanvasViewportControl();
                                        ctrl.Dock = DockStyle.Fill;
                                        _viewportControlPanel.BorderStyle = BorderStyle.FixedSingle;
                                        UpdateViewport(false);
                                        _viewportControlPanel.Controls.Add(ctrl);
                                        _viewportControlPanel.Location = new Point(0, 0);
                                        Instances.ActiveCanvas.Controls.Add(_viewportControlPanel);
                                        if (dock == "topleft")
                                        { Dock(AnchorStyles.Top | AnchorStyles.Left); }
                                        if (dock == "bottomleft")
                                        { Dock(AnchorStyles.Bottom | AnchorStyles.Left); }
                                        if (dock == "bottomright")
                                        { Dock(AnchorStyles.Bottom | AnchorStyles.Right); }
                                        if (dock == "topright")
                                        { Dock(AnchorStyles.Top | AnchorStyles.Right); }
                                    }
                                    _viewportControlPanel.Show();
                                    settings.SetValue("state", true);
                                    settings.WritePersistentSettings();
                                }
                                else
                                {
                                    if (_viewportControlPanel != null && _viewportControlPanel.Visible)
                                    {
                                        _viewportControlPanel.Hide();
                                        settings.SetValue("state", false);
                                        settings.WritePersistentSettings();
                                    }
                                }
                                viewportMenuItem.CheckedChanged += ViewportMenuItem_CheckedChanged;
                                var canvasWidgets = menuitem.DropDownItems[j] as ToolStripMenuItem;
                                if (canvasWidgets != null)
                                {
                                    canvasWidgets.DropDownOpening += (s, args) =>
                                        canvasWidgets.DropDownItems.Insert(0, viewportMenuItem);
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Panel with a "re-sizable" border that contains a viewport control
        /// </summary>
        class ViewportContainerPanel : Panel
        {
            public override Cursor Cursor
            {
                get
                {
                    var location = PointToClient(Control.MousePosition);
                    var mode = ComputeMode(location);
                    switch (mode)
                    {
                        case Mode.None:
                            return Cursors.Default;
                        case Mode.Move:
                            return Cursors.SizeAll;
                        case Mode.SizeNESW:
                            return Cursors.SizeNESW;
                        case Mode.SizeNS:
                            return Cursors.SizeNS;
                        case Mode.SizeWE:
                            return Cursors.SizeWE;
                        case Mode.SizeNWSE:
                            return Cursors.SizeNWSE;
                    }
                    return base.Cursor;
                }
                set => base.Cursor = value;
            }

            Mode ComputeMode(Point location)
            {
                var dock = Anchor;
                switch (Anchor)
                {
                    case (AnchorStyles.Left | AnchorStyles.Top):
                        {
                            if (location.X > (Width - Padding.Right))
                                return location.Y > (Height - Padding.Bottom) ? Mode.SizeNWSE : Mode.SizeWE;
                            if (location.Y > (Height - Padding.Bottom))
                                return Mode.SizeNS;
                            if (location.X < Padding.Left || location.Y < Padding.Top)
                                return Mode.None; //moving is little weird the way this is set up, don't support for now
                            return Mode.None;
                        }
                    case (AnchorStyles.Left | AnchorStyles.Bottom):
                        {
                            if (location.X > (Width - Padding.Right))
                                return location.Y < Padding.Top ? Mode.SizeNESW : Mode.SizeWE;
                            if (location.Y < Padding.Top)
                                return Mode.SizeNS;
                            if (location.X < Padding.Left || location.Y > (Height - Padding.Bottom))
                                return Mode.None; //moving is little weird the way this is set up, don't support for now
                            return Mode.None;

                        }
                    case (AnchorStyles.Right | AnchorStyles.Top):
                        {
                            if (location.X < Padding.Left)
                                return location.Y > (Height - Padding.Bottom) ? Mode.SizeNESW : Mode.SizeWE;
                            if (location.Y > (Height - Padding.Bottom))
                                return Mode.SizeNS;
                            if (location.X > (Width - Padding.Right) || location.Y < Padding.Top)
                                return Mode.None; //moving is little weird the way this is set up, don't support for now
                            return Mode.None;
                        }
                    case (AnchorStyles.Right | AnchorStyles.Bottom):
                        {
                            if (location.X < Padding.Left)
                                return location.Y < Padding.Top ? Mode.SizeNWSE : Mode.SizeWE;
                            if (location.Y < Padding.Top)
                                return Mode.SizeNS;
                            if (location.X > (Width - Padding.Right) || location.Y > (Height - Padding.Bottom))
                                return Mode.None; //moving is little weird the way this is set up, don't support for now
                            return Mode.None;
                        }
                }

                return Mode.None;
            }

            Point LeftMouseDownLocation { get; set; }
            Size LeftMouseDownSize { get; set; }

            enum Mode
            {
                None,
                SizeWE,
                SizeNS,
                SizeNWSE,
                SizeNESW,
                Move
            }
            Mode _mode;
            protected override void OnMouseDown(MouseEventArgs e)
            {
                _mode = Mode.None;
                if (e.Button == MouseButtons.Left)
                {
                    _mode = ComputeMode(e.Location);
                    LeftMouseDownLocation = e.Location;
                    LeftMouseDownSize = Size;
                }
                base.OnMouseDown(e);
            }
            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (_mode != Mode.None)
                {
                    int x = Location.X;
                    int y = Location.Y;
                    int width = Width;
                    int height = Height;

                    int deltaX = e.X - LeftMouseDownLocation.X;
                    int deltaY = e.Y - LeftMouseDownLocation.Y;
                    if (_mode == Mode.SizeNESW || _mode == Mode.SizeNS || _mode == Mode.SizeNWSE)
                    {
                        if ((Anchor & AnchorStyles.Top) == AnchorStyles.Top)
                            height = LeftMouseDownSize.Height + deltaY;
                        if ((Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
                        {
                            var pt = new Point(Location.X, Location.Y + deltaY);
                            height = Height - (pt.Y - Location.Y);
                            y = Location.Y + deltaY;
                        }
                    }
                    if (_mode == Mode.SizeNESW || _mode == Mode.SizeWE || _mode == Mode.SizeNWSE)
                    {
                        if ((Anchor & AnchorStyles.Left) == AnchorStyles.Left)
                            width = LeftMouseDownSize.Width + deltaX;
                        if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
                        {
                            var pt = new Point(Location.X + deltaX, Location.Y);
                            width = Width - (pt.X - Location.X);
                            x = Location.X + deltaX;
                        }
                    }
                    SetBounds(x, y, width, height);
                    settings.SetValue("width", width);
                    settings.SetValue("height", height);
                    settings.WritePersistentSettings();
                }
                base.OnMouseMove(e);
            }
            protected override void OnMouseUp(MouseEventArgs e)
            {
                _mode = Mode.None;
                base.OnMouseUp(e);
            }
        }

        public static void ViewportMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            var v = Rhino.RhinoApp.Version;
            if (v.Major < 6 || (v.Major == 6 && v.Minor < 3))
            {
                // The viewport control does not work very well pre 6.3
                Rhino.UI.Dialogs.ShowMessage("Canvas viewport requires Rhino 6.3 or greater version", "New Version Required");
                return;
            }
            int width = settings.GetValue("width", 400);
            int height = settings.GetValue("height", 300);
            string dock = settings.GetValue("dock", "topleft");

            var menuitem = sender as ToolStripMenuItem;
            if (menuitem != null)
            {
                if (menuitem.Checked)
                {
                    if (_viewportControlPanel == null)
                    {
                        _viewportControlPanel = new ViewportContainerPanel();
                        _viewportControlPanel.Size = new Size(width, height);
                        _viewportControlPanel.MinimumSize = new Size(50, 50);
                        _viewportControlPanel.Padding = new Padding(10);
                        ctrl = new CanvasViewportControl();
                        ctrl.Dock = DockStyle.Fill;
                        _viewportControlPanel.BorderStyle = BorderStyle.FixedSingle;
                        UpdateViewport(false);
                        _viewportControlPanel.Controls.Add(ctrl);
                        _viewportControlPanel.Location = new Point(0, 0);
                        Instances.ActiveCanvas.Controls.Add(_viewportControlPanel);
                        if (dock == "topleft")
                        { Dock(AnchorStyles.Top | AnchorStyles.Left); }
                        if (dock == "bottomleft")
                        { Dock(AnchorStyles.Bottom | AnchorStyles.Left); }
                        if (dock == "bottomright")
                        { Dock(AnchorStyles.Bottom | AnchorStyles.Right); }
                        if (dock == "topright")
                        { Dock(AnchorStyles.Top | AnchorStyles.Right); }
                    }
                    _viewportControlPanel.Show();
                    settings.SetValue("state", true);
                    settings.WritePersistentSettings();
                }
                else
                {
                    if (_viewportControlPanel != null && _viewportControlPanel.Visible)
                    {
                        _viewportControlPanel.Hide();
                        settings.SetValue("state", false);
                        settings.WritePersistentSettings();
                    }
                }
            }
        }

        public static void UpdateViewport(bool flag)
        {
            if (flag)
            {
                ctrl.Controls.Remove(lockedPic1);
                ctrl.Controls.Remove(lockedPic2);
            }
            bool icontoggle = settings.GetValue("icontoggle", false);
            if (!icontoggle)
            {
                string dockicons = settings.GetValue("dockicons", "topleft");
                string iconstyle = settings.GetValue("iconstyle", "colored");
                bool locked1 = settings.GetValue("locked1", false);
                lockedPic1 = new PictureBox();
                bool locked2 = settings.GetValue("locked2", false);
                lockedPic2 = new PictureBox();
                if (locked1)
                { if (iconstyle == "colored") { lockedPic1.Image = Resources.rotation_off; } else { lockedPic1.Image = Resources.rotation_off_s; } }
                else
                { if (iconstyle == "colored") { lockedPic1.Image = Resources.rotation_on; } else { lockedPic1.Image = Resources.rotation_on_s; } }
                lockedPic1.Size = new Size(16, 16);
                if (dockicons == "topleft")
                { DockIcons(lockedPic1, AnchorStyles.Top | AnchorStyles.Left, 3, 3); }
                if (dockicons == "bottomleft")
                { DockIcons(lockedPic1, AnchorStyles.Bottom | AnchorStyles.Left, -3, 3); }
                if (dockicons == "bottomright")
                { DockIcons(lockedPic1, AnchorStyles.Bottom | AnchorStyles.Right, -3, -24); }
                if (dockicons == "topright")
                { DockIcons(lockedPic1, AnchorStyles.Top | AnchorStyles.Right, 3, -24); }
                lockedPic1.BackColor = Rhino.ApplicationSettings.AppearanceSettings.ViewportBackgroundColor;
                lockedPic1.BringToFront();
                ctrl.Controls.Add(lockedPic1);
                if (locked2)
                { if (iconstyle == "colored") { lockedPic2.Image = Resources.drag_off; } else { lockedPic2.Image = Resources.drag_off_s; } }
                else
                { if (iconstyle == "colored") { lockedPic2.Image = Resources.drag_on; } else { lockedPic2.Image = Resources.drag_on_s; } }
                lockedPic2.Size = new Size(16, 16);
                if (dockicons == "topleft")
                { DockIcons(lockedPic2, AnchorStyles.Top | AnchorStyles.Left, 3, 24); }
                if (dockicons == "bottomleft")
                { DockIcons(lockedPic2, AnchorStyles.Bottom | AnchorStyles.Left, -3, 24); }
                if (dockicons == "bottomright")
                { DockIcons(lockedPic2, AnchorStyles.Bottom | AnchorStyles.Right, -3, -3); }
                if (dockicons == "topright")
                { DockIcons(lockedPic2, AnchorStyles.Top | AnchorStyles.Right, 3, -3); }
                lockedPic2.BackColor = Rhino.ApplicationSettings.AppearanceSettings.ViewportBackgroundColor;
                lockedPic2.BringToFront();
                ctrl.Controls.Add(lockedPic2);
            }
        }
        private void OnToggle(object sender, EventArgs e)
        {
            if (viewportMenuItem.Checked)
            { settings.SetValue("state", true); settings.WritePersistentSettings(); }
            else
            { settings.SetValue("state", false); settings.WritePersistentSettings(); }
        }
        public static void Dock(AnchorStyles anchor)
        {
            DockPanel(_viewportControlPanel, anchor);
        }
        public static void DockPanel(Control ctrl, AnchorStyles anchor)
        {
            if (ctrl == null)
                return;
            var canvas = Instances.ActiveCanvas;
            var canvasSize = canvas.ClientSize;
            int xEnd = 0;
            if ((anchor & AnchorStyles.Right) == AnchorStyles.Right)
                xEnd = canvasSize.Width - ctrl.Width;
            int yEnd = 0;
            if ((anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
                yEnd = canvasSize.Height - ctrl.Height;

            ctrl.Location = new Point(xEnd, yEnd);
            ctrl.Anchor = anchor;
        }
        public static void DockIcons(Control ctrl2, AnchorStyles anchor, int y, int x)
        {
            if (ctrl2 == null)
                return;
            var canvas = ctrl;//Instances.ActiveCanvas;
            var canvasSize = canvas.ClientSize;
            int xEnd = 0;
            if ((anchor & AnchorStyles.Right) == AnchorStyles.Right)
                xEnd = canvasSize.Width - ctrl2.Width;
            int yEnd = 0;
            if ((anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
                yEnd = canvasSize.Height - ctrl2.Height;

            ctrl2.Location = new Point(xEnd + x, yEnd + y);
            ctrl2.Anchor = anchor;
        }
        public static void DefaultSettings()
        {
            settings.SetValue("state", false);
            settings.SetValue("width", 400);
            settings.SetValue("height", 300);
            settings.SetValue("dock", "topleft");
            settings.SetValue("locked1", false);
            settings.SetValue("locked2", false);
            settings.SetValue("icontoggle", false);
            settings.SetValue("dockicons", "topleft");
            settings.SetValue("iconstyle", "colored");
            settings.SetValue("view", "Perspective");
            var wireframe = DisplayModeDescription.FindByName("Wireframe");
            settings.SetValue("displaymode", wireframe.Id.ToString());
            settings.SetValue("gridtoggle", true);
            settings.SetValue("axestoggle", true);
            settings.SetValue("worldtoggle", true);
            settings.WritePersistentSettings();
        }
    }
}
