export type VotingSessionsDTO = {
    votingSessions: VotingSessionDTO[];
};

export type VotingSessionDTO = {
    id: number,
    ended: string
    endedUnlocalized: string
}
