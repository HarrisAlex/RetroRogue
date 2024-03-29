using UnityEngine;
using Assets.Scripts.Generation;
using static Assets.Scripts.Generation.DungeonGeneration;

[RequireComponent(typeof(DungeonRenderer))]
public class GameManager : MonoBehaviour
{
    public GenerationSettings generationSettings;
    public RenderingSettings renderingSettings;

    private bool isPaused = false;

    // Player
    public GameObject playerPrefab;
    public static Transform player { get; private set; }
    private Vector3 playerSpawnPosition = Vector3.zero;

    private DungeonGenerator dungeonGenerator;
    private DungeonRenderer dungeonRenderer;
    public static Dungeon dungeon { get; private set; }

    private void Awake()
    {
        // Get references
        dungeonRenderer = GetComponent<DungeonRenderer>();
        if (!dungeonRenderer) return;

        // Initialize cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Player.PlayerDeath += GameEnd;
    }

    private void Start()
    {
        StartGame();
    }

    private void StartGame()
    {
        // Initialize cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Generate dungeon
        dungeonGenerator = new DungeonGenerator(generationSettings);
        dungeon = dungeonGenerator.Generate();

        // Render dungeon
        playerSpawnPosition = dungeonRenderer.RenderDungeon(dungeon, renderingSettings);

        // Spawn Player and register torch with light manager
        player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity, null).transform;
        LightManager.AddLight(player.GetChild(1).GetComponent<UnityEngine.Light>());

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
