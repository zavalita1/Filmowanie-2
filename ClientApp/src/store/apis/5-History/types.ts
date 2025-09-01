export type WatchedMoviesListIncomingDTO = {
    entries: WatchedMovieIncomingDTO[];
}

export type WatchedMovieIncomingDTO = {
    title: string;
    originalTitle: string;
    createdYear: number;
    nominatedBy: string;
    watched: string;
};

export type ConcludedVotingIncomingDTO = {
    ended: string;
    endedUnlocalized: string;
    id: string;
};

export type ConcludedVotingsIncomingDTO = {
    votingSessions: ConcludedVotingIncomingDTO[];
}