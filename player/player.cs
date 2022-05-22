#define JUMP_VERSION_1

using Godot;
using System;

public class player : KinematicBody2D
{
    #region Constants
    // Gravity Strength
    private readonly Vector2 _gravity = Vector2.Down * 1000f;

    // Max Jumps (Double Jumps)
    private const int JumpCount = 2;
    #endregion

    private Vector2 _velocity;
    
    // Jumping
    private bool _wasJumpingLastFrame;
    private int _jumpsLeft = JumpCount;

    // Dashing
    private bool _hasDash = true;


    public override void _PhysicsProcess(float dt)
    {
        base._PhysicsProcess(dt);

        // Apply Gravity
        _velocity += _gravity * dt;

        ProcessJumping();

        UpdateVelocityDisplay();

        // Apply Velocity
        var collision = MoveAndCollide(_velocity * dt);

        // Check for Collisions
        CheckCollisions(collision);
    }

    private void ProcessJumping()
    {
        var isJumping = Input.IsActionPressed("move_jump");
        // Jump if player has jumps and he started pressing jump this frame
        if (_jumpsLeft > 0 && !_wasJumpingLastFrame && isJumping)
        {
            _jumpsLeft--;
            
            //////// Two Jump Implementations (JUMP_VERSION_1 / JUMP_VERSION_2) [see line 1]
            //////// Jump Version 1 (Spamming spacebar is less efficient)
            // Increase Vertical Velocity up to 500 Units (capped)
            #if JUMP_VERSION_1
                if (_velocity.y > -500f)
                {
                    _velocity.y = -500f;
                }
            #endif
            //////// Jump Version 2 (Spamming spacebar is more efficient)
            // Increase Vertical Velocity by 500 Units (not capped)
            #if JUMP_VERSION_2
                _velocity += Vector2.Up * 500f;
            #endif
            ////////
        }

        _wasJumpingLastFrame = isJumping;
    }

    private void UpdateVelocityDisplay()
    {
        var velocityDisplay = GetNode<Sprite>("playerVelocityDisplay");
        ((CurveTexture) velocityDisplay.Texture).Width =
            (int) Mathf.Clamp(_velocity.Length() / 5000f * 1024f + 32f, 32f, 4096f);
        velocityDisplay.Rotation = _velocity.Angle();
    }

    private void CheckCollisions(KinematicCollision2D collision)
    {
        if (collision == null) return;

        var collidedSides = collision.Normal;
        // Reset velocity on collision
        _velocity -= -collidedSides * _velocity;


        if (collidedSides.y <= 0)
        {
            ResetActions();
        }
    }

    private void ResetActions()
    {
        _jumpsLeft = JumpCount;
        _hasDash = true;
    }
}