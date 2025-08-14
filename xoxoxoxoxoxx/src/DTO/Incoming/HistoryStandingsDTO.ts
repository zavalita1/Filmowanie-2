export type HistoryStandingsDTO = {
    rows: HistoryStandingsRowDTO[]
}

export type HistoryStandingsRowDTO = {
    movieTitle: string,
    votingPlaces: number[]
}
