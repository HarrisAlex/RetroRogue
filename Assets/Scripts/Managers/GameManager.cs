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
    private Vector3 playerSpawnPosition = Vector3.zero;

    DungeonGenerator dungeonGenerator;
    private Dungeon dungeon;

    // Start is called before the first frame update
    private void Awake()
    {
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
        playerSpawnPosition = DungeonRenderer.RenderDungeon(dungeon);

        // Spawn Player and register torch with light manager
        player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity, null).transform;
        LightManager.AddLight(player.GetChild(1).GetComponent<Light>());

        LightManager.Initialize();

        dungeon.TryGetRandomRoomCenter(out Geometry.Vertex start);
        dungeon.TryGetRandomRoomCenter(out Geometry.Vertex end);

        System.Collections.Generic.List<Geometry.Vertex> path = dungeon.navigationTree.FindPath(start, end);

        foreach (Geometry.Vertex v in path)
            Debug.Log("Next node is: " + v.x + ", " + v.y);
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
