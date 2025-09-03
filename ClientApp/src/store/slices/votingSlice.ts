import { Action, createSlice } from '@reduxjs/toolkit';
import {  votingApi } from '../apis/2-Voting/votingApi';

export interface VotingState {
}

const initialState: VotingState = {
};

export const votingSlice = createSlice({
    name: 'voting',
    initialState,
    reducers: {
        votingStarted: (state, action: Action) => {
            votingApi.util.invalidateTags(['VotingStatus']);
        },
        votingEnded: (state, action: Action) => {
            votingApi.util.invalidateTags(['VotingStatus']);
        },
        reloadMovies: (state, action: Action) => {
            votingApi.util.invalidateTags(['MoviesList']);
        }
    }
});