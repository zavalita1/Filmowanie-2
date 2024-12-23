using Filmowanie.Abstractions.OperationResult;

namespace Filmowanie.Abstractions.Interfaces;

public interface IOperationVisitor<TInput, TOutput> : IVisitor
{
    public OperationResult<TOutput> Visit(OperationResult<TInput> input);
}


public interface IOperationVisitor<TOutput> : IVisitor
{
    public OperationResult<TOutput> Visit<T>(OperationResult<T> input);
}