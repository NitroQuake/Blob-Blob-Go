using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LaserNetwork : MonoBehaviourPun, IPunObservable
{
    private Transform laserRb;
    private Vector3 networkPosition;

    private void Awake()
    {
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;

        laserRb = GetComponent<Transform>();
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.laserRb.position);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
        }
    }

    public void Update()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.MoveTowards(transform.position, networkPosition, Time.deltaTime);
        }
    }
}
