using UnityEngine;

public struct BiVector3
{
    public Vector3 force;
    public Vector3 torque;

    public BiVector3(Vector3 force, Vector3 torque)
    {
        this.force = force;
        this.torque = torque;
    }

    public static BiVector3 operator+(BiVector3 a, BiVector3 b)
    {
        return new BiVector3(a.force + b.force, a.torque + b.torque);
    }

    public static BiVector3 operator *(float f, BiVector3 a)
    {
        return new BiVector3(f * a.force, f * a.torque);
    }

    public static BiVector3 operator *(BiVector3 a, float f)
    {
        return f * a;
    }
}
