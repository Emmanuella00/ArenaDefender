using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArenaDefender.Rendering;

/// <summary>
/// Draws simple primitives from a single 1x1 white texture, keeping all low-level
/// drawing in one place so entities never touch SpriteBatch directly.
/// </summary>
public class ShapeRenderer
{
    private readonly Texture2D _pixel;

    /// <summary>Creates the renderer and its 1x1 white source texture.</summary>
    /// <exception cref="ArgumentNullException">Thrown if the graphics device is null.</exception>
    public ShapeRenderer(GraphicsDevice graphicsDevice)
    {
        if (graphicsDevice == null)
            throw new ArgumentNullException(nameof(graphicsDevice));

        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    /// <summary>Draws a filled rectangle.</summary>
    public void FillRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        spriteBatch.Draw(_pixel, rect, color);
    }

    /// <summary>Draws a filled circle centred on <paramref name="center"/>. Guards against NaN input.</summary>
    public void FillCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
    {
        if (float.IsNaN(center.X) || float.IsNaN(center.Y) || float.IsNaN(radius))
            return;

        int r = (int)radius;
        if (r <= 0) return;

        for (int y = -r; y <= r; y++)
        {
            double under = (double)radius * radius - (double)y * y;
            if (under < 0) under = 0;
            int halfWidth = (int)System.Math.Sqrt(under);

            var row = new Rectangle(
                (int)(center.X - halfWidth),
                (int)(center.Y + y),
                halfWidth * 2,
                1);
            spriteBatch.Draw(_pixel, row, color);
        }
    }

    /// <summary>Draws a line between two points with the given thickness. Guards against NaN input.</summary>
    public void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness = 2f)
    {
        if (float.IsNaN(start.X) || float.IsNaN(start.Y) || float.IsNaN(end.X) || float.IsNaN(end.Y))
            return;

        Vector2 edge = end - start;
        float length = edge.Length();
        float angle = (float)System.Math.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(
            _pixel,
            new Rectangle((int)start.X, (int)start.Y, (int)length, (int)thickness),
            null, color, angle, Vector2.Zero, SpriteEffects.None, 0f);
    }

    /// <summary>Draws a filled triangle pointing along <paramref name="rotation"/> radians. Used for the player ship.</summary>
    public void FillTriangle(SpriteBatch spriteBatch, Vector2 center, float size, float rotation, Color color)
    {
        if (float.IsNaN(center.X) || float.IsNaN(center.Y) || float.IsNaN(rotation))
            return;

        Vector2 tip = new Vector2(size, 0);
        Vector2 left = new Vector2(-size * 0.7f, -size * 0.7f);
        Vector2 right = new Vector2(-size * 0.7f, size * 0.7f);

        tip = RotateAndTranslate(tip, rotation, center);
        left = RotateAndTranslate(left, rotation, center);
        right = RotateAndTranslate(right, rotation, center);

        DrawLine(spriteBatch, tip, left, color, 3f);
        DrawLine(spriteBatch, left, right, color, 3f);
        DrawLine(spriteBatch, right, tip, color, 3f);
    }

    private static Vector2 RotateAndTranslate(Vector2 point, float rotation, Vector2 center)
    {
        float cos = (float)System.Math.Cos(rotation);
        float sin = (float)System.Math.Sin(rotation);
        return new Vector2(
            point.X * cos - point.Y * sin + center.X,
            point.X * sin + point.Y * cos + center.Y);
    }
}