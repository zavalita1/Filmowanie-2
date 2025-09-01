import { ResultRow, Results } from "../models/Results";
import { VotingResultIncomingDTO } from "../store/apis/2-Voting/types";

export function mapVotingResults(dto: VotingResultIncomingDTO): Results {
  let rank = 0;
  const votingResult = dto.votingResults.map(x => ({ rank: ++rank, movieTitle: x.movieName, isDecorated: x.isWinner ?? false, votesCount: x.votersCount } satisfies ResultRow));
  rank = 0;
  const trashVotingResult = dto.trashVotingResults.filter(x => x.voters.length !== 0).map(x => ({ rank: ++rank, movieTitle: x.movieName, isDecorated: x.isAwarded ?? false, votesCount: x.voters.length, voters: x.voters} satisfies ResultRow));
  return { voting: votingResult, trashVoting: trashVotingResult }; 
}