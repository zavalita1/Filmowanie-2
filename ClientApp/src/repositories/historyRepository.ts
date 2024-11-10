import { HistoryStandingsDTO } from "../DTO/Incoming/HistoryStandingsDTO";
import { HistoryDTO } from "../DTO/Incoming/HistoryDTO";
import getFetchWrapperBuilder from '../fetchWrapper';
import { VotingSessionsDTO } from "../DTO/Incoming/VotingSessionsDTO";

export async function getHistory() {
    const fetchWrapper = getFetchWrapperBuilder().build();
    const response = await fetchWrapper<HistoryDTO>('history/list');

    return response;
}

export async function getHistoryStandings() {
    const fetchWrapper = getFetchWrapperBuilder().build();
    const response = await fetchWrapper<HistoryStandingsDTO>('history/laststandings/9');

    return response;
}

export async function getPreviousVotingLists() {
    const fetchWrapper = getFetchWrapperBuilder().build();
    const response = await fetchWrapper<VotingSessionsDTO>('history/previousVotingSessions');
    let votesAgo = 0;
    const result = { votingSessions: response.votingSessions.map(x => ({...x, id: ++votesAgo }))}

    return result;
}

