using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace BlazorApp.Providers;

public class SbAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly ILogger<SbAuthStateProvider> _logger;
    private readonly Supabase.Client _client;


    private AuthenticationState AnonymousState => new(new ClaimsPrincipal(new ClaimsIdentity()));

    public SbAuthStateProvider(ILogger<SbAuthStateProvider> logger, Supabase.Client client)
    {
        _logger = logger;
        _client = client;
        _client.Auth.AddStateChangedListener(SbAuthStateChanged);
    }

    public void Dispose()
    {
        _client.Auth.RemoveStateChangedListener(SbAuthStateChanged);
    }

    private void SbAuthStateChanged(
        IGotrueClient<User, Session> sender, 
        Constants.AuthState state)
    {
        switch (state)
        {
            case Constants.AuthState.SignedIn:
                NotifyAuthenticationStateChanged(Task.FromResult(AuthenticatedState));
                break;
            case Constants.AuthState.SignedOut:
                NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
                break;
        }   
    }

    private AuthenticationState AuthenticatedState
    {
        get
        {
            var user = _client.Auth.CurrentUser;

            if (user is null)
                return AnonymousState;

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Role, user.Role!),
                new(ClaimTypes.Authentication, "supabase")
            };

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "supabase")));
        }    
    }
    
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _logger.LogInformation("GetAuthenticationState");

        _client.Auth.LoadSession();

        if (_client.Auth.CurrentUser is null)
        {
            _logger.LogInformation("GetAuthenticationState returned Anonymous because no authenticaed user was found");
            
            return Task.FromResult(AnonymousState);
        }
        
        return Task.FromResult(AuthenticatedState);
    }
}