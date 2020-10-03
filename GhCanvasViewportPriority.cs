using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace GhCanvasViewport
{
    public class GhCanvasViewportPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            var canvasViewport = new CanvasViewport();
            canvasViewport.AddToMenu();
            return GH_LoadingInstruction.Proceed;
        }
    }
}
