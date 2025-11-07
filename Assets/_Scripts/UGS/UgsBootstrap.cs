using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

public static class UgsBootstrap
{
    static bool _initialized;

    public static async Task InitAsync()
    {
        if (_initialized) return;

        // 1) Environment'ý sabitle
        var opts = new InitializationOptions()
            .SetOption("com.unity.services.core.environment-name", "production");

        await UnityServices.InitializeAsync(opts);

        // 2) EDITOR'de benzersiz profil adý kullan (her editör farklý olsun)
#if UNITY_EDITOR
        string profile = $"editor_{System.Diagnostics.Process.GetCurrentProcess().Id}";
        try
        {
            // Yeni API (varsa)
            AuthenticationService.Instance.SwitchProfile(profile);
        }
        catch { /* eski sürümde yoksa sorun deðil */ }
#endif

        // 3) Eski cached token'ý temizle (ayný PlayerId'yi önler)
        try { AuthenticationService.Instance.SignOut(true); } catch { }

        // 4) Anonim giriþ
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        _initialized = true;
        Debug.Log($"[UGS] Init OK. SignedIn={AuthenticationService.Instance.IsSignedIn} " +
                  $"PlayerId={AuthenticationService.Instance.PlayerId} " +
                  $"ProjectID={Application.cloudProjectId}" +
#if UNITY_EDITOR
                  $" Profile={profile}"
#endif
        );
    }
}
