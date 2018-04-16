using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static int IntegerDifference(int value1, int value2)
    {
        return Mathf.Abs(value1 - value2);
    }

    public static Vector2 Rotate(this Vector2 o, float degrees)
    {
        Debug.Log("Rotating (" + o.x + ", " + o.y + ") by " + degrees + " degrees.");

        Vector2 res = Quaternion.Euler(0, 0, -degrees) * o;
        Debug.Log("Result of rotation is " + res);

        return res;
    }
}
