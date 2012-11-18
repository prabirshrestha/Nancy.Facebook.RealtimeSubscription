namespace Nancy.Facebook.RealtimeSubscription
{
    using System;
    using System.IO;
    using System.Linq;
    using Nancy.Responses;

    /// <summary>
    /// Facebook Realtime Subscription API extensions methods.
    /// </summary>
    public static class FacebookRealtimeSubscriptionExtensions
    {
        /// <summary>
        /// Verifies the Facebook HTTP GET subscription and returns the approprite response.
        /// </summary>
        /// <param name="responseFormatter">The response formatter.</param>
        /// <param name="verifyToken">The verify token.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <returns><see cref="Response"/></returns>
        public static Response AsFacebookGetSubscription(this IResponseFormatter responseFormatter, string verifyToken, bool throwException = false)
        {
            try
            {
                var request = responseFormatter.Context.Request;

                string hubChallenge = request.Query[FacebookClient.SubscriptionHubChallengeKey];

                FacebookClient.VerifyGetSubscription(
                    request.Query[FacebookClient.SubscriptionHubModeKey],
                    request.Query[FacebookClient.SubscriptionHubVerifyTokenKey],
                    hubChallenge,
                    verifyToken);

                return new TextResponse(hubChallenge);
            }
            catch (Exception)
            {
                if (throwException)
                {
                    throw;
                }

                return new Response { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        /// <summary>
        /// Verifies the Facebook HTTP GET subscription and returns the approprite response.
        /// </summary>
        /// <param name="responseFormatter">The response formatter.</param>
        /// <param name="settings">The Facebook Realtime Subscription API settings.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <returns><see cref="Response"/></returns>
        public static Response AsFacebookGetSubscription(this IResponseFormatter responseFormatter, FacebookSubscriptionSettings settings, bool throwException = false)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            return responseFormatter.AsFacebookGetSubscription(settings.VerifyToken, throwException);
        }

        /// <summary>
        ///  Verifies the Facebook HTTP POST subscription and calls the callback when a new notification is received.
        /// </summary>
        /// <param name="responseFormatter">The response formatter.</param>
        /// <param name="appSecret">The Facebook app secret.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type to deserialize the json response into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        /// <returns><see cref="Response"/></returns>
        public static Response AsFacebookPostSubscription(this IResponseFormatter responseFormatter, string appSecret, Action<dynamic> callback, bool throwException = false, Type resultType = null, Func<string, Type, object> jsonDeserializer = null)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            var request = responseFormatter.Context.Request;
            var requestBodyString = new StreamReader(request.Body).ReadToEnd();

            try
            {
                var result = FacebookClient.VerifyPostSubscription(
                    request.Headers[FacebookClient.SubscriptionXHubSigntureRequestHeaderKey].FirstOrDefault(),
                    requestBodyString,
                    resultType,
                    appSecret,
                    jsonDeserializer ?? SimpleJson.DeserializeObject);

                callback(result);

                return HttpStatusCode.OK;
            }
            catch (Exception)
            {
                if (throwException) throw;
                return new Response { StatusCode = HttpStatusCode.BadRequest };
            }
        }

        /// <summary>
        ///  Verifies the Facebook HTTP POST subscription and calls the callback when a new notification is received.
        /// </summary>
        /// <param name="responseFormatter">The response formatter.</param>
        /// <param name="settings">The Facebook Realtime Subscription API settings.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type to deserialize the json response into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        /// <returns><see cref="Response"/></returns>
        public static object AsFacebookPostSubscription(this IResponseFormatter responseFormatter, FacebookSubscriptionSettings settings, Action<dynamic> callback, bool throwException = false, Type resultType = null, Func<string, Type, object> jsonDeserializer = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            return responseFormatter.AsFacebookPostSubscription(settings.AppSecret, callback, throwException, resultType, jsonDeserializer);
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="path">The route path.</param>
        /// <param name="condition">The route condition.</param>
        /// <param name="getSettings">Func to get <see cref="FacebookSubscriptionSettings"/>.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
            string path,
            Func<NancyContext, bool> condition,
            Func<dynamic, FacebookSubscriptionSettings> getSettings,
            Action<dynamic, dynamic> callback,
            bool throwException = false,
            Type resultType = null,
            Func<string, Type, object> jsonDeserializer = null)
        {
            module.Get[path, condition] =
                parameter =>
                {
                    FacebookSubscriptionSettings settings = getSettings(parameter);
                    if (settings == null)
                    {
                        return HttpStatusCode.NotFound;
                    }

                    return module.Response.AsFacebookGetSubscription(settings.VerifyToken, throwException);
                };

            module.Post[path, condition] =
                parameter =>
                {
                    FacebookSubscriptionSettings settings = getSettings(parameter);
                    if (settings == null)
                    {
                        return HttpStatusCode.NotFound;
                    }

                    return module.Response.AsFacebookPostSubscription(settings,
                        notification => callback(parameter, notification),
                        throwException, resultType, jsonDeserializer);
                };
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="path">The route path.</param>
        /// <param name="condition">The route condition.</param>
        /// <param name="settings">The Facebook Realtime Subscription API settings.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
            string path,
            Func<NancyContext, bool> condition,
            FacebookSubscriptionSettings settings,
            Action<dynamic, dynamic> callback,
            bool throwException = false,
            Type resultType = null,
            Func<string, Type, object> jsonDeserializer = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            module.SubscribeToFacebookRealtimeUpdates(path, condition, ctx => settings, callback, throwException, resultType, jsonDeserializer);
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="path">The route path.</param>
        /// <param name="condition">The route condition.</param>
        /// <param name="appSecret">The Facebook app secret.</param>
        /// <param name="verifyToken">The verify token.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
            string path,
            Func<NancyContext, bool> condition,
            string appSecret,
            string verifyToken,
            Action<dynamic, dynamic> callback,
            bool throwException = false,
            Type resultType = null,
            Func<string, Type, object> jsonDeserializer = null)
        {
            module.SubscribeToFacebookRealtimeUpdates(
                path,
                condition,
                new FacebookSubscriptionSettings(appSecret, verifyToken),
                callback,
                throwException,
                resultType, jsonDeserializer);
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="path">The route path.</param>
        /// <param name="getSettings">Func to get <see cref="FacebookSubscriptionSettings"/>.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
          string path,
          Func<dynamic, FacebookSubscriptionSettings> getSettings,
          Action<dynamic, dynamic> callback,
          bool throwException = false,
          Type resultType = null,
          Func<string, Type, object> jsonDeserializer = null)
        {
            module.Get[path] =
                parameter =>
                {
                    FacebookSubscriptionSettings settings = getSettings(parameter);
                    if (settings == null)
                    {
                        return HttpStatusCode.NotFound;
                    }

                    return module.Response.AsFacebookGetSubscription(settings.VerifyToken, throwException);
                };

            module.Post[path] =
                parameter =>
                {
                    FacebookSubscriptionSettings settings = getSettings(parameter);
                    if (settings == null)
                    {
                        return HttpStatusCode.NotFound;
                    }

                    return module.Response.AsFacebookPostSubscription(settings,
                        notification => callback(parameter, notification),
                        throwException, resultType, jsonDeserializer);
                };
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="path">The route path.</param>
        /// <param name="settings">The Facebook Realtime Subscription API settings.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
            string path,
            FacebookSubscriptionSettings settings,
            Action<dynamic, dynamic> callback,
            bool throwException = false,
            Type resultType = null,
            Func<string, Type, object> jsonDeserializer = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            module.SubscribeToFacebookRealtimeUpdates(path, ctx => settings, callback, throwException, resultType, jsonDeserializer);
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="path">The route path.</param>
        /// <param name="appSecret">The Facebook app secret.</param>
        /// <param name="verifyToken">The verify token.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
            string path,
            string appSecret,
            string verifyToken,
            Action<dynamic, dynamic> callback,
            bool throwException = false,
            Type resultType = null,
            Func<string, Type, object> jsonDeserializer = null)
        {
            module.SubscribeToFacebookRealtimeUpdates(
                path,
                new FacebookSubscriptionSettings(appSecret, verifyToken),
                callback,
                throwException,
                resultType, jsonDeserializer);
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="path">The route path.</param>
        /// <param name="settings">The Facebook Realtime Subscription API settings.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
            string path,
            FacebookSubscriptionSettings settings,
            Action<dynamic> callback,
            bool throwException = false,
            Type resultType = null,
            Func<string, Type, object> jsonDeserializer = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            module.SubscribeToFacebookRealtimeUpdates(path, ctx => settings, (parameter, result) => callback(result), throwException, resultType, jsonDeserializer);
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="path">The route path.</param>
        /// <param name="appSecret">The Facebook app secret.</param>
        /// <param name="verifyToken">The verify token.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
            string path,
            string appSecret,
            string verifyToken,
            Action<dynamic> callback,
            bool throwException = false,
            Type resultType = null,
            Func<string, Type, object> jsonDeserializer = null)
        {
            module.SubscribeToFacebookRealtimeUpdates(
                path,
                new FacebookSubscriptionSettings(appSecret, verifyToken),
                callback,
                throwException,
                resultType, jsonDeserializer);
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="settings">The Facebook Realtime Subscription API settings.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
            FacebookSubscriptionSettings settings,
            Action<dynamic> callback,
            bool throwException = false,
            Type resultType = null,
            Func<string, Type, object> jsonDeserializer = null)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            module.SubscribeToFacebookRealtimeUpdates("/", ctx => settings, (parameter, result) => callback(result), throwException, resultType, jsonDeserializer);
        }

        /// <summary>
        /// Subscribes to Facebook Realtime API.
        /// </summary>
        /// <param name="module">The <see cref="NancyModule"/>.</param>
        /// <param name="appSecret">The Facebook app secret.</param>
        /// <param name="verifyToken">The verify token.</param>
        /// <param name="callback">The callback to be called when a new notification is received.</param>
        /// <param name="throwException">Indicates whether to throw exception if verification fails.</param>
        /// <param name="resultType">The type ot deserialize json into.</param>
        /// <param name="jsonDeserializer">The json deserializer.</param>
        public static void SubscribeToFacebookRealtimeUpdates(this NancyModule module,
            string appSecret,
            string verifyToken,
            Action<dynamic> callback,
            bool throwException = false,
            Type resultType = null,
            Func<string, Type, object> jsonDeserializer = null)
        {
            module.SubscribeToFacebookRealtimeUpdates(
                "/",
                new FacebookSubscriptionSettings(appSecret, verifyToken),
                callback,
                throwException,
                resultType, jsonDeserializer);
        }
    }
}