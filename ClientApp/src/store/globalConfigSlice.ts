import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface GlobalConfigSlice {
    apiUrl: string;
    isLoading: boolean;
}

const initialState = {
    apiUrl: import.meta.env.VITE_APIURL,
    isLoading: false
};

export const globalConfigSlice = createSlice({
    name: 'global',
    initialState,
    reducers: {
        setLoading: (state, action: PayloadAction<boolean>) => {
            state.isLoading = action.payload;
        }
    }
});