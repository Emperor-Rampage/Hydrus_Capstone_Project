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
    LevelManager levelManager;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gameObject.transform.position, 0.25f);
        if (manager == null)
            return;

        levelManager = manager.LevelManager;

        var levels = levelManager.levels;

        if (levelToLoad < 0 || levelToLoad >= levels.Count)
        {
            Debug.LogWarning("Level To Load is out of range.");
            return;
        }

        Level level = levels[levelToLoad];
        if (level != null)
        {
            DrawCells(level);
            DrawEntitySpawns(level);
        }
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

        Vector3 size = new Vector3(0.98f, 0f, 0.98f) * levelManager.CellScale;
        for (int p = 0; p < levelPixels.Length; p++)
        {
            Gizmos.color = levelPixels[p];
            Vector3 center = new Vector3(Cell.GetX(p, width) * levelManager.CellScale, 0, Cell.GetZ(p, width) * levelManager.CellScale);

            Gizmos.DrawWireCube(center, size);
        }
    }

    void DrawEntitySpawns(Level level)
    {
        if (level.spawnList.Count > 0 && level.spawnList[0] == null)
        {
            Debug.LogWarning("WARNING: level has no spawns.");
        }
        foreach (EnemySpawn spawn in level.spawnList)
        {
            if (spawn.EnemyObject != null && spawn.EnemyObject.Enemy != null)
            {
                Gizmos.color = new Color(1, 0, 0, 0.1f);
                Vector3 center = new Vector3(spawn.X * levelManager.CellScale, 0f, spawn.Z * levelManager.CellScale);
                SkinnedMeshRenderer rend = spawn.EnemyObject.Enemy.Renderer;
                Gizmos.DrawWireMesh(rend.sharedMesh, center + (rend.transform.localPosition * levelManager.CellScale), Quaternion.identity, spawn.EnemyObject.Enemy.Instance.transform.localScale * levelManager.CellScale);
                // foreach (MeshFilter filter in spawn.EnemyObject.Enemy.Instance.GetComponentsInChildren<MeshFilter>())
                // {
                //     Transform pieceTransform = filter.transform;
                //     if (pieceTransform != null && filter.sharedMesh != null)
                //     {
                //         Gizmos.DrawWireMesh(filter.sharedMesh, center + pieceTransform.localPosition, pieceTransform.localRotation, filter.transform.localScale * levelManager.CellScale);
                //     }
                // }
            }
        }
    }
}
