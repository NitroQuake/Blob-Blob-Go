using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public bool isGameActive;

    public GameObject startScreen;
    public GameObject gameOverScreen;

    public TextMeshProUGUI waveText;
    private int waveNumber = 0;
    [SerializeField] TextMeshProUGUI timeText;
    private float timeAdded = 0;

    public float laserSpeed = 5.0f;
    private float addSpeed = 2.0f;
    private int waveTime = 30;
    private bool activateAddSpeed = true;

    public GameObject[] lasers;
    public GameObject[] powerUp;
    private int randomIndex;
    private bool activateAddLaser = true;

    private float radius = 22;
    private float powerUpRadius = 14;
    private float greenPosY = 2;
    private float yellowPosY = 2.65f;
    private float redPosY = 2;

    private float beginTime = 4; // Change to 4 later
    private float repeatRate = 4;

    private PhotonView view;

    [SerializeField] TextMeshProUGUI winLossText;

    private PlayerController playerController;
    private Rigidbody playerRB;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    //runs game for all players
    public void StartGameForAll(int difficulty)
    {
        view.RPC("StartGame", RpcTarget.All, difficulty);
    }

    [PunRPC]
    public void StartGame(int difficulty)
    {
        repeatRate /= difficulty;
        startScreen.SetActive(false);
        StartCoroutine(BeginDelay());
    }

    IEnumerator BeginDelay()
    {
        yield return new WaitForSeconds(beginTime);
        isGameActive = true;
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            playerRB = player.GetComponent<Rigidbody>();
            playerRB.velocity = Vector3.zero;
        }
    }

    public void RestartGameForAll()
    {
        view.RPC("RestartGame", RpcTarget.All);
    }

    [PunRPC]
    public void RestartGame()
    {
        PhotonNetwork.LoadLevel("My Game");
    }

    public void GameOverForAll()
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 0)
        {
            view.RPC("GameOver", RpcTarget.All);
        }
    }

    public void BackToLobby()
    {
        SceneManager.LoadScene("Lobby");
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    public void GameOver()
    {
        if (GameObject.FindGameObjectsWithTag("Spectator").Length == 1)
        {
            isGameActive = false;
            Cursor.visible = true;
            gameOverScreen.SetActive(true);
        }
        else
        {
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Spectator"))
            {
                playerController = player.GetComponent<PlayerController>();
                if (playerController.lastOneAlive)
                {
                    winLossText.color = Color.blue;
                    winLossText.SetText("You Win!");
                }
                else
                {
                    winLossText.color = Color.red;
                    winLossText.SetText("You Loss!");
                }
            }
            isGameActive = false;
            Cursor.visible = true;
            gameOverScreen.SetActive(true);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (isGameActive) 
        {
            timeAdded += Time.deltaTime;

            waveText.text = "Wave: " + waveNumber;
            timeText.text = "Time: " + Mathf.RoundToInt(timeAdded) + "s";

            if (activateAddLaser && PhotonNetwork.IsMasterClient)
            {
                SpawnRandomObject();
                StartCoroutine(SpawnLaserDelay());
                activateAddLaser = false;
            }

            if (activateAddSpeed && isGameActive && PhotonNetwork.IsMasterClient)
            {
                waveNumber++;
                SpawnPowerUp();
                laserSpeed += addSpeed;
                StartCoroutine(SpawnTimer());
                activateAddSpeed = false;
            }
            else if(activateAddSpeed && isGameActive)
            {
                waveNumber++;
                laserSpeed += addSpeed;
                StartCoroutine(SpawnTimer());
                activateAddSpeed = false;
            }
        }
    }

    IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(waveTime);
        activateAddSpeed = true;
    }

    void SpawnRandomObject()
    {
        if (isGameActive)
        {
            randomIndex = Random.Range(0, lasers.Length);

            PhotonNetwork.InstantiateRoomObject(lasers[randomIndex].name, SpawnObjectsAlongTheCircumferance(radius), transform.rotation);
        }
    }

    IEnumerator SpawnLaserDelay()
    {
        yield return new WaitForSeconds(repeatRate);
        activateAddLaser = true;
    }

    private Vector3 SpawnObjectsAlongTheCircumferance(float radius)
    {
        var vector2 = Random.insideUnitCircle.normalized * radius;
        //.normalized makes it on the circumferance due to the magnitude being 1
        if (randomIndex == 0)
        {
            return new Vector3(vector2.x, greenPosY, vector2.y);
        }
        else if (randomIndex == 1)
        {
            return new Vector3(vector2.x, yellowPosY, vector2.y);
        }
        else
        {
            return new Vector3(vector2.x, redPosY, vector2.y);
        }

    }

    void SpawnPowerUp()
    {
        if(GameObject.FindGameObjectsWithTag("Invulnerability").Length == 0 && GameObject.FindGameObjectsWithTag("Knockback").Length == 0)
        {
            randomIndex = Random.Range(0, powerUp.Length);

            PhotonNetwork.InstantiateRoomObject(powerUp[randomIndex].name, SpawnObjectInTheCircle(powerUpRadius), transform.rotation);
        }
    }

    private Vector3 SpawnObjectInTheCircle(float radius)
    {
        var Vector2 = Random.insideUnitCircle * radius;
        return new Vector3(Vector2.x, 2, Vector2.y);
    }
}