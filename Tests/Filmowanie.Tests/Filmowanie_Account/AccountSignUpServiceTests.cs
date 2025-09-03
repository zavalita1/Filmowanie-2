using AutoFixture;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Account.Interfaces;
using Filmowanie.Account.Results;
using Filmowanie.Account.Services;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Interfaces;
using Filmowanie.Database.Interfaces.ReadOnlyEntities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Filmowanie.Tests.Filmowanie_Account;

public class AccountSignUpServiceTests
{
    private readonly IUsersCommandRepository _commandRepository;
    private readonly IHashHelper _hashHelper;
    private readonly ILogger<AccountSignUpService> _log;
    private readonly ILoginResultDataExtractor _extractor;
    private readonly AccountSignUpService _service;
    private readonly IFixture _fixture = new Fixture();

    public AccountSignUpServiceTests()
    {
        _commandRepository = Substitute.For<IUsersCommandRepository>();
        _hashHelper = Substitute.For<IHashHelper>();
        _log = Substitute.For<ILogger<AccountSignUpService>>();
        _extractor = Substitute.For<ILoginResultDataExtractor>();
        _service = new AccountSignUpService(_commandRepository, _hashHelper, _log, _extractor);
    }

    [Fact]
    public async Task VisitAsync_ShouldReturnError_WhenExceptionIsThrown()
    {
        // Arrange
        var id = _fixture.Create<string>();
        var created = _fixture.Create<DateTime>();
        var domainUser = new DomainUser { Id = id, Created = created };
        var basicAuth = new BasicAuth { Password = "password" };
        var data = new OperationResult<(DomainUser, BasicAuth)>((domainUser, basicAuth));
        _hashHelper.GetHash(basicAuth.Password, domainUser.Id + domainUser.Created.Minute).Returns("hashedPassword");
        _commandRepository.UpdatePasswordAndMail(domainUser.Id, Arg.Any<BasicAuth>(), Arg.Any<CancellationToken>())
            .Throws(new Exception("Database error"));

        // Act
        var operationResult = await _service.SignUp(data, CancellationToken.None);

        // Assert
        operationResult.Result.Should().BeNull();
        operationResult.Error.Should().NotBeNull();
        operationResult.Error!.Value.ErrorMessages.Should().BeEquivalentTo("No user with such id in db");
        operationResult.Error.Value.Type.Should().Be(ErrorType.InvalidState);
        _log.Received(1).LogError(Arg.Any<Exception>(), "Error in trying to sign up");
    }

    [Fact]
    public async Task VisitAsync_ShouldReturnLoginResultData_WhenUserIsUpdated()
    {
        // Arrange
        var id = _fixture.Create<string>();
        var created = _fixture.Create<DateTime>();
        var domainUser = new DomainUser { Id = id, Created = created };
        var basicAuth = new BasicAuth { Password = "password" };
        var data = new OperationResult<(DomainUser, BasicAuth)>((domainUser, basicAuth));
        var userEntity = new UserEntity();
        var loginResultData = new LoginResultData(null!, null!);
        _hashHelper.GetHash(basicAuth.Password, domainUser.Id + domainUser.Created.Minute).Returns("hashedPassword");
        _commandRepository.UpdatePasswordAndMail(domainUser.Id, Arg.Any<BasicAuth>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyUserEntity>(userEntity));
        _extractor.GetIdentity(userEntity).Returns(new OperationResult<LoginResultData>(loginResultData));

        // Act
        var operationResult = await _service.SignUp(data, CancellationToken.None);

        // Assert
        operationResult.Result.Should().NotBeNull();
        operationResult.Result.Should().Be(loginResultData);
    }
}