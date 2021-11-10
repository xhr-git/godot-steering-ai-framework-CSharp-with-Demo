using Godot;
using System;
using GodotSteeringAI;

public class Arriver : KinematicBody
{
    [Export] private float linear_speed_max = 50;
    [Export] private float linear_acceleration_max = 50;
    [Export] private float arrival_tolerance = 0.01f;
    [Export] private float deceleration_radius = 10;
    [Export] private float angular_speed_max = 30;
    [Export] private float angular_accel_max = 15;
    [Export] private float align_tolerance = 0.5f;
    [Export] private float angular_deceleration_radius = 60;

    private GSAIKinematicBody3DAgent agent;
    private GSAIAgentLocation target;
    private GSAITargetAcceleration accel;
    private GSAIBlend blend;
    private GSAIFace face;
    private GSAIArrive arrive;

    private Spatial target_node;

    public override void _Ready()
    {
        target_node = GetNode<Spatial>("../MouseTarget");
        agent = new GSAIKinematicBody3DAgent(this);
        target = new GSAIAgentLocation();
        accel = new GSAITargetAcceleration();
        blend = new GSAIBlend(agent);
        face = new GSAIFace(agent, target, true);
        arrive = new GSAIArrive(agent, target);


        agent.LinearSpeedMax = linear_speed_max;
        agent.LinearAccelerationMax = linear_acceleration_max;
        agent.LinearDragPercentage = 0.05f;
        agent.AngularSpeedMax = angular_speed_max;
        agent.AngularAccelerationMax = angular_accel_max;
        agent.AngularDragPercentage = 0.1f;

        arrive.ArrivalTolerance = arrival_tolerance;
        arrive.DecelerationRadius = deceleration_radius;

        face.AlignmentTolerance = Mathf.Deg2Rad(align_tolerance);
        face.DecelerationRadius = Mathf.Deg2Rad(angular_deceleration_radius);

        target.Position = target_node.Transform.origin;
        blend.Add(arrive, 1);
        blend.Add(face, 1);
    }

    public override void _PhysicsProcess(float delta)
    {
        var pos = target_node.Transform.origin;
        pos.y = Transform.origin.y;
        target.Position = pos;
        blend.CalculateSteering(accel);
        agent._ApplySteering(accel, delta);
    }
}
