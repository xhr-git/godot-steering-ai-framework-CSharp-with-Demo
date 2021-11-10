using Godot;
using System;

public class SceneDesc : PanelContainer
{
    [Export(PropertyHint.MultilineText)]
    private string Description = "Scene description.";

    public override void _Ready()
    {
        GetNode<RichTextLabel>("MarginContainer/RichTextLabel").BbcodeText = Description;
    }
}
