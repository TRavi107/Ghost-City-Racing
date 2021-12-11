using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CheckPoint
{
    public int checkPointIndex;
    public bool Iscompleted;
    public float completedTime;

    public CheckPoint(int _wayPointIndex, bool _Iscompleted, float _completedTime)
    {
        checkPointIndex = _wayPointIndex;
        Iscompleted = _Iscompleted;
        completedTime = _completedTime;
    }
}

[System.Serializable]
public class Participant
{
    public playerManager playerManager;
    public float distanceTravelled;

    public Participant(playerManager manager)
    {
        playerManager = manager;
        distanceTravelled = 0;
    }
}
public class RaceManager : MonoBehaviourPun
{
    public TMP_Text timerText;
    public TMP_Text messageText;
    public GameObject racerInfoPanel;
    public GameObject racerInfoPrefab;
    public Transform checkPointsTransform;
    public Skidmarks skidMarkController;

    public List<Transform> spawnPoints;

    public List<Participant> raceParticipants =new List<Participant>();

    public static RaceManager instance;

    public float counterTime;
    public float racerInfopanelActivationDuration = 3f;
    public float lastAcivated;
    public float startedTime;


    private float timer;
    private float totalRaceDistance;

    private bool startTimer = false;
    private bool startRace = false;
    private bool activateRacerInfoPanel = false;


    public playerManager me;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        timer = counterTime;
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_StartTimer", RpcTarget.All);
        }
        //me.GetComponent<controllerDr>().enabled = false;
        float distance=0;
        for (int i = 0; i < checkPointsTransform.childCount; i++)
        {
            if (i == checkPointsTransform.childCount - 1)
            {
                distance += Vector3.Distance(checkPointsTransform.GetChild(i).localPosition, checkPointsTransform.GetChild(0).localPosition);
            }
            else
                distance += Vector3.Distance(checkPointsTransform.GetChild(i).localPosition, checkPointsTransform.GetChild(i+1).localPosition);

        }
        me.GetComponent<controllerDr>().enabled = false;
        racerInfoPanel.gameObject.SetActive(true);
        HideMessage();
    }

    // Update is called once per frame
    void Update()
    {
        if (startTimer)
        {
            timer -= Time.deltaTime;
            timerText.text = timer.ToString("00");
            if(timer<counterTime-1 )
            {
                lastAcivated = 3;
                //activateRacerInfo(0,false);
            }
            if(timer >2 && timer < 3)
            {
                timerText.text = "Ready";
            }
            else if(timer > 1 && timer < 2)
            {
                timerText.text = "To";
            }
            else if(timer > 0 && timer < 1)
            {
                timerText.text = "Race";
            }
            else if (timer <= 0)
            {
                timerText.text = "Go...";
                photonView.RPC("RPC_StartRace", RpcTarget.All);
            }
        }

        //if(activateRacerInfoPanel && lastAcivated >0 )
        //{
        //    lastAcivated -= Time.deltaTime;
        //    if (lastAcivated < 0)
        //    {
        //        activateRacerInfoPanel = false;
        //        lastAcivated = 3;
        //    }
        //}

        //if (!activateRacerInfoPanel)
        //{
        //    racerInfoPanel.gameObject.SetActive(false);

        //}

    }

    [PunRPC]
    private void RPC_StartTimer()
    {
        startTimer = true;
        racerInfoPanel.SetActive(true);
        
    }

    

    [PunRPC]
    private void RPC_StartRace()
    {
        startRace = true;
        startTimer = false;
        me.GetComponent<controllerDr>().enabled = true;
        me.GetComponent<AudioSource>().volume = 1;
        racerInfoPanel.SetActive(false);
        startedTime = Time.time;
        timerText.gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;
    }

    public void HideMessage()
    {
        messageText.gameObject.SetActive(false);

    }

    public void PlayerEnter(playerManager player,int wayPoint)
    {
        photonView.RPC("RPC_UpdateWayPointTime", RpcTarget.All, player.nickName, wayPoint);
        
    }

    [PunRPC]
    private void RPC_UpdateWayPointTime(string name,int _checkPointIndex)
    {
        foreach (Participant item in raceParticipants)
        {
            if (item.playerManager.nickName == name)
            {
                item.playerManager.checkPoints[_checkPointIndex].Iscompleted = true;
                item.playerManager.checkPoints[_checkPointIndex].completedTime = Time.time;
                item.distanceTravelled = 0;
                for (int i=0;i<item.playerManager.checkPoints.Count;i++)
                {
                    if (item.playerManager.checkPoints[i].Iscompleted)
                    {
                        item.distanceTravelled += Vector3.Distance(checkPointsTransform.GetChild(i).position,
                                                                    checkPointsTransform.GetChild(i + 1).position);
                    }
                    else
                        break;
                    
                }
            }
        }


        if (me.nickName == name)
        {
            SortRacerList();
            activateRacerInfo();
            //lastAcivated = 3;
        }

    }

    //private int GetIndex(CheckPoint point,CheckPoint[] points)
    //{
    //    int index = -1;
    //    for (int i = 0; i < points.Length; i++)
    //    {
    //        if (point.wayPointName == points[i].wayPointName)
    //        {
    //            index = i;
    //        }
    //    }

    //    return index;
    //}


    private void SortRacerList()
    {
        int n = raceParticipants.Count;

        // One by one move boundary of unsorted subarray
        for (int i = 0; i < n - 1; i++)
        {
            // Find the minimum element in unsorted array
            int min_idx = i;

            for (int j = i + 1; j < n; j++)
                if (raceParticipants[j].distanceTravelled > raceParticipants[min_idx].distanceTravelled)
                    min_idx = j;

            // Swap the found minimum element with the first
            // element
            Participant temp = raceParticipants[min_idx];
            raceParticipants[min_idx] = raceParticipants[i];
            raceParticipants[i] = temp;
        }
    }

    private void activateRacerInfo(bool showTimer = true)
    {
        foreach (Transform child in racerInfoPanel.transform)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < raceParticipants.Count; i++)
        {
            playerManager manager = raceParticipants[i].playerManager;
            GameObject tempracerInfoPrefab = Instantiate(racerInfoPrefab, racerInfoPanel.transform);
            if(raceParticipants.Count==1)
                tempracerInfoPrefab.GetComponent<raceInfoPrefabManager>().setInfo(manager.nickName, 
                                                    Mathf.Abs(raceParticipants[i].distanceTravelled), 
                                                    manager.myColor, showTimer);
            else if (i==0)
                tempracerInfoPrefab.GetComponent<raceInfoPrefabManager>().setInfo(manager.nickName,
                                                    Mathf.Abs (raceParticipants[i].distanceTravelled - raceParticipants[i+1].distanceTravelled), 
                                                    manager.myColor, showTimer);
            else
                tempracerInfoPrefab.GetComponent<raceInfoPrefabManager>().setInfo(manager.nickName,
                                                    Mathf.Abs (raceParticipants[i].distanceTravelled - raceParticipants[i -1].distanceTravelled), 
                                                    manager.myColor, showTimer);

        }

        activateRacerInfoPanel = true;
        racerInfoPanel.gameObject.SetActive(true);
    }
}
