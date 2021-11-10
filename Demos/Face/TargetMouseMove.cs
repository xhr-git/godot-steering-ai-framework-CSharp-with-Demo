using Godot;
using System;

public class TargetMouseMove : Node2D
{
    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt is InputEventMouseMotion mouse)
        {
            GlobalPosition = mouse.Position;
        }
    }
}
