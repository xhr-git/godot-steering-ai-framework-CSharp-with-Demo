using Godot;
using System;
using GodotSteeringAI;

public partial class TrianglePlayer : TriangleBoat
{
    [Export] private float thruster_strength = 1000;
    [Export] private float side_thruster_strength = 40;
    [Export] private float velocity_max = 650;
    [Export] private float angular_velocity_max = 3;
    [Export] private float angular_drag = 0.025f;
    [Export] private float linear_drag = 0.025f;

    private float _angular_velocity = 0;
    public GSAISteeringAgent agent = new GSAISteeringAgent();

    public override void _Ready()
    {
        base._Ready(); GD.Print("player");
    }

    public override void _PhysicsProcess(double _delta)
    {
        float delta = (float) _delta;
        var movement = _get_movement();
        _angular_velocity = _calculate_angular_velocity(
            movement.X, _angular_velocity, side_thruster_strength,
            angular_velocity_max, angular_drag, delta);
        Rotation += _angular_velocity * delta;

        Velocity = _calculate_linear_velocity(
            movement.Y, Velocity, Vector2.Up.Rotated(Rotation),
            linear_drag, thruster_strength, velocity_max, delta);

        MoveAndSlide();
        _update_agent();
    }

    private float _calculate_angular_velocity(float horizontal_movement, float current_velocity,
                                              float _thruster_strength, float _velocity_max,
                                              float ship_drag, float delta)
    {
        var angularVelocity = Mathf.Clamp(
            current_velocity + _thruster_strength * horizontal_movement * delta,
            -_velocity_max, _velocity_max);

        angularVelocity = Mathf.Lerp(angularVelocity, 0, ship_drag);

        return angularVelocity;
    }

    private Vector2 _calculate_linear_velocity(float vertical_movement, Vector2 current_velocity,
                                             Vector2 facing_direction, float ship_drag_coefficient,
                                             float strength, float speed_max, float delta)
    {
        var actual_strength = 0f;
        if (vertical_movement > 0)
            actual_strength = strength;
        else if (vertical_movement < 0)
            actual_strength = -strength / 1.5f;

        var linearVelocity = current_velocity + facing_direction * actual_strength * delta;
        linearVelocity = linearVelocity.Lerp(Vector2.Zero, ship_drag_coefficient);

        return linearVelocity.LimitLength(speed_max);
    }

    private Vector2 _get_movement()
    {
        return new Vector2(
            Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
            Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down")
        );
    }

    private void _update_agent()
    {
        agent.Position = GSAIUtils.ToVector3(GlobalPosition);
        agent.LinearVelocity = GSAIUtils.ToVector3(Velocity);
        agent.AngularVelocity = _angular_velocity;
        agent.Orientation = Rotation;
    }
}
