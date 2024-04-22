# Web Push Notifications

Web Push Notifications are a feature that allows websites to engage users by sending them messages even when the user is not actively on the site. This technology helps in retaining users and providing timely updates directly to their devices, such as desktops, smartphones, and tablets. Web push notifications are similar to mobile app notifications but are available for websites.

## How Web Push Notifications Work

The process of sending web push notifications involves several key components:

1. __Service Worker__: This is a script that the browser runs in the background, separate from the web page, enabling the application to leverage features that don't need a web page or user interaction. Service workers are critical for handling the push messaging logic.

2. __User Permission__: Before a website can send notifications to a user, the user must explicitly grant permission through a browser prompt. This ensures that notifications are not sent without the user's consent.

3. __Push Service__: Browsers rely on push services to handle the delivery of messages from the server to the user. Each major browser vendor operates its own push service (e.g., Google's Firebase Cloud Messaging for Chrome, Mozilla’s autopush service for Firefox).

4. __Subscription__: When a user grants permission for notifications, the service worker subscribes to the browser's push service. This subscription generates a unique endpoint associated with the user’s browser and device.

5. __Push Event__: The server sends a push message to the push service endpoint, which then delivers the notification to the correct client (browser).

## Implementation Steps

To implement web push notifications, a website needs to:

* __Register a Service Worker__: This JavaScript file handles background tasks such as receiving push messages and showing notifications.

* __Request Permission__: The site must ask the user to allow notifications.

* __Subscribe to Push Messages__: Once permission is granted, the service worker subscribes to the push service provided by the browser. This subscription involves generating keys and an endpoint that the server will use to send messages.

* __Send Notifications__: Using the subscription endpoint and keys, the server sends a message to the push service, which delivers it to the right client.

## Features and Benefits

* __Engagement__: Notifications can increase user engagement by providing timely and relevant information, prompting users to return to the site.

* __Re-engagement__: Notifications can draw users back to a site even after they've left it, which is particularly valuable for e-commerce and news sites.

* __Real-time Updates__: They provide a way to deliver real-time information like sports scores, news updates, or transaction alerts.

* __Wide Reach__: As they are supported on both desktop and mobile browsers, notifications can reach users across all their devices.

## Example

### Client

`service-worker.js`:

```javascript
self.addEventListener('push', function(event) {
    const data = event.data.json();
    const options = {
        body: data.body,
        icon: 'icon.png',
        badge: 'badge.png'
    };
    event.waitUntil(self.registration.showNotification(data.title, options));
});
```

`scripts.js`:

```javascript
if ('serviceWorker' in navigator && 'PushManager' in window) {
    navigator.serviceWorker.register('service-worker.js')
    .then(function(swReg) {
        console.log('Service Worker is registered', swReg);

        swReg.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array('YOUR_PUBLIC_VAPID_KEY')
        })
        .then(function(subscription) {
            fetch('api/notifications/subscribe', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(subscription)
            });
        })
        .catch(function(error) {
            console.error('Failed to subscribe the user: ', error);
        });
    })
    .catch(function(error) {
        console.error('Service Worker Error', error);
    });
} else {
    console.warn('Push messaging is not supported');
}

function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}
```

`index.html`:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Push Notifications</title>
</head>
<body>
    <h1>Web Push Notifications</h1>
    <script src="scripts.js"></script>
</body>
</html>
```

### Server using ASP.NET Core

```csharp
using Microsoft.AspNetCore.Mvc;
using WebPush;

[Route("api/[controller]")]
[ApiController]
public class NotificationsController : ControllerBase
{
    private static readonly VapidDetails VapidKeys = new VapidDetails(
        "mailto:example@example.com", 
        "YOUR_PUBLIC_VAPID_KEY", 
        "YOUR_PRIVATE_VAPID_KEY"
    );

    private static List<PushSubscription> Subscriptions = new List<PushSubscription>();

    [HttpPost("subscribe")]
    public IActionResult Subscribe([FromBody] PushSubscription subscription)
    {
        Subscriptions.Add(subscription);
        return Ok();
    }

    [HttpPost("broadcast")]
    public IActionResult Broadcast([FromBody] string message)
    {
        var webPushClient = new WebPushClient();
        foreach (var subscription in Subscriptions)
        {
            try
            {
                var pushMessage = new PushMessage(message, VapidKeys);
                webPushClient.SendNotification(subscription, pushMessage);
            }
            catch (WebPushException exception)
            {
                Console.WriteLine("Error sending push notification: " + exception.Message);
            }
        }

        return Ok();
    }
}
```

## VAPID

VAPID (Voluntary Application Server Identification) is a protocol that is used for application server identification in the context of web push services. It provides a way for push services to authenticate the application server that is sending push messages. This helps improve the security and reliability of web push notifications by ensuring that messages are only sent by authorized servers.

### Key Aspects of VAPID

1. __Authentication__: VAPID allows application servers to identify themselves with a push service using a digital signature. This helps push services prevent misuse of their services by ensuring that only messages from authenticated servers are delivered.

2. __Privacy__: VAPID allows the application server to communicate with the push service without exposing sensitive information or user identification data unnecessarily.

3. __Self-Description__: With VAPID, the application server provides a contact email and a public key when it subscribes to the push service. This allows the push service to have information about the server if there are any issues (like spam or abuse originating from that server).

### Components of VAPID

* __Public Key and Private Key__: VAPID uses a public-private key pair. The public key is shared with the push service when subscribing a user to notifications. The private key is used by the application server to sign authenticated requests sent to the push service.

* __JWT (JSON Web Token)__: VAPID relies on JWTs for its authentication mechanism. A JWT is created and signed by the application server using its private key. This JWT includes standard claims such as:

  * `aud` (Audience): The push service's origin.
  * `exp` (Expiration Time): The timestamp when the JWT expires.
  * `sub` (Subject): A contact URI for the application server (typically a mailto: URL).

### How VAPID Works

* __Key Generation__: Initially, the application server generates a public and private key pair. The public key is included in the subscription request sent to the push service when a user subscribes to notifications.

* __Subscription__: When a user agrees to receive push notifications, the browser subscribes to the push service, including the application server's public key in the subscription.

* __Sending Notifications__: When sending a notification, the application server creates a JWT signed with its private key. This JWT is included in the authorization header of the HTTP request to the push service. The push service verifies the JWT using the corresponding public key.

* __Verification and Delivery__: If the verification is successful, the push service delivers the notification. If the JWT is invalid, the request is rejected, which prevents unauthorized entities from sending push notifications.

### Benefits of Using VAPID

* __Security__: Enhances security by ensuring that push messages are only sent by verified senders.

* __Efficiency__: Reduces the risk of push service misuse, thereby helping maintain the service's integrity and performance.

* __Accountability__: Provides a way for push services to contact the sender if needed (via the subject claim in JWT).

VAPID is an essential part of the web push technology landscape, providing a standardized method for securing and managing the delivery of push notifications. This protocol is supported by major browsers and push services, making it a critical component for developers implementing web push notifications in their applications.

Web push notifications are a powerful tool for modern web applications, enabling interactive and engaging user experiences across platforms without needing to develop and maintain native apps. They are particularly effective for applications that benefit from timely updates and ongoing user engagement.

#web-push-notifications
