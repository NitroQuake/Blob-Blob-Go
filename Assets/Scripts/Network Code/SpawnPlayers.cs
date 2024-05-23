using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField] GameObject[] playerPrefabs;
    private float radius = 12;

    private void Start()
    {
        if(PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"] == null)
        {
            GameObject playerToSpawn = playerPrefabs[0];
            PhotonNetwork.Instantiate(playerToSpawn.name, SpawnObjectInTheCircle(radius), Quaternion.identity);
        }
        else
        {
            GameObject playerToSpawn = playerPrefabs[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"]];
            PhotonNetwork.Instantiate(playerToSpawn.name, SpawnObjectInTheCircle(radius), Quaternion.identity);
        }
    }

    private Vector3 SpawnObjectInTheCircle(float radius)
    {
        var Vector2 = Random.insideUnitCircle * radius;
        return new Vector3(Vector2.x, 2, Vector2.y);
    }
}
