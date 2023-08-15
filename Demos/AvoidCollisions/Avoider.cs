using Godot;
using System;
using GodotSteeringAI;
using System.Collections.Generic;

public partial class Avoider : CharacterBody2D
{
    private static Random random = new Random();

    private static float GetRand(float min, float max)
    {
        var t = (float)random.NextDouble();
        return t * (max - min) + min;
    }

    private bool draw_proximity;

    private float _boundary_right;
    private float _boundary_bottom;
    private float _radius;
    private GSAITargetAcceleration _accel = new GSAITargetAcceleration();
    private Vector2 _direction;
    private float _drag = 0.1f;
    private Color _color = new Color(0.4f, 1.0f, 0.89f, 0.3f);

    public PlayerOutlook collision;
    public GSAIKinematicBody2DAgent agent;
    private GSAIRadiusProximity proximity;
    private GSAIAvoidCollisions avoid;
    private GSAIAgentLocation target;
    private GSAISeek seek;
    private GSAIPriority priority;

    public override void _Ready()
    {
        collision = GetNode<PlayerOutlook>("CollisionShape2D");
        agent = new GSAIKinematicBody2DAgent(this);
        proximity = new GSAIRadiusProximity(agent, new List<GSAISteeringAgent>(), 140);
        avoid = new GSAIAvoidCollisions(agent, proximity);
        target = new GSAIAgentLocation();
        seek = new GSAISeek(agent, target);
        priority = new GSAIPriority(agent, 0.0001f);
    }

    public override void _Draw()
    {
        if (draw_proximity)
        {
            DrawCircle(Vector2.Zero, proximity.Radius, _color);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var pos = target.Position;
        pos.X = agent.Position.X + _direction.X * _radius;
        pos.Y = agent.Position.Y + _direction.Y * _radius;
        target.Position = pos;

        priority.CalculateSteering(_accel);
        agent._ApplySteering(_accel, (float) delta);
    }

    public void Setup(
        float linear_speed_max, float linear_accel_max, float proximity_radius,
        float boundary_right, float boundary_bottom, bool _draw_proximity)
    {
        _direction = new Vector2(GetRand(-1, 1), GetRand(-1, 1)).Normalized();

        agent.LinearSpeedMax = linear_speed_max;
        agent.LinearAccelerationMax = linear_accel_max;

        proximity.Radius = proximity_radius;
        _boundary_bottom = boundary_bottom;
        _boundary_right = boundary_right;

        _radius = (collision.Shape as CircleShape2D).Radius;
        agent.BoundingRadius = _radius;

        agent.LinearDragPercentage = _drag;

        draw_proximity = _draw_proximity;

        priority.Add(avoid);
        priority.Add(seek);
    }

    public void SetProximityAgents(List<GSAISteeringAgent> agents)
    {
        proximity.Agents = agents;
    }

    public void SetRandomNonoverlappingPosition(IList<Avoider> others, float distance_from_boundary_min)
    {
        var tries_max = Math.Max(100, others.Count * others.Count);
        while (tries_max > 0)
        {
            tries_max--;
            
            var pos = GlobalPosition;
            pos.X = GetRand(distance_from_boundary_min, _boundary_right - distance_from_boundary_min);
            pos.Y = GetRand(distance_from_boundary_min, _boundary_bottom - distance_from_boundary_min);
            GlobalPosition = pos;

            var done = true;
            foreach (var other in others)
            {
                if (other.GlobalPosition.DistanceTo(pos) <= _radius * 2 + distance_from_boundary_min)
                {
                    done = false; break;
                }
            }
            if (done) break;
        }
    }
}
