using UnityEngine;

namespace GliqeWorld.Tools
{
    public enum PaintTarget { World, SketchbookPage }

    /// <summary>
    /// Implemented by all art tools that apply paint to world surfaces or sketchbook pages.
    /// </summary>
    public interface IPaintTool
    {
        PaintTarget CurrentTarget { get; set; }

        /// <summary>Called when the player first contacts a surface with this tool.</summary>
        void BeginStroke(RaycastHit hit);

        /// <summary>Called every frame while the player holds the use input on a surface.</summary>
        void ContinueStroke(RaycastHit hit);

        /// <summary>Called when the player releases the use input.</summary>
        void EndStroke();
    }
}
