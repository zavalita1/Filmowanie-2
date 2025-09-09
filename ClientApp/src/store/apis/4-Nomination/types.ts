export type NominationsDataIncomingDTO = {
    nominations: string[];
}

export type MoviesThatCanBeNominatedAgainIncomingDTO = {
    moviesThatCanBeNominatedAgain: any[], // TODO
}

export type NominationOutgoingDTO = {
    movieFilmwebUrl: string;
    posterUrl?: string;
}

export type PostersIncomingDTO = {
    posterUrls: string[];
}