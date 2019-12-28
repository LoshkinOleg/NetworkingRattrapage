using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Public properties.
    public static GameManager Instance = instance;
    static GameManager instance = null;

    // Serialize fields.
    [SerializeField] List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    // Private fields.
    List<PlayerController> players = new List<PlayerController>();

    // Public methods.
    public SpawnPoint FindRespawnPoint(PlayerController caller)
    {
        Vector3 averagePlayersPos = Vector3.zero;
        foreach (var player in players)
        {
            if (player != caller)
            {
                averagePlayersPos += player.transform.position;
            }
        }
        averagePlayersPos /= players.Count - 1;

        List<KeyValuePair<SpawnPoint, Vector3>> pairs = new List<KeyValuePair<SpawnPoint, Vector3>>();
        foreach (var spawnPoint in spawnPoints)
        {
            pairs.Add(new KeyValuePair<SpawnPoint, Vector3>(spawnPoint, spawnPoint.transform.position - averagePlayersPos));
        }
        pairs.Sort((x, y) => x.Value.magnitude.CompareTo(y.Value.magnitude));

        return pairs[0].Key;
    }
    public void RegisterPlayer(PlayerController caller)
    {
        players.Add(caller);
    }
    public void RegisterSpawnPoint(SpawnPoint caller)
    {
        spawnPoints.Add(caller);
    }

    // Monobehaviour inherited.
    private void Awake()
    {
        Debug.Assert(!instance);
        instance = this;
    }
}
