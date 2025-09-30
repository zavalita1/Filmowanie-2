import * as signalR from "@microsoft/signalr";
import { store } from "../store/store";
import { notificationsSlice } from "../store/slices/notificationsSlice";
import { votingApi } from "../store/apis/2-Voting/votingApi";
import { UserState } from "../store/apis/1-User/types";
import { toast } from "sonner";

export function setupSignalRConnection() {
    const connection = new signalR.HubConnectionBuilder().withUrl("/api/votesHub").withAutomaticReconnect().build();
    connection.on("ReceiveMessage", function (user, message) {
        alert(`${user} + ${message}`);
    });

    connection.on("voting started", () => {
        store.dispatch(notificationsSlice.actions.addNotification("Głosowanie właśnie się rozpoczyyyyna"));
        store.dispatch(votingApi.util.invalidateTags(['VotingStatus', 'MoviesList', 'Results']));
    });

    connection.on("voting ended", () => {
        store.dispatch(notificationsSlice.actions.addNotification("Głosowanie się zakończyło"));
        store.dispatch(votingApi.util.invalidateTags(['VotingStatus', 'MoviesList', 'Results']));
    });

    connection.on("you can nominate", () => {
        debugger;
    });

    connection.on("movie nominated", (data: {name: string, gender: string}) => {
        const gender = data.gender === "female";
        const nominated = gender ? 'nominowała' : 'nominował';
        store.dispatch(notificationsSlice.actions.addNotification(`${data.name} właśnie ${nominated} film.`));
        store.dispatch(votingApi.util.invalidateTags(['MoviesList']));
    });

    connection.on("voted", (data: {name: string, gender: string}) => {
        const state = store.getState();
        const userData: UserState | undefined = state.api.queries["getUser(undefined)"]?.data as any;
        if (userData?.username !== data.name) {
            let message = `${data.name} właśnie zagłosował na jeden z filmów.`;
            if (data.gender === "female") {
                message = `${data.name} właśnie zagłosowała na jeden z filmów.`;
            } else if (data.gender === "unspecified") {
                message = `${data.name} właśnie zagłosowało na jeden z filmów.`;
            }
            toast.info(message);
            store.dispatch(notificationsSlice.actions.addNotification(message));
        }
    });

    connection.start();
}