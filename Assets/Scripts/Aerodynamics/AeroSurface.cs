using UnityEngine;

public enum ControlInputType { Pitch, Yaw, Roll, Flap }

public class AeroSurface : MonoBehaviour
{
    [SerializeField] AeroSurfaceConfig config = null;
    public bool IsControlSurface;
    public ControlInputType InputType;
    public float InputMultiplyer = 1;
    public AeroMesh aeroMesh = null;

    private float flapAngle;

    public void SetFlapAngle(float control, float sensitivity)
    {
        flapAngle = Mathf.Clamp(control * sensitivity * InputMultiplyer, -Mathf.Deg2Rad * 50, Mathf.Deg2Rad * 50);
        aeroMesh.SetMeshAngle(control * InputMultiplyer * 15f);
    }

    public BiVector3 CalculateForces(Vector3 worldAirVelocity, float airDensity, Vector3 relativePosition)
    {
        BiVector3 forceAndTorque = new();
        if (!gameObject.activeInHierarchy || config == null) return forceAndTorque;

        float correctedLiftSlope = CalculateCorrectedLiftSlope();
        float deltaLift = CalculateDeltaLift(correctedLiftSlope);

        var (zeroLiftAoA, stallAngleHigh, stallAngleLow) = CalculateSurfaceParameters(correctedLiftSlope, deltaLift);
        var (airVelocity, dragDirection, liftDirection) = GetForceDirections(worldAirVelocity);

        float area = config.chord * config.span;
        float dynamicPressure = 0.5f * airDensity * airVelocity.sqrMagnitude;
        float angleOfAttack = Mathf.Atan2(airVelocity.y, -airVelocity.x);

        Vector3 aerodynamicCoefficients = AeroCoefficients.CalculateCoefficients(angleOfAttack,
                                                                correctedLiftSlope,
                                                                zeroLiftAoA,
                                                                stallAngleHigh,
                                                                stallAngleLow,
                                                                flapAngle,
                                                                config);

        Vector3 lift = aerodynamicCoefficients.x * area * dynamicPressure * liftDirection;
        Vector3 drag = aerodynamicCoefficients.y * area * dynamicPressure * dragDirection;
        Vector3 torque = aerodynamicCoefficients.z * area * config.chord * dynamicPressure * -transform.forward;

        forceAndTorque.force += lift + drag;
        forceAndTorque.torque += Vector3.Cross(relativePosition, forceAndTorque.force);
        forceAndTorque.torque += torque;

        #if UNITY_EDITOR
        IsAtStall = !(angleOfAttack < stallAngleHigh && angleOfAttack > stallAngleLow);
        CurrentLift = lift;
        CurrentDrag = drag;
        CurrentTorque = torque;
        #endif

        return forceAndTorque;
    }

    private float CalculateCorrectedLiftSlope()
    {
        return config.liftSlope * config.aspectRatio / 
            (config.aspectRatio + 2 * (config.aspectRatio + 4) / (config.aspectRatio + 2));
    }

    private float CalculateDeltaLift(float correctedLiftSlope) 
    {
        float theta = Mathf.Acos(2 * config.flapFraction - 1);
        float flapEffectivness = 1 - (theta - Mathf.Sin(theta)) / Mathf.PI;
        return correctedLiftSlope * flapEffectivness * AeroCoefficients.FlapEffectivnessCorrection(flapAngle) * flapAngle;
    }

    private (float, float, float) CalculateSurfaceParameters(float correctedLiftSlope, float deltaLift)
    {
        float zeroLiftAoaBase = config.zeroLiftAoA * Mathf.Deg2Rad;
        float zeroLiftAoA = zeroLiftAoaBase - deltaLift / correctedLiftSlope;

        float stallAngleHighBase = config.stallAngleHigh * Mathf.Deg2Rad;
        float stallAngleLowBase = config.stallAngleLow * Mathf.Deg2Rad;

        float clMaxHigh = correctedLiftSlope * (stallAngleHighBase - zeroLiftAoaBase) + deltaLift * AeroCoefficients.LiftCoefficientMaxFraction(config.flapFraction);
        float clMaxLow = correctedLiftSlope * (stallAngleLowBase - zeroLiftAoaBase) + deltaLift * AeroCoefficients.LiftCoefficientMaxFraction(config.flapFraction);

        float stallAngleHigh = zeroLiftAoA + clMaxHigh / correctedLiftSlope;
        float stallAngleLow = zeroLiftAoA + clMaxLow / correctedLiftSlope;

        return (zeroLiftAoA, stallAngleHigh, stallAngleLow);
    }

    private (Vector3, Vector3, Vector3) GetForceDirections(Vector3 worldAirVelocity)
    {
        Vector3 airVelocity = transform.InverseTransformDirection(worldAirVelocity);
        airVelocity = new Vector3(airVelocity.x, airVelocity.y);
        Vector3 dragDirection = transform.TransformDirection(airVelocity.normalized);
        Vector3 liftDirection = Vector3.Cross(dragDirection, transform.forward);

        return (airVelocity, dragDirection, liftDirection);
    }

    #if UNITY_EDITOR
    public AeroSurfaceConfig Config => config;
    public float GetFlapAngle() => flapAngle;
    public Vector3 CurrentLift { get; private set; }
    public Vector3 CurrentDrag { get; private set; }
    public Vector3 CurrentTorque { get; private set; }
    public bool IsAtStall { get; private set; }
    #endif
}