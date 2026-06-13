using Silk.NET.Maths;
using Silk.NET.SDL;
using TheAdventure.Exceptions;
using TheAdventure.Map;
using TheAdventure.Models;
using TheAdventure.Systems;

namespace TheAdventure;

public class Game : IDisposable
{
    private readonly GameState _state;
    private readonly InputHandler _input;
    private readonly SaveManager _saveManager;
    private bool _disposed;

    private const int TileSize = 20;

    public Game()
    {
        _state = new GameState();
        _input = new InputHandler();
        _saveManager = new SaveManager();
    }

    public void HandleKeyDown(int key) => _input.OnKeyDown(key);
    public void HandleKeyUp(int key)   => _input.OnKeyUp(key);

    public bool Update()
    {
        var action = _input.ConsumeAction();
        if (action == GameAction.None) return false;
        if (action == GameAction.Quit) return true;

        try
        {
            switch (_state.Phase)
            {
                case GamePhase.Exploring: HandleExploring(action); break;
                case GamePhase.Combat:    HandleCombat(action);    break;
                case GamePhase.GameOver:
                case GamePhase.Victory:   return true;
            }
        }
        catch (GameOverException ex)
        {
            _state.Phase = ex.PlayerWon ? GamePhase.Victory : GamePhase.GameOver;
            _ = SaveScoreAsync();
        }

        return false;
    }

    private void HandleExploring(GameAction action)
    {
        var (dx, dy) = action switch
        {
            GameAction.MoveUp    => (0, -1),
            GameAction.MoveDown  => (0,  1),
            GameAction.MoveLeft  => (-1, 0),
            GameAction.MoveRight => (1,  0),
            _                    => (0,  0)
        };

        if (dx == 0 && dy == 0) return;

        int nx = _state.Player.X + dx;
        int ny = _state.Player.Y + dy;

        var tile = _state.CurrentMap.GetTile(nx, ny);
        if (!tile.IsWalkable) { _state.AddLog("Blocked by wall."); return; }

        var enemy = _state.Enemies.FirstOrDefault(e => e.X == nx && e.Y == ny && e.IsAlive);
        if (enemy != null)
        {
            _state.CurrentEnemy = enemy;
            _state.Phase = GamePhase.Combat;
            _state.AddLog($"You encounter a {enemy.Name}! (SPACE to attack)");
            return;
        }

        _state.Player.Move(dx, dy);

        var item = _state.Items.FirstOrDefault(i => i.X == _state.Player.X && i.Y == _state.Player.Y);
        if (item != null)
        {
            _state.Player.PickUp(item);
            _state.Items.Remove(item);
            _state.AddLog($"Picked up {item.Name}!");
        }

        var newTile = _state.CurrentMap.GetTile(_state.Player.X, _state.Player.Y);
        switch (newTile.Type)
        {
            case TileType.StairsDown when _state.Player.HasKey:
                _state.Player.HasKey = false;
                _state.AddScore(100 * _state.Player.Floor);
                _state.NextFloor();
                break;
            case TileType.StairsDown:
                _state.AddLog("You need a key to descend!");
                break;
            case TileType.BossRoom:
                _state.AddLog("The Dragon Boss awaits...");
                break;
        }

        MoveEnemies();
    }

    private void MoveEnemies()
    {
        foreach (var enemy in _state.Enemies.Where(e => e.IsAlive))
        {
            int distX = _state.Player.X - enemy.X;
            int distY = _state.Player.Y - enemy.Y;
            if (Math.Sqrt(distX * distX + distY * distY) > 8) continue;

            int nx = enemy.X + Math.Sign(distX);
            int ny = enemy.Y + Math.Sign(distY);

            bool blocked = !_state.CurrentMap.GetTile(nx, ny).IsWalkable
                           || _state.Enemies.Any(e => e != enemy && e.IsAlive && e.X == nx && e.Y == ny);

            if (!blocked) enemy.MoveTo(nx, ny);
        }
    }

    private void HandleCombat(GameAction action)
    {
        if (_state.CurrentEnemy == null || !_state.CurrentEnemy.IsAlive)
        {
            _state.Phase = GamePhase.Exploring;
            return;
        }

        if (action != GameAction.Attack && action != GameAction.Wait) return;

        var enemy = _state.CurrentEnemy;

        if (action == GameAction.Attack)
            _state.AddLog(CombatSystem.PlayerAttacks(_state.Player, enemy));

        if (!enemy.IsAlive)
        {
            CombatSystem.RewardPlayer(_state.Player, enemy);
            _state.Enemies.Remove(enemy);
            _state.AddScore(enemy.XpReward);
            _state.Phase = GamePhase.Exploring;
            _state.CurrentEnemy = null;

            if (enemy is BossEnemy)
                throw new GameOverException(playerWon: true, "You slew the Dragon Boss!");
            return;
        }

        _state.AddLog(CombatSystem.EnemyAttacks(enemy, _state.Player));

        if (!_state.Player.IsAlive)
            throw new GameOverException(playerWon: false, "You have been slain!");
    }

