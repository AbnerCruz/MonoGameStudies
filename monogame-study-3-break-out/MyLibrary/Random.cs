using System;

namespace MyLibrary.Random;

public static class Rand
{
    public static readonly System.Random Instance = new System.Random();

    public static float Core()
    {
        return (float)Instance.NextDouble();
    }

    public static float Float()
    {
        return (float)Instance.NextDouble();
    }

    public static float Float(float min, float max)
    {
        return (float)Instance.NextDouble() * (max - min) + min;
    }

    public static float Int()
    {
        return (int)Instance.Next();
    }

    public static int Int(int min, int max)
    {
        return (int)Instance.Next(min, max+1);
    }
}