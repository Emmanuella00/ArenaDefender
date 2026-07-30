using Microsoft.Xna.Framework;

namespace ArenaDefender.Entities;

/// <summary>The kinds of power-up an enemy can drop.</summary>
public enum PowerUpType
{
    /// <summary>Restores player health immediately.</summary>
    Health,
    /// <summary>Temporarily increases fire rate.</summary>
    FireRate,
    /// <summary>Temporarily increases movement speed.</summary>
    Speed
}

/// <summary>
/// A collectible dropped by a defeated enemy. Sits in place until the player
/// moves within pickup range, then applies its effect.
/// </summary>
public class PowerUp
{
    /// <summary>Position in the arena.</summary>
    public Vector2 Position { get; }

    /// <summary>Which effect this power-up grants.</summary>
    public PowerUpType Type { get; }

    /// <summary>Collision/pickup radius.</summary>
    public float Radius { get; }

    /// <summary>True until collected.</summary>
    public bool IsAlive { get; private set; } = true;

    /// <summary>Creates a power-up of the given type at a position.</summary>
    public PowerUp(Vector2 position, PowerUpType type, float radius = 14f)
    {
        Position = position;
        Type = type;
        Radius = radius;
    }

    /// <summary>Marks this power-up as collected.</summary>
    public void Collect()
    {
        IsAlive = false;
    }
}