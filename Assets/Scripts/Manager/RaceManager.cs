using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public int position;

    public Participant(playerManager manager)
    {
        playerManager = manager;
        distanceTravelled = 0;
    }
}

public class RaceManager : MonoBehaviourPun
{
    public bool offlineMode=false;
    public TMP_Text timerText;
    public TMP_Text messageText;
    public GameObject racerInfoPanel;
    public GameObject racerInfoPrefab;
    public GameObject gameOverPanel;
    public GameObject leaderBoardMenu;
    public Transform checkPointsTransform;
    public Skidmarks skidMarkController;

    public List<Transform> spawnPoints;

    public List<Participant> raceParticipants =new List<Participant>();
    public List<Transform> AIcheckpoints = new List<Transform>();

    public static RaceManager instance;

    public float counterTime;
    public float racerInfopanelActivationDuration = 3f;
    public float lastAcivated;
    public float startedTime;

    public float timeLimit;
    public GameObject explosionPrefab;
    public playerManager me;

    private float timer;
    private float messageTime=2;

    private bool startTimer = false;
    private bool startRace = false;
    private bool activateRacerInfoPanel = false;

    #region UnityFunctions
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        timer = counterTime;
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_StartTimer", RpcTarget.All);
            //for (int i = 0; i < 50; i++)
            //{
            //    GameObject missile = PhotonNetwork.Instantiate("MissileGameobject", Vector3.zero, Quaternion.identity);
            //    GameAssets.instance.ThrowBackToPool(missile.GetComponent<MissileController>());
            //} 

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
        HideMessage();
        racerInfoPanel.SetActive(true);
        if (offlineMode)
            return;

        me.GetComponent<controllerDr>().enabled = false;
    }

    void Update()
    {
        if (!offlineMode)
        {
            if ((Time.time - NetManager.instance.startTime) > timeLimit && messageTime > 0)
            {
                ShowMessage("You have played for more than " + (timeLimit / 60).ToString() + "min");
                messageTime -= Time.deltaTime;
                if (messageTime < 0)
                    messageTime = 0;
            }
        }
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
    }

    #endregion

    #region StartingRace
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

    #endregion

    #region MessageSection
    public void ShowMessage(string message)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = message;
        //StartCoroutine("HideMessage");
        //StopCoroutine("HideMessage");
    }

    public void HideMessage()
    {
        messageText.gameObject.SetActive(false);
    }

    #endregion

    #region WayPointSection
    public void PlayerEnter(playerManager player,int wayPoint,bool isOver=false)
    {
        photonView.RPC("RPC_UpdateWayPointTime", RpcTarget.All, player.nickName, wayPoint,isOver);
    }

    [PunRPC]
    private void RPC_UpdateWayPointTime(string name,int _checkPointIndex,bool isOver)
    {
        foreach (Participant item in raceParticipants)
        {
            if (item.playerManager.nickName == name)
            {
                item.playerManager.checkPoints[_checkPointIndex].Iscompleted = true;
                item.playerManager.checkPoints[_checkPointIndex].completedTime = Time.time;
                if (isOver)
                {
                    if (item.playerManager.nickName == me.nickName)
                    {
                        ShowMessage("You are " + (raceParticipants.IndexOf(item)+1).ToString());
                        me.raceIsOver = true;
                        //me.GetComponent<controllerDr>().enabled =false;
                        me.GetComponent<AIController>().enabled = true;
                        return;
                    }
                }
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

    #endregion

    public void ShotMissile(string Shooter,Vector3 pos, string target)
    {
        //GameAssets.instance.SpawnFromPool(pos, Quaternion.identity, GetPlayerByNickName(target));
        GameObject missile = PhotonNetwork.Instantiate("MissileGameobject", pos, Quaternion.identity);
        missile.GetComponent<MissileController>().target = GetPlayerByNickName(target);
    }

    #region DamagePlayer
    public void DamagePlayer(string damagedPlayer,string attacker,int damage,Vector3 pos)
    {
        photonView.RPC("RPC_DamagePlayer", RpcTarget.All, damagedPlayer, attacker,damage,pos);

    }

    [PunRPC]
    private void RPC_DamagePlayer(string damagedPlayer, string attacker, int damage,Vector3 pos)
    {
        foreach (Participant player in raceParticipants)
        {
            if (player.playerManager.nickName == damagedPlayer)
            {
                player.playerManager.TakeDamage(damage, attacker);
                GameObject.Instantiate(explosionPrefab, pos, Quaternion.identity);
            }
        }
    }
    #endregion

    public int GetIndexOfPlayer(string playerName)
    {
        for (int i = 0; i < raceParticipants.Count; i++)
        {
            if (raceParticipants[i].playerManager.nickName == playerName)
                return i ;
        }
        return 0;
    }

    #region Private Functions
    public void BackToMainMenu()
    {
        NetManager.instance.backToMainMenu = true;
        PhotonNetwork.LeaveRoom();
        //PhotonNetwork.Destroy(me.gameObject);
        SceneManager.LoadScene(1);
    }

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

    private Transform GetPlayerByNickName(string target)
    {
        foreach (Participant item in raceParticipants)
        {
            if (item.playerManager.nickName == target)
                return item.playerManager.gameObject.transform;
        }
        print(target + "was not found");
        return null;
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
                tempracerInfoPrefab.GetComponent<raceInfoPrefabManager>().setInfo(i+1,manager.nickName, 
                                                    Mathf.Abs(raceParticipants[i].distanceTravelled), 
                                                    manager.myColor, showTimer);
            else if (i==0)
                tempracerInfoPrefab.GetComponent<raceInfoPrefabManager>().setInfo(i+1,manager.nickName,
                                                    Mathf.Abs (raceParticipants[i].distanceTravelled - raceParticipants[i+1].distanceTravelled), 
                                                    manager.myColor, showTimer);
            else
                tempracerInfoPrefab.GetComponent<raceInfoPrefabManager>().setInfo(i+1,manager.nickName,
                                                    Mathf.Abs (raceParticipants[i].distanceTravelled - raceParticipants[i -1].distanceTravelled), 
                                                    manager.myColor, showTimer);

        }

        activateRacerInfoPanel = true;
        racerInfoPanel.gameObject.SetActive(true);
    }

    #endregion


}
