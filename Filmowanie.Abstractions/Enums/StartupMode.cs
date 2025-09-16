namespace Filmowanie.Abstractions.Enums;

[Flags]
public enum StartupMode
{
    Local = 0x00000001,
    LocalWithCompiledFrontend = 0x00000010,
    Production = 0x10000100
}