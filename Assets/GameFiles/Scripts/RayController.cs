using UnityEngine;
using Bolt;

public class RayController : MonoBehaviour
{
    float lifeTime = 1.0f;

    private void Start()
    {
        GetComponent<LineRenderer>().SetPositions(new Vector3[] { transform.position, transform.position + transform.forward * 100 });
    }

    private void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            BoltNetwork.Destroy(gameObject);
        }
    }
}
