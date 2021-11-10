using Godot;
using System;
using GodotSteeringAI;

public class Seeker : KinematicBody2D
{
    public GSAIAgentLocation player_agent;
    public float start_speed;
    public float start_accel;
    public bool use_seek = true;

    private Vector2 velocity = Vector2.Zero;
    private GSAIKinematicBody2DAgent agent;
    private GSAITargetAcceleration accel;
    private GSAISeek seek;
    private GSAIFlee flee;

    public override void _Ready()
    {
        agent = new GSAIKinematicBody2DAgent(this);
        accel = new GSAITargetAcceleration();
        agent.LinearAccelerationMax = start_accel;
        agent.LinearSpeedMax = start_speed;
        seek = new GSAISeek(agent, player_agent);
        flee = new GSAIFlee(agent, player_agent);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (player_agent is null)
            return;
        if (use_seek)
            seek.CalculateSteering(accel);
        else
            flee.CalculateSteering(accel);

        agent._ApplySteering(accel, delta);
    }
}
