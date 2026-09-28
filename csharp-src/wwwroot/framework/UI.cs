using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace MobileForge
{
    public readonly struct UIEvent
    {
        public readonly string Name, Id, Value;
        public UIEvent(string name, string id, string value) { Name = name; Id = id; Value = value; }
        public float Number => float.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : 0;
    }

    public sealed class UIElementState
    {
        public string? Text { get; set; }
        public string? Value { get; set; }
        public bool? Visible { get; set; }
    }

    // Declarative HTML/CSS overlay. Methods and callbacks run on the C# game loop.
    public static class UI
    {
        static readonly Dictionary<string, Action<UIEvent>> callbacks = new(StringComparer.Ordinal);
        static readonly Dictionary<string, string> values = new(StringComparer.Ordinal);
        internal static readonly Dictionary<string, UIElementState> Elements = new(StringComparer.Ordinal);
        static readonly HashSet<string> held = new(StringComparer.Ordinal);
        public static string Markup { get; private set; } = "";
        public static UIEvent Event { get; private set; }
        public static void Set(string markup) => Markup = markup ?? "";
        public static void On(string name, Action<UIEvent> callback) => callbacks[name] = callback;
        public static void On(string name, Action callback) => callbacks[name] = _ => callback();
        public static void Off(string name) => callbacks.Remove(name);
        public static bool Held(string name) => held.Contains(name);
        public static string Value(string id) => values.TryGetValue(id, out var value) ? value : "";
        public static float Number(string id) => float.TryParse(Value(id), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : 0;
        static UIElementState Element(string id)
        {
            if (!Elements.TryGetValue(id, out var state)) Elements[id] = state = new UIElementState();
            return state;
        }
        // Update individual elements without rebuilding markup or interrupting a slider.
        public static void SetText(string id, string text) => Element(id).Text = text;
        public static void SetValue(string id, string value) { Element(id).Value = value; values[id] = value; }
        public static void SetVisible(string id, bool visible) => Element(id).Visible = visible;
        internal static void Reset() { Markup = ""; callbacks.Clear(); values.Clear(); held.Clear(); Elements.Clear(); Event = default; }
        internal static void Bind(object owner)
        {
            foreach (var method in owner.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var args = method.GetParameters();
                if (method.ReturnType != typeof(void) || method.IsSpecialName) continue;
                if (args.Length == 0) callbacks[method.Name] = _ => method.Invoke(owner, null);
                else if (args.Length == 1 && args[0].ParameterType == typeof(UIEvent)) callbacks[method.Name] = e => method.Invoke(owner, new object[] { e });
            }
        }
        internal static void Receive(JsonElement frame)
        {
            held.Clear();
            if (frame.TryGetProperty("uiHeld", out var pressed))
                foreach (var item in pressed.EnumerateArray()) if (item.GetString() is string name) held.Add(name);
            if (!frame.TryGetProperty("uiEvents", out var events)) return;
            foreach (var item in events.EnumerateArray())
            {
                var name = item.GetProperty("name").GetString() ?? "";
                var id = item.GetProperty("id").GetString() ?? "";
                var value = item.GetProperty("value").GetString() ?? "";
                if (id.Length > 0) values[id] = value;
                Event = new UIEvent(name, id, value);
                if (callbacks.TryGetValue(name, out var action)) action(Event);
            }
        }
    }
}
