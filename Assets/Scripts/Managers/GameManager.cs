using UnityEngine;
using Assets.Scripts.Generation;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GenerationSettings dungeonSettings;
    public GameObject playerPrefab;

    private bool isPaused = false;

    private Vector3 playerSpawnPosition = Vector3.zero;
    public static Transform player { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
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
}
