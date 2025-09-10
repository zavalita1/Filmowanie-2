import { VapidKeyDTO, AwknowledgedDTO } from "./dtos";
import ky from 'ky';

export async function get() {
    const response = await ky<VapidKeyDTO>('api/pushNotification/vapid', { timeout: 5000}).json();
    return response;
}

export async function add(pushSubscription: PushSubscription) {
    const body = {
        endpoint: pushSubscription.endpoint,
        p256dh: arrayBufferToBase64(pushSubscription.getKey("p256dh")!),
        auth: arrayBufferToBase64(pushSubscription.getKey("auth")!)
    };

    await ky<AwknowledgedDTO>('api/pushNotification/add', { json: body, timeout: 5000, method: 'post'}).json();
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
