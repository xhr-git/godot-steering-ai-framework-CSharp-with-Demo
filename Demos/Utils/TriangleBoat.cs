using Godot;
using System;
using DemoUtils;

public partial class TriangleBoat : CharacterBody2D
{
    [Export] private Color inner_color = new Color(0.4f, 0.501961f, 1f);
    [Export] private Color outer_color = new Color(0.4f, 0.501961f, 1f);

    public override void _Ready()
    {
        var line = GetNode<TriangleBoatOutlook>("TriangleBoatOutlook");
        line.SetInnerColor(inner_color);
        line.DefaultColor = outer_color;
        line.QueueRedraw();
    }
}
