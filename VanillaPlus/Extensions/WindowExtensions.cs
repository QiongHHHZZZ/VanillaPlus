using Dalamud.Interface.Windowing;

namespace VanillaPlus.Extensions;

public static class WindowExtensions {
    public static void Open(this Window window)
        => window.IsOpen = true;

    public static void Close(this Window window)
        => window.IsOpen = false;

    public static void RemoveFromWindowSystem(this Window window)
        => System.WindowSystem.RemoveWindow(window);
    
    public static void AddToWindowSystem(this Window window)
        => System.WindowSystem.AddWindow(window);
}
