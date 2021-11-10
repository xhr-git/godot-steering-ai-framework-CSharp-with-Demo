using Godot;
using System;
using System.Collections.Generic;
using GodotSteeringAI;

public class QuickAgent : TriangleBoat
{
    /// <summary>
    /// Maximum possible linear velocity
    /// </summary>
    [Export] private float speed_max = 450;
    /// <summary>
    /// Maximum change in linear velocity
    /// </summary>
    [Export] private float acceleration_max = 2800;
    /// <summary>
    /// Maximum rotation velocity represented in degrees
    /// </summary>
    [Export] private float angular_speed_max = 360;
    /// <summary>
    /// Maximum change in rotation velocity represented in degrees
    /// </summary>
    [Export] private float angular_accel_max = 1280;

    [Export] private int health_max = 10;
    [Export] private int flee_health_threshold = 4;

    private Vector2 velocity = Vector2.Zero;
    private float angular_velocity = 0;
    private float linear_drag = 0.1f;
    private float angular_drag = 0.1f;

    private Vector2 restart_pos;

    /// <summary>
    /// Holds the linear and angular components calculated by our steering behaviors.
    /// </summary>
    private GSAITargetAcceleration acceleration;

    private int current_health;

    /// <summary>
    /// GSAISteeringAgent holds our agent's position, orientation, maximum speed and acceleration.
    /// </summary>
    private GSAISteeringAgent agent;
    /// <summary>
    /// This assumes that our player class will keep its own agent updated.
    /// </summary>
    private GSAISteeringAgent player_agent;
    /// <summary>
    /// Proximities represent an area with which an agent can identify where neighbors in its relevant
    /// group are. In our case, the group will feature the player, which will be used to avoid a
    /// collision with them. We use a radius proximity so the player is only relevant inside 100 pixels.
    /// </summary>
    private GSAIRadiusProximity proximity;
    /// <summary>
    /// GSAIBlend combines behaviors together, calculating all of their acceleration together and adding
    /// them together, multiplied by a strength. We will have one for fleeing, and one for pursuing,
    /// toggling them depending on the agent's health. Since we want the agent to rotate AND move, then
    /// we aim to blend them together.
    /// </summary>
    private GSAIBlend flee_blend;
    private GSAIBlend pursue_blend;
    /// <summary>
    /// GSAIPriority will be the main steering behavior we use. It holds sub-behaviors and will pick the
    /// first one that returns non-zero acceleration, ignoring any afterwards.
    /// </summary>
    private GSAIPriority priority;

    public override void _Ready()
    {
        base._Ready();
        var player = Owner.GetNode<QuickPlayer>("QuickPlayer");
        player.Connect("RestartQuickDemo", this, nameof(_on_restart));
        restart_pos = GlobalPosition;

        // ---------- Initialization for our agent ----------
        current_health = health_max;
        acceleration = new GSAITargetAcceleration();
        agent = new GSAISteeringAgent();
        player_agent = player.agent;
        proximity = new GSAIRadiusProximity(agent, new List<GSAISteeringAgent> { player_agent }, 100);
        flee_blend = new GSAIBlend(agent);
        pursue_blend = new GSAIBlend(agent);
        priority = new GSAIPriority(agent);

        // ---------- Configuration for our agent ----------
        agent.LinearSpeedMax = speed_max;
        agent.LinearAccelerationMax = acceleration_max;
        agent.AngularSpeedMax = Mathf.Deg2Rad(angular_speed_max);
        agent.AngularAccelerationMax = Mathf.Deg2Rad(angular_accel_max);
        agent.BoundingRadius = calculate_radius(GetNode<CollisionPolygon2D>("CollisionPolygon2D"));
        update_agent();

        // ---------- Configuration for our behaviors ----------
        // Pursue will happen while the agent is in good health. It produces acceleration that takes
        // the agent on an intercept course with the target, predicting its position in the future.
        var pursue = new GSAIPursue(agent, player_agent);
        pursue.PredictTimeMax = 1.5f;

        // Flee will happen while the agent is in bad health, so will start disabled. It produces
        // acceleration that takes the agent directly away from the target with no prediction.
        var flee = new GSAIFlee(agent, player_agent);

        // AvoidCollision tries to keep the agent from running into any of the neighbors found in its
        // proximity group. In our case, this will be the player, if they are close enough.
        var avoid = new GSAIAvoidCollisions(agent, proximity);

        // Face turns the agent to keep looking towards its target. It will be enabled while the agent
        // is not fleeing due to low health. It tries to arrive 'on alignment' with 0 remaining velocity.
        var face = new GSAIFace(agent, player_agent);

        // We use deg2rad because the math in the toolkit assumes radians.
        // How close for the agent to be 'aligned', if not exact.
        face.AlignmentTolerance = Mathf.Deg2Rad(0.5f);
        // When to start slowing down
        face.DecelerationRadius = Mathf.Deg2Rad(60);

        // LookWhereYouGo turns the agent to keep looking towards its direction of travel.
        // It will only be enabled while the agent is at low health.
        var look = new GSAILookWhereYouGo(agent);
        // How close for the agent to be 'aligned', if not exact
        look.AlignmentTolerance = Mathf.Deg2Rad(0.5f);
        // When to start slowing down
        look.DecelerationRadius = Mathf.Deg2Rad(60);

        // Behaviors that are not enabled produce 0 acceleration.
        // Adding our fleeing behaviors to a blend. The order does not matter.
        flee_blend.IsEnabled = false;
        flee_blend.Add(look, 1);
        flee_blend.Add(flee, 1);

        // Adding our pursuit behaviors to a blend. The order does not matter.
        pursue_blend.Add(face, 1);
        pursue_blend.Add(pursue, 1);

        // Adding our final behaviors to the main priority behavior. The order does matter here.
        // We want to avoid collision with the player first, flee from the player second when enabled,
        // and pursue the player last when enabled.
        priority.Add(avoid);
        priority.Add(flee_blend);
        priority.Add(pursue_blend);
    }

