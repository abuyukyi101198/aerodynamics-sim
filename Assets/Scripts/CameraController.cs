using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject cameraFollowTarget;
    [SerializeField]
    private float cameraDistance = 20f;
    [SerializeField]
    private float cameraHeight = 10f;

    private Rigidbody rb;

    void Start() 
    {
        rb = cameraFollowTarget.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb != null) 
        {
            transform.position = cameraFollowTarget.transform.position - (rb.transform.forward.normalized * cameraDistance) + (rb.transform.up.normalized * cameraHeight);
            transform.LookAt(cameraFollowTarget.transform);
        }
    }
}
