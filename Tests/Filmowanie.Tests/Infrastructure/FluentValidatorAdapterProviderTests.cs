using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Infrastructure;
using FluentAssertions;
using FluentValidation;
using NSubstitute;

namespace Filmowanie.Tests.Infrastructure;

public sealed class FluentValidatorAdapterProviderTests
{
    private readonly IEnumerable<IFluentValidatorAdapter> _validators;
    private readonly IFluentValidationAdapterFactory _factory;
    private readonly FluentValidatorAdapterProvider _provider;

    public FluentValidatorAdapterProviderTests()
    {
        _validators = Substitute.For<IEnumerable<IFluentValidatorAdapter>>();
        _factory = Substitute.For<IFluentValidationAdapterFactory>();
        _provider = new FluentValidatorAdapterProvider(_validators, _factory);
    }

    [Fact]
    public void GetAdapter_ShouldReturnAdapter_WhenValidatorIsFound()
    {
        // Arrange
        var keyedInstance = "key";
        var validator = Substitute.For<IFluentValidatorAdapter>();
        var typedValidator = Substitute.For<IValidator<TestInput>>();
        validator.CanHandle(keyedInstance, out Arg.Any<IValidator<TestInput>>()!).Returns(x =>
        {
            x[1] = typedValidator;
            return true;
        });

        var validators = new List<IFluentValidatorAdapter> { validator };
        _validators.GetEnumerator().Returns(validators.GetEnumerator());

        var adapter = Substitute.For<IFluentValidatorAdapter<TestInput>>();
        _factory.Create(typedValidator).Returns(adapter);

        // Act
        var result = _provider.GetAdapter<TestInput>(keyedInstance);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(adapter);
    }

    [Fact]
    public void GetAdapter_ShouldThrowInvalidOperationException_WhenValidatorIsNotFound()
    {
        // Arrange
        var keyedInstance = "key";
        var validators = new List<IFluentValidatorAdapter>();
        _validators.GetEnumerator().Returns(validators.GetEnumerator());

        // Act
        Action act = () => _provider.GetAdapter<TestInput>(keyedInstance);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"No registered validator found for type: {nameof(TestInput)} and key: {keyedInstance}");
    }

    [Fact]
    public void GetAdapter_WithoutKey_ShouldReturnAdapter()
    {
        // Arrange
        var validator = Substitute.For<IFluentValidatorAdapter>();
        var typedValidator = Substitute.For<IValidator<TestInput>>();
        validator.CanHandle(string.Empty, out Arg.Any<IValidator<TestInput>>()!).Returns(x =>
        {
            x[1] = typedValidator;
            return true;
        });

        var validators = new List<IFluentValidatorAdapter> { validator };
        _validators.GetEnumerator().Returns(validators.GetEnumerator());

        var adapter = Substitute.For<IFluentValidatorAdapter<TestInput>>();
        _factory.Create(typedValidator).Returns(adapter);

        // Act
        var result = _provider.GetAdapter<TestInput>();

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(adapter);
    }

    public class TestInput;
}
