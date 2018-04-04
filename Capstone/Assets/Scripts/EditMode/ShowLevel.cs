using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MapClasses;
using EntityClasses;

[ExecuteInEditMode]
public class ShowLevel : MonoBehaviour
{

    [SerializeField] GameManager manager;
    [SerializeField] int levelToLoad = 0;
    Map map;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gameObject.transform.position, 0.25f);
        if (manager == null)
            return;

        map = manager.Map;

        var levels = map.levels;

        if (levelToLoad < 0 || levelToLoad >= levels.Count)
        {
            Debug.LogWarning("Level To Load is out of range.");
            return;
        }

        Level level = levels[levelToLoad];
        DrawCells(level);
//        DrawEntitySpawns(level);
    }

    void DrawCells(Level level)
    {
        Texture2D levelMap = level.levelMap;
        if (levelMap == null)
        {
            Debug.Log("Level texture is null.");
            return;
        }

        Color[] levelPixels = levelMap.GetPixels();
        int width = levelMap.width;
        int depth = levelMap.height;

        Vector3 size = new Vector3(0.98f, 0f, 0.98f) * map.CellScale;
        for (int p = 0; p < levelPixels.Length; p++)
        {
            Gizmos.color = levelPixels[p];
            Vector3 center = new Vector3(Cell.GetX(p, width) * map.CellScale, 0, Cell.GetZ(p, width) * map.CellScale);

            Gizmos.DrawWireCube(center, size);
        }
    }

    // FIXME: Resolve null spawn.Enemy exception on line 86.
    void DrawEntitySpawns(Level level)
    {
//        Vector3 size = new Vector3(1f, 1f, 1f);
        if (map == null)
        {
            Debug.Log("MAP IS NULL.");
        }
        if (level == null)
        {
            Debug.Log("LEVEL IS NULL");
        }
        Debug.Log(level.spawnList.ToString());
        foreach (EnemySpawn spawn in level.spawnList)
        {
            if (spawn == null)
            {
                Debug.Log("SPAWN IS NULL");
            }
            if (spawn.Enemy == null)
            {
                Debug.Log("SPAWN ENEMY IS NULL");
            }
            Gizmos.color = Color.white;
            Vector3 center = new Vector3(spawn.X * map.CellScale, 0.5f, spawn.Z * map.CellScale);
            Debug.Log("Drawing " + spawn.Enemy.name + " at " + spawn.X + ", " + spawn.Z);
            Gizmos.DrawSphere(center, 0.5f);
        }
    }
}
