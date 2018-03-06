using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI.Canvas;

namespace GhCanvasViewport
{
    class CanvasViewport
    {
        Eto.Forms.UITimer _timer;
        Panel _viewportControlPanel;

        public void AddToMenu()
        {
            if (_timer != null)
                return;
            _timer = new Eto.Forms.UITimer();
            _timer.Interval = 1;
            _timer.Elapsed += SetupMenu;
            _timer.Start();
        }

        void SetupMenu(object sender, EventArgs e)
        {
            var editor = Grasshopper.Instances.DocumentEditor;
            if (null == editor || editor.Handle == IntPtr.Zero)
                return;

            var controls = editor.Controls;
            if (null == controls || controls.Count == 0)
                return;

            _timer.Stop();
            foreach (var ctrl in controls)
            {
                var menu = ctrl as Grasshopper.GUI.GH_MenuStrip;
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
                                var viewportMenuItem = new ToolStripMenuItem("Canvas Viewport");
                                viewportMenuItem.CheckOnClick = true;
                                menuitem.DropDownOpened += (s, args) => 
                                {
                                    if (_viewportControlPanel != null && _viewportControlPanel.Visible)
                                        viewportMenuItem.Checked = true;
                                    else
                                        viewportMenuItem.Checked = false;
                                };
                                viewportMenuItem.CheckedChanged += ViewportMenuItem_CheckedChanged;
                                var canvasWidgets = menuitem.DropDownItems[j] as ToolStripMenuItem;
                                if (canvasWidgets != null)
                                {
                                    canvasWidgets.DropDownOpening += (s,args) =>
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
        /// Panel with a "resizable" border that contains a viewport control
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

            Mode ComputeMode(System.Drawing.Point location)
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

            System.Drawing.Point LeftMouseDownLocation { get; set; }
            System.Drawing.Size LeftMouseDownSize { get; set; }

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
                if(e.Button == MouseButtons.Left)
                {
                    _mode = ComputeMode(e.Location);
                    LeftMouseDownLocation = e.Location;
                    LeftMouseDownSize = Size;
                }
                base.OnMouseDown(e);
            }
            protected override void OnMouseMove(MouseEventArgs e)
            {
                if(_mode!=Mode.None)
                {
                    int x = Location.X;
                    int y = Location.Y;
                    int width = Width;
                    int height = Height;

                    int deltaX = e.X - LeftMouseDownLocation.X;
                    int deltaY = e.Y - LeftMouseDownLocation.Y;
                    if( _mode == Mode.SizeNESW || _mode == Mode.SizeNS || _mode == Mode.SizeNWSE)
                    {
                        if ((Anchor & AnchorStyles.Top) == AnchorStyles.Top)
                            height = LeftMouseDownSize.Height + deltaY;
                        if ((Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
                        {
                            var pt = new System.Drawing.Point(Location.X, Location.Y + deltaY);
                            height = Height - (pt.Y - Location.Y);
                            y = Location.Y + deltaY;
                        }
                    }
                    if(_mode == Mode.SizeNESW || _mode == Mode.SizeWE || _mode == Mode.SizeNWSE)
                    {
                        if ((Anchor & AnchorStyles.Left) == AnchorStyles.Left)
                            width = LeftMouseDownSize.Width + deltaX;
                        if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
                        {
                            var pt = new System.Drawing.Point(Location.X + deltaX, Location.Y);
                            width = Width - (pt.X - Location.X);
                            x = Location.X + deltaX;
                        }
                    }
                    SetBounds(x, y, width, height);
                }
                base.OnMouseMove(e);
            }
            protected override void OnMouseUp(MouseEventArgs e)
            {
                _mode = Mode.None;
                base.OnMouseUp(e);
            }
        }

        void ViewportMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            var v = Rhino.RhinoApp.Version;
            if (v.Major < 6 || (v.Major == 6 && v.Minor < 3))
            {
                // The viewport control does not work very well pre 6.3
                Rhino.UI.Dialogs.ShowMessage("Canvas viewport requires Rhino 6.3 or greater version", "New Version Required");
                return;
            }


            var menuitem = sender as ToolStripMenuItem;
            if (menuitem != null)
            {
                if (menuitem.Checked)
                {
                    if (_viewportControlPanel == null)
                    {
                        _viewportControlPanel = new ViewportContainerPanel();
                        _viewportControlPanel.Size = new System.Drawing.Size(400, 300);
                        _viewportControlPanel.MinimumSize = new System.Drawing.Size(50, 50);
                        _viewportControlPanel.Padding = new Padding(10);
                        var ctrl = new CanvasViewportControl();
                        ctrl.Dock = DockStyle.Fill;

                        //ctrl.ContextMenu = contextMenu;
                        _viewportControlPanel.BorderStyle = BorderStyle.FixedSingle;
                        _viewportControlPanel.Controls.Add(ctrl);
                        _viewportControlPanel.Location = new System.Drawing.Point(0, 0);
                        _viewportControlPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                        Grasshopper.Instances.ActiveCanvas.Controls.Add(_viewportControlPanel);
                        Dock(AnchorStyles.Top | AnchorStyles.Right);
                    }
                    _viewportControlPanel.Show();

                }
                else
                {
                    if (_viewportControlPanel != null && _viewportControlPanel.Visible)
                        _viewportControlPanel.Hide();

                }
            }
        }

        void Dock(AnchorStyles anchor)
        {
            DockPanel(_viewportControlPanel, anchor);
        }

        public static void DockPanel(Control ctrl, AnchorStyles anchor)
        {
            if (ctrl == null)
                return;
            var canvas = Grasshopper.Instances.ActiveCanvas;
            var canvasSize = canvas.ClientSize;
            int xEnd = 0;
            if ((anchor & AnchorStyles.Right) == AnchorStyles.Right)
                xEnd = canvasSize.Width - ctrl.Width;
            int yEnd = 0;
            if ((anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
                yEnd = canvasSize.Height - ctrl.Height;

            ctrl.Location = new System.Drawing.Point(xEnd, yEnd);
            ctrl.Anchor = anchor;
        }
    }
}
