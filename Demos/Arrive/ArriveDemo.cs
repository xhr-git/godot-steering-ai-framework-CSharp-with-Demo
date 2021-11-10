using Godot;
using System;
using GodotSteeringAI;

public class ArriveDemo : Node
{
    [Export] private float linear_speed_max = 1600;
    [Export] private float linear_acceleration_max = 5000;
    [Export] private float arrival_tolerance = 15;
    [Export] private float deceleration_radius = 180;

    private KinematicBody2D arriver;
    private Node2D target_drawer;
    private float drag = 0.1f;

    private GSAIKinematicBody2DAgent gsai_agent;
    private GSAIAgentLocation gsai_target;
    private GSAIArrive gsai_arrive;
    private GSAITargetAcceleration gsai_accel;

    public override void _Ready()
    {
        arriver = GetNode<KinematicBody2D>("Arriver2d");
        target_drawer = GetNode<Node2D>("TargetDrawer");
        target_drawer.Connect("draw", this, nameof(_OnTargetDrawer_Draw));
        target_drawer.GlobalPosition = arriver.GlobalPosition;
        gsai_agent = new GSAIKinematicBody2DAgent(arriver);
        gsai_target = new GSAIAgentLocation();
        gsai_arrive = new GSAIArrive(gsai_agent, gsai_target);
        gsai_accel = new GSAITargetAcceleration();
        gsai_agent.LinearSpeedMax = linear_speed_max;
        gsai_agent.LinearAccelerationMax = linear_acceleration_max;
        gsai_agent.LinearDragPercentage = drag;
        gsai_arrive.DecelerationRadius = deceleration_radius;
        gsai_arrive.ArrivalTolerance = arrival_tolerance;
        gsai_target.Position = gsai_agent.Position;
    }

    private void _OnTargetDrawer_Draw()
    {
        target_drawer.DrawCircle(Vector2.Zero, deceleration_radius, new Color(1.0f, 0.419f, 0.592f, 0.5f));
        target_drawer.DrawCircle(Vector2.Zero, arrival_tolerance, new Color(0.278f, 0.231f, 0.47f, 0.3f));
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt is InputEventMouseButton btn)
        {
            if (btn.ButtonIndex == (int)ButtonList.Left && evt.IsPressed())
            {
                target_drawer.GlobalPosition = btn.Position;
                gsai_target.Position = GSAIUtils.ToVector3(btn.Position);
            }
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        gsai_arrive.CalculateSteering(gsai_accel);
        gsai_agent._ApplySteering(gsai_accel, delta);
    }

}
