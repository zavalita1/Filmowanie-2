namespace Filmowanie.Nomination.DTOs.Outgoing;

public class NominationsDataDTO
{
    /// <summary>
    /// Possible values: 2020s, 2010s, 2000s, 1990s, 1980s, 1970s, 1960s, 1950s, 1940s
    /// </summary>
    public required string[] Nominations { get; set; }
}