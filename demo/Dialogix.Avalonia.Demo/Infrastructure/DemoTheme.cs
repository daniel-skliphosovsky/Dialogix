using Avalonia.Media;
using Avalonia.Styling;

namespace Dialogix.Avalonia.Demo.Infrastructure;

public static class DemoTheme
{
    public static ThemeVariant Hacker { get; } = new("Hacker", ThemeVariant.Dark);

    public static FontFamily DefaultFont { get; } = new("avares://Avalonia.Fonts.Inter/Assets#Inter");

    public static FontFamily HackerFont { get; } = new("Menlo, Consolas, monospace");
}