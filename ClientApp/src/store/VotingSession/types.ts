
export interface VotingSessionState {
    state: VotingState;
}

export enum VotingState {
    Voting,
    Results,
    Loading,
    VotingStarting,
    VotingEnding
}
