using System;
using System.Drawing;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper;
using System.Windows.Forms;
using Grasshopper.Kernel;
using System.Security.Cryptography;
using Eto.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace GhCanvasViewport
{
    public class GhCanvasViewportInfo : GH_AssemblyInfo
    {
        public static ToolStripMenuItem viewportMenuItem = new ToolStripMenuItem("Canvas Viewport");
        public static ToolStripButton showView = new ToolStripButton();
        static CanvasViewport _canvasViewport;
        public GhCanvasViewportInfo()
        {
            if (_canvasViewport == null)
            {
                _canvasViewport = new CanvasViewport();
                _canvasViewport.AddToMenu();
            }
        }

        public override string Name
        {
            get
            {
                return "GhCanvasViewport";
            }
        }
        public override string Version
        {
            get
            {
                // use AssemblyInformationalVersion since this can be patched easily during CI builds
                // to keep it in sync with the yak package version
                return System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion;
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("406bd915-c42a-4f97-8ce4-4934600b43ff");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }

    public class DisableSolverPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.CanvasCreated += Instances_CanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        private void Instances_CanvasCreated(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= Instances_CanvasCreated;

            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null)
            {
                Instances.ActiveCanvas.DocumentChanged += ActiveCanvas_DocumentChanged;
                return;
            }
            AddViewoprtButton(editor);
        }

        private void ActiveCanvas_DocumentChanged(GH_Canvas sender, GH_CanvasDocumentChangedEventArgs e)
        {
            Instances.ActiveCanvas.DocumentChanged -= ActiveCanvas_DocumentChanged;

            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null)
            {
                return;
            }
            AddViewoprtButton(editor);
        }

        private void AddViewoprtButton(GH_DocumentEditor editor)
        {
            MenuStrip menu = editor.Controls[4] as MenuStrip;

            ToolStripMenuItem menuItem = menu.Items[3] as ToolStripMenuItem;

            // in case we want add it to the original loaction but there is a problem to simulate the click (line 143)
            //ToolStripMenuItem items = menuItem.DropDownItems[18] as ToolStripMenuItem;
            //items.DropDownItems.Insert(0, GhCanvasViewportInfo.viewportMenuItem);

            GhCanvasViewportInfo.viewportMenuItem.CheckOnClick = true;
            GhCanvasViewportInfo.viewportMenuItem.Image = Properties.Resources.viewport;
            menuItem.DropDownItems.Insert(19, GhCanvasViewportInfo.viewportMenuItem);

            ToolStrip oldToolbar = editor.Controls[0].Controls[1] as ToolStrip;

            GhCanvasViewportInfo.showView.Image = Properties.Resources.viewport;
            GhCanvasViewportInfo.showView.ImageScaling = ToolStripItemImageScaling.SizeToFit;
            GhCanvasViewportInfo.showView.ToolTipText = "GH Viewport";

            GhCanvasViewportInfo.showView.Click += (s, args) =>
            {
                menuItem.DropDownItems[19].PerformClick();
                GhCanvasViewportInfo.showView.Checked = GhCanvasViewportInfo.viewportMenuItem.Checked;
                //items.DropDownItems[0].PerformClick();
                UpdateCheckState();
            };
 
            oldToolbar.Items.Add(GhCanvasViewportInfo.showView);
            
            UpdateCheckState();
            GhCanvasViewportInfo.viewportMenuItem.CheckedChanged += ViewportMenuItem_CheckedChanged;
            GhCanvasViewportInfo.showView.CheckedChanged += ShowView_CheckedChanged;

        }
        private void UpdateCheckState()
        {
            GhCanvasViewportInfo.showView.Checked = GhCanvasViewportInfo.viewportMenuItem.Checked;
        }

        private void ViewportMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateCheckState();
        }

        private void ShowView_CheckedChanged(object sender, EventArgs e)
        {
            GhCanvasViewportInfo.viewportMenuItem.Checked = GhCanvasViewportInfo.showView.Checked;
        }
    }
}
