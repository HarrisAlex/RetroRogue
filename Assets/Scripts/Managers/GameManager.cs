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
    private Dictionary<Room, List<Light>> rooms = new Dictionary<Room, List<Light>>();

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        // Create dungeon and get spawn point
        playerSpawnPosition = DungeonRenderer.RenderDungeon(dungeonSettings, out rooms);

        // Spawn Player
        player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity, null).transform;

        StartCoroutine(CalculateOcclusion());
    }

    void Update()
    {
        if (isPaused)
        {
            return;
        }
    }

    IEnumerator CalculateOcclusion()
    {
        Room currentRoom = null;

        while (true)
        {
            foreach (KeyValuePair<Room, List<Light>> pair in rooms)
            {
                if (pair.Key.ContainsPoint(new Vertex(player.position.x, player.position.z)))
                {
                    currentRoom = pair.Key;
                }

                yield return new WaitForEndOfFrame();
            }

            foreach (KeyValuePair<Room, List<Light>> pair in rooms)
            {
                if (currentRoom.connectedRooms == null)
                {
                    continue;
                }

                if (currentRoom.connectedRooms.Contains(pair.Key) || currentRoom == pair.Key)
                {
                    ShowRoom(pair.Key);
                }
                else
                {
                    HideRoom(pair.Key);
                }

                yield return new WaitForEndOfFrame();
            }
        }
    }

    void ShowRoom(Room room)
    {
        foreach (Light light in rooms[room])
        {
            light.enabled = true;
        }
    }

    void HideRoom(Room room)
    {
        foreach (Light light in rooms[room])
        {
            light.enabled = false;
        }
    }
}
