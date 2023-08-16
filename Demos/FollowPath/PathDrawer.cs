using Godot;
using System;
using System.Collections.Generic;

public partial class PathDrawer : Node2D
{
    public delegate void PathEstablished(List<Vector2> points);
    public PathEstablished PathEstablishedSignal { get; set; }

    private List<Vector2> active_points = new List<Vector2>(256);
    private List<Vector2> tmp_points = new List<Vector2>(256);
    private bool is_drawing = false;
    private float distance_threshold = 100;

    public override void _UnhandledInput(InputEvent evt)
    {
        if (evt is InputEventMouseMotion mouseMove)
        {
            if (is_drawing)
            {
                active_points.Add(mouseMove.Position);
                QueueRedraw();
            }
        }
        else if (evt is InputEventMouseButton mouseBtn)
        {
            if (mouseBtn.Pressed)
            {
                if (mouseBtn.ButtonIndex == MouseButton.Left)
                {
                    is_drawing = true;
                    active_points.Clear();
                    active_points.Add(mouseBtn.Position);
                    QueueRedraw();
                }
            }
            else
            {
                is_drawing = false;
                if (active_points.Count >= 2)
                    _Simplify();
            }
        }
    }

    public override void _Draw()
    {
        if (is_drawing)
        {
            foreach (var point in active_points)
            {
                DrawCircle(point, 2, new Color("RED"));
            }
        }
        else if (active_points.Count > 0)
        {
            DrawCircle(active_points[0], 5, new Color("RED"));
            DrawCircle(active_points[active_points.Count - 1], 5, new Color("YELLOW"));
            DrawPolyline(active_points.ToArray(), new Color("SKY_BLUE"), 2);
        }
    }

    private void _Simplify()
    {
        var first = active_points[0];
        var last = active_points[active_points.Count - 1];
        var key = first;
        tmp_points.Clear();
        tmp_points.Add(first);
        foreach (var point in active_points)
        {
            var distance = point.DistanceTo(key);
            if (distance > distance_threshold)
            {
                key = point;
                tmp_points.Add(key);
            }
        }
        var t = tmp_points;
        tmp_points = active_points;
        active_points = t;
        if (active_points[active_points.Count - 1] != last)
            active_points.Add(last);
        QueueRedraw();
        PathEstablishedSignal?.Invoke(active_points);
    }
}
