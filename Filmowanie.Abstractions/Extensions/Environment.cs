using Filmowanie.Abstractions.Enums;

namespace Filmowanie;

public static class Environment
{
    private static StartupMode? _mode;

    public static StartupMode? Mode
    {
        get => _mode;
        set => _mode ??= value;
    }
}