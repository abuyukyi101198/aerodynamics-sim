using UnityEngine;

public class AeroMesh : MonoBehaviour
{
    private float neutralAngle;

    private void Start()
    {
        neutralAngle = transform.localEulerAngles.x;
    }

    public void SetMeshAngle(float angle)
    {
        Quaternion targetRotation = Quaternion.Euler(
            neutralAngle + angle,
            transform.localEulerAngles.y,
            transform.localEulerAngles.z
        );

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation, targetRotation, Mathf.Abs(angle));
    }
}