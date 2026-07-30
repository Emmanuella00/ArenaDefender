using Microsoft.Xna.Framework;
using ArenaDefender.Entities;
using NUnit.Framework;

namespace ArenaDefender.Tests;

[TestFixture]
public class PowerUpTests
{
    [Test]
    public void Collect_MarksPowerUpNotAlive()
    {
        var powerUp = new PowerUp(Vector2.Zero, PowerUpType.Health);
        powerUp.Collect();
        Assert.That(powerUp.IsAlive, Is.False);
    }

    [Test]
    public void NewPowerUp_StartsAlive()
    {
        var powerUp = new PowerUp(Vector2.Zero, PowerUpType.FireRate);
        Assert.That(powerUp.IsAlive, Is.True);
    }
}