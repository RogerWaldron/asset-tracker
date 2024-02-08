using Microsoft.JSInterop;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace BlazorApp.Providers;

public class SbSessionProvider : IGotrueSessionPersistence<Session>
{
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<SbSessionProvider> _logger;
    private const string SessionKey = "SUPABASE_SESSION";


    public SbSessionProvider(ILocalStorageService localStorage, ILogger<SbSessionProvider> logger)
    {
        _localStorage = localStorage;
        _logger = logger;
    }

    public void SaveSession(Session session)
    {
        try
        {
            var serialized = JsonConvert.SerializeObject(session);
            _localStorage.SetItem(SessionKey, session);
        }
        catch (Exception)
        {
            _logger.LogError("Exception - Session Save");
        }
    }

    public void DestroySession()
    {
        _logger.LogInformation("Session Destroy");
        _localStorage.RemoveItem(SessionKey);
    }

    public Session? LoadSession()
    {
        try
        {
            var json = _localStorage.GetItem<string>(SessionKey);

            if (string.IsNullOrEmpty(json))
                return null;

            var session = JsonConvert.DeserializeObject<Session>(json);
            
            return session?.ExpiresAt() <= DateTime.Now ? null : session;
        }
        catch (Exception)
        {
            _logger.LogError("Exception - Session Load");
            return null;
        }

    }
}