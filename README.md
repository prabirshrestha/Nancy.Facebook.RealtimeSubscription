# Facebook Realtime Subscription library for Nancy Web Framework

Nancy.Facebook.RealtimeSubscription makes your life easier to implement the server side portion of Facebook Realtime Subscription APIs.

Make sure to also read the official Facebook documentation at https://developers.facebook.com/docs/reference/api/realtime/ and
https://developers.facebook.com/blog/post/2012/09/21/bringing-real-time-updates-to-the-app-dashboard/ to understand about Facebook Realtime
Subscription API.

# Installing Nancy.Facebook.RealtimeSubscription

```
Install-Package Nancy.Facebook.RealtimeSubscription
```

# Usage

## Helloworld Facebook Subscription

The following code registers both GET and POST methods to auto handle Facebook Realtime Subscription API at `/facebook/subscriptions` route.
Every time a new facebook notification is received it will be added to the `Notifications` list. `notification` is a dynamic object which is
same as in Facebook C# SDK.
To view the notifications received navigate to `/facebook/subscriptions/show`.

```c#
using System.Collections.Generic;
using Nancy;
using Nancy.Facebook.RealtimeSubscription;

public class HelloWorldFacebookRealtimeSubscriptionModule : NancyModule {
    
    private static readonly IList<string> Notifications = new List<string>();

    public HelloWorldFacebookRealtimeSubscriptionModule()
        : base("/facebook/subscriptions") {
        
		const string appSecret = "...";
        const string verifyToken = "...";

        Get["/show"] =
            _ => string.Join(",", Notifications);

        this.SubscribeToFacebookRealtimeUpdates(appSecret, verifyToken, notification => Notifications.Add((string)notification.ToString()));
    }
}
```

Overloads

```c#
this.SubscribeToFacebookRealtimeUpdates(
    (parameter) => new FacebookSubscriptionSettings(appSecret, verifyToken),
    (notification) =>
    {

    });
```

## Multiple subscription in the same NancyModule

By default `SubscribeToFacebookRealtimeUpdates` will register both GET and POST routes for `/`. You can find some overload methods to suit your needs.

`parameter` in the below example is same as the route parameter given by Nancy.

```c#
using System.Collections.Generic;
using Nancy;
using Nancy.Facebook.RealtimeSubscription;

public class MultipleFacebookRealtimeSubscriptionModule : NancyModule {

    public MultipleFacebookRealtimeSubscriptionModule()
        : base("/facebook/subscriptions") {

        const string appSecret = "...";
        const string verifyToken = "...";

        this.SubscribeToFacebookRealtimeUpdates(
            "/app/{id}", ctx => true,
            new FacebookSubscriptionSettings(),
            (parameter, notification) =>
            {
                string id = parameter.id;
            });

        this.SubscribeToFacebookRealtimeUpdates(
            "/user", ctx => true,
            appSecret, verifyToken,
            (parameter, notification) =>
            {

            });

        this.SubscribeToFacebookRealtimeUpdates(
            "/feed",
            (parameter) => new FacebookSubscriptionSettings(appSecret, verifyToken),
            (parameter, notification) =>
            {

            });

        this.SubscribeToFacebookRealtimeUpdates(
            "/user/name",
            (parameter) => new FacebookSubscriptionSettings(appSecret, verifyToken),
            (parameter, notification) =>
            {

            });

        this.SubscribeToFacebookRealtimeUpdates(
            "/user/picture",
            appSecret, verifyToken,
            (parameter, notification) =>
            {

            });

        this.SubscribeToFacebookRealtimeUpdates(
            "/feed/comments",
            (parameter) => new FacebookSubscriptionSettings(appSecret, verifyToken),
            (notification) =>
            {

            });

        this.SubscribeToFacebookRealtimeUpdates(
            "/permissions",
            appSecret, verifyToken,
            (notification) =>
            {

            });
    }
}
```