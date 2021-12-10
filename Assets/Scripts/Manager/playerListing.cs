using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;

public class playerListing : MonoBehaviour
{
    public TMP_Text playerName;
    public TMP_Text readyText;
    public Player _player;
    public bool isReady = false;
    public playerManager playermanager;

    public void SetPlayerInfo(Player player,string readyState)
    {
        _player = player;
        readyText.text = readyState;
        if (readyState == "R")
            isReady = true;
        else
            isReady = false;
        playerName.text = _player.NickName;
    }

}
