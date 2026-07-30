namespace ArenaDefender.Systems;

/// <summary>
/// Controls wave progression and difficulty scaling. Pure logic with no rendering,
/// so the difficulty curve can be verified by unit tests.
/// </summary>
public class WaveManager
{
    /// <summary>The current wave number, starting at 1.</summary>
    public int CurrentWave { get; private set; } = 1;

    /// <summary>
    /// Number of enemies to spawn in a given wave. Scales linearly so each wave
    /// is harder than the last: wave 1 spawns 5, wave 2 spawns 7, and so on.
    /// </summary>
    /// <param name="wave">The wave number (1 or greater).</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Thrown if wave is less than 1.</exception>
    public int EnemiesForWave(int wave)
    {
        if (wave < 1)
            throw new System.ArgumentOutOfRangeException(nameof(wave), "Wave must be 1 or greater.");

        return 3 + wave * 2;
    }

    /// <summary>
    /// Seconds between enemy spawns for a given wave. Shrinks as waves progress so enemies
    /// arrive faster, with a floor of 0.3s so it never becomes impossible.
    /// </summary>
    /// <param name="wave">The wave number (1 or greater).</param>
    public float SpawnIntervalForWave(int wave)
    {
        if (wave < 1)
            throw new System.ArgumentOutOfRangeException(nameof(wave), "Wave must be 1 or greater.");

        float interval = 2.0f - wave * 0.15f;
        return interval < 0.3f ? 0.3f : interval;
    }

    /// <summary>
    /// The chance (0 to 1) that a slain enemy is a tank rather than a fast enemy.
    /// Rises with wave number so tougher enemies become more common, capped at 0.5.
    /// </summary>
    /// <param name="wave">The wave number (1 or greater).</param>
    public float TankChanceForWave(int wave)
    {
        if (wave < 1)
            throw new System.ArgumentOutOfRangeException(nameof(wave), "Wave must be 1 or greater.");

        float chance = 0.1f + wave * 0.04f;
        return chance > 0.5f ? 0.5f : chance;
    }

    /// <summary>Advances to the next wave.</summary>
    public void AdvanceWave()
    {
        CurrentWave++;
    }
}