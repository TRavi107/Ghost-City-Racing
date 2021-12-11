using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class roomManager : MonoBehaviourPunCallbacks
{
    public playerListing playerinfoPrefab;

    public List<playerManager> allPlayers;

    public List<playerListing> playersInRoom;

    public static roomManager instance;
    public Transform playersContainer;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        if(instance == null)
        {
            instance = this;
        }
        playersContainer = NetManager.instance.playersContainer;

    }

    public void AddPlayerListing(playerListing Newplayer)
    {
        foreach (playerListing availableListing in playersInRoom)
        {
            if(availableListing._player.UserId == Newplayer._player.UserId)
            {
                Debug.Log("Already in the list");
                return;
            }
        }
        playerListing tempoPlayerListing = Instantiate(playerinfoPrefab, playersContainer.transform);
        if (tempoPlayerListing != null)
        {
            tempoPlayerListing.SetPlayerInfo(Newplayer._player, "N");

        }
        playersInRoom.Add(Newplayer);

    }
    public void RemovePlayerListing(Player Newplayer)
    {
        foreach (playerListing availableListing in playersInRoom)
        {
            if (availableListing._player.UserId == Newplayer.UserId)
            {
                Destroy(availableListing.gameObject);
                playersInRoom.Remove(availableListing);
            }
        }

    }

    public bool AllReady()
    {
        return true;

        foreach (playerListing player in playersInRoom)
        {
            if(player.isReady == false)
            {
                return false;
            }

        }


    }

    public void ChangeReadyStatus(Player player,bool status)
    {
        foreach (playerListing playerInroom in playersInRoom)
        {
            if (playerInroom._player == player)
            {
                playerInroom.isReady = status;
                if (status)
                    playerInroom.readyText.text = "R";
                else
                    playerInroom.readyText.text = "N";
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        print(player.NickName);
        playerListing newPlayer = new playerListing();
        newPlayer._player = player;
        if (PhotonNetwork.IsMasterClient)
        {
            newPlayer.isReady = true;
        }
        else
            newPlayer.isReady = false;

        AddPlayerListing(newPlayer);
        
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RemovePlayerListing(otherPlayer);
    }

}
