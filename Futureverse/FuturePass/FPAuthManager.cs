using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Core;
using EmberAI.Attributes;
using UnityEngine;
using UnityEngine.Serialization;


namespace EmberAI.Futureverse.FuturePass
{
    public class FPAuthManager : EmberSingleton<FPAuthManager>
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        public static event Action OpeningBrowser = delegate { };
        public static event Action ExchangingTokens = delegate { };
        public static event Action<Dictionary<string, string>> TokenResponse = delegate { };
        public static event Action<Dictionary<string, string>> RefreshTokenResponse = delegate { };
        public static event Action ExchangingUserInfo = delegate { };
        public static event Action<Dictionary<string, string>> UserInfoResponse = delegate { };
        public static event Action<Dictionary<string, string>> FinalResponse = delegate { };
        
        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        

        // state and PKCE values
        private const string scope = "openid%20offline_access";
        private const string code_challenge_method = "S256";

        private const string _plus = "+";
        private const string _minus = "-";
        private const string _slash = "/";
        private const string _bar = "_";
        private const string _equal = "=";
        private const string _empty = "";

        private readonly string responseHtml =
            "<html><head><meta http-equiv='refresh' content='10;url=https://login.futureverse.app/'><style>body {background-color:#000000;font-size:32px;font-weight:bold;text-align:left;color:#ffffff;font-family: Roboto, Helvetica, Arial, sans-serif;}</style></head><body>You can close this page now and return to the game.</body></html>";

        
        
        private Dictionary<string, string> _tokenEndpointDecoded = new();
        private Dictionary<string, string> _userinfoResponseTextData = new();
        private readonly Dictionary<string, string> _infoDecoded = new();
        private const string ClientSecret = "null";
        
        [BoxGroup("Settings"), PlayerPref, SerializeField]
        private string ClientID;
        
        [BoxGroup("Settings"), PlayerPref, SerializeField]
        private string RedirectURI = "https://f8jkcqhg37.execute-api.us-east-1.amazonaws.com/prod/auth/proxy";
        
        [BoxGroup("API"), SerializeField, ReadOnly]
        private string AUTHEndpoint = "https://login.futureverse.app/auth";
        
        [BoxGroup("API"), SerializeField, ReadOnly]
        private string tokenEndpoint = "https://login.futureverse.app/token";
        
        [BoxGroup("API"), SerializeField, ReadOnly]
        private string userInfoEndpoint = "https://login.futureverse.app/me";
        
        [BoxGroup("API"), SerializeField, ReadOnly]
        private string userLogoutEndpoint = "https://login.futureverse.app/session/end";
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Static .................................................................................................

        #endregion

        #region Inspector ..............................................................................................

        #endregion

        #region Initialization .........................................................................................

        #endregion

        #region MonoBehaviours .........................................................................................

        protected override void OnAwake()
        {
            base.OnAwake();
            
            FPassAuth(true);
        }
        
        #endregion

        #region General ................................................................................................

        public async void FPassAuth(bool useProxy = false)
        {
            // Generates state and PKCE values.
            string state = RandomDataBase64url(32);
            string code_verifier = RandomDataBase64url(32);

            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, 51772);

