using Godot;
using System;

public class PlayerOutlook : CollisionShape2D
{
    [Export] private float Stroke = 6;
    [Export] public Color InnerColor = new Color(0.235294f, 0.639216f, 0.439216f);
    [Export] public Color OuterColor = new Color(0.560784f, 0.870588f, 0.364706f);
    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, (Shape as CircleShape2D).Radius + Stroke, OuterColor);
        DrawCircle(Vector2.Zero, (Shape as CircleShape2D).Radius, InnerColor);
    }
}
