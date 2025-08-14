import { NominationDecade } from "../../store/Nominations";

export type VotingResultDTO = {
    votingResults: VotingResultRowDTO[]; 
    trashVotingResults: TrashVotingResultRowDTO[];
}

export type VotingResultRowDTO = {
    movieName: string;
    votersCount: number;
    isWinner?: boolean;
}

export type TrashVotingResultRowDTO = {
    movieName: string;
    voters: string[];
    isAwarded: boolean;
}

export type VoteAknowledgedDTO = {
    message: string;
}

export type NominationAknowledgedDTO = {
    decade: NominationDecade
}

export type PostersListDTO = {
    posterUrls: string[]
}

export type NominationsDataDTO = {
    moviesThatCanBeNominatedAgain: any[], // TODO
    nominations: string[]
}