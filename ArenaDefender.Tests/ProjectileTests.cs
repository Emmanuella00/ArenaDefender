using Microsoft.Xna.Framework;
using ArenaDefender.Entities;
using NUnit.Framework;

namespace ArenaDefender.Tests;

[TestFixture]
public class ProjectileTests
{
    [Test]
    public void Update_MovesProjectileByVelocity()
    {
        var projectile = new Projectile(new Vector2(0, 0), new Vector2(100, 0), 10f);
        projectile.Update(1.0f);
        Assert.That(projectile.Position.X, Is.EqualTo(100f).Within(0.001f));
    }

    [Test]
    public void Kill_MarksProjectileNotAlive()
    {
        var projectile = new Projectile(Vector2.Zero, Vector2.Zero, 10f);
        projectile.Kill();
        Assert.That(projectile.IsAlive, Is.False);
    }
}