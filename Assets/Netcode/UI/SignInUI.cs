using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Netcode.UI
{
    public class SignInUI : MonoBehaviour
    {

        [SerializeField]
        private TMP_InputField nameInput;
        
        [SerializeField]
        private Button signInButton;

        [SerializeField] private MainMenuUI mainMenuUI;
        
        private void OnEnable()
        {
            signInButton.onClick.AddListener(OnSignIn);
        }

        private void OnDisable()
        {
            signInButton.onClick.RemoveAllListeners();
        }

        private void Awake()
        {
            LobbyManager.Initialize();
            RelayManager.Initialize();
        }

        private void OnSignIn()
        {
            if (string.IsNullOrWhiteSpace(nameInput.text)) return;
            LobbyManager.Instance.Authenticate(nameInput.text);
            mainMenuUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
