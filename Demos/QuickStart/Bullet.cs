using Godot;
using System;

public partial class Bullet : CharacterBody2D
{
    [Export] private Color inner_color = new Color("3ca370");
    [Export] private Color outer_color = new Color("8fde5d");
    [Export] private float stroke = 2;
    [Export] private float speed = 1060;

    private Vector2 velocity;

    public override void _Draw()
    {
        var obj = GetNode<CollisionShape2D>("CollisionShape2D");
        var shape = obj.Shape as CircleShape2D;
        DrawCircle(Vector2.Zero, shape.Radius + stroke, outer_color);
        DrawCircle(Vector2.Zero, shape.Radius, inner_color);
    }

    public override void _Ready()
    {
        var timer = GetNode<Timer>("Timer");
        timer.Connect("timeout", new Callable(this, "ClearSelf"));
        timer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        var collision = MoveAndCollide(velocity * (float) delta);
        if (collision is null)
            return;
        GetNode<Timer>("Timer").Stop();
        ClearSelf();
        collision.GetCollider().CallDeferred("Damage", 1);
    }

    public void Start(Vector2 dir)
    {
        velocity = dir * speed;
    }

    private void ClearSelf()
    {
        QueueFree();
    }
}
