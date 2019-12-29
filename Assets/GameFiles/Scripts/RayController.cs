using UnityEngine;
using Bolt;

public class RayController : MonoBehaviour
{
    float lifeTime = 1.0f;

    [SerializeField] AudioSource audioSource = null;

    private void Start()
    {
        GetComponent<LineRenderer>().SetPositions(new Vector3[] { transform.position, transform.position + transform.forward * 100 });
        audioSource.PlayOneShot(audioSource.clip);
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
