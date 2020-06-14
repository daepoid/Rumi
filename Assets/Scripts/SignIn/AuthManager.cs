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
    public bool IsSignUpOnProgress { get; private set; }
    public bool IsPwdResetOnProgress { get; private set; }

    //Main Panel Objects
    public InputField emailField;
    public InputField passwordField;
    public Button signInReqButton;
    public Button signUpReqButton;
    public Button pwdResetReqButton;
    public Text MainStateText;

    //Sign Panel Objects
    public GameObject signUpPannel;
    public InputField signUpEmailField;
    public InputField signUpPasswordField;
    public Button signUpBackButton;
    public Button signUpButton;
    public Text signUpStateText;

    //Password Reset Objects
    public GameObject pwdResetPannel;
    public InputField pwdResetEmailField;
    public Button pwdResetBackButton;
    public Button pwdResetButton;
    public Text pwdResetStateText;

    public static FirebaseApp firebaseApp;
    public static FirebaseAuth firebaseAuth;
    
    public static FirebaseUser User;
    
    public void Start()
    {
        //버튼 비활성화
        signInReqButton.interactable = false;
        signUpReqButton.interactable = false;
        pwdResetReqButton.interactable = false;

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
                MainStateText.text = "Firebase를 구동할 수 없습니다.";
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
            pwdResetReqButton.interactable = IsFirebaseReady;
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
        //파이어베이스가 준비되지 않았거나 로그인, 신규 가입, 비밀번호 재설정 중이거나 이미 유저가 있다면
        if(!IsFirebaseReady || IsSignInOnProgress || IsSignUpOnProgress || IsPwdResetOnProgress|| User!= null)
        {
            return;
        }

        IsSignInOnProgress = true;
        signInReqButton.interactable = false;
        signUpReqButton.interactable = false;
        pwdResetReqButton.interactable = false;
        MainStateText.text = "로그인 중...";


        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread((task) =>
        {
            //로그인 시도 후 실행
            Debug.Log($"Sign in status:{task.Status}");
            IsSignInOnProgress = false;
            signInReqButton.interactable = true;
            signUpReqButton.interactable = true;
            pwdResetReqButton.interactable = true;

            if (task.IsFaulted)
            {
                //Debug.LogError(task.Exception);
                MainStateText.text = "입력한 이메일이나 비밀번호를 확인해주세요.";
            }
            else if (task.IsCanceled)
            {
                Debug.LogError("Sign-in canceled");
                MainStateText.text = "로그인이 취소되었습니다.";
            }
            else
            {
                User = task.Result;
                SceneManager.LoadScene("Scenes/Lobby");
            }
        });
    }



    //main panel에서 비밀번호 재설정 버튼을 누르면 실행
    public void clickPwdResetRequestButton()
    {
        pwdResetPannel.SetActive(true);
    }
    
    //Sign Up Pannel에서 뒤로가기 버튼 클릭 시 실행
    public void clickSignUpBackButton()
    {
        cleanStateText();
        signUpPannel.SetActive(false);
    }

    //Sign Up Pannel에서 가입하기 버튼 클릭 시 실행
    public void clickSignUpButton()
    {
        //파이어베이스가 준비되지 않았거나 로그인, 신규 가입, 비밀번호 재설정 중이거나 이미 유저가 있다면
        if (!IsFirebaseReady || IsSignInOnProgress || IsSignUpOnProgress || IsPwdResetOnProgress || User != null)
        {
            return;
        }

        IsSignUpOnProgress = true;
        signUpBackButton.interactable = false;
        signUpButton.interactable = false;

        firebaseAuth.CreateUserWithEmailAndPasswordAsync(signUpEmailField.text, signUpPasswordField.text).ContinueWithOnMainThread((task) =>
        {
            //신규 가입 시도 후 실행
            Debug.Log($"Sign up status:{task.Status}");
            IsSignUpOnProgress = false;
            signUpBackButton.interactable = true;
            signUpButton.interactable = true;


            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
                signUpStateText.text = "입력한 이메일과 비밀번호를 확인해주세요.";
            }
            else if (task.IsCanceled)
            {
                Debug.LogError("Sign up canceled");
                signUpStateText.text = "가입이 취소되었습니다.";
            }
            else
            {
                cleanStateText();
                signUpPannel.SetActive(false);
                return;
            }
        });
    }

    //Password Reset Pannel에서 뒤로가기 버튼 클릭 시 실행
    public void clickPwdResetBackButton()
    {
        cleanStateText();
        pwdResetPannel.SetActive(false);
    }
    
    //Password Reset Pannel에서 비밀번호 재설정 이메일 보내기 버튼 클릭 시 실행
    public void clickPwdResetButton()
    {

        //파이어베이스가 준비되지 않았거나 로그인, 신규 가입, 비밀번호 재설정 중이거나 이미 유저가 있다면
        if (!IsFirebaseReady || IsSignInOnProgress || IsSignUpOnProgress || IsPwdResetOnProgress || User != null)
        {
            return;
        }

        IsPwdResetOnProgress = true;
        pwdResetBackButton.interactable = false;
        pwdResetButton.interactable = false;


        firebaseAuth.SendPasswordResetEmailAsync(pwdResetEmailField.text).ContinueWithOnMainThread((task) =>
        {
            //비밀번호 재설정 메일 보내기 시도후 실행
            Debug.Log($"Pwd reset status:{task.Status}");
            IsPwdResetOnProgress = false;
            pwdResetBackButton.interactable = true;
            pwdResetButton.interactable = true;

            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
                pwdResetStateText.text = "이메일을 다시 입력해주세요.";
            }
            else if (task.IsCanceled)
            {
                Debug.LogError("Pwd reset canceled");
                pwdResetStateText.text = "비밀번호 재설정이 취소되었습니다.";
            }
            else
            {
                pwdResetPannel.SetActive(false);
                cleanStateText();
                return;
            }
        });
    }

    public void cleanStateText()
    {
        MainStateText.text = "";
        signUpStateText.text = "";
        pwdResetStateText.text = "";
    }
}