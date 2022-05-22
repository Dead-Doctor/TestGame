using Godot;

public class Camera : Camera2D
{
    public override void _Process(float delta)
    {
        base._Process(delta);
        if (Input.IsActionJustPressed("window_togglefullscreen"))
        {
            OS.WindowFullscreen = !OS.WindowFullscreen;
        }
    }
}
