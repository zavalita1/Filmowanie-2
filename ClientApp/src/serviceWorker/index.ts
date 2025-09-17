import * as repository from './repository';

export async function registerServiceWorker() {
    if ('serviceWorker' in navigator && 'PushManager' in window) {
        try {
            const serviceWorkerRegistration = await navigator.serviceWorker.register('./service-worker.js');
            console.info('Service worker was registered.');
            console.info({ serviceWorkerRegistration });
            Notification.requestPermission(status => notificationRequestPermissionCallback(serviceWorkerRegistration, status));
        }
        catch(error) {
            console.error('An error occurred while registering the service worker.');
            console.error(error);
        }
  } else {
    console.error('Browser does not support service workers or push messages.');
  }
}

function notificationRequestPermissionCallback(serviceWorkerRegistration: ServiceWorkerRegistration, status: NotificationPermission) {
    if (status === "denied") {
        console.log('To smutne. Odrzucasz powiadomienia :(???');
        return;
    }

    const isLogged = localStorage.getItem("isLogged");

    if (isLogged === "True") {
        afterLoginOrPushNotificationGrant();
    } else {
        window.addEventListener("userLogsIn", afterLoginOrPushNotificationGrant);
    }

    async function afterLoginOrPushNotificationGrant() {
        const sub = await serviceWorkerRegistration.pushManager.getSubscription();

        if (sub !== null) {
            console.log('push notification subscription found.');
            return;
        }

        const vapid = await repository.get();
        let innerSub: PushSubscription | null = null;

        try {
            innerSub = await serviceWorkerRegistration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: vapid.key
            });

            await repository.add(innerSub);
        }
        catch (e) {
            console.error("Unable to subscribe to push", e);
            innerSub?.unsubscribe();
        }
    }
}