using Godot;
using System;

namespace DemoUtils
{
    public partial class TriangleBoatOutlook : Line2D
    {
        private Color inner_color = new Color();

        public void SetInnerColor(Color color)
        {
            inner_color = color;
            QueueRedraw();
        }

        public override void _Draw()
        {
            DrawColoredPolygon(Points, inner_color);
        }
    }
}