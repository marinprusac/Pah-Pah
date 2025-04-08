using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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

        [SerializeField] private LobbyManager lobbyManager;

        private void OnEnable()
        {
            signInButton.onClick.AddListener(OnSignIn);
        }

        private void OnDisable()
        {
            signInButton.onClick.RemoveAllListeners();
        }

        private void OnSignIn()
        {
            if (string.IsNullOrWhiteSpace(nameInput.text)) return;
            lobbyManager.Authenticate(nameInput.text);
            mainMenuUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
