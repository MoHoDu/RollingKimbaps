using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
#if UNITY_ANDROID //&& !UNITY_EDITOR
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif

public static class AndroidPermissionChecker
{
    private static bool setPermissionGranted = false;
    private static Action<int, int> onLoadedPermission;
    private static Action<string> onPermissionDenied;
    private static Action<string> onPermissionDeniedAndDoNotAskAgain;
#if UNITY_ANDROID //&& !UNITY_EDITOR
    private static PermissionCallbacks permissionCallbacks;
    
    // 요청할 권한 목록
    private static readonly string[] RequiredPermissions = new[]
    {
        Permission.ExternalStorageWrite,
        Permission.ExternalStorageRead
    };
#endif
    
    // 안드로이드 33 이상부터는 알림 허용 퍼미션도 요청
    private static readonly string ShowPopupPermission = "android.permission.POST_NOTIFICATION";

    public static void SetCallbacks(Action<string> onDined, Action<string> onDeniedAndDoNotAskAgain)
    {
        onPermissionDenied = onDined;
        onPermissionDeniedAndDoNotAskAgain = onDeniedAndDoNotAskAgain;
    }

    // 권한 요청 메서드
    public static async UniTaskVoid InitPermission(Action<int, int> onLoading, Action<bool> onFinished = null)
    {
        if (setPermissionGranted) return;
        setPermissionGranted = true;
        onLoadedPermission = onLoading;
        
#if UNITY_ANDROID //&& !UNITY_EDITOR
        permissionCallbacks = new PermissionCallbacks();

        permissionCallbacks.PermissionDenied += permission =>
        {
            // 권한이 하나라도 없으면 앱 종료
            // Application.Quit();
            onPermissionDenied?.Invoke(permission);
        };
        
        permissionCallbacks.PermissionRequestDismissed += permission =>
        {
            Debug.Log($"권한 거부 및 '다시 묻지 않음' 선택됨: {permission}");
            // '다시 묻지 않음'이 선택되었을 때의 처리
            // 예: 사용자에게 설정에서 권한을 수동으로 허용하도록 안내
            onPermissionDeniedAndDoNotAskAgain?.Invoke(permission);
        };
        
        // 안드로이드 api 레벨 확인
        string androidInfo = SystemInfo.operatingSystem;
        int apiLevel = int.Parse(androidInfo.Substring(androidInfo.IndexOf("_") + 1, 2));
        
        // 권한 요청
        // if (apiLevel >= 33 && !Permission.HasUserAuthorizedPermission(ShowPopupPermission))
        // {
        //     PermissionCallbacks popupCallbacks = new PermissionCallbacks();
        //     popupCallbacks.PermissionDenied += onPermissionDenied;
        //     popupCallbacks.PermissionRequestDismissed += onPermissionDeniedAndDoNotAskAgain;
        //     popupCallbacks.PermissionGranted += async (permission) =>
        //     {
        //         // bool result = await RequestPermissions();
        //         // Application.focusChanged += OnApplicationFocusChanged;
        //         // onFinished?.Invoke(result);
        //         onFinished?.Invoke(true);
        //     };
        //     
        //     Permission.RequestUserPermission(ShowPopupPermission, popupCallbacks);
        // }
        // else
        if (apiLevel >= 30)
        {
            onFinished?.Invoke(true);
        }
        else
        {
            if (apiLevel >= 26)
            {
                var channel = new AndroidNotificationChannel()
                {
                    Id = "RollingKimbaps_Android_Permission",
                    Name = "pubsdk",
                    Importance = Importance.High,
                    Description = "권한 알림 팝업 표시 요청"
                };
                AndroidNotificationCenter.RegisterNotificationChannel(channel);
            }
            
            bool grantedPermissions = await RequestPermissions();
            // OnApplicationFocusChanged(true);

            // 포커스가 on될 때마다 권한 확인 후 처리
            Application.focusChanged += OnApplicationFocusChanged;
            
            onFinished?.Invoke(grantedPermissions);
        }
#else 
        onFinished?.Invoke(true);
#endif
    }

#if UNITY_ANDROID //&& !UNITY_EDITOR
    // 앱에서 권한 요청
    private static async UniTask<bool> RequestPermissions()
    {
        int maxCount = RequiredPermissions.Length;
        int onLoadedCount = 0;
        
        onLoadedPermission?.Invoke(onLoadedCount, maxCount);
        foreach (var permission in RequiredPermissions)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission, permissionCallbacks);
            }

            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                return false;
            }

            onLoadedCount++;
            onLoadedPermission?.Invoke(onLoadedCount, maxCount);
            
            await UniTask.Delay(100);
        }
        
        return true;
    }
    
    // 앱이 포커스를 다시 얻었을 때 권한 확인
    private static void OnApplicationFocusChanged(bool hasFocus)
    {
        if (hasFocus)
        {
            foreach (var permission in RequiredPermissions)
            {
                if (!Permission.HasUserAuthorizedPermission(permission))
                {
                    // 권한이 하나라도 없으면 앱 종료
                    Application.Quit();
                    return;
                }
            }

            // 모든 권한이 부여되었으면 이벤트 제거
            Application.focusChanged -= OnApplicationFocusChanged;
        }
    }
#endif
}