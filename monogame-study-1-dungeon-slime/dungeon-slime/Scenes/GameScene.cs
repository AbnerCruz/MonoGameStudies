using System;
using DungeonSlime.GameObjects;
using DungeonSlime.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;

namespace DungeonSlime.Scenes;

public class GameScene : Scene
{
    private enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    private Slime _slime;

    private Bat _bat;

    private Tilemap _tilemap;

    private Rectangle _roomBounds;

    private SoundEffect _collectSoundEffect;

    private int _score;

    private GameSceneUI _ui;

    private GameState _state;

    private Effect _grayscaleEffect;

    private float _saturation = 1;

    private const float FADE_SPEED = 0.02f;

    public override void Initialize()
    {
        base.Initialize();

        Core.ExitOnEscape = false;

        _roomBounds = Core.GraphicsDevice.PresentationParameters.Bounds;
        _roomBounds.Inflate(-_tilemap.TileWidth, -_tilemap.TileHeight);

        _slime.BodyCollision += OnSlimeBodyCollision;

        GumService.Default.Root.Children.Clear();

        InitializeUI();

        InitializeNewGame();
    }

    private void InitializeUI()
    {
        GumService.Default.Root.Children.Clear();

        _ui = new GameSceneUI();

        _ui.ResumeButtonClick += OnResumeButtonClicked;
        _ui.RetryButtonClick += OnRetryButtonClicked;
        _ui.QuitButtonClick += OnQuitButtonClicked;
    }

    private void OnResumeButtonClicked(object sender, EventArgs args)
    {
        _state = GameState.Playing;
    }

    private void OnRetryButtonClicked(object sender, EventArgs args)
    {
        InitializeNewGame();
    }

    private void OnQuitButtonClicked(object sender, EventArgs args)
    {
        Core.ChangeScene(new TitleScene());
    }

    private void InitializeNewGame()
    {
        Vector2 slimePos = new Vector2();
        slimePos.X = (_tilemap.Columns / 2) * _tilemap.TileWidth;
        slimePos.Y = (_tilemap.Rows / 2) * _tilemap.TileHeight;

        _slime.Initialize(slimePos, _tilemap.TileWidth);

        _bat.RandomizeVelocity();
        PositionBatAwayFromSlime();

        _score = 0;

        _state = GameState.Playing;
    }

    public override void LoadContent()
    {
        TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");

        _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
        _tilemap.Scale = new Vector2(4.0f, 4.0f);

        AnimatedSprite slimeAnimation = atlas.CreateAnimatedSprite("slime-animation");
        slimeAnimation.Scale = new Vector2(4.0f, 4.0f);

        _slime = new Slime(slimeAnimation);

        AnimatedSprite batAnimation = atlas.CreateAnimatedSprite("bat-animation");
        batAnimation.Scale = new Vector2(4.0f, 4.0f);

        SoundEffect bounceSoundEffect = Content.Load<SoundEffect>("audio/bounce");

        _bat = new Bat(batAnimation, bounceSoundEffect);

        _collectSoundEffect = Content.Load<SoundEffect>("audio/collect");

        _grayscaleEffect = Content.Load<Effect>("effects/grayscaleEffect");
    }

    public override void Update(GameTime gameTime)
    {
        _ui.Update(gameTime);

        if (_state != GameState.Playing)
        {
            _saturation = Math.Max(0f, _saturation - FADE_SPEED);

            if (_state == GameState.GameOver)
            {
                return;
            }
        }

        if (_state == GameState.GameOver)
        {
            return;
        }

        if (GameController.Pause())
        {
            TogglePause();
        }

        if (_state == GameState.Paused)
        {
            return;
        }

        _slime.Update(gameTime);

        _bat.Update(gameTime);

        CollisionChecks();
    }

    private void CollisionChecks()
    {
        Circle slimeBounds = _slime.GetBounds();
        Circle batBounds = _bat.GetBounds();

        if (slimeBounds.Intersects(batBounds))
        {
            PositionBatAwayFromSlime();

            _bat.RandomizeVelocity();

            _slime.Grow();

            _score += 100;

            _ui.UpdateScoreText(_score);

            Core.Audio.PlaySoundEffect(_collectSoundEffect);
        }

        if (slimeBounds.Top < _roomBounds.Top ||
        slimeBounds.Bottom > _roomBounds.Bottom ||
        slimeBounds.Left < _roomBounds.Left ||
        slimeBounds.Right > _roomBounds.Right)
        {
            GameOver();
            return;
        }

        if (batBounds.Top <= _roomBounds.Top)
        {
            _bat.Bounce(Vector2.UnitY);
        }
        else if (batBounds.Bottom >= _roomBounds.Bottom)
        {
            _bat.Bounce(-Vector2.UnitY);
        }

        if (batBounds.Left <= _roomBounds.Left)
        {
            _bat.Bounce(Vector2.UnitX);
        }
        else if (batBounds.Right >= _roomBounds.Right)
        {
            _bat.Bounce(-Vector2.UnitX);
        }
    }

    private void PositionBatAwayFromSlime()
    {
        float roomCenterX = _roomBounds.X + _roomBounds.Width * 0.5f;
        float roomCenterY = _roomBounds.Y + _roomBounds.Height * 0.5f;
        Vector2 roomCenter = new Vector2(roomCenterX, roomCenterY);

        Circle slimeBounds = _slime.GetBounds();
        Vector2 slimeCenter = new Vector2(slimeBounds.X, slimeBounds.Y);

        Vector2 centerToSlime = slimeCenter - roomCenter;

        Circle batBounds = _bat.GetBounds();

        int padding = batBounds.Radius * 2;

        Vector2 newBatPosition = Vector2.Zero;
        if (Math.Abs(centerToSlime.X) > Math.Abs(centerToSlime.Y))
        {
            newBatPosition.Y = Random.Shared.Next(
                _roomBounds.Top + padding,
                _roomBounds.Bottom - padding
            );

            if (centerToSlime.X > 0)
            {
                newBatPosition.X = _roomBounds.Left + padding;
            }
            else
            {
                newBatPosition.X = _roomBounds.Right - padding * 2;
            }
        }
        else
        {
            newBatPosition.X = Random.Shared.Next(
                _roomBounds.Left + padding,
                _roomBounds.Right - padding
            );

            if (centerToSlime.Y > 0)
            {
                newBatPosition.Y = _roomBounds.Top + padding;
            }
            else
            {
                newBatPosition.Y = _roomBounds.Bottom - padding * 2;
            }
        }

        _bat.Position = newBatPosition;
    }

    private void OnSlimeBodyCollision(object sender, EventArgs args)
    {
        GameOver();
    }

    private void TogglePause()
    {
        if (_state == GameState.Paused)
        {
            _ui.HidePausePanel();

            _state = GameState.Playing;
        }
        else
        {
            _ui.ShowPausePanel();

            _state = GameState.Paused;

            _saturation = 1f;
        }
    }

    private void GameOver()
    {
        _ui.ShowGameOverPanel();

        _state = GameState.GameOver;

        _saturation = 1f;
    }

    public override void Draw(GameTime gameTime)
    {
        Core.GraphicsDevice.Clear(Color.CornflowerBlue);

        if (_state != GameState.Playing)
        {
            _grayscaleEffect.Parameters["Saturation"].SetValue(_saturation);

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _grayscaleEffect);
        }
        else
        {
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }

        _tilemap.Draw(Core.SpriteBatch);

        _slime.Draw();

        _bat.Draw();

        Core.SpriteBatch.End();

        _ui.Draw();
    }

    

}
