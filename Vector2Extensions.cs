using System.Numerics;

public static class Vector2Extensions
{
    public static Vector2 Rotate(this Vector2 vector, float angle)
    {
        float sin = MathF.Sin(angle);
        float cos = MathF.Cos(angle);
        float x = vector.X * cos - vector.Y * sin;
        float y = vector.X * sin + vector.Y * cos;
        return new Vector2(x, y);
    }
}

