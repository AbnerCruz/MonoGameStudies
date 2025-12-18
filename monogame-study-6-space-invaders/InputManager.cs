using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace InputManager;

public static class Input
{
    public static InputInfo Keyboard = new InputInfo();
}

public class InputInfo
{
    public KeyboardState lastKeyState { get; private set; }
    public KeyboardState currentKeyState { get; private set; }

    public InputInfo()
    {
        lastKeyState = new();
        currentKeyState = Keyboard.GetState();
    }

    public void Update()
    {
        lastKeyState = currentKeyState;
        currentKeyState = Keyboard.GetState();
    }

    public bool KeyPressed(Keys key)
    {
        return currentKeyState.IsKeyDown(key);
    }

    public bool KeyReleased(Keys key)
    {
        return currentKeyState.IsKeyUp(key);
    }

    public bool KeyJustPressed(Keys key)
    {
        return currentKeyState.IsKeyDown(key) && lastKeyState.IsKeyUp(key);
    }
}