using Microsoft.Xna.Framework;

namespace ArenaDefender.Entities;

/// <summary>A balanced enemy: moderate health, speed, and reward. The baseline threat.</summary>
public class StandardEnemy : Enemy
{
    /// <summary>Creates a standard enemy at the given position.</summary>
    public StandardEnemy(Vector2 position)
        : base(position, health: 50f, speed: 100f, scoreValue: 20, contactDamage: 10, radius: 18f)
    {
    }
}