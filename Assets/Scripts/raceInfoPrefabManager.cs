using UnityEngine;
using TMPro;
using System;

public class raceInfoPrefabManager : MonoBehaviour
{
    public TMP_Text racerName;
    public TMP_Text time;
    public float raceTime;

    public void setInfo(string name,float _time,Color color,bool showTimer)
    {
        racerName.text = name;
        time.text = _time.ToString("000");
        racerName.color = color;
        time.color = color;
        if (showTimer)
        {
            time.gameObject.SetActive(true);
        }
        else
            time.gameObject.SetActive(false);

        time.gameObject.SetActive(false);

    }
}