    private async Task SaveScoreAsync()
    {
        try { await _saveManager.SaveHighScoreAsync(_state.Score, _state.Player.Floor, _state.Player.Gold); }
        catch (Exception ex) { Console.Error.WriteLine($"Failed to save score: {ex.Message}"); }
    }

    // ── Rendering ──────────────────────────────────────────────────────────────

    public unsafe void Render(Renderer* renderer, Sdl sdl)
    {
        switch (_state.Phase)
        {
            case GamePhase.Exploring:
            case GamePhase.Combat:   RenderGame(renderer, sdl);              break;
            case GamePhase.GameOver: RenderOverlay(renderer, sdl, false);    break;
            case GamePhase.Victory:  RenderOverlay(renderer, sdl, true);     break;
        }
    }

    private unsafe void FillRect(Renderer* renderer, Sdl sdl, int x, int y, int w, int h)
    {
        var rect = new Rectangle<int>(x, y, w, h);
        sdl.RenderFillRect(renderer, ref rect);
    }

    private unsafe void RenderGame(Renderer* renderer, Sdl sdl)
    {
        int camX = _state.Player.X - 20;
        int camY = _state.Player.Y - 12;

        for (int x = 0; x < _state.CurrentMap.Width; x++)
        {
            for (int y = 0; y < _state.CurrentMap.Height; y++)
            {
                var tile = _state.CurrentMap.GetTile(x, y);
                var (r, g, b) = tile.Type switch
                {
                    TileType.Floor      => (40,  40,  40),
                    TileType.Wall       => (80,  80,  120),
                    TileType.StairsDown => (180, 160, 50),
                    TileType.BossRoom   => (180, 50,  50),
                    TileType.Door       => (120, 80,  40),
                    _                   => (0,   0,   0)
                };
                sdl.SetRenderDrawColor(renderer, (byte)r, (byte)g, (byte)b, 255);
                FillRect(renderer, sdl, (x - camX) * TileSize, (y - camY) * TileSize, TileSize - 1, TileSize - 1);
            }
        }

        // Items (yellow)
        sdl.SetRenderDrawColor(renderer, 255, 220, 50, 255);
        foreach (var item in _state.Items)
            FillRect(renderer, sdl, (item.X - camX) * TileSize + 4, (item.Y - camY) * TileSize + 4, TileSize - 8, TileSize - 8);

        // Enemies
        foreach (var enemy in _state.Enemies.Where(e => e.IsAlive))
        {
            var (er, eg, eb) = enemy switch
            {
                BossEnemy => (220, 0,   180),
                Troll     => (220, 80,  0),
                Skeleton  => (200, 200, 200),
                _         => (220, 50,  50)
            };
            sdl.SetRenderDrawColor(renderer, (byte)er, (byte)eg, (byte)eb, 255);
            FillRect(renderer, sdl, (enemy.X - camX) * TileSize + 2, (enemy.Y - camY) * TileSize + 2, TileSize - 4, TileSize - 4);
        }

        // Player (bright green)
        sdl.SetRenderDrawColor(renderer, 50, 255, 100, 255);
        FillRect(renderer, sdl, (_state.Player.X - camX) * TileSize + 2, (_state.Player.Y - camY) * TileSize + 2, TileSize - 4, TileSize - 4);

        DrawHpBar(renderer, sdl, _state.Player.Hp, _state.Player.MaxHp, 10, 10, 200, 16);

        Console.Write($"\rHP:{_state.Player.Hp}/{_state.Player.MaxHp} | Floor:{_state.Player.Floor} | Gold:{_state.Player.Gold} | Score:{_state.Score} | Key:{(_state.Player.HasKey ? "YES" : "NO")}  ");
    }

    private unsafe void DrawHpBar(Renderer* renderer, Sdl sdl, int current, int max, int x, int y, int w, int h)
    {
        sdl.SetRenderDrawColor(renderer, 80, 0, 0, 255);
        FillRect(renderer, sdl, x, y, w, h);
        int fillW = (int)((double)current / max * w);
        sdl.SetRenderDrawColor(renderer, 0, 200, 50, 255);
        FillRect(renderer, sdl, x, y, fillW, h);
    }

    private unsafe void RenderOverlay(Renderer* renderer, Sdl sdl, bool gameWon)
    {
        sdl.SetRenderDrawColor(renderer, gameWon ? (byte)0 : (byte)80, gameWon ? (byte)80 : (byte)0, 0, 200);
        FillRect(renderer, sdl, 200, 280, 400, 120);
        Console.Write(gameWon
            ? $"\rVICTORY! You slew the Dragon Boss!  Score: {_state.Score}  (Press any key)"
            : $"\rGAME OVER!  Score: {_state.Score}  (Press any key)");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}