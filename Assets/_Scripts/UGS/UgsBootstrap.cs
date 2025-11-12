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

        var opts = new InitializationOptions()
            .SetOption("com.unity.services.core.environment-name", "production");

        await UnityServices.InitializeAsync(opts);


#if UNITY_EDITOR
        string profile = $"editor_{System.Diagnostics.Process.GetCurrentProcess().Id}";
        try
        {
            
            AuthenticationService.Instance.SwitchProfile(profile);
        }
        catch {  }
#endif


        try { AuthenticationService.Instance.SignOut(true); } catch { }


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
