import * as signalR from "@microsoft/signalr";
import { store } from "../store/store";
import { notificationsSlice } from "../store/slices/notificationsSlice";
import { votingSlice } from "../store/slices/votingSlice";
import { UserState } from "../store/apis/1-User/types";
import { toast } from "sonner";

export function setupSignalRConnection() {
    const connection = new signalR.HubConnectionBuilder().withUrl("/api/votesHub").withAutomaticReconnect().build();
    connection.on("ReceiveMessage", function (user, message) {
        alert(`${user} + ${message}`);
    });

    connection.on("voting started", () => {
        store.dispatch(notificationsSlice.actions.addNotification("Głosowanie właśnie się rozpoczyyyyna"));
        store.dispatch(votingSlice.actions.votingStarted());
    });

    connection.on("voting ended", () => {
        store.dispatch(notificationsSlice.actions.addNotification("Głosowanie się zakończyło"));
        store.dispatch(votingSlice.actions.votingEnded());
    });

    connection.on("you can nominate", () => {
        debugger;
    });

    connection.on("movie nominated", (user: string) => {
        const gender = user.endsWith('a'); // TODO
        const nominated = gender ? 'nominowała' : 'nominował';
        store.dispatch(notificationsSlice.actions.addNotification(`${user} właśnie ${nominated} film.`));
        store.dispatch(votingSlice.actions.reloadMovies());
    });

    connection.on("voted", (user: string) => {
        const state = store.getState();
        const userData: UserState | undefined = state.api.queries["getUser(undefined)"]?.data as any;
        if (userData?.username !== user) {
            const gender = user.endsWith('a'); // TODO 
            const nominated = gender ? 'zagłosowała' : 'zagłosował';
            const message = `${user} właśnie ${nominated} na jeden z filmów.`;
            toast.info(message);
            store.dispatch(notificationsSlice.actions.addNotification(message));
        }
    });

    connection.start();
}