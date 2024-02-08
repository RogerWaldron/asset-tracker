using BlazorApp.Interfaces;
using BlazorApp.Providers;
using BlazorApp.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Postgrest.Interfaces;
using Supabase;

namespace BlazorApp.Extensions;
public static class SupabaseExtensions {

  public static void AddSupabaseServices(this IServiceCollection services) {

  // Register Supabase
    services.AddScoped<AuthenticationStateProvider, SbAuthStateProvider>(
      provider => new SbAuthStateProvider(
        provider.GetRequiredService<ILogger<SbAuthStateProvider>>(),
        provider.GetRequiredService<Supabase.Client>()
      )
    );

    try {
      var url = Environment.GetEnvironmentVariable("SUPABASE_URL") ?? "";
      var key = Environment.GetEnvironmentVariable("SUPABASE_KEY") ?? "";
    
      var options = new SupabaseOptions {
        AutoRefreshToken = true,
        AutoConnectRealtime = true,
        // SessionHandler = new SupabaseSessionProvider(),
      }; 
      services.AddScoped<Supabase.Client>(provider => new Supabase.Client(url, key, options));
    }
    catch (Exception)
    {
      throw new Exception("Failed to read Supabase environment variables");
    }
    
    services.AddScoped<SbAuthService>();
    services.AddScoped<SbStorageService>();
    services.AddScoped<IAppStateService>(p => new AppStateService(p.GetRequiredService<Supabase.Client>()));
  
    // Register postgrest cache provider, comes with Supabase
  
    // services.AddScoped<IPostgrestCacheProvider, PostgrestCacheProvider>();

  }

} 