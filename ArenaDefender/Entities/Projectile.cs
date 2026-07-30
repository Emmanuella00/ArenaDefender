using Microsoft.Xna.Framework;

namespace ArenaDefender.Entities;

/// <summary>
/// A player-fired projectile. Travels in a straight line at constant velocity
/// and expires when it leaves the arena or strikes an enemy.
/// </summary>
public class Projectile
{
    /// <summary>Current position in the arena.</summary>
    public Vector2 Position { get; private set; }

    /// <summary>Velocity in pixels per second.</summary>
    public Vector2 Velocity { get; }

    /// <summary>Damage dealt to an enemy on hit.</summary>
    public float Damage { get; }

    /// <summary>Collision radius.</summary>
    public float Radius { get; }

    /// <summary>True while the projectile is still in play.</summary>
    public bool IsAlive { get; private set; } = true;

    /// <summary>Creates a projectile with a starting position, velocity and damage.</summary>
    public Projectile(Vector2 position, Vector2 velocity, float damage, float radius = 5f)
    {
        Position = position;
        Velocity = velocity;
        Damage = damage;
        Radius = radius;
    }

    /// <summary>Advances the projectile by its velocity.</summary>
    public void Update(float delta)
    {
        Position += Velocity * delta;
    }

    /// <summary>Marks the projectile for removal (on hit or when off-screen).</summary>
    public void Kill()
    {
        IsAlive = false;
    }
}