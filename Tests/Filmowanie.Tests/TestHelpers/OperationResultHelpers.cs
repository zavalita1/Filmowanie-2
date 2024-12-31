using Filmowanie.Abstractions.OperationResult;
using NSubstitute;

namespace Filmowanie.Tests.TestHelpers;

public static class OperationResultHelpers
{
    public static OperationResult<(T1, T2)> GetEquivalent<T1, T2>(OperationResult<T1> first, OperationResult<T2> second) =>
        Arg.Is<OperationResult<(T1, T2)>>(x => Equals(x.Result.Item1, first.Result) && Equals(x.Result.Item2, second.Result));

    public static OperationResult<(T1, T2, T3)> GetEquivalent<T1, T2, T3>(OperationResult<T1> first, OperationResult<T2> second, OperationResult<T3> third) =>
        Arg.Is<OperationResult<(T1, T2, T3)>>(x => Equals(x.Result.Item1, first.Result) && Equals(x.Result.Item2, second.Result) && Equals(x.Result.Item3, third.Result));
}