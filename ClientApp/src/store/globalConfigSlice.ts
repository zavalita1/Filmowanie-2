import { createSlice } from '@reduxjs/toolkit';

export interface GlobalConfigSlice {
    apiUrl: string;
}

const initialState = {
    apiUrl: import.meta.env.VITE_APIURL,
};

export const globalConfigSlice = createSlice({
    name: 'global',
    initialState,
    reducers: {}
});