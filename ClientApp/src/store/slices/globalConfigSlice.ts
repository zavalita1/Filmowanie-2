import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GlobalConfigSlice {
    apiUrl: string;
    defaultApiTimeout: number;
    isLoading: boolean;
    ongoingLoadings: number;
}

const initialState: GlobalConfigSlice = {
    apiUrl: import.meta.env.VITE_APIURL,
    defaultApiTimeout: import.meta.env.VITE_DEFAULTAPITIMEOUT ? parseInt(import.meta.env.VITE_DEFAULTAPITIMEOUT) : 10000,
    isLoading: false,
    ongoingLoadings: 0,
};

export const globalConfigSlice = createSlice({
    name: 'global',
    initialState,
    reducers: {
        setLoading: (state, action: PayloadAction<boolean>) => {
            let newOngoingLoadings = state.ongoingLoadings;
            if (action.payload) newOngoingLoadings++;
            else newOngoingLoadings--;

            return {...state, isLoading: newOngoingLoadings !== 0, ongoingLoadings: newOngoingLoadings };
        }
    }
});