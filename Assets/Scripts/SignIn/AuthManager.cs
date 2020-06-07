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

    public InputField emailField;
    public InputField passwordField;
    public Button signInButton;

    public static FirebaseApp firebaseApp;
    public static FirebaseAuth firebaseAuth;

    public static FirebaseUser User;
    
    public void Start()
    {
        //버튼 비활성화
        signInButton.interactable = false;
        
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

            signInButton.interactable = IsFirebaseReady;
        }
        );
    }
    
    public void SignIn()
    {
        //파이어베이스가 준비되지 않았거나 로그인 중이거나 유저정보가 이미 있다면
        if(!IsFirebaseReady || IsSignInOnProgress || User!= null)
        {
            return;
        }

        IsSignInOnProgress = true;
        signInButton.interactable = false;
        
        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread((task) =>
        {
            //로그인 시도 후 실행
            Debug.Log($"Sign in status:{task.Status}");
            IsSignInOnProgress = false;
            signInButton.interactable = true;

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
}
