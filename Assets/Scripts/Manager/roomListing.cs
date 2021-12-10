using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;

public class roomListing : MonoBehaviour
{
    public TMP_Text text;
    public RoomInfo rooInfo;
    public Button buttonMenu;

    public void SetRoomInfo(RoomInfo _roomInfo)
    {
        rooInfo = _roomInfo;
        text.text = _roomInfo.Name;
    }
    
    // Start is called before the first frame update
    protected virtual void Start()
    {
        buttonMenu = GetComponent<Button>();
        buttonMenu.onClick.AddListener(buttonAction);
    }
    public virtual void buttonAction()
    {
        NetManager.instance.JoinRoom(rooInfo.Name);
    }
}
