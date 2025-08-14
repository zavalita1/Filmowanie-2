import { VapidKeyDTO } from "../DTO/Incoming/VapidKeyDTO";
import { AwknowledgedDTO } from "../DTO/Incoming/AwknowledgedDTO";
import getFetchWrapperBuilder from '../fetchWrapper';

export async function get() {
    const fetchWrapper = getFetchWrapperBuilder().build();
    const response = await fetchWrapper<VapidKeyDTO>('api/pushNotification/vapid');
    return response;
}

export async function add(pushSubscription: PushSubscription) {
    const body = JSON.stringify({
        endpoint: pushSubscription.endpoint,
        p256dh: arrayBufferToBase64(pushSubscription.getKey("p256dh")!),
        auth: arrayBufferToBase64(pushSubscription.getKey("auth")!)
    });

    const fetchOptions = { 
        method: "POST", 
        body, 
        headers: { 'content-type': 'application/json;charset=UTF-8', } 
    };
    const fetchWrapper = getFetchWrapperBuilder().useTimeout(5000).build();
    await fetchWrapper<AwknowledgedDTO>('api/pushNotification/add', fetchOptions);
}

function arrayBufferToBase64(buffer: ArrayBuffer) {
    var binary = "";
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}
