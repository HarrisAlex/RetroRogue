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

    DungeonGenerator dungeon;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayerDeath += GameEnd;

        //StartGame();
    }

    void StartGame()
    {
        // Initialize cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Generate dungeon
        dungeon = new DungeonGenerator(dungeonSettings);
        dungeon.Generate((int)DateTime.Now.Ticks);

        // Render dungeon
        playerSpawnPosition = DungeonRenderer.RenderDungeon(dungeon);

        // Spawn Player and register torch with light manager
        player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity, null).transform;
        LightManager.AddLight(player.GetChild(1).GetComponent<Light>());

        LightManager.Initialize();
    }

    void Update()
    {
        if (isPaused) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            dungeon = new DungeonGenerator(dungeonSettings);
            dungeon.Generate((int)DateTime.Now.Ticks);

            DungeonRenderer.RenderDungeon(dungeon);
        }
    }

    private void GameEnd()
    {
        Debug.Log("Game is over");
    }
}
