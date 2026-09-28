using System;
using System.Collections.Generic;
using System.Text.Json;
namespace MobileForge
{
    public abstract class Game
    {
        public virtual void Start() { }
        public virtual void Update(float dt) { }
        public virtual void Draw() { }
        public int Width => 360;
        public int Height => 640;
        public void Clear(string color = "#101827") => Graphics.Clear(color);
    }
    public static class Input
    {
        public static float X { get; internal set; }
        public static float Y { get; internal set; }
        public static bool Down { get; internal set; }
        internal static HashSet<string> Keys = new();
        public static bool Key(string name) => Keys.Contains(name);
    }
    public static class Graphics
    {
        internal static List<object> Commands = new();
        static void Add(object value)
        {
            if (Commands.Count >= 10000) throw new InvalidOperationException("Limite de 10.000 comandos de desenho por quadro.");
            Commands.Add(value);
        }
        public static void Clear(string color = "#101827") => Add(new { type = "clear", color });
        public static void Rect(float x, float y, float width, float height, string color = "#ffffff")
            => Add(new { type = "rect", x, y, width, height, color });
        public static void Circle(float x, float y, float radius, string color = "#ffffff")
            => Add(new { type = "circle", x, y, radius, color });
        public static void Text(string text, float x, float y, float size = 20, string color = "#ffffff")
            => Add(new { type = "text", text, x, y, size, color });
        public static void Sprite(string name, float x, float y, float scale = 1)
            => Add(new { type = "sprite", name, x, y, scale });
        public static void Tile(string sheet, int index, float x, float y, float scale = 1)
            => Add(new { type = "tile", sheet, index, x, y, scale });
        public static void Tilemap(string name, float x, float y, float scale = 1)
            => Add(new { type = "tilemap", name, x, y, scale });
    }
    public static class __Entry
    {
        static global::MainGame game = null!;
        public static void Start()
        {
            Input.Down = false;
            Input.Keys.Clear();
            Graphics.Commands.Clear();
            UI.Reset();
            game = new global::MainGame();
            UI.Bind(game);
            game.Start();
        }
        public static string Frame(string json)
        {
            using var document = JsonDocument.Parse(json);
            var data = document.RootElement;
            Input.X = data.GetProperty("x").GetSingle();
            Input.Y = data.GetProperty("y").GetSingle();
            Input.Down = data.GetProperty("down").GetBoolean();
            Input.Keys.Clear();
            foreach (var key in data.GetProperty("keys").EnumerateArray()) Input.Keys.Add(key.GetString()!);
            UI.Receive(data);
            Graphics.Commands.Clear();
            game.Update(Math.Clamp(data.GetProperty("dt").GetSingle(), 0, 0.05f));
            game.Draw();
            return JsonSerializer.Serialize(new { ok = true, commands = Graphics.Commands, ui = UI.Markup, uiState = UI.Elements });
        }
    }
}
