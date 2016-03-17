using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class Example : MonoBehaviour {
    
    public Text OutputText;
    public Button LoginButton;
    public Button GetTokenButton;

	// Use this for initialization
	void Start () {
	    LoginButton.onClick.AddListener(OnLoginButtonPress);
        GetTokenButton.onClick.AddListener(OnGetTokenButtonPress);

        LoginWithAmazonAuthManager.LoginOnSuccess += OnLoginSuccess;
        LoginWithAmazonAuthManager.LoginOnFailure += OnLoginError;
        LoginWithAmazonAuthManager.LoginOnCancel += OnLoginCancel;
        LoginWithAmazonAuthManager.GetTokenOnSuccess += OnGetTokenSuccess;
        LoginWithAmazonAuthManager.GetTokenOnFailure += OnGetTokenError;
        LoginWithAmazonAuthManager.GetTokenOnCancel += OnGetTokenCancel;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnLoginButtonPress()
    {
        OutputText.text = "OnLoginButtonPress";
        LoginWithAmazonAuthManager.Authorize();
    }
    
    void OnGetTokenButtonPress()
    {   
        if(LoginWithAmazonAuthManager.IsAuthed()) {
            LoginWithAmazonAuthManager.GetToken();
        } else
        {
            OutputText.text = "Can't get token without logging in";
        }
    }

    void OnLoginCancel()
    {
        OutputText.text = "Login was cancelled";
    }

    void OnGetTokenCancel()
    {
        OutputText.text = "GetToken was cancelled";
    }

    void OnLoginError(string error)
    {
        OutputText.text = "Login Exception: "+ error;
    }

    void OnGetTokenError(string error)
    {
        OutputText.text = "GetToken Exception: "+ error;
    }
    
    void OnLoginSuccess()
    {
        OutputText.text = "Login successful";
    }

    void OnGetTokenSuccess(string token)
    {
        OutputText.text = "OAuth token: "+ token;
    }
    
}
