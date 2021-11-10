using Godot;
using System;
using GodotSteeringAI;
using System.Collections.Generic;

public class Member : KinematicBody2D
{
    private static Random random = new Random();

    public static float GetRand(float min, float max)
    {
        var t = (float)random.NextDouble();
        return t * (max - min) + min;
    }

    private MemberOutlook outlook;

    private GSAISeparation separation;
    private GSAICohesion cohesion;
    public GSAIRadiusProximity proximity;
    public GSAIKinematicBody2DAgent agent;
    private GSAIBlend blend;
    private GSAITargetAcceleration acceleration;
    public bool draw_proximity = false;


    public override void _Draw()
    {
        if (draw_proximity)
            DrawCircle(Vector2.Zero, proximity.Radius, new Color(0.4f, 1, 0.89f, 0.3f));
    }

    public override void _Ready()
    {
        outlook = GetNode<MemberOutlook>("MemberOutlook");
    }

    public void Setup(float linear_speed_max, float linear_accel_max, float proximity_radius,
            float separation_decay_coefficient, float cohesion_strength, float separation_strength)
    {
        outlook.SetShapeColor(new Color(GetRand(0.5f, 1), GetRand(0.25f, 1), GetRand(0, 1)), outlook.outer_color);

        acceleration = new GSAITargetAcceleration();

        agent = new GSAIKinematicBody2DAgent(this);
        agent.LinearAccelerationMax = linear_accel_max;
        agent.LinearSpeedMax = linear_speed_max;
        agent.LinearDragPercentage = 0.1f;

        proximity = new GSAIRadiusProximity(agent, new List<GSAISteeringAgent>(), proximity_radius);
        separation = new GSAISeparation(agent, proximity);
        separation.DecayCoefficient = separation_decay_coefficient;
        cohesion = new GSAICohesion(agent, proximity);
        blend = new GSAIBlend(agent);
        blend.Add(separation, separation_strength);
        blend.Add(cohesion, cohesion_strength);
    }

    public override void _PhysicsProcess(float delta)
    {
        if (blend is null)
            return;
        blend.CalculateSteering(acceleration);
        agent._ApplySteering(acceleration, delta);
    }

    public void SetNeighbors(List<GSAISteeringAgent> neighbor)
    {
        proximity.Agents = neighbor;
    }
}
