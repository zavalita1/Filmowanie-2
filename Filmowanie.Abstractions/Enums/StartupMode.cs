namespace Filmowanie.Abstractions.Enums;

[Flags]
public enum StartupMode
{
    Local = 0x00000001,
    LocalWithDevFrontend = 0x00000011,
    DevServerFrontend = 0x00000010,
    CopiledFrontend = 0x00000100,
    LocalWithCompiledFrontend = 0x00000101,
    WithCosmosEmulator = 0x00001000,
    LocalWithCosmosEmulator = 0x00001011,
    LocalWithCosmosEmulatorAndCompiledFrontend = 0x00001101,
    Production = 0x10001000
}