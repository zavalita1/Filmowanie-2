namespace Filmowanie.Abstractions.Enums;

[Flags]
public enum StartupMode
{
    Local = 0x00000001,
    CompiledFrontend = 0x00000010,
    WithAppInsights = 0x00000100,
    Production = CompiledFrontend | WithAppInsights,
}