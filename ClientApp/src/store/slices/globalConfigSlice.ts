import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GlobalConfigSlice {
    apiUrl: string;
    isLoading: boolean;
    ongoingLoadings: number;
}

const initialState: GlobalConfigSlice = {
    apiUrl: import.meta.env.VITE_APIURL,
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