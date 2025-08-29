export type ResultRow = {
    rank: number;
    movieTitle: string;
    votesCount: number;
    isDecorated: boolean;
    voters?: string[];
}

export type Results = {
    voting: ResultRow[];
    trashVoting: ResultRow[];
}