using UnityEngine;

public enum RotationAxis { X, Y, Z, nX, nY, nZ };

public class AeroMesh : MonoBehaviour
{
    public RotationAxis rotationAxis;
    private float neutralAngle;

    private void Start()
    {
        switch (rotationAxis)
        {
            case RotationAxis.X:
                neutralAngle = transform.localEulerAngles.x;
                break;
            case RotationAxis.Y:
                neutralAngle = transform.localEulerAngles.y;
                break;
            case RotationAxis.Z:
                neutralAngle = transform.localEulerAngles.z;
                break;
            case RotationAxis.nX:
                neutralAngle = transform.localEulerAngles.x;
                break;
            case RotationAxis.nY:
                neutralAngle = transform.localEulerAngles.y;
                break;
            case RotationAxis.nZ:
                neutralAngle = transform.localEulerAngles.z;
                break;
        }
        
    }

    public void SetMeshAngle(float angle)
    {
        Quaternion targetRotation = transform.localRotation;
        
        switch (rotationAxis)
        {
            case RotationAxis.X:
                targetRotation = Quaternion.Euler(
                    neutralAngle + angle,
                    transform.localEulerAngles.y,
                    transform.localEulerAngles.z);
                break;
            case RotationAxis.Y:
                targetRotation = Quaternion.Euler(
                    transform.localEulerAngles.x,
                    neutralAngle + angle,
                    transform.localEulerAngles.z);
                break;
            case RotationAxis.Z:
                targetRotation = Quaternion.Euler(
                    transform.localEulerAngles.x,
                    transform.localEulerAngles.y,
                    neutralAngle + angle);
                break;
            case RotationAxis.nX:
                targetRotation = Quaternion.Euler(
                    neutralAngle - angle,
                    transform.localEulerAngles.y,
                    transform.localEulerAngles.z);
                break;
            case RotationAxis.nY:
                targetRotation = Quaternion.Euler(
                    transform.localEulerAngles.x,
                    neutralAngle - angle,
                    transform.localEulerAngles.z);
                break;
            case RotationAxis.nZ:
                targetRotation = Quaternion.Euler(
                    transform.localEulerAngles.x,
                    transform.localEulerAngles.y,
                    neutralAngle - angle);
                break;
        }

        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            Mathf.Abs(angle));
    }
}