using Bolt;

[BoltGlobalBehaviour]
class GlobalNetworkCallbacks : GlobalEventListener
{
    BoltEntity player = null;

    public override void SceneLoadLocalDone(string scene)
    {
        if (scene == "Main")
        {
            player = BoltNetwork.Instantiate(BoltPrefabs.Player, UnityEngine.Vector3.up, UnityEngine.Quaternion.identity);
            player.TakeControl();
        }
    }
}
