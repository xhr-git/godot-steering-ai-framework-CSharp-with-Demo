using Godot;
using System;
using GodotSteeringAI;

public class QuickPlayer : TriangleBoat
{
    [Signal] delegate void RestartQuickDemo();

    [Export] private float speed_max = 750;
    [Export] private float acceleration_max = 4200;
    [Export] private float rotation_speed_max = 360;
    [Export] private float rotation_accel_max = 1280;
    [Export] private PackedScene BulletScene;

    private Vector2 velocity = Vector2.Zero;
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
        agent.AngularSpeedMax = Mathf.Deg2Rad(rotation_speed_max);
        agent.AngularAccelerationMax = Mathf.Deg2Rad(rotation_accel_max);
        agent.BoundingRadius = calculate_radius(GetNode<CollisionPolygon2D>("CollisionPolygon2D"));
        update_agent();

        proxy_target.Position = GSAIUtils.ToVector3(GetGlobalMousePosition());

        face.AlignmentTolerance = Mathf.Deg2Rad(0.5f);
        face.DecelerationRadius = Mathf.Deg2Rad(45);
    }

    public override void _PhysicsProcess(float delta)
    {
        // Check whether to restart
        if (Input.IsActionJustPressed("switch_mode"))
        {
            EmitSignal("RestartQuickDemo");
            return;
        }

        update_agent();

        var movement = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");

        direction = GSAIUtils.AngleToVector2(Rotation);

        velocity += direction * acceleration_max * movement * delta;
        velocity = velocity.Clamped(speed_max);
        velocity = velocity.LinearInterpolate(Vector2.Zero, 0.1f);
        velocity = MoveAndSlide(velocity);

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
            if (btn.ButtonIndex == (int)ButtonList.Left && evt.IsPressed())
            {
                var bullet = BulletScene.Instance() as Bullet;
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
        agent.LinearVelocity = GSAIUtils.ToVector3(velocity);
        agent.AngularVelocity = angular_velocity;
    }

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
}
