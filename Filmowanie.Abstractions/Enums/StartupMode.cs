namespace Filmowanie.Abstractions.Enums;

[Flags]
public enum StartupMode
{
    LocalWithFrontendDevServer = 0x00000001,
    LocalWithCompiledFrontend = 0x00000010,
    Production = 0x00000100,
}