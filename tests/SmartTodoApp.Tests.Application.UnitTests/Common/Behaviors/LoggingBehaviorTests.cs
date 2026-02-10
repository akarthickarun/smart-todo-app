using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTodoApp.Application.Common.Behaviors;

namespace SmartTodoApp.Tests.Application.UnitTests.Common.Behaviors;

public class LoggingBehaviorTests
{
    public record TestRequest(string Value) : IRequest<string>;

    [Fact]
    public async Task Handle_SuccessfulRequest_ShouldLogInformationMessages()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(loggerMock.Object);
        var request = new TestRequest("test");

        Task<string> Next() => Task.FromResult("result");

        // Act
        var result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        result.Should().Be("result");

        // Verify "Handling" log
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling TestRequest")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify "Handled successfully" log
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handled TestRequest successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SuccessfulRequest_ShouldLogElapsedTime()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(loggerMock.Object);
        var request = new TestRequest("test");

        async Task<string> Next()
        {
            await Task.Delay(50); // Simulate some work
            return "result";
        }

        // Act
        await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ms")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FailedRequest_ShouldLogErrorMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(loggerMock.Object);
        var request = new TestRequest("test");
        var expectedException = new InvalidOperationException("Test exception");

        Task<string> Next() => throw expectedException;

        // Act
        var act = async () => await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        // Verify "Handling" log
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling TestRequest")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Verify error log
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error handling TestRequest")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_FailedRequest_ShouldLogElapsedTimeBeforeError()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(loggerMock.Object);
        var request = new TestRequest("test");
        var expectedException = new InvalidOperationException("Test exception");

        async Task<string> Next()
        {
            await Task.Delay(50); // Simulate some work
            throw expectedException;
        }

        // Act
        var act = async () => await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Verify error log includes elapsed time
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("ms")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleRequests_ShouldLogEachRequest()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(loggerMock.Object);

        Task<string> Next() => Task.FromResult("result");

        // Act
        await behavior.Handle(new TestRequest("test1"), Next, CancellationToken.None);
        await behavior.Handle(new TestRequest("test2"), Next, CancellationToken.None);

        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling TestRequest")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handled TestRequest successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ReturnsCorrectResult()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(loggerMock.Object);
        var request = new TestRequest("test");
        var expectedResult = "expected result";

        Task<string> Next() => Task.FromResult(expectedResult);

        // Act
        var result = await behavior.Handle(request, Next, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
    }
}
