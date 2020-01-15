using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Bolt;

public enum EventType
{
    SHOT,
    DAMAGE,
    KILL
}

public class EventRelayer : GlobalEventListener
{
    // Private fields.
    Dictionary<BoltEntity, PlayerController> players = new Dictionary<BoltEntity, PlayerController>();

    // Public methods.
    public static EventRelayer Get()
    {
        foreach (var item in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (item.tag == "GameController")
            {
                return item.GetComponent<EventRelayer>();
            }
        }

        Debug.LogError("EventRelayer not found!");
        return null;
    }
    public void Send(EventType type, params BoltEntity[] entities)
    {
        switch (type)
        {
            case EventType.SHOT:
                {
                    var evnt = ShotEvent.Create(GlobalTargets.Everyone);
                    evnt.Shooter = entities[0];
                    evnt.Send();
                }
                break;
            case EventType.DAMAGE:
                {
                    var evnt = DamageEvent.Create(GlobalTargets.Everyone);
                    evnt.Sender = entities[0];
                    evnt.Target = entities[1];
                    evnt.Send();
                }
                break;
            case EventType.KILL:
                {
                    var evnt = KillEvent.Create(GlobalTargets.Everyone);
                    evnt.Killer = entities[0];
                    evnt.Send();
                }
                break;
        }
    }
    public void SetPlayers(PlayerController[] players)
    {
        foreach (var player in players)
        {
            if (!this.players.ContainsKey(player.entity))
            {
                this.players.Add(player.entity, player);
            }
        }
    }

    // Inherited.
    public override void OnEvent(ShotEvent evnt)
    {
        RelayEvent(evnt, EventType.SHOT);
    }
    public override void OnEvent(DamageEvent evnt)
    {
        RelayEvent(evnt, EventType.DAMAGE);
    }
    public override void OnEvent(KillEvent evnt)
    {
        RelayEvent(evnt, EventType.KILL);
    }

    void RelayEvent(Bolt.Event evnt, EventType type)
    {
        foreach (var player in players.Values)
        {
            player.ProcessEvent(evnt, type);
        }
    }
}
