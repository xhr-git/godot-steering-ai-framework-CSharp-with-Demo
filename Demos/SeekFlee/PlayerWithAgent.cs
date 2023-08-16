using Godot;
using System;
using GodotSteeringAI;

public partial class PlayerWithAgent : CharacterBody2D
{
    private float speed = 600;
    public GSAIAgentLocation agent = new GSAIAgentLocation();

    public override void _Ready()
    {
        agent.Position = GSAIUtils.ToVector3(GlobalPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
        var movement = _get_movement();
        if (movement.LengthSquared() < 0.01f)
            return;

        Velocity = movement * speed;
        MoveAndSlide();
        agent.Position = GSAIUtils.ToVector3(GlobalPosition);
    }

    private Vector2 _get_movement()
    {
        return new Vector2(
            Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
            Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up"));
    }
}
