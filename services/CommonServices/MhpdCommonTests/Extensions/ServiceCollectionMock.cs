using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MhpdCommonTests.Extensions;

internal sealed class ServiceCollectionMock
{
    private readonly Mock<IServiceCollection> serviceCollectionMock;

    public ServiceCollectionMock()
    {
        serviceCollectionMock = new Mock<IServiceCollection>();

        ServiceCollectionVerifier = new ServiceCollectionVerifier(serviceCollectionMock);
    }

    internal IServiceCollection ServiceCollection => serviceCollectionMock.Object;

    internal ServiceCollectionVerifier ServiceCollectionVerifier { get; }

    internal void ContainsSingletonService<TService, TInstance>()
    {
        ServiceCollectionVerifier.ContainsSingletonService<TService, TInstance>();
    }

    internal void ContainsTransientService<TService, TInstance>()
    {
        ServiceCollectionVerifier.ContainsTransientService<TService, TInstance>();
    }

    internal void ContainsScopedService<TService, TInstance>()
    {
        ServiceCollectionVerifier.ContainsScopedService<TService, TInstance>();
    }
}
