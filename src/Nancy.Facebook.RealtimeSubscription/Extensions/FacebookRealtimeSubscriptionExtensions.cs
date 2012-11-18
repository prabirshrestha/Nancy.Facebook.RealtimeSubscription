namespace Nancy.Facebook.RealtimeSubscription.Extensions
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

        public static object AsFacebookPostSubscription(this IResponseFormatter responseFormatter, string appSecret, Func<object, object> callback, bool throwsException = false)
        {
            var request = responseFormatter.Context.Request;
            var requestBodyString = new StreamReader(request.Body).ReadToEnd();

            try
            {
                var result = FacebookClient.VerifyPostSubscription(
                    request.Headers[FacebookClient.SubscriptionXHubSigntureRequestHeaderKey].FirstOrDefault(),
                    requestBodyString,
                    null,
                    appSecret,
                    null);

                if (callback != null)
                {
                    return callback(result);
                }

                return HttpStatusCode.OK;
            }
            catch (Exception)
            {
                if (throwsException) throw;
                return new Response { StatusCode = HttpStatusCode.NotFound };
            }
        }
    }
}