﻿using System.Collections;
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
        DrawEntitySpawns(level);
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
        foreach (EnemySpawn spawn in level.spawnList)
        {
            Gizmos.color = new Color(1, 0, 0, 0.1f);
            Vector3 center = new Vector3(spawn.X * map.CellScale, 1f, spawn.Z * map.CellScale);
            Gizmos.DrawWireMesh(spawn.Enemy.Entity.Instance.GetComponent<MeshFilter>().sharedMesh, center);
        }
    }
}
