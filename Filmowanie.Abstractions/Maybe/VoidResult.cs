namespace Filmowanie.Abstractions.Maybe;

public readonly record struct VoidResult
{
    public static Maybe<VoidResult> Void => new(new (), null);
}