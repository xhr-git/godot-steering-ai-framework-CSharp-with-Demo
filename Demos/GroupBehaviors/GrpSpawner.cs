using Godot;
using System;
using GodotSteeringAI;
using System.Collections.Generic;

public class GrpSpawner : Node2D
{
    [Export] private float linear_speed_max = 600;
    [Export] private float linear_accel_max = 40;
    [Export] private float proximity_radius = 140;
    [Export] private float separation_decay_coefficient = 2000;
    [Export] private float cohesion_strength = 0.1f;
    [Export] private float separation_strength = 1.5f;
    [Export] private bool show_proximity_radius = true;

    [Export] private PackedScene memberScene;

    private void follower_input_event(Viewport node, InputEvent input, int shape, Member follower)
    {
        if (input.IsActionPressed("click"))
        {
            foreach (Member other in GetChildren())
            {
                if (other.draw_proximity)
                {
                    other.draw_proximity = false;
                    other.Update();
                }
            }
            follower.draw_proximity = true;
            follower.Update();
            MoveChild(follower, GetChildCount());
        }
    }

    public override void _Ready()
    {
        var followers = new List<GSAISteeringAgent>();

        for (int i = 0; i < 20; i++)
        {
            var follower = memberScene.Instance() as Member;
            AddChild(follower);
            follower.Position += new Vector2(Member.GetRand(-60, 60), Member.GetRand(-60, 60));
            follower.Setup(linear_speed_max, linear_accel_max, proximity_radius,
                    separation_decay_coefficient, cohesion_strength, separation_strength);
            followers.Add(follower.agent);
            if (i == 0 && show_proximity_radius)
            {
                follower.draw_proximity = true;
                follower.Update();
            }
            follower.proximity.Agents = followers;
            follower.Connect("input_event", this, nameof(follower_input_event),
                new Godot.Collections.Array { follower });
        }
    }

}
