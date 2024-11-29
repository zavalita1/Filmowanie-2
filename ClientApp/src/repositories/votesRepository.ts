import { VoteAknowledgedDTO, VotingResultDTO } from "../DTO/Incoming/VotingResultDTO";
import getFetchWrapperBuilder from '../fetchWrapper';

export async function getVotingResult() {
    const fetchWrapper = getFetchWrapperBuilder().useTimeout(5000).build();
    const response = await fetchWrapper<VotingResultDTO>('votes/');

    return response;
}

export async function getPreviousVotingResult(votingSessionsAgo: number) {
    const fetchWrapper = getFetchWrapperBuilder().useTimeout(5000).build();
    const response = await fetchWrapper<VotingResultDTO>(`votes/previousVotingSessions?votingSessionsAgo=${votingSessionsAgo}`);

    return response;
}

export async function placeVote(movieId: string, movieTitle: string, votes: Number) {
    const body = JSON.stringify({ movieId, movieTitle, votes});
    const fetchOptions = { 
        method: "POST", 
        body, 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };
    const fetchWrapper = getFetchWrapperBuilder().useTimeout(5000).build();
    await fetchWrapper<VoteAknowledgedDTO>('api/voting/vote', fetchOptions);
}

export async function resetVote(movieId: string, movieTitle: string) {
    const body = JSON.stringify({ movieId, movieTitle, votes: 0});
    const fetchOptions = { 
        method: "POST", 
        body, 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };
    const fetchWrapper = getFetchWrapperBuilder().useTimeout(5000).build();
    await fetchWrapper<VoteAknowledgedDTO>('api/voting/vote', fetchOptions);
}