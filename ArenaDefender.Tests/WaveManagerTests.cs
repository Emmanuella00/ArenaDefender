using ArenaDefender.Systems;
using NUnit.Framework;

namespace ArenaDefender.Tests;

[TestFixture]
public class WaveManagerTests
{
    [Test]
    public void EnemiesForWave_LaterWaveSpawnsMore()
    {
        var manager = new WaveManager();
        Assert.That(manager.EnemiesForWave(5), Is.GreaterThan(manager.EnemiesForWave(1)));
    }

    [Test]
    public void SpawnInterval_ShrinksWithWave()
    {
        var manager = new WaveManager();
        Assert.That(manager.SpawnIntervalForWave(5), Is.LessThan(manager.SpawnIntervalForWave(1)));
    }

    [Test]
    public void SpawnInterval_NeverGoesBelowFloor()
    {
        var manager = new WaveManager();
        Assert.That(manager.SpawnIntervalForWave(100), Is.GreaterThanOrEqualTo(0.3f));
    }

    [Test]
    public void TankChance_NeverExceedsCap()
    {
        var manager = new WaveManager();
        Assert.That(manager.TankChanceForWave(100), Is.LessThanOrEqualTo(0.5f));
    }

    [Test]
    public void AdvanceWave_IncrementsWaveNumber()
    {
        var manager = new WaveManager();
        manager.AdvanceWave();
        Assert.That(manager.CurrentWave, Is.EqualTo(2));
    }

    [Test]
    public void EnemiesForWave_InvalidWave_Throws()
    {
        var manager = new WaveManager();
        Assert.Throws<System.ArgumentOutOfRangeException>(() => manager.EnemiesForWave(0));
    }
}