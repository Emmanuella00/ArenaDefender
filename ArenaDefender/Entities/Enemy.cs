using Microsoft.Xna.Framework;
using ArenaDefender.Math;

namespace ArenaDefender.Entities;

/// <summary>
/// Abstract base for all enemies. Holds shared state (position, health, speed, score value)
/// and shared movement, while leaving type-specific tuning to subclasses.
/// </summary>
public abstract class Enemy
{
    /// <summary>Current position in the arena.</summary>
    public Vector2 Position { get; protected set; }

    /// <summary>Current health. When it reaches zero the enemy is no longer alive.</summary>
    public float Health { get; protected set; }

    /// <summary>Movement speed in pixels per second.</summary>
    public float Speed { get; protected set; }

    /// <summary>Points awarded to the player when this enemy is destroyed.</summary>
    public int ScoreValue { get; protected set; }

    /// <summary>Damage dealt to the player on contact.</summary>
    public int ContactDamage { get; protected set; }

    /// <summary>Collision radius, used for distance-based hit detection.</summary>
    public float Radius { get; protected set; }

    /// <summary>True while the enemy still has health.</summary>
    public bool IsAlive => Health > 0;

    /// <summary>Initialises shared enemy state.</summary>
    protected Enemy(Vector2 position, float health, float speed, int scoreValue, int contactDamage, float radius)
    {
        Position = position;
        Health = health;
        Speed = speed;
        ScoreValue = scoreValue;
        ContactDamage = contactDamage;
        Radius = radius;
    }

    /// <summary>
    /// Moves the enemy toward the target by its speed. Uses a normalised direction vector,
    /// guarded against the zero-length case so an enemy sitting exactly on the target does not produce NaN.
    /// </summary>
    /// <param name="target">The point to move toward (usually the player).</param>
    /// <param name="delta">Elapsed seconds since the last update.</param>
    public virtual void MoveToward(Vector2 target, float delta)
    {
        Vector2 direction = GameMath.SafeNormalize(target - Position);
        Position += direction * Speed * delta;
    }

    /// <summary>Applies damage. Health is clamped so it never goes below zero.</summary>
    /// <param name="amount">Damage to apply. Non-positive values are ignored.</param>
    public void TakeDamage(float amount)
    {
        if (amount <= 0)
            return;

        Health -= amount;
        if (Health < 0)
            Health = 0;
    }
}