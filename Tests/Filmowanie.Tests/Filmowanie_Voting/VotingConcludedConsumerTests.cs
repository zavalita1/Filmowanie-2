using Filmowanie.Abstractions.Interfaces;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Consumers;
using Filmowanie.Voting.Deciders;
using Filmowanie.Voting.Deciders.PickUserNomination;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Voting;

public class VotingConcludedConsumerTests
{
    private readonly VotingConcludedConsumer _sut;
    private readonly IVotingSessionCommandRepository _votingSessionCommandRepository;
    private readonly IVotingSessionQueryRepository _votingSessionQueryRepository;
    private readonly IVotingDeciderFactory _votingDeciderFactory;
    private readonly IPickUserToNominateStrategyFactory _pickUserToNominateStrategyFactory;

    public VotingConcludedConsumerTests()
    {
        var logger = Substitute.For<ILogger<VotingConcludedConsumer>>();
        _votingSessionCommandRepository = Substitute.For<IVotingSessionCommandRepository>();
        var dateProvider = Substitute.For<IDateTimeProvider>();
        dateProvider.Now.Returns(new DateTime(2010, 04, 10));
        _votingSessionQueryRepository = Substitute.For<IVotingSessionQueryRepository>();
        _votingDeciderFactory = Substitute.For<IVotingDeciderFactory>();
        _pickUserToNominateStrategyFactory = Substitute.For<IPickUserToNominateStrategyFactory>();
        _sut = new VotingConcludedConsumer(logger, _votingSessionCommandRepository, dateProvider, _votingSessionQueryRepository, _votingDeciderFactory, _pickUserToNominateStrategyFactory);

    }

    [Fact]
    public async Task Test()
    {
        // Arrange

    }
}