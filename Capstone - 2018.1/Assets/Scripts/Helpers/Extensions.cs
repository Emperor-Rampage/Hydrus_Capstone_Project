using System.Collections;
using System.Collections.Generic;
using EntityClasses;
using UnityEngine;

public static class Extensions
{
    public static Vector3 GetHitLocation(Player player, Entity target) {
        Vector3 location = Vector3.zero;
        if (player == null || target == null) {
            Debug.LogError("ERROR: Passed in null argument into GetHitLocation.");
        } else {
            if (player.Instance == null || target.Instance == null) {
                Debug.Log("ERROR: Player or target instance is null in GetHitLocation.");
            } else {
                // TODO: Use Vector3.Lerp or Vector3.MoveTowards to get distance from target in player's direction.
                Vector3 playerLoc = target.Instance.transform.position;
                Vector3 targetLoc = target.Instance.transform.position;
                location = Vector3.MoveTowards(targetLoc, playerLoc, 0.25f);
            }
        }
        // location = Vector3.zero;
        return location;
    }

    public static int IntegerDifference(int value1, int value2)
    {
        return Mathf.Abs(value1 - value2);
    }

    public static Vector2 Rotate(this Vector2 o, float degrees)
    {
        Vector2 res = Quaternion.Euler(0, 0, -degrees) * o;
        return res;
    }
}
