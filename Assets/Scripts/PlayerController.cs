using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    private SpawnManager spawnManager;
    
    private float speed = 10.0f;
    private GameObject focalPoint;
    private Vector3 gravity = new Vector3(0, -9.81f, 0);
    public float gravityModifier;
    private Rigidbody playerRb;

    private float jumpForce = 17.0f;
    [SerializeField] bool allowJump;

    private Vector3 smallSize = new Vector3(0.5f, 0.5f, 0.5f);
    private Vector3 normalSize = new Vector3(1, 1, 1);

    [SerializeField] GameObject powerupIndicator;
    private float powerUpTime = 8;
    private bool hasInvulnurabilityPow = false;

    private float knockbackForce = 25;
    private bool hasKnockbackPow = false;

    private AudioSource audioSource;
    [SerializeField] AudioClip laserHitSound;
    [SerializeField] ParticleSystem blueExplosion;

    private PhotonView view;
    [SerializeField] Camera myCam;

    private SphereCollider playerCollider;
    private bool isSpectator = false;
    private Vector3 spectatorSpawnPos = new Vector3(0, 5, 0);
    private string spectatorTag = "Spectator";
    private MeshRenderer playerMR;
    private float specTime = 0.5f;

    public bool lastOneAlive;

    // Start is called before the first frame update
    void Start()
    {   
        spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();

        focalPoint = GameObject.Find("Focal Point");

        audioSource = GetComponent<AudioSource>();

        playerRb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<SphereCollider>();
        playerMR = GetComponent<MeshRenderer>();


        Physics.gravity = gravity * gravityModifier;

        view = GetComponent<PhotonView>();
        myCam.enabled = false;
        lastOneAlive = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (view.IsMine)
        {
            myCam.enabled = true;
            if (spawnManager.isGameActive && !isSpectator)
            {
                PlayerMovement();
                PlayerJumpConstraints();
                PlayerSizeChange();
                if (transform.position.y < -3)
                {
                    SetSpectator();
                    spawnManager.GameOverForAll();
                }
            }
            if (spawnManager.isGameActive && isSpectator)
            {
                SpectatorMode();
                PlayerMovement();
            }
        }
    }
    
    private void SetSpectator()
    {
        blueExplosion.Play();
        hasInvulnurabilityPow = false;
        hasKnockbackPow = false;
        this.powerupIndicator.gameObject.SetActive(false);
        playerRb.useGravity = false;
        view.RPC("SetSpectatorForAll", RpcTarget.All, spectatorTag);
        isSpectator = true;
        playerRb.velocity = Vector3.zero;
        StartCoroutine(SpectatorSpawnPosTimer());
    }

    [PunRPC]
    private void SetSpectatorForAll(string spectator)
    {
        this.tag = spectator;
        this.playerCollider.enabled = false;
        Color color = playerMR.material.color;
        color.a = 0;
        playerMR.material.color = color;
        if (!view.IsMine)
        {
            this.playerMR.enabled = false;
            lastOneAlive = false;
        }
        else
        {
            lastOneAlive = true;
        }
    }

    private void SpectatorMode()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            playerRb.AddForce(Vector3.up * speed);
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            playerRb.AddForce(Vector3.up * -speed);
        }
    }

    // Player movement 
    void PlayerMovement()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        playerRb.AddForce(focalPoint.transform.forward * speed * verticalInput);
            //don't add Time.deltaTime to Addforce
        playerRb.AddForce(focalPoint.transform.right * speed * horizontalInput);
    }

    // Player jump constraints
    void PlayerJumpConstraints()
    {
        if (Input.GetKey(KeyCode.Space) && allowJump)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                //don't add Time.deltaTime tp Addforce
            allowJump = false;
        }
    }

    // Player changes size to avoid lasers
    void PlayerSizeChange()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.localScale = smallSize;
            jumpForce = 12.0f;
        }
        else
        {
            transform.localScale = normalSize;
            jumpForce = 17.0f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Arena"))
        {
            allowJump = true;
        }

        if (collision.gameObject.CompareTag("Player") && hasKnockbackPow && spawnManager.isGameActive)
        {
            Rigidbody opponentRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 oppositeDirection = collision.gameObject.transform.position - this.transform.position;

            opponentRb.AddForce(oppositeDirection * knockbackForce, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Invulnerability") && spawnManager.isGameActive)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(other.gameObject);
            }
            else
            {
                view.TransferOwnership(view.Owner);
                PhotonNetwork.Destroy(other.gameObject);
            }
            view.RPC("PowerUpShownForAllInvulnerability", RpcTarget.All);
            StartCoroutine(PowerUpTimerInvulnerability());
        }

        if (other.gameObject.CompareTag("Knockback") && spawnManager.isGameActive)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(other.gameObject);
            }
            else
            {
                view.TransferOwnership(view.Owner);
                PhotonNetwork.Destroy(other.gameObject);
            }
            view.RPC("PowerUpShownForAllKnockback", RpcTarget.All);
            StartCoroutine(PowerUpTimerKnockback());
        }

        if (other.gameObject.CompareTag("Red Laser") && !hasInvulnurabilityPow && spawnManager.isGameActive)
        {
            audioSource.PlayOneShot(laserHitSound);
            SetSpectator();
            spawnManager.GameOverForAll();
        }
        else if (other.gameObject.CompareTag("Yellow Laser") && !hasInvulnurabilityPow && spawnManager.isGameActive)
        {
            audioSource.PlayOneShot(laserHitSound);
            SetSpectator();
            spawnManager.GameOverForAll();
        }
    }

    [PunRPC]
    private void PowerUpShownForAllInvulnerability()
    {
        this.hasInvulnurabilityPow = true;
        this.powerupIndicator.gameObject.SetActive(true);
    }

    [PunRPC]
    private void PowerUpDisShownForAllInvulnerability()
    {
        this.hasInvulnurabilityPow = false;
        this.powerupIndicator.gameObject.SetActive(false);
    }

    IEnumerator PowerUpTimerInvulnerability()
    {
        yield return new WaitForSeconds(powerUpTime);
        view.RPC("PowerUpDisShownForAllInvulnerability", RpcTarget.All);
    }

    [PunRPC]
    private void PowerUpShownForAllKnockback()
    {
        this.hasKnockbackPow = true;
        this.powerupIndicator.gameObject.SetActive(true);
    }

    [PunRPC]
    private void PowerUpDisShownForAllKnockback()
    {
        this.hasKnockbackPow = false;
        this.powerupIndicator.gameObject.SetActive(false);
    }

    IEnumerator PowerUpTimerKnockback()
    {
        yield return new WaitForSeconds(powerUpTime);
        view.RPC("PowerUpDisShownForAllKnockback", RpcTarget.All);
    }

    IEnumerator SpectatorSpawnPosTimer()
    {
        yield return new WaitForSeconds(specTime);
        transform.position = spectatorSpawnPos;
    }
}