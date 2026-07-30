using Microsoft.Xna.Framework;

namespace ArenaDefender.Entities;

/// <summary>A slow but tough enemy. Absorbs many hits and deals heavy contact damage.</summary>
public class TankEnemy : Enemy
{
    /// <summary>Creates a tank enemy at the given position.</summary>
    public TankEnemy(Vector2 position)
        : base(position, health: 100f, speed: 55f, scoreValue: 30, contactDamage: 15, radius: 24f)
    {
    }
}