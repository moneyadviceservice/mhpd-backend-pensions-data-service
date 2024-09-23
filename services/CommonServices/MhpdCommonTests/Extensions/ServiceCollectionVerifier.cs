using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace MhpdCommonTests.Extensions;

internal sealed class ServiceCollectionVerifier(Mock<IServiceCollection> serviceCollectionMock)
{
    internal void ContainsSingletonService<TService, TInstance>()
    {
        IsServiceRegistered<TService, TInstance>(ServiceLifetime.Singleton);
    }

    internal void ContainsTransientService<TService, TInstance>()
    {
        IsServiceRegistered<TService, TInstance>(ServiceLifetime.Transient);
    }

    internal void ContainsScopedService<TService, TInstance>()
    {
        IsServiceRegistered<TService, TInstance>(ServiceLifetime.Scoped);
    }

    private void IsServiceRegistered<TService, TInstance>(ServiceLifetime lifetime)
    {
        serviceCollectionMock
            .Verify(serviceCollection => serviceCollection.Add(
                It.Is<ServiceDescriptor>(serviceDescriptor => serviceDescriptor.Is<TService, TInstance>(lifetime))));

    }
}
