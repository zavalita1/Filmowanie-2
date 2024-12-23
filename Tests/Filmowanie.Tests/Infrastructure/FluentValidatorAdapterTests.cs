using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Infrastructure;
using FluentAssertions;
using FluentValidation.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Infrastructure;

public sealed class FluentValidatorAdapterTests
{
    private readonly IValidator<TestInput> _validator;
    private readonly FluentValidatorAdapter<TestInput> _adapter;

    public FluentValidatorAdapterTests()
    {
        _validator = Substitute.For<IValidator<TestInput>>();
        var log = Substitute.For<ILogger<FluentValidatorAdapter<TestInput>>>();
        _adapter = new FluentValidatorAdapter<TestInput>(_validator, log);
    }

    [Fact]
    public void Validate_ShouldReturnValidResult_WhenInputIsValid()
    {
        // Arrange
        var input = new TestInput();
        var validationResult = new ValidationResult();
        _validator.Validate(input).Returns(validationResult);

        // Act
        var result = _adapter.Validate(input);

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be(input);
    }

    [Fact]
    public void Validate_ShouldReturnErrorResult_WhenInputIsInvalid()
    {
        // Arrange
        var input = new TestInput();
        var validationResult = new ValidationResult([new ValidationFailure("Property", "Error message")]);
        _validator.Validate(input).Returns(validationResult);

        // Act
        var result = _adapter.Validate(input);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Error message");
        result.Error.Value.Type.Should().Be(ErrorType.ValidationError);
    }

    [Fact]
    public void Validate_ShouldReturnErrorResultWithProperMessage_WhenInputIsInvalid()
    {
        // Arrange
        var input = new TestInput();
        var validationResult = new ValidationResult([
            new ValidationFailure("Property", "Error message"),
            new ValidationFailure("Property2", "Error message 2")
        ]);
        _validator.Validate(input).Returns(validationResult);

        // Act
        var result = _adapter.Validate(input);

        // Assert
        result.Error.Should().NotBeNull();
        result.Error!.Value.ErrorMessages.Should().BeEquivalentTo("Error message", "Error message 2");
        result.Error.Value.Type.Should().Be(ErrorType.ValidationError);
    }

    [Fact]
    public void Visit_ShouldCallValidateWithResult()
    {
        // Arrange
        var input = new OperationResult<TestInput>(new TestInput(), null);
        var validationResult = new ValidationResult();
        _validator.Validate(input.Result!).Returns(validationResult);

        // Act
        var result = _adapter.Visit(input);

        // Assert
        result.Error.Should().BeNull();
        result.Result.Should().Be(input.Result);
    }

    public class TestInput;
}

