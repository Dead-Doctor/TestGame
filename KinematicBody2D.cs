using Godot;
using System;

public class KinematicBody2D : Godot.KinematicBody2D
{

    public override void _Ready()
    {
        
    }

    public override void _Process(float delta)
    {
        MoveAndCollide(new Vector2(4,5));
    }
}