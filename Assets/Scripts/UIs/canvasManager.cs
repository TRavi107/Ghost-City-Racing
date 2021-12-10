using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class canvasManager : MonoBehaviour
{
    public List<canvasElement> allCanvases;
    private canvasElement lastActiveCanvas;

    [HideInInspector]
    public static canvasManager instance;
    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        foreach (canvasElement canvases in allCanvases)
        {
            canvases.gameObject.SetActive(false);
        }
        switchCanvas(CanvasType.loadingPanel);
    }

    public void switchCanvas(CanvasType _Type)
    {
        if(lastActiveCanvas != null)
        {
            lastActiveCanvas.gameObject.SetActive(false);
        }
        canvasElement desiredCanvas = allCanvases.Find(x=>x.canvasType==_Type);
        if (desiredCanvas != null)
        {
            desiredCanvas.gameObject.SetActive(true);
            lastActiveCanvas = desiredCanvas;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