    public override void _PhysicsProcess(float delta)
    {
        // Make sure any change in position and speed has been recorded.
        update_agent();

        if (current_health <= flee_health_threshold)
        {
            pursue_blend.IsEnabled = false;
            flee_blend.IsEnabled = true;
        }

        // Calculate the desired acceleration.
        priority.CalculateSteering(acceleration);

        // We add the discovered acceleration to our linear velocity. The toolkit does not limit
        // velocity, just acceleration, so we clamp the result ourselves here.
        velocity = (velocity + GSAIUtils.ToVector2(acceleration.Linear) * delta).Clamped(agent.LinearSpeedMax);

        // This applies drag on the agent's motion, helping it to slow down naturally.
        velocity = velocity.LinearInterpolate(Vector2.Zero, linear_drag);

        // And since we're using a KinematicBody2D, we use Godot's excellent move_and_slide to actually
        // apply the final movement, and record any change in velocity the physics engine discovered.
        velocity = MoveAndSlide(velocity);

        // We then do something similar to apply our agent's rotational speed.
        angular_velocity = Mathf.Clamp(angular_velocity + acceleration.Angular * delta,
                                       -agent.AngularSpeedMax, agent.AngularSpeedMax);

        // This applies drag on the agent's rotation, helping it slow down naturally.
        angular_velocity = Mathf.Lerp(angular_velocity, 0, angular_drag);
        Rotation += angular_velocity * delta;
    }

    /// <summary>
    /// In order to support both 2D and 3D, the toolkit uses Vector3, so the conversion is required
    /// when using 2D nodes. The Z component can be left to 0 safely.
    /// </summary>
    private void update_agent()
    {
        agent.Position = GSAIUtils.ToVector3(GlobalPosition);
        agent.Orientation = Rotation;
        agent.LinearVelocity = GSAIUtils.ToVector3(velocity);
        agent.AngularVelocity = angular_velocity;
    }

    /// <summary>
    /// We calculate the radius from the collision shape - this will approximate the agent's size in the
    /// game world, to avoid collisions with the player.
    /// </summary>
    /// <param name="collision"></param>
    /// <returns></returns>
    private float calculate_radius(CollisionPolygon2D collision)
    {
        var furthest_point = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
        foreach (Vector2 p in collision.Polygon)
        {
            if (Mathf.Abs(p.x) > furthest_point.x)
                furthest_point.x = p.x;
            if (Mathf.Abs(p.y) < furthest_point.y)
                furthest_point.y = p.y;
        }
        return furthest_point.Length();
    }

    private void _on_restart()
    {
        GlobalPosition = restart_pos;
        current_health = health_max;
        pursue_blend.IsEnabled = true;
        flee_blend.IsEnabled = false;
        Show();
    }

    public void Damage(int amount)
    {
        current_health -= amount;
        if (current_health <= 0)
            Hide();
    }
}
