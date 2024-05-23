using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Difficulty : MonoBehaviour
{
    private SpawnManager spawnManager;
    private Button button;
    public int difficulty;

    // Start is called before the first frame update
    void Start()
    {
        spawnManager = GameObject.Find("Spawn Manager").GetComponent<SpawnManager>();
        button = GetComponent<Button>();
        button.onClick.AddListener(DifficultyChosen);
    }

    private void DifficultyChosen()
    {
        Cursor.visible = false;
        spawnManager.StartGameForAll(difficulty);
    }
}