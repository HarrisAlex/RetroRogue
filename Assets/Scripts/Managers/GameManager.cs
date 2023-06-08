using UnityEngine;
using Assets.Scripts.Generation;
using System;

public class GameManager : MonoBehaviour
{
    public GenerationSettings dungeonSettings;

    private bool isPaused = false;

    // Player
    public GameObject playerPrefab;
    public static Transform player { get; private set; }
    public event Action PlayerDeath;
    private Vector3 playerSpawnPosition = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayerDeath += GameEnd;

        StartGame();
    }

    void StartGame()
    {
        // Initialize cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Create dungeon and get spawn point
        playerSpawnPosition = DungeonRenderer.RenderDungeon(dungeonSettings);

        // Spawn Player
        player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity, null).transform;
        LightManager.AddLight(player.GetChild(1).GetComponent<Light>());

        LightManager.Initialize();
    }

    void Update()
    {
        if (isPaused) return;
    }

    private void GameEnd()
    {
        Debug.Log("Game is over");
    }
}
