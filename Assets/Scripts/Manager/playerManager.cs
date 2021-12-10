using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class playerManager : MonoBehaviourPunCallbacks,IPunObservable,IPunInstantiateMagicCallback
{
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


    public List<CheckPoint> checkPoints =new List<CheckPoint>() ;


    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "checkPoints")
        {
            for (int i = 0; i < checkPoints.Count; i++)
            {
                if(checkPoints[i].checkPointIndex== other.gameObject.transform.GetSiblingIndex())
                {
                    if(checkPoints[i].Iscompleted != true)
                    {
                        RaceManager.instance.PlayerEnter(this, i);
                    }
                }
            }
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < RaceManager.instance.checkPointsTransform.childCount; i++)
        {
            checkPoints.Add(new CheckPoint(i, false, 0));
        }
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

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            return;
        }
        var lagDistance = remotePosition - transform.position;
        if (lagDistance.magnitude > 5)
        {
            transform.position = remotePosition;
            lagDistance = Vector3.zero;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, remotePosition,0.2f);
            transform.rotation = Quaternion.Lerp(transform.rotation, remoterotation, 0.2f);
        }
        GetComponent<AudioSource>().pitch = (GetComponent<Rigidbody>().velocity.magnitude / maxVelocity) * 2.8f;

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

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        CheckPoint[] waypoint = new CheckPoint[12];
        RaceManager.instance.raceParticipants.Add(new Participant(this));
    }
}
