namespace Filmowanie.Interfaces;

public interface IFluentValidatorAdapterFactory
{
    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>(string keyedInstance);
    public IFluentValidatorAdapter<TInput> GetAdapter<TInput>();
}