import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export interface UserPreferencesSlice {
    preferAltMovieDescriptions: boolean;
    preferSimplifiedCardView: boolean;
    preferDarkMode: boolean;
}

const initialState: UserPreferencesSlice = {
   preferAltMovieDescriptions: (localStorage.getItem("preferAltMovieDescriptions") ?? "false") == "true",
   preferSimplifiedCardView: (localStorage.getItem("preferSimplifiedCardView") ?? "false") == "true",
   preferDarkMode: (localStorage.getItem("preferDarkMode") ?? "false") == "true"
};

export const userPreferencesSlice = createSlice({
    name: 'userPreferences',
    initialState,
    reducers: {
        setPreferAltMovieDescriptions: (state, action: PayloadAction<boolean>) => {
            localStorage.setItem("preferAltMovieDescriptions", action.payload.toString());
            return ({...state, preferAltMovieDescriptions: action.payload })
        },
        setPreferSimplifiedCardView: (state, action: PayloadAction<boolean>) => {
            localStorage.setItem("preferSimplifiedCardView", action.payload.toString());
            return ({...state, preferSimplifiedCardView: action.payload })
        },
        setPreferDarkMode: (state, action: PayloadAction<boolean>) => {
            localStorage.setItem("preferDarkMode", action.payload.toString());
            return ({...state, preferDarkMode: action.payload })
        },
    }
});