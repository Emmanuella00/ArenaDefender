using Microsoft.Xna.Framework;

namespace ArenaDefender.Entities;

/// <summary>A weak but quick enemy. Easy to kill, dangerous in numbers.</summary>
public class FastEnemy : Enemy
{
    /// <summary>Creates a fast enemy at the given position.</summary>
    public FastEnemy(Vector2 position)
        : base(position, health: 20f, speed: 160f, scoreValue: 10, contactDamage: 5, radius: 12f)
    {
    }
}