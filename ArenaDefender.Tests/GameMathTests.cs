using Microsoft.Xna.Framework;
using ArenaDefender.Math;
using NUnit.Framework;

namespace ArenaDefender.Tests;

[TestFixture]
public class GameMathTests
{
    [Test]
    public void Cross2D_TargetToOneSide_ReturnsPositive()
    {
        float result = GameMath.Cross2D(new Vector2(1, 0), new Vector2(0, 1));
        Assert.That(result, Is.GreaterThan(0f));
    }

    [Test]
    public void Cross2D_TargetToOtherSide_ReturnsNegative()
    {
        float result = GameMath.Cross2D(new Vector2(1, 0), new Vector2(0, -1));
        Assert.That(result, Is.LessThan(0f));
    }

    [Test]
    public void Cross2D_ParallelVectors_ReturnsZero()
    {
        float result = GameMath.Cross2D(new Vector2(1, 0), new Vector2(2, 0));
        Assert.That(result, Is.EqualTo(0f));
    }

    [Test]
    public void Dot_SameDirection_ReturnsPositive()
    {
        float result = GameMath.Dot(new Vector2(1, 0), new Vector2(1, 0));
        Assert.That(result, Is.EqualTo(1f).Within(0.0001f));
    }

    [Test]
    public void Distance_KnownPoints_ReturnsExpected()
    {
        // 3-4-5 triangle.
        float result = GameMath.Distance(new Vector2(0, 0), new Vector2(3, 4));
        Assert.That(result, Is.EqualTo(5f).Within(0.0001f));
    }

    [Test]
    public void IsWithinCone_TargetStraightAhead_ReturnsTrue()
    {
        bool result = GameMath.IsWithinCone(new Vector2(1, 0), new Vector2(1, 0), 30f);
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsWithinCone_TargetBehind_ReturnsFalse()
    {
        bool result = GameMath.IsWithinCone(new Vector2(1, 0), new Vector2(-1, 0), 30f);
        Assert.That(result, Is.False);
    }

    [Test]
    public void SafeNormalize_ZeroVector_ReturnsZeroNotNaN()
    {
        Vector2 result = GameMath.SafeNormalize(Vector2.Zero);
        Assert.That(result, Is.EqualTo(Vector2.Zero));
        Assert.That(float.IsNaN(result.X), Is.False);
    }

    [Test]
    public void CirclesOverlap_Overlapping_ReturnsTrue()
    {
        bool result = GameMath.CirclesOverlap(new Vector2(0, 0), 10f, new Vector2(5, 0), 10f);
        Assert.That(result, Is.True);
    }

    [Test]
    public void CirclesOverlap_FarApart_ReturnsFalse()
    {
        bool result = GameMath.CirclesOverlap(new Vector2(0, 0), 10f, new Vector2(100, 0), 10f);
        Assert.That(result, Is.False);
    }
}