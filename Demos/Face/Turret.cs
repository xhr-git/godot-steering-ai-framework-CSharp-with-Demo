using Godot;
using System;
using GodotSteeringAI;

public partial class Turret : CharacterBody2D
{
    [Export] private float align_tolerance = 0.5f;
    [Export] private float deceleration_radius = 135;
    [Export] private float angular_accel_max = 15;
    [Export] private float angular_speed_max = 50;

    private GSAIFace face;
    private GSAIKinematicBody2DAgent agent;
    private GSAITargetAcceleration accel;
    private GSAIAgentLocation target;
    private Node2D target_node;
    private float angular_drag = 0.1f;

    public override void _Ready()
    {
        target_node = GetNode<Node2D>("../Target");
        target = new GSAIAgentLocation();
        target.Position = GSAIUtils.ToVector3(target_node.GlobalPosition);
        agent = new GSAIKinematicBody2DAgent(this);
        face = new GSAIFace(agent, target);
        accel = new GSAITargetAcceleration();

        face.AlignmentTolerance = Mathf.DegToRad(align_tolerance);
        face.DecelerationRadius = Mathf.DegToRad(deceleration_radius);

        agent.AngularAccelerationMax = angular_accel_max;
        agent.AngularSpeedMax = angular_speed_max;
        agent.AngularDragPercentage = angular_drag;
    }

    public override void _PhysicsProcess(double delta)
    {
        target.Position = GSAIUtils.ToVector3(target_node.GlobalPosition);
        face.CalculateSteering(accel);
        agent._ApplySteering(accel, (float) delta);
    }
}
