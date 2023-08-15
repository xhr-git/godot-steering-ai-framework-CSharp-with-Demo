using Godot;
using System;

public partial class Arrive3dDemo : Node
{
    private Camera3D camera;
    private RayCast3D ray;
    private Node3D mouseTarget;

    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Camera3D");
        ray = camera.GetNode<RayCast3D>("RayCast3D");
        mouseTarget = GetNode<Node3D>("MouseTarget");
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt is InputEventMouseMotion input)
        {
            ray.TargetPosition = camera.ProjectLocalRayNormal(input.Position) * 100000;
            ray.ForceRaycastUpdate();
            if (ray.IsColliding())
            {
                var point = ray.GetCollisionPoint();
                var target = mouseTarget.Transform;
                target.Origin = point;
                mouseTarget.Transform = target;
            }
        }
    }
}
