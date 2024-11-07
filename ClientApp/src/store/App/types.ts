import { PayloadAction } from "@reduxjs/toolkit";

export interface AppState {
    isLoading: boolean;
    isError: boolean;
    errorMessage?: string;
    infoMessage?: string;
    theme: SupportedTheme;
    isMobile: boolean;
}

export type SetLoadingAction = PayloadAction<{ isLoading: boolean }>;
export type SetErrorAction = PayloadAction<{ errorMessage: string }>;
export type SetThemeAction = PayloadAction<{ theme: string }>;
export type SupportedTheme = 'light' | 'dark';