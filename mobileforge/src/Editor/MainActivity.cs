using Android.App;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using MobileForge.Core;

namespace MobileForge.Editor;

[Activity(Label = "MobileForge", MainLauncher = true, Exported = true)]
public sealed class MainActivity : Activity
{
    private const string FileName = "Game.cs";
    private EditText editor = null!;
    private TextView status = null!;
    private GamePreview preview = null!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        var root = new LinearLayout(this) { Orientation = Orientation.Vertical };
        root.SetPadding(12, 12, 12, 12);
        root.SetBackgroundColor(Android.Graphics.Color.Rgb(18, 20, 29));

        var title = new TextView(this) { Text = "Game.cs", TextSize = 20 };
        title.SetTextColor(Android.Graphics.Color.White);
        root.AddView(title);

        editor = new EditText(this)
        {
            Text = File.Exists(Path.Combine(FilesDir!.AbsolutePath, FileName))
                ? File.ReadAllText(Path.Combine(FilesDir.AbsolutePath, FileName)) : StarterCode.Source,
            TextSize = 14,
            Gravity = GravityFlags.Top,
            InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextFlagMultiLine |
                        Android.Text.InputTypes.TextFlagNoSuggestions
        };
        editor.SetTextColor(Android.Graphics.Color.Rgb(222, 230, 242));
        editor.SetBackgroundColor(Android.Graphics.Color.Rgb(32, 36, 48));
        root.AddView(editor, new LinearLayout.LayoutParams(-1, 0, 1));

        var actions = new LinearLayout(this) { Orientation = Orientation.Horizontal };
        var play = new Button(this) { Text = "Jogar" };
        var save = new Button(this) { Text = "Salvar" };
        var export = new Button(this) { Text = "Exportar fonte" };
        actions.AddView(play, new LinearLayout.LayoutParams(0, -2, 1));
        actions.AddView(save, new LinearLayout.LayoutParams(0, -2, 1));
        actions.AddView(export, new LinearLayout.LayoutParams(0, -2, 1.5f));
        root.AddView(actions);

        status = new TextView(this) { Text = "Edite o C# e toque em Jogar.", TextSize = 13 };
        status.SetTextColor(Android.Graphics.Color.Rgb(180, 197, 220));
        root.AddView(status);
        preview = new GamePreview(this);
        root.AddView(preview, new LinearLayout.LayoutParams(-1, 0, 1));
        SetContentView(root);

        save.Click += (_, _) => Save();
        play.Click += (_, _) =>
        {
            Save();
            (GetSystemService(InputMethodService) as InputMethodManager)?.HideSoftInputFromWindow(editor.WindowToken, HideSoftInputFlags.None);
            status.Text = "Compilando...";
            // Compile away from the UI thread; only change Android views on the UI thread.
            var source = editor.Text ?? "";
            _ = Task.Run(() =>
            {
                try
                {
                    var result = ScriptCompiler.Compile(source);
                    RunOnUiThread(() =>
                    {
                        status.Text = result.message;
                        if (result.game is not null) preview.SetGame(result.game);
                    });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() => status.Text = $"Compilação indisponível: {ex.Message}");
                }
            });
        };
        export.Click += (_, _) =>
        {
            Save();
            var intent = new Android.Content.Intent(Android.Content.Intent.ActionCreateDocument);
            intent.AddCategory(Android.Content.Intent.CategoryOpenable);
            intent.SetType("text/plain");
            intent.PutExtra(Android.Content.Intent.ExtraTitle, FileName);
            StartActivityForResult(intent, 100);
        };
    }

    private void Save()
    {
        File.WriteAllText(Path.Combine(FilesDir!.AbsolutePath, FileName), editor.Text ?? "");
        status.Text = "Salvo no aparelho.";
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Android.Content.Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode != 100 || resultCode != Result.Ok || data?.Data is null) return;
        using var stream = ContentResolver!.OpenOutputStream(data.Data);
        if (stream is null) { status.Text = "Não foi possível exportar."; return; }
        using var writer = new StreamWriter(stream);
        writer.Write(editor.Text ?? "");
        status.Text = "Código C# exportado.";
    }
}
