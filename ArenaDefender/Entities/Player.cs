using Microsoft.Xna.Framework;

namespace ArenaDefender.Entities;

/// <summary>
/// Represents the player character: position, health, and score.
/// Contains no rendering logic so it can be unit-tested without a graphics device.
/// </summary>
public class Player
{
    /// <summary>Maximum health the player can have.</summary>
    public int MaxHealth { get; }

    /// <summary>Current health. Never negative and never above <see cref="MaxHealth"/>.</summary>
    public int Health { get; private set; }

    /// <summary>Accumulated score.</summary>
    public int Score { get; private set; }

    /// <summary>Current position in the arena, in pixels.</summary>
    public Vector2 Position { get; set; }

    /// <summary>True while the player still has health remaining.</summary>
    public bool IsAlive => Health > 0;

    /// <summary>Creates a player at full health.</summary>
    /// <param name="maxHealth">Starting and maximum health. Must be positive.</param>
    /// <param name="startPosition">Where the player begins.</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if maxHealth is not positive.</exception>
    public Player(int maxHealth, Vector2 startPosition)
    {
        if (maxHealth <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be positive.");

        MaxHealth = maxHealth;
        Health = maxHealth;
        Position = startPosition;
        Score = 0;
    }

    /// <summary>Reduces health by the given amount, clamped so health never drops below zero.</summary>
    /// <param name="amount">Damage to apply. Negative values are ignored.</param>
    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        Health -= amount;
        if (Health < 0)
            Health = 0;
    }

    /// <summary>Restores health, clamped so it never exceeds <see cref="MaxHealth"/>.</summary>
    /// <param name="amount">Health to restore. Negative values are ignored.</param>
    public void Heal(int amount)
    {
        if (amount <= 0)
            return;

        Health += amount;
        if (Health > MaxHealth)
            Health = MaxHealth;
    }

    /// <summary>Adds points to the score.</summary>
    /// <param name="points">Points to add. Negative values are ignored.</param>
    public void AddScore(int points)
    {
        if (points <= 0)
            return;

        Score += points;
    }
}