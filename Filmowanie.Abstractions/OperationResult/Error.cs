using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Abstractions.OperationResult;

public readonly record struct Error(IEnumerable<string> ErrorMessages, ErrorType Type)
{
    public Error(string errorMessage, ErrorType type) : this([errorMessage], type) { }
}