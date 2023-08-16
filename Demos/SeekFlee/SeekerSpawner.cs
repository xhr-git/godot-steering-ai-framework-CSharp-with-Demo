using Godot;
using System;

public partial class SeekerSpawner : Node2D
{
    enum SeekMode
    {
        Flee, Seek,
    }

    [Export] private PackedScene Entity;

    [Export] private SeekMode behavior_mod = SeekMode.Seek;
    [Export] private float linear_speed_max = 570;
    [Export] private float linear_accel_max = 1110;

    [Export] private int entity_count = 10;

    private Rect2 camera_boundaries;
    private PlayerWithAgent player;

    public override void _Ready()
    {
        player = Owner.GetNode<PlayerWithAgent>("Player");
        camera_boundaries = new Rect2(
            Vector2.Zero, new Vector2(
                ProjectSettings.GetSetting("display/window/size/width").As<float>(),
                ProjectSettings.GetSetting("display/window/size/height").As<float>()));
        var rng = new Random();
        for (int i = 0; i < entity_count; i++)
        {
            var pos = new Vector2(
                rng.Next(20, (int)camera_boundaries.Size.X - 40),
                rng.Next(20, (int)camera_boundaries.Size.Y - 40));
            var entity = Entity.Instantiate() as Seeker;
            entity.GlobalPosition = pos;
            entity.player_agent = player.agent;
            entity.start_speed = linear_speed_max;
            entity.start_accel = linear_accel_max;
            entity.use_seek = behavior_mod == SeekMode.Seek;
            AddChild(entity);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("switch_mode"))
        {
            foreach (Seeker e in GetChildren())
            {
                e.use_seek = !e.use_seek;
            }
        }
        if (Input.IsActionJustPressed("press_e"))
        {
            var rng = new Random();
            foreach (Seeker e in GetChildren())
            {
                e.GlobalPosition = new Vector2(
                    rng.Next(20, (int)camera_boundaries.Size.X - 40),
                    rng.Next(20, (int)camera_boundaries.Size.Y - 40));
            }
        }
    }
}
