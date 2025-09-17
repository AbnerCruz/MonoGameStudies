using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MyLibrary;

public class InputManager
{

    static float smoothTransition = 0.075f;
    static float xAxis = 0;
    static float yAxis = 0;
    public static float HorizontalAxis(bool smooth = false)
    {
        KeyboardState keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Left))
        {
            if (smooth && xAxis > -1)
            {
                xAxis -= smoothTransition;
            }
            else
            {
                xAxis = -1;
            }
        }
        else if (keyboard.IsKeyDown(Keys.Right))
        {
            if (smooth && xAxis < 1)
            {
                xAxis += smoothTransition;
            }
            else
            {
                xAxis = 1;
            }
        }
        else
        {
            if (smooth)
            {
                if (xAxis > 0) xAxis = Math.Max(xAxis - smoothTransition, 0f);
                else if (xAxis < 0) xAxis = Math.Min(xAxis + smoothTransition, 0f);
            }
            else
            {
                xAxis = 0;
            }
        }

        // Better way to implement
        // if (keyboard.IsKeyDown(Keys.Left))
        //     xAxis = smooth ? Math.Max(xAxis - smoothTransition, -1f) : -1f;
        // else if (keyboard.IsKeyDown(Keys.Right))
        //     xAxis = smooth ? Math.Min(xAxis + smoothTransition, 1f) : 1f;
        // else
        //     xAxis = smooth ? MathF.Abs(xAxis) < smoothTransition ? 0f : xAxis - MathF.CopySign(smoothTransition, xAxis) : 0f;


        return xAxis;
    }

    public static float VerticalAxis(bool smooth = false)
    {
        KeyboardState keyboard = Keyboard.GetState();
        if (keyboard.IsKeyDown(Keys.Up))
        {
            if (smooth && yAxis > -1)
            {
                yAxis -= smoothTransition;
            }
            else
            {
                yAxis = -1;
            }
        }
        else if (keyboard.IsKeyDown(Keys.Down))
        {
            if (smooth && yAxis < 1)
            {
                yAxis += smoothTransition;
            }
            else
            {
                yAxis = 1;
            }
        }
        else
        {
            if (smooth)
            {
                if (yAxis > 0) yAxis = Math.Max(yAxis - smoothTransition, 0f);
                else if (yAxis < 0) yAxis = Math.Min(yAxis + smoothTransition, 0f);
            }
            else
            {
                yAxis = 0;
            }
        }
        return yAxis;
    }
}