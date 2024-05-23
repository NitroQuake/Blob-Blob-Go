using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerNetwork : MonoBehaviourPun, IPunObservable
{
    private Rigidbody playerRb;
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private float smoothPos = 5;
    private float smoothRot = 5;

    private void Awake()
    {
        PhotonNetwork.SendRate = 30;
        PhotonNetwork.SerializationRate = 30;

        playerRb = GetComponent<Rigidbody>();
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.playerRb.position);
            stream.SendNext(this.playerRb.rotation);
            stream.SendNext(this.playerRb.velocity);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            playerRb.velocity = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkPosition += (this.playerRb.velocity * lag);
        }
    }

    public void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            playerRb.position = Vector3.Lerp(playerRb.position, networkPosition, smoothPos * Time.fixedDeltaTime);
            playerRb.rotation = Quaternion.Lerp(playerRb.rotation, networkRotation, smoothRot * Time.fixedDeltaTime);
        }
    }
}
