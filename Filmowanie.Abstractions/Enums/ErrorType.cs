namespace Filmowanie.Abstractions.Enums;

/// <summary>
/// Error codes. Higher error codes have greater priority, i.e. they overtake lesser ones when merging.
/// </summary>
[Flags]
public enum ErrorType 
{
    None = 0,
    Network = 0x00000001,
    InvalidState = 0x00000010,
    IncomingDataIssue = 0x00000100,
    ValidationError = 0x00001000,
    Canceled = 0x00010000,
    AuthorizationIssue = 0x00100000,
    AuthenticationIssue = 0x01000000,
    Unknown = 0x10000000,
}