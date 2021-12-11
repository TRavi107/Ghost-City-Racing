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
    //public GameObject player;
    public controllerDr controler;
    public string nickName="Noobie";
    private float maxVelocity;

    private Vector3 remotePosition;
    private Quaternion remoterotation;

    public SpriteRenderer miniMapVision;

    private float lastDriftSoundPlayed = 0;
    public float DriftSoundDuration;


    public List<CheckPoint> checkPoints =new List<CheckPoint>() ;

    private bool ghostMode=false;

    // Start is called before the first frame update
    void Awake()
    {
        if (RaceManager.instance != null)
        {
            for (int i = 0; i < RaceManager.instance.checkPointsTransform.childCount; i++)
            {
                checkPoints.Add(new CheckPoint(i, false, 0));
            }
        }
        skidmarksController = RaceManager.instance.skidMarkController;

        if (!photonView.IsMine)
        {
            maxVelocity = GetComponent<controllerDr>().maxVelocity;
            //PhotonNetwork.Instantiate("Free_Racing_Car_BlueNetwork", new Vector3(5, 0, 0), Quaternion.identity);
            Destroy(FindObjectOfType<Camera>().gameObject);
            Destroy(GetComponent<controllerDr>());
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
            if (!ghostMode)
            {
                checkAFK();
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
            return;
        }
        var lagDistance = remotePosition - transform.position;
        if (lagDistance.magnitude > 5)
        {
            transform.position = remotePosition;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, remotePosition,GetComponent<Rigidbody>().velocity.magnitude);
            transform.rotation = Quaternion.Lerp(transform.rotation, remoterotation, 0.2f);
        }
        GetComponent<AudioSource>().pitch = (GetComponent<Rigidbody>().velocity.magnitude / maxVelocity) * 2.8f;

    }
    public void Skid(float skidTotal, int lastSkid, Vector3 normal, Vector3 _position, float radius)
    {
        photonView.RPC("RPC_Skid", RpcTarget.All,skidTotal,lastSkid,normal,_position,radius);
    }

    [PunRPC]
    void RPC_Skid(float skidTotal,int lastSkid,Vector3 normal,Vector3 _position,float radius)
    {

        float intensity = Mathf.Clamp01(skidTotal / 2);
        // Account for further movement since the last FixedUpdate
        Vector3 skidPoint = _position - transform.up * radius;//wheelHitInfo.point;// + (rb.velocity * (Time.time - lastFixedUpdateTime));

        lastSkid = skidmarksController.AddSkidMark(skidPoint,normal, intensity, lastSkid);
        if (lastDriftSoundPlayed + DriftSoundDuration < Time.time)
        {
            SoundManager.PlaySound(Sound.skid, transform.position);
            lastDriftSoundPlayed = Time.time;
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
                print(i);
                break;
            }

        }
        this.transform.position = RaceManager.instance.checkPointsTransform.GetChild(lastCheckPoint).position;
                                
        this.transform.rotation = RaceManager.instance.checkPointsTransform.GetChild(lastCheckPoint).rotation;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void checkAFK()
    {
        float lastCheckPointTime = 0;
        int check = 0;
        for (int i = 0; i < checkPoints.Count; i++)
        {
            if (checkPoints[i].Iscompleted)
            {
                lastCheckPointTime = checkPoints[i].completedTime;
                check = i;
            }
            else
            {
                break;
            }

        }
        if (lastCheckPointTime != 0)
        {
            if ((Time.time- lastCheckPointTime) > 2 && (Time.time - lastCheckPointTime) < 7)
            {
                RaceManager.instance.ShowMessage("warning out of track return to track");
            }
            else if((Time.time - lastCheckPointTime) > 7)
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
            for (int i = 0; i < checkPoints.Count; i++)
            {
                if (checkPoints[i].checkPointIndex == other.gameObject.transform.GetSiblingIndex())
                {
                    if (checkPoints[i].Iscompleted != true)
                    {
                        RaceManager.instance.PlayerEnter(this, i);
                        print(i);
                    }
                }
            }
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        CheckPoint[] waypoint = new CheckPoint[12];
        RaceManager.instance.raceParticipants.Add(new Participant(this));
    }
}