            // Creates an HttpListener to listen for requests on that redirect URI.
            HttpListener http = new();
            http.Prefixes.Add(redirectURI);
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}?response_type=code&scope={1}&redirect_uri={2}&client_id={3}&state={4}&code_challenge={5}&code_challenge_method={6}",
                AUTHEndpoint,
                scope,
                Uri.EscapeDataString(useProxy ? RedirectURI : redirectURI),
                ClientID,
                state,
                Base64urlencodeNoPadding(Sha256(code_verifier)),
                code_challenge_method);

            // Opens request in the browser.
            OpeningBrowser();
            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            HttpListenerContext context = await http.GetContextAsync();
            
            Debug.LogWarning(context.Response.ToString());

            // Sends an HTTP response to the browser.
            HttpListenerResponse response = context.Response;
            byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
            response.ContentLength64 = buffer.Length;
            Stream responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                Console.Write(string.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
                return;
            }
            if (context.Request.QueryString.Get("code") == null ||
                context.Request.QueryString.Get("state") == null)
            {
                Console.Write("Malformed authorization response. " + context.Request.QueryString);
                return;
            }

            // extracts the code
            string incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                Console.Write(string.Format("Received request with invalid state ({0})", incoming_state));
                return;
            }

            // Starts the code exchange at the Token Endpoint.
            ExchangeTokens(context.Request.QueryString.Get("code"), code_verifier, useProxy ? RedirectURI : redirectURI);
        }

        public void Logout()
        {
            OpeningBrowser();

            System.Diagnostics.Process.Start(userLogoutEndpoint);
        }

        private async void ExchangeTokens(string code, string code_verifier, string redirectURI)
        {
            ExchangingTokens();

            // builds the request
            //string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=openid%20offline_access&grant_type=authorization_code",
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&grant_type=authorization_code",
                code,
                Uri.EscapeDataString(redirectURI),
                ClientID,
                code_verifier,
                ClientSecret
                );

            // sends the request
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            //tokenRequest.Accept = "Accept=application/json;charset=UTF-8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using StreamReader reader = new(tokenResponse.GetResponseStream());
                // reads response body
                string responseText = await reader.ReadToEndAsync();

                // converts to dictionary
                _tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);
                TokenResponse(_tokenEndpointDecoded);

                ExchangeUserinfo(_tokenEndpointDecoded["access_token"]);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    if (ex.Response is HttpWebResponse response)
                    {
                        Console.Write("HTTP: " + response.StatusCode);
                        using StreamReader reader = new(response.GetResponseStream());
                        // reads response body
                        string responseText = await reader.ReadToEndAsync();
                        Console.Write(responseText);
                    }
                }
            }
        }

        private async void ExchangeUserinfo(string access_token)
        {
            ExchangingUserInfo();

            // sends the request
            HttpWebRequest userinfoRequest = (HttpWebRequest)WebRequest.Create(userInfoEndpoint);
            userinfoRequest.Method = "GET";
            userinfoRequest.Headers.Add(string.Format("Authorization: Bearer {0}", access_token));
            userinfoRequest.ContentType = "application/x-www-form-urlencoded";
            //userinfoRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            // gets the response
            WebResponse userinfoResponse = await userinfoRequest.GetResponseAsync();
            using StreamReader userinfoResponseReader = new(userinfoResponse.GetResponseStream());

            // reads response body
            string userinfoResponseText = await userinfoResponseReader.ReadToEndAsync();

            _userinfoResponseTextData = JsonConvert.DeserializeObject<Dictionary<string, string>>(userinfoResponseText);
            UserInfoResponse(_userinfoResponseTextData);
            _infoDecoded.Add("access_token", access_token);
            _infoDecoded.Add("eoa", _userinfoResponseTextData["eoa"]);
            FinalResponse(_infoDecoded);
        }

        public async void RefreshAccessToken(string refreshToken)
        {
            // Build the Request Body
            string refreshTokenRequestBody = string.Format("refresh_token={0}&client_id={1}&grant_type=refresh_token",
                refreshToken,
                ClientID);

            // Send Request for new Access Token
            HttpWebRequest refreshRequest = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
            refreshRequest.Method = "POST";
            refreshRequest.ContentType = "application/x-www-form-urlencoded";
            byte[] refreshTokenBytes = Encoding.ASCII.GetBytes(refreshTokenRequestBody);
            refreshRequest.ContentLength = refreshTokenBytes.Length;

            await using (Stream stream = await refreshRequest.GetRequestStreamAsync())
            {
                await stream.WriteAsync(refreshTokenBytes, 0, refreshTokenBytes.Length);
            }

            try
            {
                // Get Response from request
                WebResponse refreshResponse = await refreshRequest.GetResponseAsync();
                using StreamReader reader = new(refreshResponse.GetResponseStream()!);

                // Read Response Body
                string responseText = await reader.ReadToEndAsync();

                // Deserialize to Dicitonary
                Dictionary<string, string> refreshedTokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);

                RefreshTokenResponse(refreshedTokenData);
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    if (ex.Response is HttpWebResponse response)
                    {
                        Console.Write("HTTP: " + response.StatusCode);
                        using StreamReader reader = new(response.GetResponseStream()!);

                        // reads response body
                        string responseText = await reader.ReadToEndAsync();
                        Console.Write(responseText);
                    }
                }

                RefreshTokenResponse(null);
            }
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        private string RandomDataBase64url(uint length)
        {
            RNGCryptoServiceProvider rng = new();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64urlencodeNoPadding(bytes);
        }

        /// <summary>
        /// Returns the SHA256 hash of the input string.
        /// </summary>
        private byte[] Sha256(string inputStirng)
        {
            SHA256Managed sha256 = new();
            return sha256.ComputeHash(Encoding.ASCII.GetBytes(inputStirng));
        }

        /// <summary>
        /// Base64url no-padding encodes the given input buffer.
        /// </summary>
        private string Base64urlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace(_plus, _minus);
            base64 = base64.Replace(_slash, _bar);
            // Strips padding.
            base64 = base64.Replace(_equal, _empty);

            return base64;
        }
        
        #endregion

        #region Event Handlers .........................................................................................

        #endregion

        #endregion
    }
}