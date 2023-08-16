using Godot;
using System;

public partial class DrawBoundaries : Node2D
{
    public override void _Draw()
    {
        foreach (StaticBody2D b in GetChildren())
        {
            var extens = (b.GetNode<CollisionShape2D>("CollisionShape2D").Shape as RectangleShape2D).Size;
            DrawRect(new Rect2(b.GlobalPosition - extens, extens * 2), new Color("8fde5d"));
        }
    }
}
