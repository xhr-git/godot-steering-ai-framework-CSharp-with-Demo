using Godot;
using System;
using System.Collections.Generic;
using GodotSteeringAI;

public partial class Spawner : Node2D
{
    [Export] private float linear_speed_max = 350;
    [Export] private float linear_acceleration_max = 40;
    [Export] private float proximity_radius = 140;
    [Export] private bool draw_proximity = true;

    [Export] private PackedScene avoider_template;
    [Export] private Color inner_color = new Color();
    [Export] private Color outer_color = new Color();
    [Export] private int agent_count = 60;

    private Vector2 boundaries;

    private List<Avoider> avoiders;

    public override void _Ready()
    {
        boundaries = new Vector2(
            ProjectSettings.GetSetting("display/window/size/width").As<float>(),
            ProjectSettings.GetSetting("display/window/size/height").As<float>());

        avoiders = new List<Avoider>();
        var avoider_agents = new List<GSAISteeringAgent>();
        for (int i = 0; i < agent_count; i++)
        {
            var avoider = avoider_template.Instantiate() as Avoider;
            AddChild(avoider);
            avoider.Setup(linear_speed_max, linear_acceleration_max, proximity_radius,
                        boundaries.X, boundaries.Y, i == 0 && draw_proximity);
            avoider_agents.Add(avoider.agent);
            avoider.SetRandomNonoverlappingPosition(avoiders, 16);
            if (i == 0)
            {
                avoider.collision.InnerColor = inner_color;
                avoider.collision.OuterColor = outer_color;
                avoider.collision.QueueRedraw();
            }
            avoiders.Add(avoider);
        }
        foreach (var avoider in avoiders)
        {
            avoider.SetProximityAgents(avoider_agents);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        foreach (var avoider in avoiders)
        {
            avoider.GlobalPosition = avoider.GlobalPosition.PosMod(boundaries);
        }
    }
}
