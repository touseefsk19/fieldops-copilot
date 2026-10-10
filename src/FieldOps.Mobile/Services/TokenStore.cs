namespace FieldOps.Mobile.Services;

// DEV ONLY: holds a token made by `dotnet user-jwts`.
// Real sign-in (Microsoft Entra ID + SecureStorage) replaces this in a later part.
public class TokenStore
{
    private const string Key = "dev_jwt";

    public string? Token
    {
        get => Preferences.Default.Get<string?>(Key, null);
        set
        {
            if (string.IsNullOrWhiteSpace(value)) Preferences.Default.Remove(Key);
            else Preferences.Default.Set(Key, value.Trim());
        }
    }
}