
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetEnv;

public class MsalAuthenticationService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tenantId;
    private readonly string _redirectUri;
    private readonly string[] _scopes = [];

    private IConfidentialClientApplication _app;

    public MsalAuthenticationService()
    {
        // Load .env file
        Env.Load();

        // Read values from environment variables
        _clientId = Environment.GetEnvironmentVariable("MSAL_CLIENT_ID") 
            ?? throw new ArgumentNullException("MSAL_CLIENT_ID not found in environment variables");
        _clientSecret = Environment.GetEnvironmentVariable("MSAL_CLIENT_SECRET") 
            ?? throw new ArgumentNullException("MSAL_CLIENT_SECRET not found in environment variables");
        _tenantId = Environment.GetEnvironmentVariable("MSAL_TENANT_ID") 
            ?? throw new ArgumentNullException("MSAL_TENANT_ID not found in environment variables");
        _redirectUri = Environment.GetEnvironmentVariable("MSAL_REDIRECT_URI") 
            ?? "https://localhost";

        _scopes = [Environment.GetEnvironmentVariable("MSAL_SCOPE")];
            

        _app = ConfidentialClientApplicationBuilder
            .Create(_clientId)
            .WithAuthority(AzureCloudInstance.AzurePublic, _tenantId)
            .WithClientSecret(_clientSecret)
            .WithRedirectUri(_redirectUri)
            .Build();
    }

    public string GetAuthorizationUrl()
    {
        var authUrlBuilder = _app.GetAuthorizationRequestUrl(_scopes);
        return authUrlBuilder.ExecuteAsync().GetAwaiter().GetResult().ToString();
    }

    public async Task<AuthenticationResult> GetTokenFromAuthorizationCodeAsync(string authorizationCode)
    {
        if (string.IsNullOrEmpty(authorizationCode))
        {
            throw new ArgumentNullException(nameof(authorizationCode), "Authorization code cannot be null or empty");
        }

        try
        {
            // Extract just the code part if the full URL was pasted
            if (authorizationCode.Contains("code="))
            {
                var uri = new Uri(authorizationCode);
                var queryParams = System.Web.HttpUtility.ParseQueryString(uri.Query);
                authorizationCode = queryParams["code"] ?? authorizationCode;
            }

            var result = await _app.AcquireTokenByAuthorizationCode(_scopes, authorizationCode)
                .ExecuteAsync();
            return result;
        }
        catch (MsalException ex)
        {
            Console.WriteLine($"Error acquiring token: {ex.Message}");
            throw;
        }
    }
}

public class Program
{
    public static async Task Main()
    {
        Console.WriteLine("Starting authentication process...");
        var authService = new MsalAuthenticationService();

        // Step 1: Get the authorization URL and redirect user to it
        var authUrl = authService.GetAuthorizationUrl();
        Console.WriteLine("\nPlease go to this URL to authorize:");
        Console.WriteLine(authUrl);
        Console.WriteLine("\nAfter authentication, you will be redirected to a URL.");
        Console.WriteLine("Please paste the FULL redirect URL here (or just the code parameter):");

        // Step 2: Get the authorization code from the redirect URI
        var authCode = Console.ReadLine();

        // Step 3: Exchange the authorization code for tokens
        try
        {
            if (string.IsNullOrEmpty(authCode))
            {
                Console.WriteLine("Authorization code cannot be empty");
                return;
            }

            var authResult = await authService.GetTokenFromAuthorizationCodeAsync(authCode);
            Console.WriteLine($"\nAuthentication successful!");
            Console.WriteLine($"\nAccess Token: {authResult.AccessToken}");
            Console.WriteLine($"Token Type: {authResult.TokenType}");
            Console.WriteLine($"Expires On: {authResult.ExpiresOn}");
            Console.WriteLine($"Scopes: {string.Join(" ", authResult.Scopes)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError: {ex.Message}");
            Console.WriteLine("\nPlease check:");
            Console.WriteLine("1. Client ID is correct");
            Console.WriteLine("2. Client Secret is correct");
            Console.WriteLine("3. Redirect URI matches exactly what's registered in Azure AD");
            Console.WriteLine("4. The authorization code hasn't expired");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}