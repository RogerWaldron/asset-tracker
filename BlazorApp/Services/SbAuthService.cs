using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Supabase.Gotrue;

namespace BlazorApp.Services;

public class SbAuthService
{
    private readonly Supabase.Client _client;
    private readonly AuthenticationStateProvider _sbAuthStateProvider;
    private readonly ILogger<SbAuthService> _logger;
    private readonly ILocalStorageService _localStorage;

    public SbAuthService(
        Supabase.Client client,
        AuthenticationStateProvider sbAuthStateProvider,
        ILogger<SbAuthService> logger,
        ILocalStorageService localStorage
        )
    {
        _client = client;
        _sbAuthStateProvider = sbAuthStateProvider;
        _logger = logger;
        _localStorage = localStorage;
    }

    public async Task Logout()
    {
        try
        {
            
            await _client.Auth.SignOut();
            _logger.LogInformation("AuthService - Logout");
            _localStorage.RemoveItem("token");
            await _sbAuthStateProvider.GetAuthenticationStateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, ex.Message);
        }
    }

    public async Task Login(string email, string pwd)
    {
        try
        {
            var user = await _client.Auth.SignIn(email, pwd);
            _logger.LogInformation("AuthService - Login Success");
            await _sbAuthStateProvider.GetAuthenticationStateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, ex.Message);
        }
    }

    public async Task<User?> GetUser()
    {
        var session = await _client.Auth.RetrieveSessionAsync();

        return session?.User;
    }
}