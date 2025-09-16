using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;
using MonoGameLibrary.Input;

namespace DungeonSlime;

public static class GameController
{
    private static KeyboardInfo s_keyboard => Core.Input.Keyboard;
    private static GamePadInfo s_gamePad => Core.Input.GamePads[(int)PlayerIndex.One];

    public static bool MoveUp()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Up) || s_keyboard.WasKeyJustPressed(Keys.W) || s_gamePad.WasButtonJustPressed(Buttons.DPadUp) || s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickUp);
    }

    public static bool MoveDown()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Down) || s_keyboard.WasKeyJustPressed(Keys.S) || s_gamePad.WasButtonJustPressed(Buttons.DPadDown) || s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickDown);
    }

    public static bool MoveLeft()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Left) || s_keyboard.WasKeyJustPressed(Keys.A) || s_gamePad.WasButtonJustPressed(Buttons.DPadLeft) || s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickLeft);
    }

    public static bool MoveRight()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Right) || s_keyboard.WasKeyJustPressed(Keys.D) || s_gamePad.WasButtonJustPressed(Buttons.DPadRight) || s_gamePad.WasButtonJustPressed(Buttons.LeftThumbstickRight);
    }

    public static bool Pause()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Escape) || s_gamePad.WasButtonJustPressed(Buttons.Start);
    }

    public static bool Action()
    {
        return s_keyboard.WasKeyJustPressed(Keys.Enter) || s_gamePad.WasButtonJustPressed(Buttons.A);
    }

    
}