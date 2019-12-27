using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;
using Bolt.Matchmaking;
using UdpKit;
using System;

[BoltGlobalBehaviour]
public class SimpleMenu : GlobalEventListener
{
    BoltEntity player = null;

    public override void BoltStartDone()
    {
        List<UdpSession> sessions = new List<UdpSession>();
        foreach (var item in BoltNetwork.SessionList)
        {
            sessions.Add(item.Value);
        }

        if (BoltNetwork.IsServer)
        {
            BoltMatchmaking.CreateSession("test", null, "Main");
        }
        else
        {
            UdpSession session = sessions.Find(x => x.HostName == "test");
            if (session != null)
            {
                BoltMatchmaking.JoinSession(sessions.Find(x => x.HostName == "test"));
            }
            else
            {
                BoltLog.Error("Session not found.");
                foreach (var item in sessions)
                {
                    BoltLog.Error(item.HostName);
                }
            }
        }
    }
    public override void SceneLoadLocalDone(string scene)
    {
        if (scene == "Main")
        {
            player = BoltNetwork.Instantiate(BoltPrefabs.Player);
            player.TakeControl();
        }
    }
}
