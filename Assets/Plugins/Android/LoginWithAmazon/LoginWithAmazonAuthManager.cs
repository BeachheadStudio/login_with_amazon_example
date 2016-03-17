using System;
using UnityEngine;
using System.Collections;

public class LoginWithAmazonAuthManager : MonoBehaviour
{
    #region static strings

    private static readonly string MANAGER_CLASS_NAME = "com.amazon.identity.auth.device.authorization.api.AmazonAuthorizationManager";
    private static readonly string PROXY_CLASS_NAME = "com.amazon.identity.auth.device.authorization.api.AuthorizationListener";
    private static readonly string CONSTANT_CLASS_NAME = "com.amazon.identity.auth.device.authorization.api.AuthzConstants$BUNDLE_KEY";
    public static readonly string[] APP_AUTH_SCOPES = { "profile" };

    #endregion

    #region event handlers

    public static event Action LoginOnSuccess;

    public static event Action<string> LoginOnFailure;

    public static event Action LoginOnCancel;

    public static event Action<string> GetTokenOnSuccess;

    public static event Action<string> GetTokenOnFailure;

    public static event Action GetTokenOnCancel;

    #endregion

    #region listener proxies

    class AuthListener : AndroidJavaProxy
    {
        public AuthListener() : base(PROXY_CLASS_NAME)
        {
            Log("Starting AuthListener");
        }

        public void onSuccess(AndroidJavaObject bundle)
        {
            Log("AuthListener onSuccess");
			
            authed = true;
			
            if (LoginOnSuccess != null)
            {
                LoginOnSuccess();
            }
        }

        public void onError(AndroidJavaObject exception)
        {
            AndroidJavaClass errorEnum = exception.Call<AndroidJavaClass>("getType");
            
            Log("AuthListener onError", errorEnum.Call<string>("toString"), exception.Call<string>("toString"));
            
            if (LoginOnFailure != null)
            {
                LoginOnFailure(exception.Call<string>("toString"));
            }
        }

        public void onCancel(AndroidJavaObject cause)
        {
            Log("AuthListener onCancel");
            if (LoginOnCancel != null)
            {
                LoginOnCancel();
            }
        }
    }

    class GetTokenListener : AndroidJavaProxy
    {
        public GetTokenListener() : base(PROXY_CLASS_NAME)
        {
            Log("Starting GetTokenListener");
        }

        public void onSuccess(AndroidJavaObject bundle)
        {
            Log("GetTokenListener onSuccess");

            // in java: bundle.getString(AuthzConstants.BUNDLE_KEY.TOKEN.val) != null
            using (AndroidJavaClass clazz = new AndroidJavaClass(CONSTANT_CLASS_NAME))
            using (AndroidJavaObject obj2 = clazz.CallStatic<AndroidJavaObject>("valueOf", "TOKEN"))
            {
                string value = obj2.Get<string>("val");
                if (value == null)
                {
                    Log("GetTokenListener onSuccess could not get azm constant value!");
                    return;
                }

                string token = bundle.Call<string>("getString", value);
                if (token == null)
                {
                    Log("GetTokenListener onSuccess returned an empty token!");
                    return;
                }

                if (GetTokenOnSuccess != null)
                {
                    GetTokenOnSuccess(token);
                }	
            }
        }

        public void onError(AndroidJavaObject exception)
        {
            Log("GetTokenListener onError");
            if (GetTokenOnFailure != null)
            {
                GetTokenOnFailure(exception.Call<string>("toString"));
            }
        }

        public void onCancel(AndroidJavaObject cause)
        {
            Log("GetTokenListener onCancel");
            if (GetTokenOnCancel != null)
            {
                GetTokenOnCancel();
            }
        }
    }

    #endregion

    #region class params

    private static AndroidJavaObject JavaObject;
    private static bool authed = false;

    #endregion

    #region class methods

    static LoginWithAmazonAuthManager()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            if (currentActivity == null)
            {
                throw new System.Exception("Could not get currentActivity to use");
            }
            
            JavaObject = new AndroidJavaObject(MANAGER_CLASS_NAME, currentActivity, null);
	
            if (JavaObject.GetRawClass() == IntPtr.Zero)
            {
                LoginWithAmazonAuthManager.Log("No java class " + MANAGER_CLASS_NAME + " present, can't use LoginWithAmazonAuthManager");
                return;
            }
        }
    }

    public static void Authorize()
    {
        using (AndroidJavaClass bundle = new AndroidJavaClass("android.os.Bundle"))
        using (AndroidJavaObject emptyBundle = bundle.GetStatic<AndroidJavaObject>("EMPTY"))
        {
            AuthListener authListener = new AuthListener();
            
            if (authListener.javaInterface.GetRawObject().ToInt32() == 0)
            {
                Log("AuthListener raw object is null");
            }

            JavaObject.Call<AndroidJavaObject>("authorize", APP_AUTH_SCOPES, emptyBundle, authListener);
        }
    }

    public static void GetToken()
    {
        if (!authed)
        {
            Debug.LogError("Cannot call GetToken before a successful Authorize call!");
            return;	
        }

        JavaObject.Call<AndroidJavaObject>("getToken", APP_AUTH_SCOPES, new GetTokenListener());
    }

    public static void Log(string message, params object[] args)
    {
        if (args == null || args.Length == 0)
        {
            Debug.Log(string.Format("LoginWithAmazonAuthManager: {0}", message));
        } else
        {
            Debug.Log(string.Format("LoginWithAmazonAuthManager: {0}", message));
            foreach (object obj in args)
            {
                Debug.Log(string.Format("{0}", obj));
            }
        }
    }

    public static bool IsAuthed()
    {
        return authed;
    }

    void OnDestroy()
    {
        JavaObject.Dispose();
    }

    #endregion

}