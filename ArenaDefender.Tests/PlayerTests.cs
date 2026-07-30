using Microsoft.Xna.Framework;
using ArenaDefender.Entities;
using NUnit.Framework;

namespace ArenaDefender.Tests;

[TestFixture]
public class PlayerTests
{
    [Test]
    public void TakeDamage_ReducesHealthByAmount()
    {
        var player = new Player(100, Vector2.Zero);
        player.TakeDamage(30);
        Assert.That(player.Health, Is.EqualTo(70));
    }

    [Test]
    public void TakeDamage_MoreThanHealth_ClampsAtZero()
    {
        var player = new Player(100, Vector2.Zero);
        player.TakeDamage(150);
        Assert.That(player.Health, Is.EqualTo(0));
        Assert.That(player.IsAlive, Is.False);
    }

    [Test]
    public void Heal_NeverExceedsMaxHealth()
    {
        var player = new Player(100, Vector2.Zero);
        player.TakeDamage(20);
        player.Heal(50);
        Assert.That(player.Health, Is.EqualTo(100));
    }

    [Test]
    public void AddScore_IncreasesScore()
    {
        var player = new Player(100, Vector2.Zero);
        player.AddScore(10);
        player.AddScore(25);
        Assert.That(player.Score, Is.EqualTo(35));
    }

    [Test]
    public void Constructor_NonPositiveHealth_Throws()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => new Player(0, Vector2.Zero));
    }
}