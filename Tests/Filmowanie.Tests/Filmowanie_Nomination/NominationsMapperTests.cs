using Filmowanie.Abstractions.DomainModels;
using Filmowanie.Abstractions.Enums;
using Filmowanie.Abstractions.Extensions;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Nomination.Mappers;
using Filmowanie.Nomination.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Filmowanie.Tests.Filmowanie_Nomination;

public sealed class NominationsMapperTests
{
    private readonly NominationsMapper _sut;

    public NominationsMapperTests()
    {
        var logger = Substitute.For<ILogger<NominationsEnricher>>();
        _sut = new NominationsMapper(logger);
    }

    [Fact]
    public void Map_WhenUserHasNoNominations_ShouldReturnEmptyNominations()
    {
        // Arrange
        var user = new DomainUser("user-" + Guid.NewGuid(), "Mr Bean", false, false, new TenantId(2137), DateTime.UtcNow, Gender.Unspecified);
        var currentNominations = new CurrentNominationsData(
            new VotingSessionId(Guid.NewGuid()),
            Guid.NewGuid(),
            []);

        // Act
        var result = _sut.Map(currentNominations.AsMaybe(), user.AsMaybe());

        // Assert
        result.Result.Should().NotBeNull();
        result.Result!.Nominations.Should().BeEmpty();
    }

    [Fact]
    public void Map_WhenUserHasNominationsInDifferentDecades_ShouldReturnCorrectDecades()
    {
        // Arrange
        var userId = "user-" + Guid.NewGuid();
        var user = new DomainUser(userId, "Mr Bean", false, false, new TenantId(2137), DateTime.UtcNow, Gender.Unspecified);        
        var nominationData = new[]
        {
            CreateNominationData(2023, userId, concluded: null),
            CreateNominationData(2015, userId, concluded: null),
            CreateNominationData(1995, userId, concluded: null),
            // This one should be excluded as it's concluded
            CreateNominationData(2010, userId, concluded: DateTime.UtcNow),
            // This one should be excluded as it's for a different user
            CreateNominationData(2020, "other-user", concluded: null)
        };

        var currentNominations = new CurrentNominationsData(
            new VotingSessionId(Guid.NewGuid()),
            Guid.NewGuid(),
            nominationData);

        // Act
        var result = _sut.Map(currentNominations.AsMaybe(), user.AsMaybe());

        // Assert
        result.Result.Should().NotBeNull();
        result.Result!.Nominations.Should().BeEquivalentTo("2020s", "2010s", "1990s");
    }

    private static NominationData CreateNominationData(int year, string userId, DateTime? concluded)
    {
        return new NominationData { User = new NominationDataEmbeddedUser { DisplayName = "Test User", Id = userId }, Concluded = concluded, Year = year.ToDecade() };
    }
}
