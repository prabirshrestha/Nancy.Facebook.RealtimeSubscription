// This is a modifed version taken from the Facebook C# SDK library.
//
//-----------------------------------------------------------------------
// <copyright file="FacebookClient.Subscription.cs" company="The Outercurve Foundation">
//    Copyright (c) 2011, The Outercurve Foundation. 
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// <author>Nathan Totten (ntotten.com), Jim Zimmerman (jimzimmerman.com) and Prabir Shrestha (prabir.me)</author>
// <website>https://github.com/facebook-csharp-sdk/facbook-csharp-sdk</website>
//-----------------------------------------------------------------------

namespace Nancy.Facebook.RealtimeSubscription
{
    using System;
    using System.Text;

    class FacebookClient
    {
        public const string SubscriptionXHubSigntureRequestHeaderKey = "X-Hub-Signature";

        public const string SubscriptionHubChallengeKey = "hub.challenge";
        public const string SubscriptionHubVerifyTokenKey = "hub.verify_token";
        public const string SubscriptionHubModeKey = "hub.mode";

        private const string InvalidHttpXHubSignature = "Invalid " + SubscriptionXHubSigntureRequestHeaderKey + " request header";
        private const string InvalidHubChallenge = "Invalid " + SubscriptionHubChallengeKey;
        private const string InvalidVerifyToken = "Invalid " + SubscriptionHubVerifyTokenKey;
        private const string InvalidHubMode = "Invalid " + SubscriptionHubModeKey;

        /// <summary>
        /// Verify HTTP_X_HUB_SIGNATURE for HTTP GET.
        /// </summary>
        /// <param name="requestHubMode">The request hub.mode</param>
        /// <param name="requestVerifyToken">The request hub.verify_token</param>
        /// <param name="requestHubChallenge">The request hub.challenge</param>
        /// <param name="verifyToken">Expected verify token.</param>
        public static void VerifyGetSubscription(string requestHubMode, string requestVerifyToken, string requestHubChallenge, string verifyToken)
        {
            if (string.IsNullOrEmpty(verifyToken))
                throw new ArgumentNullException("verifyToken");

            if (requestHubMode == "subscribe")
            {
                if (requestVerifyToken == verifyToken)
                {
                    if (string.IsNullOrEmpty(requestHubChallenge))
                    {
                        throw new ArgumentException(InvalidHubChallenge, "requestHubChallenge");
                    }
                }
                else
                {
                    throw new ArgumentException(InvalidVerifyToken, requestVerifyToken);
                }
            }
            else
            {
                throw new ArgumentException(InvalidHubMode, "requestHubMode");
            }
        }

        /// <summary>
        /// Verify HTTP_X_HUB_SIGNATURE for HTTP POST.
        /// </summary>
        /// <param name="requestHttpXHubSignature">The request HTTP_X_HUB_SIGNATURE</param>
        /// <param name="requestBody">The request body.</param>
        /// <param name="resultType">The result type.</param>
        /// <param name="appSecret">The App secret.</param>
        public virtual object VerifyPostSubscription(string requestHttpXHubSignature, string requestBody, Type resultType, string appSecret, Func<string, Type, object> deserializeJson)
        {
            // httpXHubSignature looks somewhat like "sha1=4594ae916543cece9de48e3289a5ab568f514b6a"

            if (string.IsNullOrEmpty(appSecret))
                throw new ArgumentNullException("appSecret");

            if (!string.IsNullOrEmpty(requestHttpXHubSignature) && requestHttpXHubSignature.StartsWith("sha1="))
            {
                var expectedSha1 = requestHttpXHubSignature.Substring(5);

                if (string.IsNullOrEmpty(expectedSha1))
                {
                    throw new ArgumentException(InvalidHttpXHubSignature, requestHttpXHubSignature);
                }
                else
                {
                    if (string.IsNullOrEmpty(requestBody))
                    {
                        throw new ArgumentException(requestBody, "requestBody");
                    }

                    var sha1 = ComputeHmacSha1Hash(Encoding.UTF8.GetBytes(requestBody), Encoding.UTF8.GetBytes(appSecret));

                    var hashString = new StringBuilder();
                    foreach (var b in sha1)
                    {
                        hashString.Append(b.ToString("x2"));
                    }

                    if (expectedSha1 == hashString.ToString())
                    {
                        return deserializeJson(requestBody, resultType);
                    }

                    throw new ArgumentException(InvalidHttpXHubSignature, "requestHttpXHubSignature");
                }
            }
            else
            {
                throw new ArgumentException(InvalidHttpXHubSignature, requestHttpXHubSignature);
            }
        }

        private static byte[] ComputeHmacSha1Hash(byte[] data, byte[] key)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            if (key == null)
                throw new ArgumentNullException("key");

            using (var crypto = new System.Security.Cryptography.HMACSHA1(key))
            {
                return crypto.ComputeHash(data);
            }
        }
    }
}