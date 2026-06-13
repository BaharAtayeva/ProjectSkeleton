using Silk.NET.SDL;

namespace TheAdventure.Systems;

public enum GameAction
{
    None,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Attack,
    Wait,
    Quit
}

public class InputHandler
{
    // Use raw scancode int values to avoid KeyCode enum mismatch
    private readonly HashSet<int> _pressedThisFrame = new();
    private readonly HashSet<int> _heldKeys = new();

    public void OnKeyDown(int scancode)
    {
        if (!_heldKeys.Contains(scancode))
            _pressedThisFrame.Add(scancode);
        _heldKeys.Add(scancode);
    }

    public void OnKeyUp(int scancode)
    {
        _heldKeys.Remove(scancode);
    }

    public GameAction ConsumeAction()
    {
        GameAction action = GameAction.None;

        // These are SDL Scancode values (matching ScancodeW=26, ScancodeS=22, etc.)
        if (_pressedThisFrame.Contains((int)ScancodeValue.Up)    || _pressedThisFrame.Contains((int)ScancodeValue.W))
            action = GameAction.MoveUp;
        else if (_pressedThisFrame.Contains((int)ScancodeValue.Down)  || _pressedThisFrame.Contains((int)ScancodeValue.S))
            action = GameAction.MoveDown;
        else if (_pressedThisFrame.Contains((int)ScancodeValue.Left)  || _pressedThisFrame.Contains((int)ScancodeValue.A))
            action = GameAction.MoveLeft;
        else if (_pressedThisFrame.Contains((int)ScancodeValue.Right) || _pressedThisFrame.Contains((int)ScancodeValue.D))
            action = GameAction.MoveRight;
        else if (_pressedThisFrame.Contains((int)ScancodeValue.Space) || _pressedThisFrame.Contains((int)ScancodeValue.Return))
            action = GameAction.Attack;
        else if (_pressedThisFrame.Contains((int)ScancodeValue.Period))
            action = GameAction.Wait;
        else if (_pressedThisFrame.Contains((int)ScancodeValue.Escape))
            action = GameAction.Quit;

        _pressedThisFrame.Clear();
        return action;
    }

    // SDL Scancode numeric values (from SDL_scancode.h)
    private enum ScancodeValue
    {
        A      = 4,
        D      = 7,
        S      = 22,
        W      = 26,
        Return = 40,
        Escape = 41,
        Space  = 44,
        Period = 55,
        Right  = 79,
        Left   = 80,
        Down   = 81,
        Up     = 82,
    }
}