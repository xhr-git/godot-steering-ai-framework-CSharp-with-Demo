using Godot;
using System;

public class MemberOutlook : CollisionShape2D
{

    [Export] public Color inner_color = new Color(0, 0, 0);
    [Export] public Color outer_color = new Color(0.301961f, 0.65098f, 1);
    [Export] public float stroke = 5;

    public void SetShapeColor(Color inner, Color outer)
    {
        inner_color = inner;
        outer_color = outer;
        Update();
    }

    public override void _Draw()
    {
        var shape = Shape as CircleShape2D;
        DrawCircle(Vector2.Zero, shape.Radius + stroke, outer_color);
        DrawCircle(Vector2.Zero, shape.Radius, inner_color);
    }
}
