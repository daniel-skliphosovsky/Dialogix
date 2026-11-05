const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

if (document.body.dataset.mobile !== "true") {
    const { dotnet } = await import('./_framework/dotnet.js');

    const dotnetRuntime = await dotnet
        .withConfig({ maxParallelDownloads: 4 })
        .withDiagnosticTracing(false)
        .withApplicationArgumentsFromQuery()
        .create();

    const config = dotnetRuntime.getConfig();

    await dotnetRuntime.runMain(config.mainAssemblyName, [globalThis.location.href]);
}
