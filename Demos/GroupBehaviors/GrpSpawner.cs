using Godot;
using System;
using GodotSteeringAI;
using System.Collections.Generic;

public partial class GrpSpawner : Node2D
{
    [Export] private float linear_speed_max = 600;
    [Export] private float linear_accel_max = 40;
    [Export] private float proximity_radius = 140;
    [Export] private float separation_decay_coefficient = 2000;
    [Export] private float cohesion_strength = 0.1f;
    [Export] private float separation_strength = 1.5f;
    [Export] private bool show_proximity_radius = true;

    [Export] private PackedScene memberScene;

    private void follower_input_event(InputEvent input, Member follower)
    {
        if (input.IsActionPressed("click"))
        {
            foreach (Member other in GetChildren())
            {
                if (other.draw_proximity)
                {
                    other.draw_proximity = false;
                    other.QueueRedraw();
                }
            }
            follower.draw_proximity = true;
            follower.QueueRedraw();
            MoveChild(follower, GetChildCount());
        }
    }

    public override void _Ready()
    {
        var followers = new List<GSAISteeringAgent>();

        for (int i = 0; i < 20; i++)
        {
            var follower = memberScene.Instantiate() as Member;
            AddChild(follower);
            follower.Position += new Vector2(Member.GetRand(-60, 60), Member.GetRand(-60, 60));
            follower.Setup(linear_speed_max, linear_accel_max, proximity_radius,
                    separation_decay_coefficient, cohesion_strength, separation_strength);
            followers.Add(follower.agent);
            if (i == 0 && show_proximity_radius)
            {
                follower.draw_proximity = true;
                follower.QueueRedraw();
            }
            follower.proximity.Agents = followers;
        }
    }

}
