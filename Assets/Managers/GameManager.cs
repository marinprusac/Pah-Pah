using System;
using UnityEngine;

namespace Managers
{
    public class GameManager
    {
        private static GameManager _instance;

        public static GameManager Instance
        {
            get => _instance ?? throw new Exception("GameManager not initialize. Start the game first.");
            private set => _instance = value;
        }
        
        private GameManager()
        {
        }


        public static Action GameStarted;


        public static void StartGame(bool asHost)
        {
            if (asHost)
            {
                RelayManager.CreateRelay(LobbyManager.Instance.LobbyId);
            }
            GameStarted?.Invoke();
        }

        public static void Initialize()
        {
            Instance = new GameManager();
        }
    }
}