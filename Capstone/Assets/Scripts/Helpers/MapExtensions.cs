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
    }
}
