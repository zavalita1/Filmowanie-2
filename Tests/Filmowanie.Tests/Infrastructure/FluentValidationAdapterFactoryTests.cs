using Filmowanie.Infrastructure;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Infrastructure;

public sealed class FluentValidationAdapterFactoryTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FluentValidationAdapterFactory _factory;

    public FluentValidationAdapterFactoryTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        _factory = new FluentValidationAdapterFactory(_serviceProvider);
    }

    [Fact]
    public void Create_ShouldReturnFluentValidatorAdapter()
    {
        // Arrange
        var innerValidator = Substitute.For<IValidator<TestInput>>();
        var logger = Substitute.For<ILogger<FluentValidatorAdapter<TestInput>>>();
        _serviceProvider.GetService(typeof(ILogger<FluentValidatorAdapter<TestInput>>)).Returns(logger);

        // Act
        var adapter = _factory.Create(innerValidator);

        // Assert
        adapter.Should().NotBeNull();
        adapter.Should().BeOfType<FluentValidatorAdapter<TestInput>>();
        adapter.Log.Should().Be(logger);
    }

    [Fact]
    public void Create_ShouldThrowException_WhenLoggerNotFound()
    {
        // Arrange
        var innerValidator = Substitute.For<IValidator<TestInput>>();
        _serviceProvider.GetService(typeof(ILogger<FluentValidatorAdapter<TestInput>>)).Returns(_ => throw new InvalidOperationException());

        // Act
        Action act = () => _factory.Create(innerValidator);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    public class TestInput;
}

