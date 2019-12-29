using Bolt;

[BoltGlobalBehaviour]
class GlobalNetworkCallbacks : GlobalEventListener
{
    public override void SceneLoadLocalDone(string scene)
    {
        if (scene == "Main")
        {
            BoltNetwork.Instantiate(BoltPrefabs.Player, UnityEngine.Vector3.up, UnityEngine.Quaternion.identity).TakeControl();
        }
    }
}
