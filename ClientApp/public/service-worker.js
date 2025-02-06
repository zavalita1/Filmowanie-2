self.addEventListener('activate', async () => {
    console.log('service worker activated!')
  });

  // TODO

self.addEventListener('push', event => {
    let data = event.data.json();
    const image = 'https://cdn.glitch.com/614286c9-b4fc-4303-a6a9-a4cef0601b74%2Flogo.png?v=1605150951230';
    const options = {
        body: data.options.body,
        icon: image
    };

    self.registration.showNotification(
      data.title,
      options
    );
});

self.addEventListener('notificationClick', event => {
    const notification = event.notification;
    const action = event.action;

    console.log("noti " + notification);
    console.log("actio  " + action);
})
