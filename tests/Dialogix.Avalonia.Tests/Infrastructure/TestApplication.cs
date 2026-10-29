using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;

[assembly: AvaloniaTestApplication(typeof(Dialogix.Avalonia.Tests.Infrastructure.TestApplication))]

namespace Dialogix.Avalonia.Tests.Infrastructure;

public class TestApplication : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApplication>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
