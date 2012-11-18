namespace Nancy.Facebook.RealtimeSubscription
{
    /// <summary>
    /// Settings required for Facebook Realttime Subscription API.
    /// </summary>
    public class FacebookSubscriptionSettings
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FacebookSubscriptionSettings"/>.
        /// </summary>
        public FacebookSubscriptionSettings()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="FacebookSubscriptionSettings"/>.
        /// </summary>
        /// <param name="appSecret">The app secret.</param>
        /// <param name="verifyToken">The verify token.</param>
        public FacebookSubscriptionSettings(string appSecret, string verifyToken)
        {
            this.AppSecret = appSecret;
            this.VerifyToken = verifyToken;
        }

        /// <summary>
        /// The app secret.
        /// </summary>
        public string AppSecret { get; set; }

        /// <summary>
        /// The verify token.
        /// </summary>
        public string VerifyToken { get; set; }

    }
}