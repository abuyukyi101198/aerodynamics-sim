using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AirplaneController : MonoBehaviour
{
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    List<WheelCollider> wheels = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    [SerializeField]
    GameObject propeller = null;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    public float Flap;
    [Range(0, 1)]
    public float Thrust;
    [SerializeField]
    TMP_Text displayText = null;

    float brakesTorque;
    float ROTATION_SPEED = 0f;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    private void Start()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = Input.GetAxis("Yaw");
        Thrust = Input.GetAxis("Thrust");

        if (Thrust > 0) 
        {
            ROTATION_SPEED = ROTATION_SPEED < 50f ? ROTATION_SPEED + 0.1f : 50f;
        }
        else
        {
            ROTATION_SPEED = ROTATION_SPEED > 0 ? ROTATION_SPEED - 0.1f : 0;
        }
        
        propeller.transform.Rotate(Vector3.back, ROTATION_SPEED);

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }

        displayText.text = "V: " + ((int)rb.velocity.magnitude).ToString("D3") + " m/s\n";
        displayText.text += "A: " + ((int)transform.position.y).ToString("D4") + " m\n";
        displayText.text += "T: " + (int)(Thrust * 100) + "%\n";
        displayText.text += brakesTorque > 0 ? "B: ON" : "B: OFF";
    }

    private void FixedUpdate()
    {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(Thrust);
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque;
            wheel.motorTorque = 0.01f;
        }
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (AeroSurface surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;

            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch, pitchControlSensitivity);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll, rollControlSensitivity);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw, yawControlSensitivity);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(flap, 1);
                    break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
    }
}
