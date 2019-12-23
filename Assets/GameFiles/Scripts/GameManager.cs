using System.Collections.Generic;
using Bolt;

[BoltGlobalBehaviour(BoltNetworkModes.Server)]
class GameManager : GlobalEventListener
{
    Dictionary<BoltConnection, BoltEntity> clientPlayers = new Dictionary<BoltConnection, BoltEntity>();
    BoltEntity serverPlayer = null;

    public override void BoltStartDone()
    {
        // Instanciate a player prefab for the listen server.
        serverPlayer = BoltNetwork.Instantiate(BoltPrefabs.Player);
        serverPlayer.TakeControl();
    }

    public override void Connected(BoltConnection connection)
    {
        // Instanciate player prefabs for new clients.
        clientPlayers.Add(connection, BoltNetwork.Instantiate(BoltPrefabs.Player));
        clientPlayers[connection].AssignControl(connection);
    }

    public override void Disconnected(BoltConnection connection)
    {
        // Clean up after disconnected player.
        clientPlayers[connection].ReleaseControl();
        clientPlayers[connection].DestroyDelayed(0.0f);
        clientPlayers.Remove(connection);
    }
}
