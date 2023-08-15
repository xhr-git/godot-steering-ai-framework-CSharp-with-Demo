using Godot;
using System;

public partial class SceneDesc : PanelContainer
{
    [Export(PropertyHint.MultilineText)]
    private string Description = "Scene description.";

    public override void _Ready()
    {
        GetNode<RichTextLabel>("MarginContainer/RichTextLabel").Text = Description;
    }
}
