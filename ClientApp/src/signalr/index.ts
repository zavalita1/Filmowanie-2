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

    connection.on("movie nominated", (data: {name: string, gender: string}) => {
        const gender = data.gender === "female";
        const nominated = gender ? 'nominowała' : 'nominował';
        store.dispatch(notificationsSlice.actions.addNotification(`${data.name} właśnie ${nominated} film.`));
        store.dispatch(votingSlice.actions.reloadMovies());
    });

    connection.on("voted", (data: {name: string, gender: string}) => {
        const state = store.getState();
        const userData: UserState | undefined = state.api.queries["getUser(undefined)"]?.data as any;
        if (userData?.username !== data.name) {
            const gender = data.gender === "female";
            const nominated = gender ? 'zagłosowała' : 'zagłosował';
            const message = `${data.name} właśnie ${nominated} na jeden z filmów.`;
            toast.info(message);
            store.dispatch(notificationsSlice.actions.addNotification(message));
        }
    });

    connection.start();
}