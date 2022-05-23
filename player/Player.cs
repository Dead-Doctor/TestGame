#define JUMP_VERSION_1
#define REGAIN_JUMPS_FROM_DASH

using System;
using Godot;

public class Player : KinematicBody2D
{
    //// Velocity
    private Vector2 _velocity;

    //// Friction
    // Amount of Friction
    private const float Friction = 3f;
    private const float AirFriction = 0.2f;

    //// Gravity
    // Gravity Strength
    private readonly Vector2 _gravity = Vector2.Down * 800f;

    //// Jumping
    // Jump Strength
    private const float UpwardsJumpStrength = -200f;
    private const float UpwardsJumpHeightBonus = 1.5f;
    private const float SidewardsJumpStrength = 200f;
    // Max Jumps (Double Jumps)
    private const int JumpCount = 2;
    // Jumps left
    private int _jumpsLeft = JumpCount;

    //// Dashing
    private const int DashCount = 1;
    private const float DashTime = 0.2f;
    private const float DashSpeed = 300f;
    private int _dashesLeft = DashCount;
    private bool _dashing;
    private Vector2 _dashingVelocity;
    private float _dashTimeLeft;


    public override void _PhysicsProcess(float dt)
    {
        base._PhysicsProcess(dt);

        // Apply Gravity
        _velocity += _gravity * dt;

        // Process Actions
        // Round to int to eliminate advantage using controller
        var directionAiming = Mathf.RoundToInt(Input.GetActionStrength("move_right")) -
                              Mathf.RoundToInt(Input.GetActionStrength("move_left"));
        ProcessJumping(directionAiming);
        ProcessDashing(directionAiming, dt);

        UpdateVelocityDisplay();

        // Air Friction
        _velocity -= _velocity * AirFriction * dt;
        // Apply Velocity
        var collision = MoveAndCollide(_velocity * dt);

        // Check for Collisions
        CheckCollisions(collision, dt);
    }

    private void ProcessJumping(int directionAiming)
    {
        // Jump if player has jumps
        if (_jumpsLeft <= 0) return;

        if (!Input.IsActionJustPressed("move_jump")) return;

        _jumpsLeft--;


        var jumpHeight = UpwardsJumpStrength;
        if (directionAiming == 0) jumpHeight *= UpwardsJumpHeightBonus;
        //////// Two Jump Implementations (JUMP_VERSION_1 / JUMP_VERSION_2) [see line 1]
        //////// Jump Version 1 (Spamming spacebar is less efficient)
        // Increase Vertical Velocity up to 500 Units (capped)
#if JUMP_VERSION_1
        // '>' because going up is decreasing y
        if (_velocity.y > jumpHeight)
        {
            _velocity.y = jumpHeight;
        }

        _velocity.x = directionAiming switch
        {
            -1 when _velocity.x > -SidewardsJumpStrength => -SidewardsJumpStrength,
            0 => 0,
            1 when _velocity.x < SidewardsJumpStrength => _velocity.x = SidewardsJumpStrength,
            _ => _velocity.x
        };
#endif
        //////// Jump Version 2 (Spamming spacebar is more efficient)
        // Increase Vertical Velocity by 500 Units (not capped)
#if JUMP_VERSION_2
        _velocity += new Vector2(
                directionAiming * SidewardsJumpStrength,
                jumpHeight
            );
#endif
    }

    private void ProcessDashing(int directionAiming, float dt)
    {
        if (!_dashing)
        {
            // Check if player started dashing
            if (_dashesLeft > 0 && Input.IsActionJustPressed("move_dash"))
            {
                _dashesLeft--;
                _dashing = true;
                _dashingVelocity = directionAiming switch
                {
                    -1 => Vector2.Left * DashSpeed,
                    0 => Vector2.Up * DashSpeed,
                    1 => Vector2.Right * DashSpeed,
                    _ => throw new ArgumentException("Unexpected Value for directionAiming: " + directionAiming)
                };
                _dashTimeLeft = DashTime;
            }
            else return;
            // Return if player is not dashing and hasn't started dashing
        }

        // Only runs if player was already dashing or just started dashing
        if (_dashTimeLeft > 0)
        {
            _dashTimeLeft -= dt;
            _velocity = new Vector2(_dashingVelocity);
        }
        else
        {
            _dashing = false;
            _velocity = new Vector2();

#if REGAIN_JUMPS_FROM_DASH
            _jumpsLeft = JumpCount;
#endif
        }
    }

    private void UpdateVelocityDisplay()
    {
        var velocityDisplay = GetNode<Sprite>("playerVelocityDisplay");
        ((GradientTexture) velocityDisplay.Texture).Width = Mathf.RoundToInt(_velocity.Length()) + 1;
        velocityDisplay.Rotation = _velocity.Angle();
    }

    private void CheckCollisions(KinematicCollision2D collision, float dt)
    {
        if (collision == null) return;

        var collidedSides = collision.Normal;
        // Reset velocity on collision
        _velocity -= collidedSides.Abs() * _velocity;
        // Apply Friction
        _velocity -= _velocity * Friction * dt;

        // Regain Actions only on slopes with an angle of 45 degree or less.
        if (collidedSides.Dot(Vector2.Up) > 0.5)
        {
            ResetActions();
        }

        UpdateCollisionDisplay(collidedSides);
    }

    private void UpdateCollisionDisplay(Vector2 collidedSides)
    {
        var collisionDisplay = GetNode<Sprite>("playerCollisionDisplay");
        collisionDisplay.Rotation = collidedSides.Angle();
    }

    private void ResetActions()
    {
        _jumpsLeft = JumpCount;
        _dashesLeft = DashCount;
    }
}