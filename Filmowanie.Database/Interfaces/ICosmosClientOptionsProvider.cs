using Microsoft.Azure.Cosmos;

namespace Filmowanie.Database.Extensions;

public interface ICosmosClientOptionsProvider
{
    ICosmosClientOptionsDecorator Get();
}
