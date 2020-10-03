using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace GhCanvasViewport
{
    public class GhCanvasViewportInfo : GH_AssemblyInfo
    {
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
}
