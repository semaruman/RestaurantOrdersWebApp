using NetArchTest.Rules;

namespace RestaurantOrders.Architecture.Tests;

public class DependencyTests
{
    [Fact]
    public void Domain_DoesNotDependOnOuterLayers()
    {
        var result = Types.InAssembly(typeof(Domain.Restaurants.Restaurant).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("RestaurantOrders.Application", "RestaurantOrders.Infrastructure", "RestaurantOrders.Web")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrWeb()
    {
        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("RestaurantOrders.Infrastructure", "RestaurantOrders.Web")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}