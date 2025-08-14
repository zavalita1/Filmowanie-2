import { HistoryStandingsDTO } from "../DTO/Incoming/HistoryStandingsDTO";
import { HistoryDTO } from "../DTO/Incoming/HistoryDTO";
import getFetchWrapperBuilder from '../fetchWrapper';
import { VotingSessionsDTO } from "../DTO/Incoming/VotingSessionsDTO";

export async function getHistory() {
    const fetchWrapper = getFetchWrapperBuilder().build();
    const response = await fetchWrapper<HistoryDTO>('api/voting/results/winners');

    return response;
}

export async function getHistoryStandings() {
    const fetchWrapper = getFetchWrapperBuilder().build();
    const response = await fetchWrapper<HistoryStandingsDTO>('api/voting/results/laststandings/');

    return response;
}

export async function getPreviousVotingLists() {
    const fetchWrapper = getFetchWrapperBuilder().build();
    const response = await fetchWrapper<VotingSessionsDTO>('api/voting/results/list');
    
    return response;
}

