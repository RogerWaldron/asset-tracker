using System.ComponentModel;
using System.Runtime.CompilerServices;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using BlazorApp.Interfaces;

namespace BlazorApp.Services
{
    public class AppStateService : IAppStateService
    {
        private readonly Supabase.Client _client;
        private bool _isLoading;
        private bool _isLoggedIn;
        public User? User => _client.Auth.CurrentUser;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AppStateService(Supabase.Client client) {
            _client = client;
            _client.Auth.AddStateChangedListener(AuthEventHandler);

            if (_client.Auth.CurrentUser != null)
              IsLoggedIn = true;
        }

        public bool IsLoading {
          get => _isLoading;
          set => SetField(ref _isLoading, value);
        }
       public bool IsLoggedIn {
          get => _isLoggedIn;
          private set => SetField(ref _isLoggedIn, value); 
        }


        public string? AvatarUrl {
          get {
            if (User != null && User.UserMetadata.TryGetValue("avatar_url", out var avatarUrl)) {
              return avatarUrl.ToString();
            }

            return null;
          }
        }

        private void AuthEventHandler(
          IGotrueClient<User, Session> sender,
          Constants.AuthState state
        ) {
          IsLoggedIn = state switch {
            Constants.AuthState.SignedIn => true,
            Constants.AuthState.SignedOut => false,
            _ => IsLoggedIn
          };
        }

        private void OnPropertyChanged(
          [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) 
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            OnPropertyChanged(propertyName);
        }

    }
}