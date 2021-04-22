using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;


namespace multiplayerPracticeGame
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields 
        [Tooltip("Maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;
        [Tooltip("minimum amount of players needed to start a game")]
        [SerializeField]
        private byte minimumPlayersNeeded = 2;
       
        [Tooltip("The Ui group to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject loginGroup;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressLabel;
        #endregion

        #region Private Fields

        /// <summary>
        /// this client's version number. Users are separetated from each other by gameVersion
        /// </summary>
        string gameVersion = "1";
        private TextMeshProUGUI progressText;
     
        #endregion

        #region MonoBehaviour CallBacks 

        private void Awake()
        {          
            progressText = progressLabel.GetComponent<TextMeshProUGUI>();
            //#critical 
            // this make sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true; 
        }
        private void Start()
        {
            ShowLoginMenu();
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            ShowLoadingLabel();
            // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // #Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }   

        #endregion

        #region Private Methods

        private void ShowLoginMenu()
        {
            loginGroup.SetActive(true);
            progressLabel.SetActive(false);
        }
        // maybe add animation later 
        private void ShowLoadingLabel()
        {
            loginGroup.SetActive(false);
            progressText.text = "Connecting";
            progressLabel.SetActive(true);
        }

        #endregion

        #region MonoBehaviourPunCallbacks CallBacks 

        public override void OnConnectedToMaster()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");

            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            ShowLoginMenu();
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher:OnDisconnected was called by PUN with reason {0}", cause);
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = maxPlayersPerRoom });
        }
        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");
            
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            
              
            if(playerCount < (int)minimumPlayersNeeded){
                progressText.text = "Waiting for players";
            }
            else{
                progressText.text = "Match is ready to begin";
                Debug.Log("Minimum or more players in the room.Loading level");
                PhotonNetwork.LoadLevel("MazeLvl");
            }
        }
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            base.OnPlayerEnteredRoom(newPlayer);
            //#Critical : add displaying player counter

            // shown to players, who are already in the room
            if(PhotonNetwork.CurrentRoom.PlayerCount==(int)maxPlayersPerRoom){
                PhotonNetwork.CurrentRoom.IsOpen = false;
                Debug.Log("Maximum players achieved. Room is closed");
            }
            
            Debug.Log("New player entered the room");
        }
        #endregion

    }

}
