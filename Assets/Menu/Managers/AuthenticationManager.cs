using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

namespace Managers
{
    public class AuthenticationManager
    {

        private static AuthenticationManager _instance;
        public static AuthenticationManager Instance
        {
            get => _instance ?? throw new Exception("Not yet authenticated. Use Authenticate()!");
            private set => _instance = value;
        }
        private AuthenticationManager()
        {
        }

        public static Action SignedIn;
        public static Action SignedOut;

        public string PlayerName { get; private set; }
        
        
        public static async void Authenticate()
        {
            try
            {
                if(UnityServices.State == ServicesInitializationState.Uninitialized)
                    await InitializeUnityServices();
                AuthenticationService.Instance.SignedIn += OnSignedIn;
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Instance = new AuthenticationManager();
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to sign in. Error: " + e.StackTrace);
            }
        }
        
        private static async Task InitializeUnityServices()
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.Log("Unable to initialize Unity Services. Error: " + e.Message);
            }
        }

        private static void OnSignedIn()
        {
            Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
            AuthenticationService.Instance.SignedIn -= OnSignedIn;
            SignedIn?.Invoke();
        }
        
        public static void SignOut()
        {
            AuthenticationService.Instance.SignOut();
            Instance = null;
            SignedOut?.Invoke();
            
        }
    }
}