using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class createRoomBTN : MonoBehaviour
{
    public TMP_Text _roomName;
    public CanvasType desiredCanvasType;
    public Button buttonMenu;
    // Start is called before the first frame update
    void Start()
    {
        buttonMenu = GetComponent<Button>();
        buttonMenu.onClick.AddListener(CreateRoom);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateRoom()
    {
        if (_roomName.text.Length <=1) 
        {
            Debug.Log("Invalid Room Name");
            
        }
        else {
            NetManager.instance.Create_Room(_roomName.text);
        }
    }

}
