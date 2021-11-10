using Godot;
using System;

public class BoundaryManager : Node2D
{
    private Vector2 _world_bounds;
    public override void _Ready()
    {
        _world_bounds = new Vector2(
            (float)(ProjectSettings.GetSetting("display/window/size/width") as int?),
            (float)(ProjectSettings.GetSetting("display/window/size/height") as int?));
    }

    public override void _PhysicsProcess(float delta)
    {
        foreach (Node2D child in GetChildren())
        {
            child.Position = child.Position.PosMod(_world_bounds);
        }
    }
}
