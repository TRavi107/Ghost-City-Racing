using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonAction
{
    others,
    leaveRoom,
}

[RequireComponent(typeof (Button))]
public class canvasSwichter : MonoBehaviour
{
    public CanvasType desiredCanvasType;
    public Button buttonMenu;
    public ButtonAction action;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        buttonMenu = GetComponent<Button>();
        buttonMenu.onClick.AddListener(buttonAction);
    }
    public virtual void buttonAction()
    {
        switch (action)
        {
            case ButtonAction.others:
                canvasManager.instance.switchCanvas(desiredCanvasType);
                break;
            case ButtonAction.leaveRoom:
                NetManager.instance.LeaveRoom();
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
