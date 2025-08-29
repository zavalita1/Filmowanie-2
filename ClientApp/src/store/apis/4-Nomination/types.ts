export type NominationsDataIncomingDTO = {
    nominations: string[];
}

export type MoviesThatCanBeNominatedAgainIncomingDTO = {
    moviesThatCanBeNominatedAgain: any[], // TODO
}

export type NominationOutgoingDTO = {
    filmwebUrl: string;
    posterUrl?: string;
}

export type PostersIncomingDTO = {
    posterUrls: string[];
}