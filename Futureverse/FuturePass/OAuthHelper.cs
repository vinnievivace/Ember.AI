
namespace EmberAI.Futureverse.FuturePass
{
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class OAuthHelper
{
    /// <summary>
    /// Starts an OAuth2 PKCE Authorization Code flow using a loopback HTTP listener.
    /// Returns the authorization code, or throws on error/timeout.
    /// </summary>
    public static async Task<string> AuthenticateAsync(
        string authorizationEndpoint,
        string clientId,
        string scope,
        string redirectProxyUri = null,
        string codeChallengeMethod = "S256",
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        timeout ??= TimeSpan.FromMinutes(5);

        // 1) Generate state & PKCE verifier/challenge
        string state        = RandomDataBase64Url(32);
        string verifier     = RandomDataBase64Url(32);
        string challenge    = Base64UrlEncodeNoPadding(Sha256(verifier));

        // 2) Spin up a loopback HTTP listener on a free port
        int    port        = GetRandomUnusedPort();
        string redirectUri = $"http://127.0.0.1:{port}/";
        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        // 3) Build the authorization URL
        var queryParams = new Dictionary<string, string>
        {
            ["response_type"]        = "code",
            ["client_id"]            = clientId,
            ["scope"]                = scope,
            ["redirect_uri"]         = redirectProxyUri ?? redirectUri,
            ["state"]                = state,
            ["code_challenge"]       = challenge,
            ["code_challenge_method"]= codeChallengeMethod
        };
        string authUrl = authorizationEndpoint +
                         "?" +
                         string.Join("&", queryParams
                             .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));

        Debug.Log($"Opening browser for OAuth: {authUrl}");
        Application.OpenURL(authUrl);

        // 4) Wait for the incoming HTTP request or timeout
        var contextTask   = listener.GetContextAsync();
        var completedTask = await Task.WhenAny(contextTask, Task.Delay(timeout.Value, cancellationToken));
        if (completedTask != contextTask)
        {
            listener.Stop();
            throw new TimeoutException("OAuth redirect timed out.");
        }

        var context = contextTask.Result;

        // 5) Send a simple “you can close this” HTML page
        const string html = "<html><body><h1>Authentication complete.</h1>" +
                            "<p>You may now close this window.</p></body></html>";
        byte[] htmlBytes = Encoding.UTF8.GetBytes(html);
        context.Response.ContentLength64 = htmlBytes.Length;
        await context.Response.OutputStream.WriteAsync(htmlBytes, 0, htmlBytes.Length);
        context.Response.OutputStream.Close();
        listener.Stop();

        // 6) Extract & validate
        var req = context.Request;
        if (req.QueryString["error"] != null)
            throw new InvalidOperationException($"OAuth error: {req.QueryString["error"]}");
        if (req.QueryString["state"] != state)
            throw new InvalidOperationException($"Invalid state: {req.QueryString["state"]}");

        string code = req.QueryString["code"];
        if (string.IsNullOrEmpty(code))
            throw new InvalidOperationException("Authorization code not found.");

        Debug.Log("OAuth flow complete, code=" + code);
        return code;
    }

    // --- Helpers ---

    private static string RandomDataBase64Url(int length)
    {
        byte[] bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncodeNoPadding(bytes);
    }

    private static byte[] Sha256(string input)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(Encoding.ASCII.GetBytes(input));
    }

    private static string Base64UrlEncodeNoPadding(byte[] input) =>
        Convert.ToBase64String(input)
               .TrimEnd('=')
               .Replace('+', '-')
               .Replace('/', '_');

    private static int GetRandomUnusedPort()
    {
        var socket = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        socket.Start();
        int port = ((IPEndPoint)socket.LocalEndpoint).Port;
        socket.Stop();
        return port;
    }
}

}