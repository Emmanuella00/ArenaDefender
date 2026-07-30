using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ArenaDefender.Entities;
using ArenaDefender.Math;
using ArenaDefender.Rendering;
using ArenaDefender.Systems;

namespace ArenaDefender;

/// <summary>The high-level states the game can be in.</summary>
public enum GameState
{
    /// <summary>Title screen with a Play button.</summary>
    Menu,
    /// <summary>Active gameplay.</summary>
    Playing,
    /// <summary>Shown when the player's health reaches zero.</summary>
    GameOver,
    /// <summary>Shown when the player reaches the winning score.</summary>
    Victory
}

/// <summary>
/// Main game class. Owns the MonoGame loop and delegates all rules to the tested logic classes.
/// Handles only input reading, state transitions, and rendering.
/// </summary>
public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private ShapeRenderer _shapes;
    private SpriteFont _font;
    private Texture2D _playerTexture;
    private Texture2D _fastTexture;
    private Texture2D _standardTexture;
    private Texture2D _tankTexture;
    private Texture2D _backgroundTexture;

    private const int ScreenWidth = 1280;
    private const int ScreenHeight = 720;
    private const float PlayerSpeed = 300f;

    private const float BaseFireRate = 0.15f;
    private const float BulletSpeed = 600f;
    private const float BulletDamage = 20f;

    private const int EnemiesPerWave = 15;
    private const float PickupRange = 32f;
    private const double DropChance = 0.30;
    private const float BuffDuration = 6f;
    private const int WinScore = 1000;
    private const float AimAssistAngle = 12f; // half-angle of the aim-assist cone, in degrees

    private GameState _state = GameState.Menu;

    private Player _player;
    private float _playerRotation;

    private readonly List<Projectile> _projectiles = new();
    private float _fireCooldown;

    private readonly List<Enemy> _enemies = new();
    private readonly List<PowerUp> _powerUps = new();
    private readonly Random _random = new();
    private WaveManager _waveManager = new();
    private float _spawnTimer;
    private int _enemiesKilledThisWave;

    private readonly Dictionary<PowerUpType, float> _activeBuffs = new();

    private MouseState _previousMouse;
    private Rectangle _playButton;

    private float _displayedHealth = 100f;   // Lerp #1: eases toward real health for a sliding bar
    private float _screenFade;               // Lerp #3: eases 0->1 to fade end screens in

    // Theme palette.
    private static readonly Color ColBackground = new Color(34, 16, 51);
    private static readonly Color ColNeon = new Color(63, 240, 255);
    private static readonly Color ColHealth = new Color(63, 255, 143);
    private static readonly Color ColBuff = new Color(255, 210, 63);
    private static readonly Color ColPanel = new Color(22, 10, 34);
    private static readonly Color ColPanelBar = new Color(40, 25, 55);
    private static readonly Color ColMuted = new Color(139, 123, 168);


    /// <summary>Sets up the MonoGame graphics device and window size.</summary>
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
    }

    /// <summary>Initialises fixed layout values such as the Play button bounds.</summary>
    protected override void Initialize()
    {
        _playButton = new Rectangle(ScreenWidth / 2 - 120, ScreenHeight / 2 + 40, 240, 70);
        base.Initialize();
    }

    /// <summary>Loads sprites, font, and background; missing assets fall back gracefully.</summary>
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _shapes = new ShapeRenderer(GraphicsDevice);

        _font = TryLoad<SpriteFont>("font");
        _playerTexture = TryLoad<Texture2D>("player");
        _fastTexture = TryLoad<Texture2D>("enemy_fast");
        _standardTexture = TryLoad<Texture2D>("enemy_standard");
        _tankTexture = TryLoad<Texture2D>("enemy_tank");
        _backgroundTexture = TryLoad<Texture2D>("background");
    }

    /// <summary>Loads a content asset, returning default if missing so the game still runs.</summary>
    private T TryLoad<T>(string assetName)
    {
        try { return Content.Load<T>(assetName); }
        catch (Exception) { return default; }
    }

    /// <summary>Resets all game state to start a fresh run.</summary>
    private void StartNewGame()
    {
        _player = new Player(100, new Vector2(ScreenWidth / 2f, ScreenHeight / 2f));
        _projectiles.Clear();
        _enemies.Clear();
        _powerUps.Clear();
        _activeBuffs.Clear();
        _waveManager = new WaveManager();
        _spawnTimer = 0f;
        _fireCooldown = 0f;
        _enemiesKilledThisWave = 0;
        _playerRotation = 0f;
        _displayedHealth = 100f;
        _screenFade = 0f;
        _state = GameState.Playing;
    }

    /// <summary>Reads input each frame and dispatches to the current game state's update.</summary>
    protected override void Update(GameTime gameTime)
    {
        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse = Mouse.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();

        switch (_state)
        {
            case GameState.Menu:
                UpdateMenu(mouse);
                break;
            case GameState.Playing:
                UpdatePlaying(delta, keyboard, mouse);
                break;
            case GameState.GameOver:
            case GameState.Victory:
                UpdateEndScreen(keyboard, delta);
                break;
        }

        _previousMouse = mouse;
        base.Update(gameTime);
    }

    /// <summary>Starts the game when the Play button is clicked.</summary>
    private void UpdateMenu(MouseState mouse)
    {
        bool clicked = mouse.LeftButton == ButtonState.Released
                       && _previousMouse.LeftButton == ButtonState.Pressed;
        if (clicked && _playButton.Contains(mouse.Position))
            StartNewGame();
    }

    /// <summary>Fades the end screen in and restarts the game when R is pressed.</summary>
    private void UpdateEndScreen(KeyboardState keyboard, float delta)
    {
        _screenFade = GameMath.Lerp(_screenFade, 1f, 3f * delta); // Lerp #3
        if (keyboard.IsKeyDown(Keys.R))
            StartNewGame();
    }

    /// <summary>Runs one frame of active gameplay: movement, firing, spawning, collisions, and win/lose checks.</summary>
    private void UpdatePlaying(float delta, KeyboardState keyboard, MouseState mouse)
    {
        UpdateBuffs(delta);

        float speed = PlayerSpeed * (_activeBuffs.ContainsKey(PowerUpType.Speed) ? 1.6f : 1f);

        Vector2 move = Vector2.Zero;
        if (keyboard.IsKeyDown(Keys.W)) move.Y -= 1;
        if (keyboard.IsKeyDown(Keys.S)) move.Y += 1;
        if (keyboard.IsKeyDown(Keys.A)) move.X -= 1;
        if (keyboard.IsKeyDown(Keys.D)) move.X += 1;
        move = GameMath.SafeNormalize(move);
        _player.Position += move * speed * delta;
        _player.Position = new Vector2(
            MathHelper.Clamp(_player.Position.X, 20, ScreenWidth - 20),
            MathHelper.Clamp(_player.Position.Y, 20, ScreenHeight - 20));

        Vector2 toMouse = mouse.Position.ToVector2() - _player.Position;
        if (toMouse.LengthSquared() > 0.0001f)
            _playerRotation = (float)System.Math.Atan2(toMouse.Y, toMouse.X);

        float fireRate = BaseFireRate * (_activeBuffs.ContainsKey(PowerUpType.FireRate) ? 0.45f : 1f);
        _fireCooldown -= delta;
        if (mouse.LeftButton == ButtonState.Pressed && _fireCooldown <= 0f)
        {
            Vector2 aimDir = GameMath.SafeNormalize(toMouse);
            if (aimDir != Vector2.Zero)
            {
                // Aim assist (DOT PRODUCT): if an enemy sits within a narrow cone of the aim
                // direction, snap the shot onto it. IsWithinCone compares directions via the dot product.
                Enemy target = null;
                float nearest = float.MaxValue;
                foreach (var e in _enemies)
                {
                    if (!e.IsAlive) continue;
                    Vector2 toEnemy = e.Position - _player.Position;
                    if (GameMath.IsWithinCone(aimDir, toEnemy, AimAssistAngle))
                    {
                        float dist = toEnemy.LengthSquared();
                        if (dist < nearest) { nearest = dist; target = e; }
                    }
                }
                if (target != null)
                    aimDir = GameMath.SafeNormalize(target.Position - _player.Position);

                _projectiles.Add(new Projectile(_player.Position, aimDir * BulletSpeed, BulletDamage));
                _fireCooldown = fireRate;
            }
        }

        foreach (var p in _projectiles)
        {
            p.Update(delta);
            if (p.Position.X < 0 || p.Position.X > ScreenWidth ||
                p.Position.Y < 0 || p.Position.Y > ScreenHeight)
                p.Kill();
        }
        _projectiles.RemoveAll(p => !p.IsAlive);

        _spawnTimer -= delta;
        if (_spawnTimer <= 0f)
        {
            SpawnEnemy();
            _spawnTimer = _waveManager.SpawnIntervalForWave(_waveManager.CurrentWave);
        }

        foreach (var e in _enemies)
            e.MoveToward(_player.Position, delta);

        foreach (var p in _projectiles)
        {
            foreach (var e in _enemies)
            {
                if (!e.IsAlive || !p.IsAlive) continue;

                if (GameMath.CirclesOverlap(p.Position, p.Radius, e.Position, e.Radius))
                {
                    e.TakeDamage(p.Damage);
                    p.Kill();

                    if (!e.IsAlive)
                    {
                        _player.AddScore(e.ScoreValue);
                        _enemiesKilledThisWave++;
                        TryDropPowerUp(e.Position);
                    }
                }
            }
        }

        foreach (var e in _enemies)
        {
            if (!e.IsAlive) continue;

            if (GameMath.CirclesOverlap(_player.Position, 18f, e.Position, e.Radius))
            {
                _player.TakeDamage(e.ContactDamage);
                e.TakeDamage(1000f);
            }
        }

        foreach (var pu in _powerUps)
        {
            if (!pu.IsAlive) continue;

            if (GameMath.CirclesOverlap(_player.Position, PickupRange, pu.Position, pu.Radius))
            {
                ApplyPowerUp(pu.Type);
                pu.Collect();
            }
        }

        _enemies.RemoveAll(e => !e.IsAlive);
        _projectiles.RemoveAll(p => !p.IsAlive);
        _powerUps.RemoveAll(pu => !pu.IsAlive);

        if (_enemiesKilledThisWave >= EnemiesPerWave)
        {
            _waveManager.AdvanceWave();
            _enemiesKilledThisWave = 0;
        }

        if (_player.Score >= WinScore)
            _state = GameState.Victory;
        else if (!_player.IsAlive)
            _state = GameState.GameOver;

        _displayedHealth = GameMath.Lerp(_displayedHealth, _player.Health, 8f * delta); // Lerp #1

        Window.Title = $"Arena Defender  |  Health: {_player.Health}  Score: {_player.Score}  Wave: {_waveManager.CurrentWave}";
    }

    /// <summary>Counts down active timed buffs and removes any that have expired.</summary>
    private void UpdateBuffs(float delta)
    {
        var keys = new List<PowerUpType>(_activeBuffs.Keys);
        foreach (var key in keys)
        {
            _activeBuffs[key] -= delta;
            if (_activeBuffs[key] <= 0f)
                _activeBuffs.Remove(key);
        }
    }

    /// <summary>Applies a collected power-up: heals instantly, or starts/refreshes a timed buff.</summary>
    private void ApplyPowerUp(PowerUpType type)
    {
        if (type == PowerUpType.Health)
            _player.Heal(30);
        else
            _activeBuffs[type] = BuffDuration;
    }

    /// <summary>Randomly drops a power-up at a defeated enemy's position based on the drop chance.</summary>
    private void TryDropPowerUp(Vector2 position)
    {
        if (_random.NextDouble() > DropChance)
            return;

        PowerUpType type = _random.Next(3) switch
        {
            0 => PowerUpType.Health,
            1 => PowerUpType.FireRate,
            _ => PowerUpType.Speed,
        };
        _powerUps.Add(new PowerUp(position, type));
    }

    /// <summary>Spawns one enemy from a random edge, choosing a type weighted by the current wave.</summary>
    private void SpawnEnemy()
    {
        int side = _random.Next(4);
        Vector2 pos = side switch
        {
            0 => new Vector2(_random.Next(ScreenWidth), -30),
            1 => new Vector2(_random.Next(ScreenWidth), ScreenHeight + 30),
            2 => new Vector2(-30, _random.Next(ScreenHeight)),
            _ => new Vector2(ScreenWidth + 30, _random.Next(ScreenHeight)),
        };

        double roll = _random.NextDouble();
        double tankChance = _waveManager.TankChanceForWave(_waveManager.CurrentWave);

        Enemy enemy;
        if (roll < tankChance)
            enemy = new TankEnemy(pos);
        else if (roll < tankChance + 0.4)
            enemy = new StandardEnemy(pos);
        else
            enemy = new FastEnemy(pos);

        _enemies.Add(enemy);
    }

    /// <summary>Clears the screen and draws the correct visuals for the current game state.</summary>
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(ColBackground);
        _spriteBatch.Begin();

        DrawBackground();

        switch (_state)
        {
            case GameState.Menu:
                DrawMenu();
                break;
            case GameState.Playing:
                DrawPlaying();
                break;
            case GameState.GameOver:
                DrawPlaying();
                DrawEndScreen("GAME OVER", new Color(255, 107, 43));
                break;
            case GameState.Victory:
                DrawPlaying();
                DrawEndScreen("VICTORY!", ColHealth);
                break;
        }

        _spriteBatch.End();
        base.Draw(gameTime);
    }

    /// <summary>Draws the background image stretched across the arena, if one is loaded.</summary>
    private void DrawBackground()
    {
        if (_backgroundTexture != null)
            _spriteBatch.Draw(_backgroundTexture, new Rectangle(0, 0, ScreenWidth, ScreenHeight), new Color(120, 120, 150));
    }

    /// <summary>Draws the start menu: title, win target, Play button, and controls.</summary>
    private void DrawMenu()
    {
        var panel = new Rectangle(ScreenWidth / 2 - 320, ScreenHeight / 2 - 200, 640, 420);
        _shapes.FillRectangle(_spriteBatch, panel, new Color(ColPanel.R, ColPanel.G, ColPanel.B, (byte)210));

        _shapes.FillRectangle(_spriteBatch, _playButton, ColNeon);

        if (_font != null)
        {
            DrawCentered("ARENA DEFENDER", new Vector2(ScreenWidth / 2f, ScreenHeight / 2f - 120), Color.White, 1.5f);
            DrawCentered("REACH " + WinScore + " TO WIN",
                new Vector2(ScreenWidth / 2f, ScreenHeight / 2f - 60), ColMuted, 0.8f);
            DrawCentered("PLAY", new Vector2(_playButton.Center.X, _playButton.Center.Y), new Color(20, 10, 30), 1.2f);
            DrawCentered("WASD  -  MOUSE  -  CLICK",
                new Vector2(ScreenWidth / 2f, ScreenHeight / 2f + 150), ColMuted, 0.8f);
        }
    }


    /// <summary>Draws all active gameplay entities and the HUD.</summary>
    private void DrawPlaying()
    {
        foreach (var pu in _powerUps)
        {
            Color c = pu.Type switch
            {
                PowerUpType.Health => ColHealth,
                PowerUpType.FireRate => ColBuff,
                _ => ColNeon,
            };
            _shapes.FillCircle(_spriteBatch, pu.Position, pu.Radius, c);
            if (_font != null)
                DrawCentered(pu.Type.ToString(), pu.Position - new Vector2(0, 28), c, 0.6f);
        }

        foreach (var p in _projectiles)
            _shapes.FillRectangle(_spriteBatch, CenteredRect(p.Position, p.Radius), ColBuff);

        foreach (var e in _enemies)
        {
            Texture2D tex = e switch
            {
                TankEnemy => _tankTexture,
                StandardEnemy => _standardTexture,
                _ => _fastTexture,
            };

            if (tex != null)
            {
                var origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
                float scale = (e.Radius * 2f) / tex.Width;
                _spriteBatch.Draw(tex, e.Position, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                Color color = e switch
                {
                    TankEnemy => new Color(255, 107, 43),
                    StandardEnemy => new Color(200, 80, 220),
                    _ => new Color(91, 123, 255),
                };
                _shapes.FillRectangle(_spriteBatch, CenteredRect(e.Position, e.Radius), color);
            }
        }

        if (_player != null)
        {
            if (_playerTexture != null)
            {
                var origin = new Vector2(_playerTexture.Width / 2f, _playerTexture.Height / 2f);
                float scale = 48f / _playerTexture.Width;
                _spriteBatch.Draw(_playerTexture, _player.Position, null, Color.White,
                    _playerRotation + MathHelper.PiOver2, origin, scale, SpriteEffects.None, 0f);
            }
            else
            {
                _shapes.FillTriangle(_spriteBatch, _player.Position, 18f, _playerRotation, ColHealth);
            }
        }

        DrawHud();
        DrawThreatIndicator();
    }

    /// <summary>Draws the restyled HUD: bordered health bar, score, wave, and buff timers.</summary>
    private void DrawHud()
    {
        if (_player == null) return;

        var outer = new Rectangle(20, 20, 260, 26);
        _shapes.FillRectangle(_spriteBatch, outer, ColNeon);
        _shapes.FillRectangle(_spriteBatch, Inset(outer, 2), ColPanelBar);
        float frac = MathHelper.Clamp(_displayedHealth / _player.MaxHealth, 0f, 1f);
        var fill = new Rectangle(outer.X + 2, outer.Y + 2, (int)((outer.Width - 4) * frac), outer.Height - 4);
        _shapes.FillRectangle(_spriteBatch, fill, ColHealth);

        if (_font == null) return;

        _spriteBatch.DrawString(_font, $"{_player.Health}/{_player.MaxHealth}", new Vector2(290, 22), Color.White);
        _spriteBatch.DrawString(_font, $"Score: {_player.Score} / {WinScore}", new Vector2(20, 54), Color.White);
        _spriteBatch.DrawString(_font, $"Wave: {_waveManager.CurrentWave}", new Vector2(20, 84), ColNeon);

        int y = 124;
        foreach (var buff in _activeBuffs)
        {
            var bo = new Rectangle(20, y, 150, 14);
            _shapes.FillRectangle(_spriteBatch, bo, ColBuff);
            _shapes.FillRectangle(_spriteBatch, Inset(bo, 2), ColPanelBar);
            float bf = MathHelper.Clamp(buff.Value / BuffDuration, 0f, 1f);   // Lerp #2 (fill fraction)
            _shapes.FillRectangle(_spriteBatch, new Rectangle(bo.X + 2, bo.Y + 2, (int)((bo.Width - 4) * bf), bo.Height - 4), ColBuff);
            _spriteBatch.DrawString(_font, buff.Key.ToString(), new Vector2(180, y - 4), ColBuff);
            y += 24;
        }
    }

    /// <summary>
    /// Draws a marker on the screen edge pointing toward the nearest enemy, using the CROSS PRODUCT
    /// to decide whether that enemy lies to the player's left or right of their facing direction.
    /// </summary>
    private void DrawThreatIndicator()
    {
        if (_player == null) return;

        Enemy nearest = null;
        float best = float.MaxValue;
        foreach (var e in _enemies)
        {
            if (!e.IsAlive) continue;
            float d = (e.Position - _player.Position).LengthSquared();
            if (d < best) { best = d; nearest = e; }
        }
        if (nearest == null) return;

        Vector2 facing = new Vector2((float)System.Math.Cos(_playerRotation), (float)System.Math.Sin(_playerRotation));
        Vector2 toEnemy = nearest.Position - _player.Position;

        // Cross product sign decides the side: positive => right, negative => left (screen-space Y is down).
        float cross = GameMath.Cross2D(facing, toEnemy);
        Color side = cross > 0 ? new Color(255, 107, 43) : ColNeon;
        int x = cross > 0 ? ScreenWidth - 30 : 10;
        int yPos = (int)MathHelper.Clamp(_player.Position.Y, 20, ScreenHeight - 40);
        _shapes.FillRectangle(_spriteBatch, new Rectangle(x, yPos, 20, 20), side);
    }

    /// <summary>Shared end screen for both Game Over and Victory; only title and colour differ.</summary>
    private void DrawEndScreen(string title, Color titleColor)
    {
        int alpha = (int)(200 * _screenFade);
        _shapes.FillRectangle(_spriteBatch, new Rectangle(0, 0, ScreenWidth, ScreenHeight), new Color(0, 0, 0, alpha));

        if (_font == null) return;

        var panel = new Rectangle(ScreenWidth / 2 - 300, ScreenHeight / 2 - 150, 600, 300);
        _shapes.FillRectangle(_spriteBatch, panel, new Color(ColPanel.R, ColPanel.G, ColPanel.B, (byte)(220 * _screenFade)));

        DrawCentered(title, new Vector2(ScreenWidth / 2f, ScreenHeight / 2f - 70), titleColor, 2f);
        DrawCentered($"Final Score: {_player.Score}", new Vector2(ScreenWidth / 2f, ScreenHeight / 2f), Color.White, 1.2f);
        DrawCentered($"Wave Reached: {_waveManager.CurrentWave}", new Vector2(ScreenWidth / 2f, ScreenHeight / 2f + 40), ColMuted);
        DrawCentered("Press R to play again", new Vector2(ScreenWidth / 2f, ScreenHeight / 2f + 90), ColMuted);
    }

    /// <summary>Returns a square rectangle centred on a point, sized by radius.</summary>
    private Rectangle CenteredRect(Vector2 center, float radius)
        => new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2));

    /// <summary>Returns a rectangle shrunk inward by the given amount on all sides.</summary>
    private static Rectangle Inset(Rectangle r, int by)
        => new Rectangle(r.X + by, r.Y + by, r.Width - by * 2, r.Height - by * 2);

    /// <summary>Draws text centred on a point at the given scale.</summary>
    private void DrawCentered(string text, Vector2 center, Color color, float scale = 1f)
    {
        Vector2 size = _font.MeasureString(text) * scale;
        _spriteBatch.DrawString(_font, text, center - size / 2f, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}