using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public bool IsFirebaseReady { get; private set; }
    public bool IsSignInOnProgress { get; private set; }

    //Main Panel Objects
    public InputField emailField;
    public InputField passwordField;
    public Button signInReqButton;
    public Button signUpReqButton;
    public GameObject signUpPannel;

    //Sign Panel Objects
    public InputField signUpEmailField;
    public InputField signUpPasswordField;
    public Button backButton;
    public Button signUpButton;
    public Text WarningText;

    public static FirebaseApp firebaseApp;
    public static FirebaseAuth firebaseAuth;
    
    public static FirebaseUser User;
    
    public void Start()
    {
        //버튼 비활성화
        signInReqButton.interactable = false;
        signUpReqButton.interactable = false;
        
        //앱또는 환경이 파이어베이스 구동할 수 있는지 확인
        //안될 경우 dependency를 fix할 수 있다면 자동 fix
        //둘 다 안되면 log메세지로 알려줌
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            //구동가능한지, dependency fix할 수 있는 지 확인후 실행
            var result = task.Result;
            if (result != DependencyStatus.Available)
            {
                Debug.LogError(result.ToString());
                IsFirebaseReady = false;
            }
            else
            {
                IsFirebaseReady = true;

                firebaseApp = FirebaseApp.DefaultInstance;
                firebaseAuth = FirebaseAuth.DefaultInstance;
            }

            signInReqButton.interactable = IsFirebaseReady;
            signUpReqButton.interactable = IsFirebaseReady;
        }
        );
    }

    //main panel에서 신규 가입 버튼을 누르면 실행
    public void clickSignUpRequestButton()
    {
        signUpPannel.SetActive(true);
    }
    
    //main panel에서 로그인 버튼을 누르면 실행
    public void clickSignInRequestButton()
    {
        //파이어베이스가 준비되지 않았거나 로그인 중이거나 유저정보가 이미 있다면
        if(!IsFirebaseReady || IsSignInOnProgress || User!= null)
        {
            return;
        }

        IsSignInOnProgress = true;
        signInReqButton.interactable = false;
        
        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread((task) =>
        {
            //로그인 시도 후 실행
            Debug.Log($"Sign in status:{task.Status}");
            IsSignInOnProgress = false;
            signInReqButton.interactable = true;

            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
            else if (task.IsCanceled)
            {
                Debug.LogError("Sign In Canceled");
            }
            else
            {
                User = task.Result;
                Debug.Log(User.Email);
                SceneManager.LoadScene("Scenes/Lobby");
            }
        });
    }
 
    //Sign Up Pannel에서 뒤로가기 버튼 클릭 시 실행
    public void clickBackButton()
    {
        signUpPannel.SetActive(false);
    }

    //Sign Up Pannel에서 가입하기 버튼 클릭 시 실행
    public void clickSignUpButton()
    {
        backButton.interactable = false;
        signUpButton.interactable = false;

        firebaseAuth.CreateUserWithEmailAndPasswordAsync(signUpEmailField.text, signUpPasswordField.text).ContinueWithOnMainThread((task) =>
        {
            if (task.IsFaulted)
            {
                WarningText.text = "이미 존재하거나 잘못된 이메일입니다";
            }
            else if (task.IsCanceled)
            {
                WarningText.text = "가입이 취소되었습니다";
            }
            else
            {
                signUpPannel.SetActive(false);
                return;
            }
            backButton.interactable = true;
            signUpButton.interactable = true;
        });
    }
}