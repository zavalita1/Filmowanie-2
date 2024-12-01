namespace Filmowanie.Voting.DTOs.Outgoing
{
    public class AknowledgedDTO
    {
        public string Message { get; set; }
    }

    public class AknowledgedNominationDTO
    {
        public string Message { get; set; }
        public string Decade { get; set; }
    }

    public class NominationsDataDTO
    {
        public MovieDTO[] MoviesThatCanBeNominatedAgain { get; set; }
        public string[] Nominations { get; set; }
    }
}
