using Godot;
using System;
using GodotSteeringAI;

public partial class TrianglePursuer : TriangleBoat
{
    [Export] private bool use_seek = false;
    [Export] private float predict_time = 1;
    [Export] private float linear_speed_max = 1200;
    [Export] private float linear_accel_max = 1040;

    private GSAIBlend _blend;
    private float _linear_drag_coefficient = 0.025f;
    private float _angular_drag = 0.1f;
    private GSAIAgentLocation _direction_face = new GSAIAgentLocation();

    private GSAIKinematicBody2DAgent agent;
    private GSAITargetAcceleration accel;
    private GSAISteeringAgent player_agent;

    public override void _Ready()
    {
        base._Ready();
        agent = new GSAIKinematicBody2DAgent(this);
        accel = new GSAITargetAcceleration();
        player_agent = Owner.GetNode<TrianglePlayer>("TrianglePlayer").agent;
        agent.CalculateVelocities = false;

        GSAISteeringBehavior behavior;
        if (use_seek)
            behavior = new GSAISeek(agent, player_agent);
        else
            behavior = new GSAIPursue(agent, player_agent, predict_time);

        var orient_behavior = new GSAIFace(agent, _direction_face);
        orient_behavior.AlignmentTolerance = Mathf.DegToRad(5);
        orient_behavior.DecelerationRadius = Mathf.DegToRad(30);

        _blend = new GSAIBlend(agent);
        _blend.Add(behavior, 1);
        _blend.Add(orient_behavior, 1);

        agent.AngularAccelerationMax = Mathf.DegToRad(1080);
        agent.AngularSpeedMax = Mathf.DegToRad(360);
        agent.LinearAccelerationMax = linear_accel_max;
        agent.LinearSpeedMax = linear_speed_max;
    }

    public override void _PhysicsProcess(double _delta)
    {
        float delta = (float) _delta;
        _direction_face.Position = agent.Position + accel.Linear.Normalized();

        _blend.CalculateSteering(accel);
        agent.AngularVelocity = Mathf.Clamp(
            agent.AngularVelocity + accel.Angular * delta, -agent.AngularSpeedMax, agent.AngularSpeedMax);
        agent.AngularVelocity = Mathf.Lerp(agent.AngularVelocity, 0, _angular_drag);

        Rotation += agent.AngularVelocity * delta;

        Velocity = GSAIUtils.ToVector2(agent.LinearVelocity) +
            (GSAIUtils.AngleToVector2(Rotation) * -agent.LinearAccelerationMax * delta);

        Velocity = Velocity.LimitLength(agent.LinearSpeedMax);
        Velocity = Velocity.Lerp(Vector2.Zero, _linear_drag_coefficient);

        MoveAndSlide();
        agent.LinearVelocity = GSAIUtils.ToVector3(Velocity);
    }
}
