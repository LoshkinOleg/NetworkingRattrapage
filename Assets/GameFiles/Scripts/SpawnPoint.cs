using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    bool isRegistered = false;

    private void Update()
    {
        if (!isRegistered)
        {
            isRegistered = true;
            GameManager.Get().RegisterSpawnPoint(this);
        }
    }
}
