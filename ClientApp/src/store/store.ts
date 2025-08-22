import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import { combineReducers } from 'redux';

import counterReducer from '../features/Counter/counterSlice';
import { globalConfigSlice } from './globalConfigSlice';
import { votingApi } from './apis/Voting/votingApi';

export const store = configureStore({
  devTools: process.env.NODE_ENV === 'development',
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat([votingApi.middleware]),
  reducer: combineReducers({
    global: globalConfigSlice.reducer,
    counter: counterReducer,
    [votingApi.reducerPath]: votingApi.reducer,
  }),
})

// Infer the `RootState` and `AppDispatch` types from the store itself
export type RootState = ReturnType<typeof store.getState>
// Inferred type: {posts: PostsState, comments: CommentsState, users: UsersState}
export type AppDispatch = typeof store.dispatch

setupListeners(store.dispatch)
