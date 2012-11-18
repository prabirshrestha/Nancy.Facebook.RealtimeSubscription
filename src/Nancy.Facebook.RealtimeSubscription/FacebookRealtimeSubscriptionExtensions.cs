namespace Nancy.Facebook.RealtimeSubscription
{
    using System;
    using System.IO;
    using System.Linq;
    using Nancy.Responses;

    public static class FacebookRealtimeSubscriptionExtensions
    {
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

        public static object AsFacebookPostSubscription(this IResponseFormatter responseFormatter, string appSecret, Func<object, object> callback, bool throwException = false, Type resultType = null, Func<string, Type, object> jsonDeserializer = null)
        {
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

                if (callback != null)
                {
                    return callback(result);
                }

                return HttpStatusCode.OK;
            }
            catch (Exception)
            {
                if (throwException) throw;
                return new Response { StatusCode = HttpStatusCode.NotFound };
            }
        }

        public static object AsFacebookPostSubscription<T>(this IResponseFormatter responseFormatter, string appSecret, Func<T, object> callback, bool throwException = false, Func<string, Type, object> jsonDeserializer = null)
        {
            return responseFormatter.AsFacebookPostSubscription(
                appSecret,
                x =>
                {
                    var y = (T)x;

                    if (callback != null)
                    {
                        return callback(y);
                    }

                    return HttpStatusCode.OK;
                },
                throwException,
                typeof(T),
                jsonDeserializer);
        }
    }
}