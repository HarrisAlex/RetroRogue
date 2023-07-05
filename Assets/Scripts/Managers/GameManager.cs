using UnityEngine;
using Assets.Scripts.Generation;
using System;
using static Assets.Scripts.Generation.DungeonGeneration;
using Assets.Scripts.AI;

[RequireComponent(typeof(DungeonRenderer))]
public class GameManager : MonoBehaviour
{
    public GenerationSettings dungeonSettings;

    private bool isPaused = false;

    // Player
    public GameObject playerPrefab;
    public static Transform player { get; private set; }
    private Vector3 playerSpawnPosition = Vector3.zero;

    private DungeonGenerator dungeonGenerator;
    private DungeonRenderer dungeonRenderer;
    private Dungeon dungeon;

    // Start is called before the first frame update
    private void Awake()
    {
        // Get references
        dungeonRenderer = GetComponent<DungeonRenderer>();
        if (!dungeonRenderer) return;

        // Initialize cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Player.PlayerDeath += GameEnd;

        StartGame();
    }

    private void StartGame()
    {
        // Initialize cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Generate dungeon
        dungeonGenerator = new DungeonGenerator(dungeonSettings);
        dungeon = dungeonGenerator.Generate((int)DateTime.Now.Ticks);

        // Render dungeon
        playerSpawnPosition = dungeonRenderer.RenderDungeon(dungeon);

        // Spawn Player and register torch with light manager
        player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity, null).transform;
        LightManager.AddLight(player.GetChild(1).GetComponent<Light>());

        LightManager.Initialize();
    }

    private void Update()
    {
        if (isPaused) return;
    }

    private void GameEnd()
    {
        Debug.Log("Game is over");
    }
}
