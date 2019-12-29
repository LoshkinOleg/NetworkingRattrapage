using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;

public class GameManager : MonoBehaviour
{
    // Private fields.
    List<PlayerController> players = new List<PlayerController>();
    List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    // Public methods.
    public static GameManager GetGM()
    {
        foreach (var item in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (item.tag == "GameController")
            {
                return item.GetComponent<GameManager>();
            }
        }

        Debug.LogError("GM not found!");
        return null;
    }
    public void RegisterSpawnPoint(SpawnPoint caller)
    {
        spawnPoints.Add(caller);
    }
    public void RegisterPlayer(PlayerController caller)
    {
        players.Add(caller);
    }
    public Vector3 FindRespawnPosition(PlayerController caller)
    {
        // Calculate average enemy position.
        Vector3 averagePlayersPos = Vector3.zero;
        foreach (var player in players)
        {
            if (player != caller)
            {
                averagePlayersPos += player.transform.position;
            }
        }
        averagePlayersPos /= players.Count - 1;

        // Find the respawn point furthest away from the average enemy position.
        List<KeyValuePair<SpawnPoint, Vector3>> pairs = new List<KeyValuePair<SpawnPoint, Vector3>>();
        foreach (var spawnPoint in spawnPoints)
        {
            pairs.Add(new KeyValuePair<SpawnPoint, Vector3>(spawnPoint, spawnPoint.transform.position - averagePlayersPos));
        }
        pairs.Sort((x, y) => x.Value.magnitude.CompareTo(y.Value.magnitude));

        // And return it.
        return pairs[pairs.Count - 1].Key.transform.position;
    }
    public void BroadcastDamagePlayerEvent(NetworkId target)
    {
        foreach (var player in players)
        {
            player.DmgPlayer(target);
        }
    }
}
