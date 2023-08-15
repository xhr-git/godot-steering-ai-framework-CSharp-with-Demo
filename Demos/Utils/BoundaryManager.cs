using Godot;
using System;

public partial class BoundaryManager : Node2D
{
    private Vector2 _world_bounds;
    public override void _Ready()
    {
        _world_bounds = new Vector2(
            ProjectSettings.GetSetting("display/window/size/width").As<int>(),
            ProjectSettings.GetSetting("display/window/size/height").As<int>());
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (Node2D child in GetChildren())
        {
            child.Position = child.Position.PosMod(_world_bounds);
        }
    }
}
