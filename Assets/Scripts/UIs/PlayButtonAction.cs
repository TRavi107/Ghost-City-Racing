using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayButtonAction : MonoBehaviour
{
    public CanvasType desiredCanvasType;
    public Button buttonMenu;
    public TMP_Text playerName;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        buttonMenu = GetComponent<Button>();
        buttonMenu.onClick.AddListener(buttonAction);
    }
    public virtual void buttonAction()
    {
        string text=playerName.text;
        if (playerName.text.Length <= 1)
        {
            text = "Noobie" + Random.Range(0, 100);
        }
        NetManager.instance.CreateOrJoinRoomOption(desiredCanvasType, text);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
