using Microsoft.Xna.Framework;
using ArenaDefender.Entities;
using NUnit.Framework;

namespace ArenaDefender.Tests;

[TestFixture]
public class EnemyTests
{
    [Test]
    public void FastEnemy_HasLessHealthThanTank()
    {
        var fast = new FastEnemy(Vector2.Zero);
        var tank = new TankEnemy(Vector2.Zero);
        Assert.That(fast.Health, Is.LessThan(tank.Health));
    }

    [Test]
    public void FastEnemy_MovesFasterThanTank()
    {
        var fast = new FastEnemy(Vector2.Zero);
        var tank = new TankEnemy(Vector2.Zero);
        Assert.That(fast.Speed, Is.GreaterThan(tank.Speed));
    }

    [Test]
    public void MoveToward_MovesEnemyCloserToTarget()
    {
        var enemy = new FastEnemy(new Vector2(0, 0));
        var target = new Vector2(100, 0);

        float before = Vector2.Distance(enemy.Position, target);
        enemy.MoveToward(target, 0.1f);
        float after = Vector2.Distance(enemy.Position, target);

        Assert.That(after, Is.LessThan(before));
    }

    [Test]
    public void MoveToward_TargetOnTopOfEnemy_DoesNotThrowOrProduceNaN()
    {
        var enemy = new FastEnemy(new Vector2(50, 50));
        enemy.MoveToward(new Vector2(50, 50), 0.1f);
        Assert.That(float.IsNaN(enemy.Position.X), Is.False);
    }

    [Test]
    public void TakeDamage_KillsEnemyWhenHealthReachesZero()
    {
        var enemy = new FastEnemy(Vector2.Zero);
        enemy.TakeDamage(1000f);
        Assert.That(enemy.IsAlive, Is.False);
    }

    [Test]
    public void StandardEnemy_StatsSitBetweenFastAndTank()
    {
        var fast = new FastEnemy(Vector2.Zero);
        var standard = new StandardEnemy(Vector2.Zero);
        var tank = new TankEnemy(Vector2.Zero);

        Assert.That(standard.Health, Is.GreaterThan(fast.Health));
        Assert.That(standard.Health, Is.LessThan(tank.Health));
        Assert.That(standard.Speed, Is.LessThan(fast.Speed));
        Assert.That(standard.Speed, Is.GreaterThan(tank.Speed));
    }
}