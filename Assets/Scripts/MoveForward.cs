using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public float speed = 5.0f;
    private float time = 6.0f;
    private GameObject target;

    private SpawnManager spawnManager;

    // Start is called before the first frame update
    void Start()
    {
        spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        Ylookfdirection();
        StartCoroutine(DeathTimer());
        speed = spawnManager.laserSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    void Ylookfdirection()
    {
        target = GameObject.Find("Origin");

        Vector3 lookPos = target.transform.position;
        lookPos.y = transform.position.y;
            // Only gets the y value of the position, but keeps it as a Vector 3
        Quaternion rotation = Quaternion.LookRotation(lookPos - transform.position);
        transform.rotation = rotation;
        transform.Rotate(0, 0, 90);
    }
}