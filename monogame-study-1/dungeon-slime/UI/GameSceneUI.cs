using System;
using Gum.DataTypes;
using Gum.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using MonoGameGum;
using MonoGameGum.Forms.Controls;
using MonoGameGum.GueDeriving;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;

namespace DungeonSlime.UI;

public class GameSceneUI : ContainerRuntime
{
    private static readonly string s_scoreFormat = "SCORE: {0:D6}";

    private SoundEffect _uiSoundEffect;

    private Panel _pausePanel;

    private AnimatedButton _resumeButton;

    private Panel _gameOverPanel;

    private AnimatedButton _retryButton;

    private TextRuntime _scoreText;

    public event EventHandler ResumeButtonClick;
    public event EventHandler QuitButtonClick;
    public event EventHandler RetryButtonClick;

    public GameSceneUI()
    {
        Dock(Gum.Wireframe.Dock.Fill);

        this.AddToRoot();

        ContentManager content = GumService.Default.ContentLoader.XnaContentManager;

        _uiSoundEffect = content.Load<SoundEffect>("audio/ui");
        TextureAtlas atlas = TextureAtlas.FromFile(content, "images/atlas-definition.xml");

        _scoreText = CreateScoreText();
        AddChild(_scoreText);

        _pausePanel = CreatePausePanel(atlas);
        AddChild(_pausePanel.Visual);

        _gameOverPanel = CreateGameOverPanel(atlas);
        AddChild(_gameOverPanel.Visual);
    }

    private TextRuntime CreateScoreText()
    {
        TextRuntime text = new TextRuntime();
        text.Anchor(Gum.Wireframe.Anchor.TopLeft);
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.X = 20.0f;
        text.Y = 5.0f;
        text.UseCustomFont = true;
        text.CustomFontFile = @"fonts/04b_30.fnt";
        text.FontScale = 0.25f;
        text.Text = string.Format(s_scoreFormat, 0);

        return text;
    }

