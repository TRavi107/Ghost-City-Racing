using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuUiManager : MonoBehaviour
{
    public Transform playersContainer;
    public TMP_Text readyBTNText;
    public GameObject RoomContainer;

    public static MainMenuUiManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void OnReadyClicked()
    {
        NetManager.instance.OnReadyClicked();
    }
}
