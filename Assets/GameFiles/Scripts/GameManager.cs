using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Bolt.Matchmaking;

[BoltGlobalBehaviour]
class GameManager : GlobalEventListener
{
    Dictionary<BoltConnection, BoltEntity> clientPlayers = new Dictionary<BoltConnection, BoltEntity>();
    BoltEntity serverPlayer = null;

    public override void BoltStartDone()
    {
        if (BoltNetwork.IsServer)
        {
            serverPlayer = BoltNetwork.Instantiate(BoltPrefabs.Player);
            serverPlayer.TakeControl();
        }
    }

    public override void Connected(BoltConnection connection)
    {
        base.Connected(connection);

        if (BoltNetwork.IsServer)
        {
            clientPlayers.Add(connection, BoltNetwork.Instantiate(BoltPrefabs.Player));
            clientPlayers[connection].AssignControl(connection);
        }
    }

    public override void Disconnected(BoltConnection connection)
    {
        base.Disconnected(connection);

        if (BoltNetwork.IsServer)
        {
            clientPlayers[connection].ReleaseControl();
            clientPlayers[connection].DestroyDelayed(0.0f);
            clientPlayers.Remove(connection);
        }
    }
}
