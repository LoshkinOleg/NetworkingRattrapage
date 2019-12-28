using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Bolt.Matchmaking;
using System;
using UdpKit;

[BoltGlobalBehaviour]
public class NetworkCallbacks : GlobalEventListener
{
    BoltEntity player = null;

    public override void SceneLoadLocalDone(string scene)
    {
        if (scene == "Main")
        {
            player = BoltNetwork.Instantiate(BoltPrefabs.Player);
            player.TakeControl();
        }
    }

    public override void SessionListUpdated(Map<Guid, UdpSession> sessionList)
    {
        BoltLog.Info(sessionList.Count);
    }

    public override void SessionCreated(UdpSession session)
    {
        BoltLog.Info("Session created");
    }
    public override void SessionCreationFailed(UdpSession session)
    {
        BoltLog.Info("Creation failed");
    }
}

public class Menu : GlobalEventListener
{
    private void OnGUI()
    {
        if (GUILayout.Button("Join"))
        {
            BoltLauncher.StartClient();
        }
        if (GUILayout.Button("Create"))
        {
            BoltLauncher.StartServer();
        }
    }

    public override void BoltStartDone()
    {
        if (BoltNetwork.IsServer)
        {
            BoltMatchmaking.CreateSession("test", null, "Main");
        }
        else
        {
            BoltMatchmaking.JoinRandomSession();
        }
    }
}
