using Godot;
using System;
using GodotSteeringAI;
using System.Collections.Generic;
using Godot.Collections;

public partial class FollowPathDemo : Node
{
    [Export] private float linear_speed_max = 920;
    [Export] private float linear_acceleration_max = 3740;
    [Export] private float arrival_tolerance = 10;
    [Export] private float deceleration_radius = 200;
    [Export] private float predict_time = 0.3f;
    [Export] private float path_offset = 20;

    private CharacterBody2D pathFollower;
    public override void _Ready()
    {
        var drawer = GetNode<PathDrawer>("PathDrawer");
        pathFollower = GetNode<CharacterBody2D>("PathFollower");
        agent = new GSAIKinematicBody2DAgent(pathFollower);
        _accel = new GSAITargetAcceleration();
        path = new GSAIPath(new List<Vector3>(new Vector3[] {Vector3.Zero, Vector3.Zero}), true);
        follow = new GSAIFollowPath(agent, path);

        agent.LinearAccelerationMax = linear_acceleration_max;
        agent.LinearSpeedMax = linear_speed_max;
        agent.LinearDragPercentage = _drag;

        follow.PathOffset = path_offset;
        follow.PredictionTime = predict_time;
        follow.DecelerationRadius = deceleration_radius;
        follow.ArrivalTolerance = arrival_tolerance;

        drawer.PathEstablishedSignal += _on_Drawer_path_established;
    }

    private bool _valid = false;
    private float _drag = 0.1f;
    private GSAITargetAcceleration _accel;
    private GSAIKinematicBody2DAgent agent;
    private GSAIFollowPath follow;
    private GSAIPath path;

    public override void _PhysicsProcess(double delta)
    {
        if (_valid)
        {
            follow.CalculateSteering(_accel);
            agent._ApplySteering(_accel, (float) delta);
        }
    }

    private void _on_Drawer_path_established(List<Vector2> points)
    {
        var _waypoints = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            _waypoints[i] = GSAIUtils.ToVector3(points[i]);
        }
        var waypoints = new List<Vector3>(_waypoints);
        path.CreatePath(waypoints);
        _valid = true;
    }
}
