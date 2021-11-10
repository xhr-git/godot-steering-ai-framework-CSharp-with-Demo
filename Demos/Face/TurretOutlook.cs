using Godot;
using System;

public class TurretOutlook : CollisionShape2D
{
    [Export] private Color inner_color = new Color(0.890196f, 0.411765f, 0.337255f);
    [Export] private Color outer_color = new Color(1, 0.709804f, 0.439216f);
    [Export] private float stroke = 8;

    public override void _Draw()
    {
        var rad = (Shape as CircleShape2D).Radius;
        var rect = new Rect2(new Vector2(-5, 0), new Vector2(10, -rad * 2));
        DrawRect(rect, outer_color);
        DrawCircle(Vector2.Zero, rad + stroke, outer_color);
        DrawCircle(Vector2.Zero, rad, inner_color);
    }
}
