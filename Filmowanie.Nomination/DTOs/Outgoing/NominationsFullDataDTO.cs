namespace Filmowanie.Nomination.DTOs.Outgoing;

public sealed class NominationsFullDataDTO
{
    public MovieDTO[] MoviesThatCanBeNominatedAgain { get; set; }

    /// <summary>
    /// Possible values: 2020s, 2010s, 2000s, 1990s, 1980s, 1970s, 1960s, 1950s, 1940s
    /// </summary>
    public string[] Nominations { get; set; }
}