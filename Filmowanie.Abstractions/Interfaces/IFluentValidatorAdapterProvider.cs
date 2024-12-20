namespace Filmowanie.Abstractions.Interfaces;

public interface IFluentValidatorAdapterProvider
{
    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>(string keyedInstance);
    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>();
}