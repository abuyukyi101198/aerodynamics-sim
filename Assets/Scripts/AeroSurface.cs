using System;
using UnityEngine;

public enum ControlInputType { Pitch, Yaw, Roll, Flap }

public class AeroSurface : MonoBehaviour
{
    [SerializeField] AeroSurfaceConfig config = null;
    public bool IsControlSurface;
    public ControlInputType InputType;
    public float InputMultiplyer = 1;

    private float flapAngle;

    public void SetFlapAngle(float angle)
    {
        flapAngle = Mathf.Clamp(angle, -Mathf.Deg2Rad * 50, Mathf.Deg2Rad * 50);
    }

    public BiVector3 CalculateForces(Vector3 worldAirVelocity, float airDensity, Vector3 relativePosition)
    {
        BiVector3 forceAndTorque = new BiVector3();
        if (!gameObject.activeInHierarchy || config == null) return forceAndTorque;

        // Accounting for aspect ratio effect on lift coefficient.
        float correctedLiftSlope = config.liftSlope * config.aspectRatio /
           (config.aspectRatio + 2 * (config.aspectRatio + 4) / (config.aspectRatio + 2));

        // Calculating flap deflection influence on zero lift angle of attack
        // and angles at which stall happens.
        float theta = Mathf.Acos(2 * config.flapFraction - 1);
        float flapEffectivness = 1 - (theta - Mathf.Sin(theta)) / Mathf.PI;
        float deltaLift = correctedLiftSlope * flapEffectivness * AeroCoefficients.FlapEffectivnessCorrection(flapAngle) * flapAngle;

        float zeroLiftAoaBase = config.zeroLiftAoA * Mathf.Deg2Rad;
        float zeroLiftAoA = zeroLiftAoaBase - deltaLift / correctedLiftSlope;

        float stallAngleHighBase = config.stallAngleHigh * Mathf.Deg2Rad;
        float stallAngleLowBase = config.stallAngleLow * Mathf.Deg2Rad;

        float clMaxHigh = correctedLiftSlope * (stallAngleHighBase - zeroLiftAoaBase) + deltaLift * AeroCoefficients.LiftCoefficientMaxFraction(config.flapFraction);
        float clMaxLow = correctedLiftSlope * (stallAngleLowBase - zeroLiftAoaBase) + deltaLift * AeroCoefficients.LiftCoefficientMaxFraction(config.flapFraction);

        float stallAngleHigh = zeroLiftAoA + clMaxHigh / correctedLiftSlope;
        float stallAngleLow = zeroLiftAoA + clMaxLow / correctedLiftSlope;

        // Calculating air velocity relative to the surface's coordinate system.
        // Z component of the velocity is discarded. 
        Vector3 airVelocity = transform.InverseTransformDirection(worldAirVelocity);
        airVelocity = new Vector3(airVelocity.x, airVelocity.y);
        Vector3 dragDirection = transform.TransformDirection(airVelocity.normalized);
        Vector3 liftDirection = Vector3.Cross(dragDirection, transform.forward);

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

        Vector3 lift = liftDirection * aerodynamicCoefficients.x * dynamicPressure * area;
        Vector3 drag = dragDirection * aerodynamicCoefficients.y * dynamicPressure * area;
        Vector3 torque = -transform.forward * aerodynamicCoefficients.z * dynamicPressure * area * config.chord;

        forceAndTorque.force += lift + drag;
        forceAndTorque.torque += Vector3.Cross(relativePosition, forceAndTorque.force);
        forceAndTorque.torque += torque;

#if UNITY_EDITOR
        // For gizmos drawing.
        IsAtStall = !(angleOfAttack < stallAngleHigh && angleOfAttack > stallAngleLow);
        CurrentLift = lift;
        CurrentDrag = drag;
        CurrentTorque = torque;
#endif

        return forceAndTorque;
    }

#if UNITY_EDITOR
    // For gizmos drawing.
    public AeroSurfaceConfig Config => config;
    public float GetFlapAngle() => flapAngle;
    public Vector3 CurrentLift { get; private set; }
    public Vector3 CurrentDrag { get; private set; }
    public Vector3 CurrentTorque { get; private set; }
    public bool IsAtStall { get; private set; }
#endif
}
