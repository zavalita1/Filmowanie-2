namespace Filmowanie.Abstractions.Enums;

/// <summary>
/// Error codes. Higher error codes have greater priority, i.e. they overtake lesser ones when merging.
/// </summary>
public enum ErrorType 
{
    InvalidState = 100,
    IncomingDataIssue = 200,
    ValidationError = 300,
    Canceled = 400,
    AuthorizationIssue = 500,
}