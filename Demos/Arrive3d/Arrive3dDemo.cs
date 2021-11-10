using Godot;
using System;

public class Arrive3dDemo : Node
{
    private Camera camera;
    private RayCast ray;
    private Spatial mouseTarget;

    public override void _Ready()
    {
        camera = GetNode<Camera>("Camera");
        ray = camera.GetNode<RayCast>("RayCast");
        mouseTarget = GetNode<Spatial>("MouseTarget");
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt is InputEventMouseMotion input)
        {
            ray.CastTo = camera.ProjectLocalRayNormal(input.Position) * 100000;
            ray.ForceRaycastUpdate();
            if (ray.IsColliding())
            {
                var point = ray.GetCollisionPoint();
                var target = mouseTarget.Transform;
                target.origin = point;
                mouseTarget.Transform = target;
            }
        }
    }
}
