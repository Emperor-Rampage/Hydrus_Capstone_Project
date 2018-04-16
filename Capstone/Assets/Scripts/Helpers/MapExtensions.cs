using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapClasses
{
    public static class MapClassesExtensions
    {
        public static int IntegerDifference(int value1, int value2)
        {
            return Mathf.Abs(value1 - value2);
        }

        public static Vector2Int Rotate(this Vector2Int o, float degrees)
        {
                float rad = degrees * Mathf.Deg2Rad;
                float s = Mathf.Sin(rad);
                float c = Mathf.Cos(rad);
                return new Vector2Int(
                    (int)(o.x * c + o.y * s),
                    (int)(o.y * c - o.x * s));
         // degrees *= Mathf.Deg2Rad;
         // return new Vector2(Mathf.Cos(degrees), Mathf.Sin(degrees));
        }
    }
}
