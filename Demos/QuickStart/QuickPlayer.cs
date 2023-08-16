using Godot;
using System;
using GodotSteeringAI;

public partial class QuickPlayer : TriangleBoat
{
    [Signal] public delegate void RestartQuickDemoEventHandler();

    [Export] private float speed_max = 750;
    [Export] private float acceleration_max = 4200;
    [Export] private float rotation_speed_max = 360;
    [Export] private float rotation_accel_max = 1280;
    [Export] private PackedScene BulletScene;

    private float angular_velocity = 0;
    private Vector2 direction = Vector2.Right;

    public GSAISteeringAgent agent;
    private GSAIAgentLocation proxy_target;
    private GSAIFace face;
    private GSAITargetAcceleration accel;
    private Node2D bulletNode;

    public override void _Ready()
    {
        base._Ready();
        bulletNode = Owner.GetNode<Node2D>("Bullets");
        agent = new GSAISteeringAgent();
        proxy_target = new GSAIAgentLocation();
        face = new GSAIFace(agent, proxy_target);
        accel = new GSAITargetAcceleration();

        agent.LinearSpeedMax = speed_max;
        agent.LinearAccelerationMax = acceleration_max;
        agent.AngularSpeedMax = Mathf.DegToRad(rotation_speed_max);
        agent.AngularAccelerationMax = Mathf.DegToRad(rotation_accel_max);
        agent.BoundingRadius = calculate_radius(GetNode<CollisionPolygon2D>("CollisionPolygon2D"));
        update_agent();

        proxy_target.Position = GSAIUtils.ToVector3(GetGlobalMousePosition());

        face.AlignmentTolerance = Mathf.DegToRad(0.5f);
        face.DecelerationRadius = Mathf.DegToRad(45);
    }

    public override void _PhysicsProcess(double _delta)
    {
        float delta = (float) _delta;
        // Check whether to restart
        if (Input.IsActionJustPressed("switch_mode"))
        {
            EmitSignal(SignalName.RestartQuickDemo);
            return;
        }

        update_agent();

        var movement = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");

        direction = GSAIUtils.AngleToVector2(Rotation);

        Velocity += direction * acceleration_max * movement * delta;
        Velocity = Velocity.LimitLength(speed_max);
        Velocity = Velocity.Lerp(Vector2.Zero, 0.1f);
        MoveAndSlide();

        face.CalculateSteering(accel);
        angular_velocity += accel.Angular * delta;
        angular_velocity = Mathf.Clamp(angular_velocity, -agent.AngularSpeedMax, agent.AngularSpeedMax);
        //angular_velocity = Mathf.Lerp(angular_velocity, 0, 0.1f);
        Rotation += angular_velocity * delta;
    }

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt is InputEventMouseMotion motion)
        {
            proxy_target.Position = GSAIUtils.ToVector3(motion.Position);
        }
        else if (evt is InputEventMouseButton btn)
        {
            if (btn.ButtonIndex == MouseButton.Left && evt.IsPressed())
            {
                var bullet = BulletScene.Instantiate<Bullet>();
                bullet.GlobalPosition = GlobalPosition - direction * (agent.BoundingRadius - 5);
                bullet.Start(-direction);
                bulletNode.AddChild(bullet);
            }
        }
    }

    private void update_agent()
    {
        agent.Position = GSAIUtils.ToVector3(GlobalPosition);
        agent.Orientation = Rotation;
        agent.LinearVelocity = GSAIUtils.ToVector3(Velocity);
        agent.AngularVelocity = angular_velocity;
    }

    private float calculate_radius(CollisionPolygon2D collision)
    {
        var furthest_point = new Vector2(float.NegativeInfinity, float.PositiveInfinity);
        foreach (Vector2 p in collision.Polygon)
        {
            if (Mathf.Abs(p.X) > furthest_point.X)
                furthest_point.X = p.X;
            if (Mathf.Abs(p.Y) < furthest_point.Y)
                furthest_point.Y = p.Y;
        }
        return furthest_point.Length();
    }
}
