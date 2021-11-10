using Godot;
using System;
using System.Collections.Generic;

public class DemoMain : Node
{
    private ItemList demoList;

    private List<string> demoPaths;

    private List<CanvasItem> hidenList;
    private Button goBackBtn;

    private Button loadBtn;

    private Node2D demo_player;

    public override void _Ready()
    {
        demo_player = GetNode<Node2D>("DemoPlayer");

        InitDemoList();

        InitHideShowList();
    }

    private void InitHideShowList()
    {
        goBackBtn = GetNode<Button>("ButtonGoBack");
        goBackBtn.Connect("pressed", this, nameof(_on_ButtonGoBack_pressed));

        loadBtn = GetNode<Button>("DemoPickerUI/VBoxContainer/Button");
        loadBtn.Connect("pressed", this, nameof(_on_DemoPickerUI_demo_requested));

        hidenList = new List<CanvasItem>();
        hidenList.Add(GetNode<CanvasItem>("Background"));
        hidenList.Add(GetNode<CanvasItem>("DemoPickerUI"));
    }

    private void InitDemoList()
    {
        demoList = GetNode<ItemList>("DemoPickerUI/VBoxContainer/ItemList");
        demoList.Connect("item_selected", this, nameof(_on_item_selected));
        demoList.Connect("item_activated", this, nameof(_on_ItemList_item_activated));

        demoPaths = _find_demos("res://Demos/", new string[] { "*Demo.tscn" }, true);
        foreach (var demo in demoPaths)
        {
            var tmp = demo.Split(new char[] { '/' });
            var name = tmp[tmp.Length - 1].Replace("Demo.tscn", "");
            demoList.AddItem(name);
        }
        demoList.Select(0);
    }

    private List<string> _find_demos(string root, string[] patterns, bool is_recursive = false, bool do_skip_hidden = true)
    {
        var paths = new List<string>();
        var directory = new Directory();

        if (!directory.DirExists(root))
        {
            GD.PrintErr("The directory does not exist: ", root);
            return paths;
        }
        if (directory.Open(root) != Error.Ok)
        {
            GD.PrintErr("Could not open the following dirpath: ", root);
            return paths;
        }

        directory.ListDirBegin(true, do_skip_hidden);
        var file_name = directory.GetNext();
        while (file_name != "")
        {
            if (directory.CurrentIsDir() && is_recursive)
            {
                var subdirectory = root.PlusFile(file_name);
                paths.AddRange(_find_demos(subdirectory, patterns, is_recursive, do_skip_hidden));
            }
            else
            {
                foreach (var pattern in patterns)
                {
                    if (file_name.Match(pattern))
                    {
                        paths.Add(root.PlusFile(file_name));
                    }
                }
            }
            file_name = directory.GetNext();
        }
        directory.ListDirEnd();
        return paths;
    }

    private void _on_ButtonGoBack_pressed()
    {
        foreach (Node node in demo_player.GetChildren())
        {
            node.QueueFree();
        }
        foreach (var node in hidenList)
        {
            node.Show();
        }
        goBackBtn.Hide();
    }

    private int demoItemIndex = 0;

    private void _on_ItemList_item_activated(int index)
    {
        demoItemIndex = index;
        _on_DemoPickerUI_demo_requested();
    }

    private void _on_DemoPickerUI_demo_requested()
    {
        foreach (var node in hidenList)
        {
            node.Hide();
        }
        var demo = GD.Load(demoPaths[demoItemIndex]) as PackedScene;
        if (demo is null)
            return;
        demo_player.AddChild(demo.Instance());
        goBackBtn.Show();
    }

    private void _on_item_selected(int index)
    {
        if (index < demoPaths.Count)
        {
            demoItemIndex = index;
        }
    }
}
