using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using SmartTodoApp.Application.Common.Behaviors;
using SmartTodoApp.Application.Common.Exceptions;

namespace SmartTodoApp.Tests.Application.UnitTests.Common.Behaviors;

public class ValidationBehaviorTests
{
    public record TestRequest(string Value) : IRequest<string>;

    [Fact]
    public async Task Handle_NoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("test");
        var nextCalled = false;
        
        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("result");
        }

        // Act
        var result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        result.Should().Be("result");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCallNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("test");
        var nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("result");
        }

        // Act
        var result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        result.Should().Be("result");
        nextCalled.Should().BeTrue();
        validatorMock.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var validationFailure = new ValidationFailure("Value", "Value is required");
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("");
        var nextCalled = false;

        Task<string> Next()
        {
            nextCalled = true;
            return Task.FromResult("result");
        }

        // Act
        var act = async () => await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<SmartTodoApp.Application.Common.Exceptions.ValidationException>()
            .WithMessage("Validation failed");
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MultipleValidationFailures_ShouldThrowValidationExceptionWithAllErrors()
    {
        // Arrange
        var validationFailures = new[]
        {
            new ValidationFailure("Value", "Value is required"),
            new ValidationFailure("Value", "Value must be at least 3 characters"),
            new ValidationFailure("AnotherProperty", "AnotherProperty is invalid")
        };
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("");

        Task<string> Next() => Task.FromResult("result");

        // Act
        var act = async () => await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<SmartTodoApp.Application.Common.Exceptions.ValidationException>();
        exception.Which.Errors.Should().ContainKey("Value");
        exception.Which.Errors["Value"].Should().HaveCount(2);
        exception.Which.Errors.Should().ContainKey("AnotherProperty");
        exception.Which.Errors["AnotherProperty"].Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_MultipleValidators_ShouldRunAllValidators()
    {
        // Arrange
        var validator1Mock = new Mock<IValidator<TestRequest>>();
        validator1Mock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validator2Mock = new Mock<IValidator<TestRequest>>();
        validator2Mock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validator1Mock.Object, validator2Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("test");

        Task<string> Next() => Task.FromResult("result");

        // Act
        await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        validator1Mock.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        validator2Mock.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CancellationRequested_ShouldPassCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest("test");

        Task<string> Next() => Task.FromResult("result");

        // Act
        var act = async () => await behavior.Handle(request, Next, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
