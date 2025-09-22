using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using MyLibrary.Random;

namespace MyLibrary.InputManager;

public static class Input
{
    public static KeyboardInfo Keyboard = new KeyboardInfo();
    private static float smoothTransition = 0.05f;
    private static float horizontalAxis = 0f;
    private static float verticalAxis = 0f;
    private static Vector2 randomAxis = Vector2.Zero;

    public static void Update(GameTime gameTime)
    {
        Keyboard.Update();
    }

    public static float GetHorizontalAxis(bool smooth = false)
    {
        bool left = InputDirections.Left();
        bool right = InputDirections.Right();
        float x = 0f;

        if (left && !right)
        {
            x = -1;
        }
        else if (right && !left)
        {
            x = 1;
        }
        else
        {
            x = 0;
        }

        horizontalAxis = smooth ? MathHelper.Lerp(horizontalAxis, x, smoothTransition) : x;
        horizontalAxis = Math.Abs(horizontalAxis) < 0.01f ? 0f : horizontalAxis;

        return horizontalAxis;
    }

    public static float GetVerticalAxis(bool smooth = false)
    {
        bool up = InputDirections.Up();
        bool down = InputDirections.Down();
        float y = 0f;

        if (up && !down)
        {
            y = -1;
        }
        else if (down && !up)
        {
            y = 1;
        }
        else
        {
            y = 0;
        }

        verticalAxis = smooth ? MathHelper.Lerp(verticalAxis, y, smoothTransition) : y;
        verticalAxis = Math.Abs(verticalAxis) < 0.01f ? 0f : verticalAxis;

        return verticalAxis;
    }

    public static Vector2 RandomAxis(bool horizontal, bool vertical, bool smooth = false)
    {
        float x = 0f, y = 0f;

        do
        {
            if (horizontal)
            {
                x = Rand.Int(-1, 1);
            }
            if (vertical)
            {
                y = Rand.Int(-1, 1);
            }
        }
        while (x == 0f || y == 0f);
        randomAxis.X = smooth ? MathHelper.Lerp(randomAxis.X, x, smoothTransition) : x;
        randomAxis.X = Math.Abs(randomAxis.X) < 0.01f ? 0f : randomAxis.X;
        randomAxis.Y = smooth ? MathHelper.Lerp(randomAxis.Y, y, smoothTransition) : y;
        randomAxis.Y = Math.Abs(randomAxis.Y) < 0.01f ? 0f : randomAxis.Y;

        return randomAxis;
    }
}


public static class InputDirections
{
    private static KeyboardInfo Keyboard => Input.Keyboard;
    public static bool Up()
    {
        return Keyboard.IsKeyDown(Keys.Up) || Keyboard.IsKeyDown(Keys.W); // || Gamepad inputs
    }

    public static bool Down()
    {
        return Keyboard.IsKeyDown(Keys.Down) || Keyboard.IsKeyDown(Keys.S); // || Gamepad inputs
    }

    public static bool Left()
    {
        return Keyboard.IsKeyDown(Keys.Left) || Keyboard.IsKeyDown(Keys.A);
    }

    public static bool Right()
    {
        return Keyboard.IsKeyDown(Keys.Right) || Keyboard.IsKeyDown(Keys.D);
    }
}

public class KeyboardInfo
{
    public KeyboardState PreviousState { get; set; }
    public KeyboardState CurrentState { get; set; }

    public KeyboardInfo()
    {
        PreviousState = new KeyboardState();
        CurrentState = Keyboard.GetState();
    }

    public void Update()
    {
        PreviousState = CurrentState;
        CurrentState = Keyboard.GetState();
    }

    public bool IsKeyDown(Keys key)
    {
        return CurrentState.IsKeyDown(key);
    }

    public bool IsKeyUp(Keys key)
    {
        return CurrentState.IsKeyUp(key);
    }

    public bool JustPressed(Keys key)
    {
        return CurrentState.IsKeyDown(key) && PreviousState.IsKeyUp(key);
    }

    public bool JustReleased(Keys key)
    {
        return CurrentState.IsKeyUp(key) && PreviousState.IsKeyDown(key);
    }
}