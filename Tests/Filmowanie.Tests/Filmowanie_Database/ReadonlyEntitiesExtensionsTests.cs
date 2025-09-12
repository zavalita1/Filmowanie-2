using AutoFixture;
using Filmowanie.Database.Entities;
using Filmowanie.Database.Entities.Voting;
using Filmowanie.Database.Extensions;
using FluentAssertions;

namespace Filmowanie.Tests.Filmowanie_Database;

public class ReadonlyEntitiesExtensionsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Vote_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<Vote>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    }

    [Fact]
    public void EmbeddedUser_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<EmbeddedUser>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    }

    [Fact]
    public void EmbeddedUserWithNominationAward_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<EmbeddedUserWithNominationAward>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    }

    [Fact]
    public void EmbeddedMovieWithNominationContext_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<EmbeddedMovieWithNominationContext>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    }

    [Fact]
    public void EmbeddedMovieWithVotes_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<EmbeddedMovieWithVotes>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    }

    [Fact]
    public void EmbeddedMovie_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<EmbeddedMovie>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    }

    [Fact]
    public void MovieEntity_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<MovieEntity>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    }


    [Fact]
    public void NominatedMovieAgainEvent_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<NominatedMovieEvent>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    }

    [Fact]
    public void User_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<UserEntity>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    } 
    
    [Fact]
    public void VotingResult_ShouldBeEquivalent()
    {
        // Arrange
        var entity = _fixture.Create<VotingResult>();

        // Act
        var result = entity.AsMutable();

        // Assert
        result.Should().BeEquivalentTo(entity);
        result.Should().NotBe(entity);
    }
}