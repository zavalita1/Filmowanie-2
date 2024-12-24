//using Filmowanie.Abstractions.Enums;
//using Filmowanie.Abstractions.OperationResult;
//using Filmowanie.Account.Interfaces;
//using Filmowanie.Account.Results;
//using Filmowanie.Account.Visitors;
//using Filmowanie.Database.Interfaces;
//using FluentAssertions;
//using Microsoft.Azure.Cosmos;
//using Microsoft.Extensions.Logging;
//using NSubstitute;

//namespace Filmowanie.Tests.Filmowanie_Account;

//public class AccountCodeLoginVisitorTests
//{
//    private readonly IUsersQueryRepository _usersQueryRepository;
//    private readonly ILogger<AccountSignUpVisitor> _log;
//    private readonly ILoginResultDataExtractor _extractor;
//    private readonly AccountCodeLoginVisitor _visitor;

//    public AccountCodeLoginVisitorTests()
//    {
//        _usersQueryRepository = Substitute.For<IUsersQueryRepository>();
//        _log = Substitute.For<ILogger<AccountSignUpVisitor>>();
//        _extractor = Substitute.For<ILoginResultDataExtractor>();
//        _visitor = new AccountCodeLoginVisitor(_usersQueryRepository, _log, _extractor);
//    }

//    [Fact]
//    public async Task VisitAsync_ShouldReturnError_WhenUserIsNull()
//    {
//        // Arrange
//        var result = new OperationResult<string>("testCode");
//        _usersQueryRepository.GetUserAsync(Arg.Any<Func<User, bool>>(), Arg.Any<CancellationToken>())
//            .Returns(Task.FromResult<User>(null));

//        // Act
//        var operationResult = await _visitor.VisitAsync(result, CancellationToken.None);

//        // Assert
//        operationResult.IsSuccess.Should().BeFalse();
//        operationResult.Error.Should().NotBeNull();
//        operationResult.Error.Message.Should().Be("Invalid credentials");
//        operationResult.Error.Type.Should().Be(ErrorType.IncomingDataIssue);
//    }

//    [Fact]
//    public async Task VisitAsync_ShouldReturnLoginResultData_WhenUserIsFound()
//    {
//        // Arrange
//        var result = new OperationResult<string>("testCode");
//        var user = new User { Code = "testCode" };
//        var loginResultData = new LoginResultData();
//        _usersQueryRepository.GetUserAsync(Arg.Any<Func<User, bool>>(), Arg.Any<CancellationToken>())
//            .Returns(Task.FromResult(user));
//        _extractor.GetIdentity(user).Returns(new OperationResult<LoginResultData>(loginResultData));

//        // Act
//        var operationResult = await _visitor.VisitAsync(result, CancellationToken.None);

//        // Assert
//        operationResult.IsSuccess.Should().BeTrue();
//        operationResult.Result.Should().Be(loginResultData);
//    }

//    [Fact]
//    public void Log_ShouldReturnLoggerInstance()
//    {
//        // Act
//        var logger = _visitor.Log;

//        // Assert
//        logger.Should().Be(_log);
//    }
//}