using Filmowanie.Abstractions.Configuration;
using Filmowanie.Abstractions.Maybe;
using Filmowanie.Database.Entities.Voting.Events;
using Filmowanie.Database.Interfaces;
using Filmowanie.Voting.Constants;
using Filmowanie.Voting.DTOs.Incoming;
using Filmowanie.Voting.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Filmowanie.Voting.Consumers;

public sealed class MovieAddedEventEventConsumer : IConsumer<MovieAddedEvent>, IConsumer<Fault<MovieAddedEvent>>
{
    private readonly ILogger<MovieAddedEventEventConsumer> log;
    private readonly IOptions<FilmwebOptions> filmwebOptions;
    private readonly IOptions<ImdbOptions> imdbOptions;
    private readonly IOpenAIClient openAiClient;
    private readonly IMovieCommandRepository movieCommandRepository;
    private readonly IHttpClientFactory clientFactory;

    public MovieAddedEventEventConsumer(ILogger<MovieAddedEventEventConsumer> log, IOptions<FilmwebOptions> filmwebOptions, IOptions<ImdbOptions> imdbOptions, IOpenAIClient openAiClient, IMovieCommandRepository movieCommandRepository, IHttpClientFactory clientFactory)
    {
        this.log = log;
        this.filmwebOptions = filmwebOptions;
        this.imdbOptions = imdbOptions;
        this.openAiClient = openAiClient;
        this.movieCommandRepository = movieCommandRepository;
        this.clientFactory = clientFactory;
    }

    public Task Consume(ConsumeContext<Fault<MovieAddedEvent>> context)
    {
        this.log.LogError($"Processing fault in {nameof(MovieAddedEventEventConsumer)}...");
        // no enrichment is okay if this happens.
        return Task.CompletedTask;
    }

    public async Task Consume(ConsumeContext<MovieAddedEvent> context)
    {
        this.log.LogInformation($"Consuming {nameof(MovieAddedEvent)}...");
        try
        {
            var result = await this.openAiClient.GetResponseAsync($"Napisz krótki opis filmu: {context.Message.Movie.Name} ({context.Message.Movie.MovieCreationYear}) opierając się na opisie i opiniach. Bądź radykalny i sarkastyczny, bądź nieznośnie \"smartass\". Użyj co najmniej jednego wulgaryzmu nie wahaj się wyrażać kontrowersyjnych opinii. Bądź brutalny i do przesady krytyczny i cyniczny. Możesz poszukać najbardziej agresywnych opinii na letterboxd. Opis ma mieć co najwyżej 100 słów.", context.CancellationToken);
            var movie = await this.movieCommandRepository.UpdateAltDescriptionAsync(context.Message.Movie.id, result, context.CancellationToken);

            if (!this.filmwebOptions.Value.FallbackPosterUrl.Equals(movie.PosterUrl))
                return;

            var imdbClient = this.clientFactory.CreateClient(HttpClientNames.Imdb);
            var imdbUrl = $"{this.imdbOptions.Value.SuggestionUrl}{movie.Name}.json";
            using var response = await imdbClient.GetAsync(imdbUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                this.log.LogError("Unsuccessfull call to imdb. Got response: {}", await response.Content.ReadAsStringAsync(CancellationToken.None));
                return;
            }

            var typedResponse = await response.Content.ReadFromJsonAsync<ImdbMovieSearchResponseDTO>(context.CancellationToken);

            var potentialResults = typedResponse!.Results.Where(x => x.Year == movie.CreationYear && x.Cast.Split(", ").Intersect(movie.Actors, StringComparer.InvariantCultureIgnoreCase).Any());

            if (!potentialResults.Any())
            {
                this.log.LogWarning("No potential movies in imdb found! Searched: {}", imdbUrl);
                return;
            }
            if (potentialResults.Count() > 1)
            {
                this.log.LogWarning("Found multiple potential movies in imdb. Withholding decision which one to use. Found: {}", potentialResults.Select(x => x.Title).ToArray());
                return;
            }

            var posterUrl = potentialResults.Single().Image.ImageUrl;
            await this.movieCommandRepository.UpdatePosterAsync(movie.id, posterUrl, posterUrl, context.CancellationToken);
        }
        catch (Exception ex)
        {
            LogError(context, ex);
        }

        this.log.LogInformation($"Consumed {nameof(MovieAddedEvent)}.");
    }
    
      private void LogError(ConsumeContext<MovieAddedEvent> context, Exception? ex = null, Error<VoidResult>? error = null)
    {
        var msg = "Error occurred during enriching the movie..." + error?.ToString();

        if (ex == null)
            this.log.LogError(msg);
        else
            this.log.LogError(ex, msg);
    }
}
