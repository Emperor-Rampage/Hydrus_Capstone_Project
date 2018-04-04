using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MapClasses;

[ExecuteInEditMode]
public class ShowLevel : MonoBehaviour
{

    [SerializeField] float cellScale = 1;
    [SerializeField] int levelToLoad = 0;
    [SerializeField] Level[] levels;

    void OnDrawGizmos()
    {
        if (levelToLoad < 0 || levelToLoad >= levels.Length)
        {
            Debug.LogWarning("Level To Load is out of range.");
            return;
        }

        Level level = levels[levelToLoad];

        Texture2D levelMap = level.levelMap;
        if (levelMap == null)
        {
            Debug.Log("Level texture is null.");
            return;
        }

        Color[] levelPixels = levelMap.GetPixels();
        int width = levelMap.width;
        int depth = levelMap.height;

        for (int p = 0; p < levelPixels.Length; p++)
        {
            Gizmos.color = levelPixels[p];
            Vector3 center = new Vector3(Cell.GetX(p, width) * cellScale, 0, Cell.GetZ(p, width) * cellScale);
            Vector3 size = new Vector3(0.98f, 0f, 0.98f) * cellScale;

            Gizmos.DrawWireCube(center, size);
        }


        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gameObject.transform.position, 0.25f);
    }
}
