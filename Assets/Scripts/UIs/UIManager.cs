using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public controllerDr RR ;
    public GameObject needle;
    public TMP_Text speed;
    public TMP_Text gear;
    public float startPosition, endPosition;

    public float vehicleSpeed;

    private float desiredPosition;
    // Start is called before the first frame update
    void Start()
    {
        if(RR==null)
            RR = playerManager.instance.controler;

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateNeedle();
    }

    void UpdateNeedle()
    {
        vehicleSpeed = RR.KPH;
        desiredPosition = startPosition - endPosition;
        float temp = vehicleSpeed / Mathf.Abs(startPosition-endPosition);
        needle.transform.eulerAngles = new Vector3(0, 0, (startPosition - temp * desiredPosition));
        speed.text = vehicleSpeed.ToString("00");
        gear.text = (RR.gearNum +1).ToString("0");
    }
}
