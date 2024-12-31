using AutoFixture;
using Filmowanie.Abstractions;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Abstractions.OperationResult;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Nomination.DTOs.Incoming;
using Filmowanie.Nomination.DTOs.Outgoing;
using Filmowanie.Nomination.Interfaces;
using Filmowanie.Nomination.Routes;
using Filmowanie.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Nomination;

public sealed class NominationRoutesTests
{
    private readonly IUserIdentityVisitor _userIdentityVisitor;
    private readonly IGetNominationsVisitor _getNominationsVisitor;
    private readonly IGetNominationsDTOVisitor _getNominationsDtoVisitor;
    private readonly IGetPostersVisitor _getPostersVisitor;
    private readonly INominationsCompleterVisitor _nominationsCompleterVisitor;
    private readonly INominationsResetterVisitor _nominationsResetterVisitor;
    private readonly IGetCurrentVotingSessionIdVisitor _currentVotingSessionIdVisitor;
    private readonly IRequireCurrentVotingSessionIdVisitor _requireCurrentVotingSessionIdVisitor;
    private readonly IMovieThatCanBeNominatedAgainEnricherVisitor _movieThatCanBeNominatedAgainEnricherVisitor;
    private readonly IFluentValidatorAdapterProvider _validatorAdapterProvider;
    private readonly NominationRoutes _nominationRoutes;
    private readonly IRoutesResultHelper _routesHelper;

    private readonly IFixture _fixture = new Fixture();

    public NominationRoutesTests()
    {
        _userIdentityVisitor = Substitute.For<IUserIdentityVisitor>();
        _getNominationsVisitor = Substitute.For<IGetNominationsVisitor>();
        _getNominationsDtoVisitor = Substitute.For<IGetNominationsDTOVisitor>();
        _getPostersVisitor = Substitute.For<IGetPostersVisitor>();
        _nominationsCompleterVisitor = Substitute.For<INominationsCompleterVisitor>();
        _nominationsResetterVisitor = Substitute.For<INominationsResetterVisitor>();
        _currentVotingSessionIdVisitor = Substitute.For<IGetCurrentVotingSessionIdVisitor>();
        _requireCurrentVotingSessionIdVisitor = Substitute.For<IRequireCurrentVotingSessionIdVisitor>();
        _movieThatCanBeNominatedAgainEnricherVisitor = Substitute.For<IMovieThatCanBeNominatedAgainEnricherVisitor>();
        _validatorAdapterProvider = Substitute.For<IFluentValidatorAdapterProvider>();
         _routesHelper = Substitute.For<IRoutesResultHelper>();

        _nominationRoutes = new NominationRoutes(
            _userIdentityVisitor,
            _getNominationsVisitor,
            _currentVotingSessionIdVisitor,
            _movieThatCanBeNominatedAgainEnricherVisitor,
            _validatorAdapterProvider,
            _getNominationsDtoVisitor,
            _getPostersVisitor,
            _nominationsCompleterVisitor,
            _nominationsResetterVisitor,
            _requireCurrentVotingSessionIdVisitor,
            _routesHelper
        );
    }

    [Fact]
    public async Task GetNominationsAsync_ShouldReturnExpectedResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var expectedResult = Substitute.For<IResult>();

        var operationResult = new OperationResult<DomainUser>(default, default);
        _userIdentityVisitor.Visit(OperationResultExtensions.Empty).Returns(operationResult);
        var operationResult1 = new OperationResult<VotingSessionId?>(default, default);
        _currentVotingSessionIdVisitor.VisitAsync(operationResult, cancellationToken).Returns(Task.FromResult(operationResult1));

        var operationResult2 = new OperationResult<CurrentNominationsResponse>(default, default);
        _getNominationsVisitor.VisitAsync(operationResult1, cancellationToken).Returns(Task.FromResult(operationResult2));

        var operationResult3 = new OperationResult<NominationsDataDTO>(default, default);
        _getNominationsDtoVisitor.Visit(OperationResultHelpers.GetEquivalent(operationResult2, operationResult))
            .Returns(operationResult3);

        _routesHelper.UnwrapOperationResult(operationResult3).Returns(expectedResult);

