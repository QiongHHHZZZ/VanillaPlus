using Dalamud.Interface.Windowing;

namespace VanillaPlus.Extensions;

public static class WindowExtensions {
    public static void OpenWindow(this Window window)
        => window.IsOpen = true;

    public static void ToggleWindow(this Window window)
        => window.IsOpen = !window.IsOpen;

    public static void CloseWindow(this Window window)
        => window.IsOpen = false;

    public static void RemoveFromWindowSystem(this Window window)
        => System.WindowSystem.RemoveWindow(window);
    
    public static void AddToWindowSystem(this Window window)
        => System.WindowSystem.AddWindow(window);
}
