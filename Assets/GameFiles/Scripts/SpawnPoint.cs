using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    bool isRegistered = false;

    private void Update()
    {
        if (!isRegistered)
        {
            if (GameManager.Instance)
            {
                isRegistered = true;
                GameManager.Instance.RegisterSpawnPoint(this);
            }
        }
    }
}
