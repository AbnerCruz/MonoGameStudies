using Android.App;
using Android.OS;
using MobileForge.EditorRuntime;

namespace MobileForge.GameTemplate;

[Activity(Label = "Meu Jogo", MainLauncher = true, Exported = true)]
public sealed class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(new StandaloneGameView(this, new MyGame()));
    }
}
