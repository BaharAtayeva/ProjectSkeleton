using System.Diagnostics;
using Silk.NET.SDL;
using TheAdventure;

using GameSdlContext = TheAdventure.SdlContext;

var sdl = new Sdl(new GameSdlContext());
var timer = new Stopwatch();

var sdlInitResult = sdl.Init(Sdl.InitVideo | Sdl.InitAudio | Sdl.InitEvents | Sdl.InitTimer |
                              Sdl.InitGamecontroller | Sdl.InitJoystick);
if (sdlInitResult < 0)
    throw new InvalidOperationException("Failed to initialize SDL.");

IntPtr window;
unsafe
{
    window = (IntPtr)sdl.CreateWindow(
        "The Adventure - Dungeon Diver",
        Sdl.WindowposUndefined, Sdl.WindowposUndefined,
        800, 520,
        (uint)WindowFlags.Resizable | (uint)WindowFlags.AllowHighdpi
    );
    if (window == IntPtr.Zero) throw sdl.GetErrorAsException() ?? new Exception("Failed to create window.");
}

IntPtr renderer;
unsafe
{
    renderer = (IntPtr)sdl.CreateRenderer((Window*)window, -1, (uint)RendererFlags.Accelerated);
    sdl.RenderSetVSync((Renderer*)renderer, 1);
    if (renderer == IntPtr.Zero) throw sdl.GetErrorAsException() ?? new Exception("Failed to create renderer.");
}

var game = new Game();
var ev = new Event();
bool quit = false;

Console.WriteLine("=== THE ADVENTURE: DUNGEON DIVER ===");
Console.WriteLine("WASD / Arrow keys: Move   SPACE/Enter: Attack   .: Wait   ESC: Quit");
Console.WriteLine("Find the KEY then reach the STAIRS. Defeat the Dragon Boss on floor 3!");

while (!quit)
{
    while (sdl.PollEvent(ref ev) != 0)
    {
        if (ev.Type == (uint)EventType.Quit) { quit = true; break; }

        switch (ev.Type)
        {
            case (uint)EventType.Windowevent:
                if (ev.Window.Event == (byte)WindowEventID.TakeFocus)
                    unsafe { sdl.SetWindowInputFocus(sdl.GetWindowFromID(ev.Window.WindowID)); }
                break;

            case (uint)EventType.Keydown:
                game.HandleKeyDown((int)ev.Key.Keysym.Scancode);
                break;

            case (uint)EventType.Keyup:
                game.HandleKeyUp((int)ev.Key.Keysym.Scancode);
                break;
        }
    }

    quit |= game.Update();
    timer.Restart();

    unsafe
    {
        var r = (Renderer*)renderer;
        sdl.SetRenderDrawColor(r, 15, 15, 20, 255);
        sdl.RenderClear(r);
        game.Render(r, sdl);
        sdl.RenderPresent(r);
    }
}

game.Dispose();
unsafe { sdl.DestroyWindow((Window*)window); }
sdl.Quit();