        // Act
        var result = await _nominationRoutes.GetNominationsAsync(cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetNominationsFullDataAsync_ShouldReturnExpectedResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var expectedResult = Substitute.For<IResult>();

        var operationResult1 = new OperationResult<DomainUser>(default);
        _userIdentityVisitor.Visit(OperationResultExtensions.Empty).Returns(operationResult1);

        var operationResult2 = new OperationResult<VotingSessionId?>(default);
        _currentVotingSessionIdVisitor.VisitAsync(operationResult1, cancellationToken).Returns(operationResult2);

        var operationResult3 = new OperationResult<CurrentNominationsResponse>(default!);
        _getNominationsVisitor.VisitAsync(operationResult2, cancellationToken).Returns(operationResult3);

        var operationResult4 = new OperationResult<NominationsDataDTO>(default!);
        _getNominationsDtoVisitor.Visit(OperationResultHelpers.GetEquivalent(operationResult3, operationResult1)).Returns(operationResult4);

        var operationResult5 = new OperationResult<NominationsFullDataDTO>(default!);
        _movieThatCanBeNominatedAgainEnricherVisitor.VisitAsync(OperationResultHelpers.GetEquivalent(operationResult4, operationResult1), cancellationToken).Returns(operationResult5
        );

        _routesHelper.UnwrapOperationResult(operationResult5).Returns(expectedResult);

        // Act
        var result = await _nominationRoutes.GetNominationsFullDataAsync(cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task GetPostersAsync_ShouldReturnExpectedResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var expectedResult = Substitute.For<IResult>();
        var movieUrl = _fixture.Create<string>();

        var adapter = Substitute.For<IFluentValidatorAdapter<string>>();
        _validatorAdapterProvider.GetAdapter<string>("movieUrl").Returns(adapter);

        var operationResult1 = new OperationResult<string>(default!);
        adapter.Visit(Arg.Is<OperationResult<string>>(x => x.Result == movieUrl)).Returns(operationResult1);

        var operationResult2 = new OperationResult<PostersDTO>();
        _getPostersVisitor.VisitAsync(operationResult1, cancellationToken).Returns(operationResult2);

        _routesHelper.UnwrapOperationResult(operationResult2).Returns(expectedResult);

        // Act
        var result = await _nominationRoutes.GetPostersAsync(movieUrl, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task DeleteMovieAsync_ShouldReturnExpectedResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var expectedResult = Substitute.For<IResult>();
        var movieId = _fixture.Create<string>();

        var adapter = Substitute.For<IFluentValidatorAdapter<string>>();
        _validatorAdapterProvider.GetAdapter<string>("movieId").Returns(adapter);

        var operationResult1 = new OperationResult<DomainUser>(default);
        _userIdentityVisitor.Visit(OperationResultExtensions.Empty).Returns(operationResult1);

        var operationResult2 = new OperationResult<VotingSessionId?>(default);
        _currentVotingSessionIdVisitor.VisitAsync(operationResult1, cancellationToken).Returns(operationResult2);

        var operationResult3 = new OperationResult<VotingSessionId>();
        _requireCurrentVotingSessionIdVisitor.Visit(operationResult2).Returns(operationResult3);

        var operationResult4 = new OperationResult<string>();
        adapter.Visit(Arg.Is<OperationResult<string>>(x => x.Result == movieId)).Returns(operationResult4
        );

        var operationResult5 = new OperationResult<AknowledgedNominationDTO>(default!);
        _nominationsResetterVisitor.VisitAsync(OperationResultHelpers.GetEquivalent(operationResult4, operationResult1, operationResult3), cancellationToken).Returns(operationResult5);

        _routesHelper.UnwrapOperationResult(operationResult5).Returns(expectedResult);

        // Act
        var result = await _nominationRoutes.DeleteMovieAsync(movieId, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public async Task NominateAsync_ShouldReturnExpectedResult()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var expectedResult = Substitute.For<IResult>();
        var dto = _fixture.Create<NominationDTO>();
        
        var adapter = Substitute.For<IFluentValidatorAdapter<(NominationDTO, DomainUser, CurrentNominationsResponse)>>();
        _validatorAdapterProvider.GetAdapter<(NominationDTO, DomainUser, CurrentNominationsResponse)>().Returns(adapter);

        var operationResult1 = new OperationResult<DomainUser>(default);
        _userIdentityVisitor.Visit(OperationResultExtensions.Empty).Returns(operationResult1);

        var operationResul2 = new OperationResult<VotingSessionId?>(default);
        _currentVotingSessionIdVisitor.VisitAsync(operationResult1, cancellationToken).Returns(operationResul2);

        var operationResult3 = new OperationResult<CurrentNominationsResponse>(default!);
        _getNominationsVisitor.VisitAsync(operationResul2, cancellationToken).Returns(operationResult3);

        var operationResult4 = new OperationResult<(NominationDTO, DomainUser, CurrentNominationsResponse)>(default);
        adapter.Visit(Arg.Is<OperationResult<(NominationDTO, DomainUser, CurrentNominationsResponse)>>(x => x.Result.Item1 == dto)).Returns(operationResult4);

        var operationResult5 = new OperationResult<AknowledgedNominationDTO>(default!);
        _nominationsCompleterVisitor.VisitAsync(operationResult4, cancellationToken).Returns(operationResult5);

        _routesHelper.UnwrapOperationResult(operationResult5).Returns(expectedResult);

        // Act
        var result = await _nominationRoutes.NominateAsync(dto, cancellationToken);

        // Assert
        result.Should().Be(expectedResult);
    }
}