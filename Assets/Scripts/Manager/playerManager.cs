using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class playerManager : MonoBehaviourPunCallbacks,IPunObservable,IPunInstantiateMagicCallback
{

    [SerializeField]
    Skidmarks skidmarksController;
    public Color myColor;
    public static playerManager instance;
    public Camera _camera;
    public controllerDr controler;
    public string nickName="Noobie";
    public SpriteRenderer miniMapVision;
    public float DriftSoundDuration;
    public List<CheckPoint> checkPoints = new List<CheckPoint>();
    public bool raceIsOver = false;
    public int health = 100;
    public missileLauncher missileLauncher;

    private float maxVelocity;
    private Vector3 remotePosition;
    private Quaternion remoterotation;
    private float lastDriftSoundPlayed = 0;
    private bool ghostMode=false;
    
    // Start is called before the first frame update
    void Awake()
    {
        GetComponent<AIController>().enabled = false;
        if (RaceManager.instance != null)
        {
            for (int i = 0; i < RaceManager.instance.checkPointsTransform.childCount; i++)
            {
                if (i < 17)
                {
                    checkPoints.Add(new CheckPoint(i, true, 0));
                }
                else
                    checkPoints.Add(new CheckPoint(i, false, 0));

            }
        }
        skidmarksController = RaceManager.instance.skidMarkController;

        if (!photonView.IsMine)
        {
            maxVelocity = GetComponent<controllerDr>().carConstant.maxVelocity;
            //PhotonNetwork.Instantiate("Free_Racing_Car_BlueNetwork", new Vector3(5, 0, 0), Quaternion.identity);
            Destroy(FindObjectOfType<Camera>().gameObject);
            Destroy(GetComponent<controllerDr>());
            Destroy(GetComponentInChildren<missileLauncher>());
        }
        else
        {

            if (instance == null)
            {
                instance = this;
            }
            //Vector3 pos = new Vector3(Random.Range(11, 70), 0, 44);
            //player = PhotonNetwork.Instantiate("Free_Racing_Car_BlueNetwork", pos, Quaternion.identity);
            Transform lookAt = gameObject.transform.Find("lookAt");
            controler = GetComponent<controllerDr>();

            _camera = FindObjectOfType<Camera>();
            _camera.GetComponent<cameraFollow>().player = this.gameObject;
            _camera.GetComponent<cameraFollow>().lookAt = lookAt;
            int tempColor = PhotonNetwork.LocalPlayer.ActorNumber;
            if (PhotonNetwork.LocalPlayer.ActorNumber > 7)
            {
                tempColor = (tempColor + 1) % 8;
            }
            photonView.RPC("RPC_UpdateMyColor", RpcTarget.All, tempColor);
            photonView.RPC("RPC_SetMyNickName", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);

        }
    }

    float ghostTimer = 0;
    // Update is called once per frame
    void Update()
    {

        if (photonView.IsMine)
        {
            if (!raceIsOver) { 
                if (!ghostMode)
                {
                    CheckAFK();
                }
                else
                {
                    if (ghostTimer > 10)
                    {
                        ghostTimer = 0;
                        ghostMode = false;
                    }
                    else
                        ghostTimer += Time.deltaTime;
                }
            }
            else
            {
                ShowLeaderBoard();
            }
            return;
        }
        var lagDistance = remotePosition - transform.position;
        if (lagDistance.magnitude > 5)
        {
            transform.position = remotePosition;
        }
        else
        {
            transform.SetPositionAndRotation(Vector3.MoveTowards(transform.position, 
                remotePosition,
                GetComponent<Rigidbody>().velocity.magnitude), 
                Quaternion.Lerp(transform.rotation, remoterotation, 0.2f));
        }
        GetComponent<AudioSource>().pitch = (GetComponent<Rigidbody>().velocity.magnitude / maxVelocity) * 2.8f;

    }

    public void TakeDamage(int damage,string attacker)
    {
        health -= damage;
        print(nickName + " took damage of " + damage.ToString());
        if (health <= 0)
            health = 0;
    }

    private void ShowLeaderBoard()
    {
        
        if(RaceManager.instance != null)
        {
            if (RaceManager.instance.gameOverPanel.gameObject.activeSelf)
            {
                return;
            }
            RaceManager.instance.gameOverPanel.SetActive(true);
            for (int i = 0; i < RaceManager.instance.raceParticipants.Count; i++)
            {
                playerManager manager = RaceManager.instance.raceParticipants[i].playerManager;
                GameObject tempracerInfoPrefab = Instantiate(RaceManager.instance.racerInfoPrefab, RaceManager.instance.leaderBoardMenu.transform);
                if (RaceManager.instance.raceParticipants.Count == 1)
                    tempracerInfoPrefab.GetComponent<raceInfoPrefabManager>().setInfo(i+1,manager.nickName,
                                                        Mathf.Abs(RaceManager.instance.raceParticipants[i].distanceTravelled),
                                                        manager.myColor, true);
                else if (i == 0)
                    tempracerInfoPrefab.GetComponent<raceInfoPrefabManager>().setInfo(i+1,manager.nickName,
                                                        Mathf.Abs(RaceManager.instance.raceParticipants[i].distanceTravelled - RaceManager.instance.raceParticipants[i + 1].distanceTravelled),
                                                        manager.myColor, true);
                else
                    tempracerInfoPrefab.GetComponent<raceInfoPrefabManager>().setInfo(i+1,manager.nickName,
                                                        Mathf.Abs(RaceManager.instance.raceParticipants[i].distanceTravelled - RaceManager.instance.raceParticipants[i - 1].distanceTravelled),
                                                        manager.myColor, true);

                if (RaceManager.instance.raceParticipants[i].playerManager.nickName == nickName)
                {
                    break;
                }
            }
        }
    }

    public void ResetToLastCheckPoint()
    {
        int lastCheckPoint = 0;
        ghostMode = true;
        for (int i = 0; i < checkPoints.Count; i++)
        {
            if (checkPoints[i].Iscompleted)
            {
                lastCheckPoint = i;
            }
            else
            {
                break;
            }

        }
        transform.position = RaceManager.instance.checkPointsTransform.GetChild(lastCheckPoint-5).position;
        this.transform.rotation = Quaternion.identity;
        this.transform.rotation =Quaternion.Euler(RaceManager.instance.checkPointsTransform.GetChild(lastCheckPoint).rotation.eulerAngles.x,
            RaceManager.instance.checkPointsTransform.GetChild(lastCheckPoint).rotation.eulerAngles.y+180,
            RaceManager.instance.checkPointsTransform.GetChild(lastCheckPoint).rotation.eulerAngles.z);
        GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity/2;
    }

    private void CheckAFK()
    {
        float lastCheckPointTime = 0;
        //int check = 0;
        for (int i = 0; i < checkPoints.Count; i++)
        {
            if (checkPoints[i].Iscompleted)
            {
                lastCheckPointTime = checkPoints[i].completedTime;
                //check = i;
            }
            else
            {
                break;
            }

        }
        if (lastCheckPointTime != 0)
        {
            if ((Time.time- lastCheckPointTime) > 1 && (Time.time - lastCheckPointTime) < 4)
            {
                RaceManager.instance.ShowMessage("You missed checkpoint Resseting back to last saved checkpoint");
            }
            else if((Time.time - lastCheckPointTime) > 4)
            {
                ResetToLastCheckPoint();
                RaceManager.instance.HideMessage();
            }
        }
    }

    [PunRPC]
    private void RPC_SetMyNickName(string _nickname)
    {
        nickName = _nickname;
    }

    [PunRPC]
    private void RPC_UpdateMyColor(int colorId)
    {
        Color tempColor;
        switch (colorId)
        {
            case 4:
                tempColor = Color.black;
                break;
            case 2:
                tempColor = Color.red;
                break;
            case 3:
                tempColor = Color.gray;
                break;
            case 1:
                tempColor = Color.yellow;
                break;
            case 5:
                tempColor = Color.magenta;
                break;
            case 6:
                tempColor = Color.white;
                break;
            case 7:
                tempColor = Color.cyan;
                break;
            default:
                tempColor = Color.green;
                break;
        }
        myColor = tempColor;
        miniMapVision.color = tempColor;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            remotePosition = (Vector3)stream.ReceiveNext();
            remoterotation = (Quaternion)stream.ReceiveNext();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "checkPoints")
        {
            if (!raceIsOver)
            {
                for (int i = 0; i < checkPoints.Count; i++)
                {
                    if (checkPoints[i].checkPointIndex == other.gameObject.transform.GetSiblingIndex())
                    {
                        if (checkPoints[i].Iscompleted != true)
                        {
                            if (other.gameObject.name == "checkpoint 1 (874)")
                            {
                                //print((i, RaceManager.instance.checkPointsTransform.childCount));
                                RaceManager.instance.PlayerEnter(this, i, true);
                            }
                            else
                            {
                                RaceManager.instance.PlayerEnter(this, i);
                            }
                        }
                    }
                }
            }
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        RaceManager.instance.raceParticipants.Add(new Participant(this));
    }
}
