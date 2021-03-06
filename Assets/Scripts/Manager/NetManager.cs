using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public bool myState = false;
    public string myNickName="Noobie";

    public Transform playersContainer;
    public TMP_Text readyBTNText;
    public GameObject RoomContainer;

    public roomListing roominfoPrefab;

    public List<roomListing> availableRooms;
    public List<playerListing> playersInRoom;
    //public List<playerManager> playersInScene;
    public playerListing playerinfoPrefab;

    public bool backToMainMenu = false;

    public float startTime;

    #region Static Implementation
    public static NetManager instance;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        if (instance == null)
        {
            instance = this;
        }

        SceneManager.LoadScene(1);

    }

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.GameVersion = "0.0.0.1";
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
        //canvasManager.instance.switchCanvas(CanvasType.loadingPanel);
        startTime = Time.time;
    }

    void ResetList()
    {
        playersInRoom.Clear();
        availableRooms.Clear();
    }

    #region SceneLoading

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.buildIndex == 2)
        {
            int post = PhotonNetwork.LocalPlayer.ActorNumber;
            Vector3 pos = RaceManager.instance.spawnPoints[post].position;
            Quaternion rot = RaceManager.instance.spawnPoints[post].rotation;
            
            GameObject newPlayer= PhotonNetwork.Instantiate("Free_Racing_Car_BlueNetwork", pos,rot);
            newPlayer.GetComponent<playerManager>().nickName = PhotonNetwork.NickName;
            RaceManager.instance.me = newPlayer.GetComponent<playerManager>();
        }
        else if(arg0.buildIndex == 1 && !backToMainMenu )
        {
            playersContainer = MainMenuUiManager.instance.playersContainer;
            readyBTNText = MainMenuUiManager.instance.readyBTNText;
            RoomContainer = MainMenuUiManager.instance.RoomContainer;
            ResetList();
            //StartMainMenu();
        }
        else if (arg0.buildIndex == 1 && backToMainMenu)
        {
            canvasManager.instance.switchCanvas(CanvasType.loadingPanel);
            playersContainer = MainMenuUiManager.instance.playersContainer;
            readyBTNText = MainMenuUiManager.instance.readyBTNText;
            RoomContainer = MainMenuUiManager.instance.RoomContainer;
            ResetList();
            backToMainMenu = false;
            if (PhotonNetwork.InLobby)
            {
                canvasManager.instance.switchCanvas(CanvasType.mainMenu);
            }
            else
            {
                PhotonNetwork.JoinLobby();
            }
        }
    }


    #endregion


    public void JoinRoom(string _roomName)
    {
        canvasManager.instance.switchCanvas(CanvasType.loadingPanel);
        PhotonNetwork.JoinRoom(_roomName);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom(true);
        canvasManager.instance.switchCanvas(CanvasType.loadingPanel);
    }

    public void CreateOrJoinRoomOption(CanvasType type, string playerName)
    {
        canvasManager.instance.switchCanvas(type);
        PhotonNetwork.LocalPlayer.NickName = playerName;
        myNickName = playerName;
    }


    public void OnReadyClicked()
    {
        print(PhotonNetwork.MasterClient);

        if (PhotonNetwork.IsMasterClient)
        {
            print("Here");

            if (AllReady())
            {
                PhotonNetwork.LoadLevel(2);
            }
        }
        else
        {
            myState = !myState;
            base.photonView.RPC("RPC_ChangeReadyStatus", RpcTarget.All, PhotonNetwork.LocalPlayer, myState);
        }
    }


    [PunRPC]
    private void RPC_ChangeReadyStatus(Player player, bool state)
    {
        ChangeReadyStatus(player, state);
    }

    public void Create_Room(string _roomName)
    {
        if (!PhotonNetwork.IsConnected)
            return;
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 8
        };
        canvasManager.instance.switchCanvas(CanvasType.loadingPanel);
        PhotonNetwork.JoinOrCreateRoom(_roomName, options, TypedLobby.Default);
    }


    #region Updating RoomList


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
            {
                int index = availableRooms.FindIndex(x => x.rooInfo.Name == room.Name);
                if (index != -1)
                {
                    Destroy(availableRooms[index].gameObject);
                    availableRooms.RemoveAt(index);
                }
                
            }
            else
            {
                foreach (roomListing roomListed in availableRooms)
                {
                    if (roomListed.rooInfo.Name == room.Name)
                    {
                        return;
                    }
                }
                roomListing tempoRoomListing = Instantiate(roominfoPrefab, RoomContainer.transform);
                if (tempoRoomListing != null)
                {
                    tempoRoomListing.SetRoomInfo(room);
                    availableRooms.Add(tempoRoomListing);
                }
            }
        }
        print("called");
    }



    #endregion
    

    private void RPC_PlayerLeft(Player player)
    {
        RemovePlayerListing(player);
    }


    public void AddPlayerListing(Player player,string state="n")
    {
        playerListing newPlayer = new playerListing();
        newPlayer._player = player;
        foreach (playerListing availableListing in playersInRoom)
        {
            if (availableListing._player.ActorNumber == player.ActorNumber)
            {
                Debug.Log("Already in the list");
                return;
            }
        }
        playerListing tempoplayerlisting = Instantiate(playerinfoPrefab, playersContainer.transform);
        if (tempoplayerlisting != null)
        {
            tempoplayerlisting.SetPlayerInfo(newPlayer._player, state);

        }
        playersInRoom.Add(tempoplayerlisting);

    }
    public void RemovePlayerListing(Player Newplayer)
    {
        foreach (playerListing availableListing in playersInRoom)
        {
            if (availableListing._player.ActorNumber == Newplayer.ActorNumber)
            {

                print(availableListing);                                        //Need to solve later shows null after player leaves the room
                playersInRoom.Remove(availableListing);
                if(availableListing.gameObject != null)
                    Destroy(availableListing.gameObject);
                return;
            }
        }

    }

    public bool AllReady()
    {

        foreach (playerListing player in playersInRoom)
        {
            if (player.isReady == false)
            {
                return false;
            }
        }
        return true;
    }

    public void ChangeReadyStatus(Player player, bool status)
    {
        foreach (playerListing playerInroom in playersInRoom)
        {
            if (playerInroom._player.ActorNumber == player.ActorNumber)
            {
                playerInroom.isReady = status;
                if (status)
                    playerInroom.readyText.text = "R";
                else
                    playerInroom.readyText.text = "N";
            }
        }
    }

    #region Overriden Photon Functions
    public override void OnCreateRoomFailed(short returnCode, string message)
    {

    }
    public override void OnCreatedRoom()
    {
        canvasManager.instance.switchCanvas(CanvasType.roomLobby);
        Debug.Log("Room Created");
        AddPlayerListing(PhotonNetwork.LocalPlayer, "R");
    }
    public override void OnPlayerEnteredRoom(Player player)
    {
        print(player.ActorNumber);
        AddPlayerListing(player);
    }

    public override void OnJoinedRoom()
    {
        print("Joined Room");
        canvasManager.instance.switchCanvas(CanvasType.roomLobby);
        if (PhotonNetwork.IsMasterClient)
        {
            readyBTNText.text = "Start";
        }
        else
        {
            readyBTNText.text = "Ready";
            //Need to instantiate for each previous player in the room
            AddPlayerListing(PhotonNetwork.MasterClient, "R");
            AddPlayerListing(PhotonNetwork.LocalPlayer);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemovePlayerListing(otherPlayer);
    }
    public override void OnJoinedLobby()
    {
        canvasManager.instance.switchCanvas(CanvasType.mainMenu);
    }

    public override void OnConnectedToMaster()
    {
        canvasManager.instance.switchCanvas(CanvasType.loadingPanel);
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    #endregion
}
