using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class MissileController : MonoBehaviour
{
    [SerializeField]
    public Transform target;
    public NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (!gameObject.GetComponent<PhotonView>().IsMine)
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        agent.destination = target.position;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if (RaceManager.instance.me.nickName == other.gameObject.GetComponent<playerManager>().nickName)
                return;
            RaceManager.instance.DamagePlayer(other.gameObject.GetComponent<playerManager>().nickName, RaceManager.instance.me.nickName, 50,this.transform.position);
            PhotonNetwork.Destroy(this.gameObject);
        }
    }
}