    private Panel CreatePausePanel(TextureAtlas atlas)
    {
        Panel panel = new Panel();
        panel.Anchor(Gum.Wireframe.Anchor.Center);
        panel.Visual.WidthUnits = DimensionUnitType.Absolute;
        panel.Visual.HeightUnits = DimensionUnitType.Absolute;
        panel.Visual.Width = 264.0f;
        panel.Visual.Height = 70.0f;
        panel.IsVisible = false;

        TextureRegion backgroundRegion = atlas.GetRegion("panel-background");

        NineSliceRuntime background = new NineSliceRuntime();
        background.Dock(Gum.Wireframe.Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureWidth = backgroundRegion.Width;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        panel.AddChild(background);

        TextRuntime text = new TextRuntime();
        text.Text = "PAUSED";
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.FontScale = 0.5f;
        text.X = 10.0f;
        text.Y = 10.0f;
        panel.AddChild(text);

        _resumeButton = new AnimatedButton(atlas);
        _resumeButton.Text = "RESUME";
        _resumeButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _resumeButton.Visual.X = 9.0f;
        _resumeButton.Visual.Y = -9.0f;

        _resumeButton.Click += OnResumeButtonClicked;
        _resumeButton.GotFocus += OnElementGotFocus;

        panel.AddChild(_resumeButton);

        AnimatedButton quitButton = new AnimatedButton(atlas);
        quitButton.Text = "QUIT";
        quitButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
        quitButton.Visual.X = -9.0f;
        quitButton.Visual.Y = -9.0f;

        quitButton.Click += OnQuitButtonClicked;
        quitButton.GotFocus += OnElementGotFocus;

        panel.AddChild(quitButton);

        return panel;
    }

    private Panel CreateGameOverPanel(TextureAtlas atlas)
    {
        Panel panel = new Panel();
        panel.Anchor(Gum.Wireframe.Anchor.Center);
        panel.Visual.WidthUnits = DimensionUnitType.Absolute;
        panel.Visual.HeightUnits = DimensionUnitType.Absolute;
        panel.Visual.Width = 264.0f;
        panel.Visual.Height = 70.0f;
        panel.IsVisible = false;

        TextureRegion backgroundRegion = atlas.GetRegion("panel-background");

        NineSliceRuntime background = new NineSliceRuntime();
        background.Dock(Gum.Wireframe.Dock.Fill);
        background.Texture = backgroundRegion.Texture;
        background.TextureAddress = TextureAddress.Custom;
        background.TextureHeight = backgroundRegion.Height;
        background.TextureWidth = backgroundRegion.Width;
        background.TextureTop = backgroundRegion.SourceRectangle.Top;
        background.TextureLeft = backgroundRegion.SourceRectangle.Left;
        panel.AddChild(background);

        TextRuntime text = new TextRuntime();
        text.Text = "GAME OVER";
        text.WidthUnits = DimensionUnitType.RelativeToChildren;
        text.UseCustomFont = true;
        text.CustomFontFile = "fonts/04b_30.fnt";
        text.FontScale = 0.5f;
        text.X = 10.0f;
        text.Y = 10.0f;
        panel.AddChild(text);

        _retryButton = new AnimatedButton(atlas);
        _retryButton.Text = "RETRY";
        _retryButton.Anchor(Gum.Wireframe.Anchor.BottomLeft);
        _retryButton.Visual.X = 9.0f;
        _retryButton.Visual.Y = -9.0f;

        _retryButton.Click += OnRetryButtonClicked;
        _retryButton.GotFocus += OnElementGotFocus;

        panel.AddChild(_retryButton);

        AnimatedButton quitButton = new AnimatedButton(atlas);
        quitButton.Text = "QUIT";
        quitButton.Anchor(Gum.Wireframe.Anchor.BottomRight);
        quitButton.Visual.X = -9.0f;
        quitButton.Visual.Y = -9.0f;

        quitButton.Click += OnQuitButtonClicked;
        quitButton.GotFocus += OnElementGotFocus;

        panel.AddChild(quitButton);

        return panel;
    }

    private void OnResumeButtonClicked(object sender, EventArgs args)
    {
        Core.Audio.PlaySoundEffect(_uiSoundEffect);

        HidePausePanel();

        if (ResumeButtonClick != null)
        {
            ResumeButtonClick(sender, args);
        }
    }

    private void OnRetryButtonClicked(object sender, EventArgs args)
    {
        Core.Audio.PlaySoundEffect(_uiSoundEffect);

        HideGameOverPanel();

        if (RetryButtonClick != null)
        {
            RetryButtonClick(sender, args);
        }
    }

    private void OnQuitButtonClicked(object sender, EventArgs args)
    {
        Core.Audio.PlaySoundEffect(_uiSoundEffect);

        HidePausePanel();
        HideGameOverPanel();

        if (QuitButtonClick != null)
        {
            QuitButtonClick(sender, args);
        }
    }

    private void OnElementGotFocus(object sender, EventArgs args)
    {
        Core.Audio.PlaySoundEffect(_uiSoundEffect);
    }

    public void UpdateScoreText(int score)
    {
        _scoreText.Text = string.Format(s_scoreFormat, score);
    }

    public void ShowPausePanel()
    {
        _pausePanel.IsVisible = true;

        _resumeButton.IsFocused = true;

        _gameOverPanel.IsVisible = false;
    }

    public void HidePausePanel()
    {
        _pausePanel.IsVisible = false;
    }

    public void ShowGameOverPanel()
    {
        _gameOverPanel.IsVisible = true;

        _retryButton.IsFocused = true;

        _pausePanel.IsVisible = false;
    }

    public void HideGameOverPanel()
    {
        _gameOverPanel.IsVisible = false;
    }

    public void Update(GameTime gameTime)
    {
        GumService.Default.Update(gameTime);
    }

    public void Draw()
    {
        GumService.Default.Draw();
    }
    
    
}
