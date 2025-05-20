using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.UI
{
    public class SignInUI : MonoBehaviour
    {
        
        [SerializeField]
        private Button signInButton;

        [SerializeField] private MainMenuUI mainMenuUI;
        
        private void OnEnable()
        {
            signInButton.onClick.AddListener(OnSignInPressed);
            AuthenticationManager.SignedIn += OnSignedIn;
        }

        private void OnDisable()
        {
            signInButton.onClick.RemoveAllListeners();
            AuthenticationManager.SignedIn -= OnSignedIn;
        }

        private void OnSignInPressed()
        {
            AuthenticationManager.Authenticate();
        }

        private void OnSignedIn()
        {
            mainMenuUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
