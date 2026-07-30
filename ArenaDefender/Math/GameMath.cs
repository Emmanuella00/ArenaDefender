using Microsoft.Xna.Framework;

namespace ArenaDefender.Math;

/// <summary>
/// Pure mathematical helper functions used across the game.
/// Contains no rendering or engine dependencies, so every method here is unit-testable.
/// </summary>
public static class GameMath
{
    /// <summary>
    /// Returns the 2D cross product (z-component of the 3D cross product) of two vectors.
    /// The sign indicates which side <paramref name="b"/> lies relative to <paramref name="a"/>,
    /// which is used to decide whether an enemy should turn clockwise or counter-clockwise toward a target.
    /// </summary>
    public static float Cross2D(Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    /// <summary>
    /// Returns the dot product of two vectors. For normalised vectors this equals the cosine
    /// of the angle between them, which is used for the player's aim-assist cone.
    /// </summary>
    public static float Dot(Vector2 a, Vector2 b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    /// <summary>
    /// Returns the straight-line distance between two points, used for pickup range and enemy detection.
    /// </summary>
    public static float Distance(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b);
    }

    /// <summary>
    /// Determines whether <paramref name="toTarget"/> falls within a cone of the given half-angle
    /// around <paramref name="facing"/>. Used for aim assistance and enemy field-of-view checks.
    /// </summary>
    /// <param name="facing">The direction the entity is facing (need not be normalised).</param>
    /// <param name="toTarget">The direction from the entity to the target (need not be normalised).</param>
    /// <param name="halfAngleDegrees">Half the width of the cone, in degrees.</param>
    public static bool IsWithinCone(Vector2 facing, Vector2 toTarget, float halfAngleDegrees)
    {
        Vector2 f = SafeNormalize(facing);
        Vector2 t = SafeNormalize(toTarget);
        float cosThreshold = (float)System.Math.Cos(MathHelper.ToRadians(halfAngleDegrees));
        return Dot(f, t) >= cosThreshold;
    }

    /// <summary>
    /// Normalises a vector to unit length, returning <see cref="Vector2.Zero"/> for a zero-length
    /// input instead of producing NaN. This guards the case where the mouse sits exactly on the player.
    /// </summary>
    public static Vector2 SafeNormalize(Vector2 v)
    {
        if (v.LengthSquared() < 0.0000001f)
            return Vector2.Zero;

        v.Normalize();
        return v;
    }

    /// <summary>
    /// Linearly interpolates between <paramref name="from"/> and <paramref name="to"/> by amount
    /// <paramref name="t"/>, clamped to the range [0, 1]. Used for smooth health-bar and UI animation.
    /// </summary>
    public static float Lerp(float from, float to, float t)
    {
        t = MathHelper.Clamp(t, 0f, 1f);
        return from + (to - from) * t;
    }

    /// <summary>
    /// Returns true if two circles overlap: the distance between their centres
    /// is less than the sum of their radii. Used for all in-game collision checks.
    /// </summary>
    public static bool CirclesOverlap(Vector2 aPos, float aRadius, Vector2 bPos, float bRadius)
    {
        float combined = aRadius + bRadius;
        return (bPos - aPos).LengthSquared() <= combined * combined;
    }
}
