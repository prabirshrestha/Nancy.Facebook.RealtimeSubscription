namespace Nancy.Facebook.RealtimeSubscription.Extensions
{
    using System;
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
    }
}