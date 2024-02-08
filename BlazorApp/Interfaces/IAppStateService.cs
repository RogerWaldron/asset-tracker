using System.ComponentModel;
using Supabase.Gotrue;

namespace BlazorApp.Interfaces;

public interface IAppStateService : INotifyPropertyChanged
{
    User? User { get; }
    string? AvatarUrl { get; }
    bool IsLoading { get;  }
    bool IsLoggedIn { get; }
}