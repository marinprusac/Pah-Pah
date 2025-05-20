using Managers;
using Netcode.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.UI
{
    public class MainMenuUI : MonoBehaviour
    {

        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button logOutButton;

        [SerializeField] private SignInUI signInUI;
        [SerializeField] private CreateLobbyUI createLobbyUI;
        [SerializeField] private JoinLobbyUI joinLobbyUI;
        
        private void OnEnable()
        {
            hostButton.onClick.AddListener(OnHost);
            joinButton.onClick.AddListener(OnJoin);
            logOutButton.onClick.AddListener(OnLogOut);
        }

        private void OnDisable()
        {
            hostButton.onClick.RemoveAllListeners();
            joinButton.onClick.RemoveAllListeners();
            logOutButton.onClick.RemoveAllListeners();
        }

        private void OnHost()
        {
            createLobbyUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        private void OnJoin()
        {
            joinLobbyUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

        private void OnLogOut()
        {
            AuthenticationManager.SignOut();
            signInUI.gameObject.SetActive(true);
            gameObject.SetActive(false);
        }

    }
}
