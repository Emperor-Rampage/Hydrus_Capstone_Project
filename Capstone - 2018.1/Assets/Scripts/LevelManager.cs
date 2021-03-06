﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityClasses;

namespace MapClasses
{
    [System.Serializable]
    public class LevelPrefab
    {
        [SerializeField] GameObject prefab;
        public GameObject Prefab { get { return prefab; } }

        [Range(0f, 1f)]
        [SerializeField]
        float weight;
        public float Weight { get { return weight; } }
    }

    // A map class that mainly serves as a container for the base level fields, properties, and methods.
    // Contains a list of Levels, the current level index, and methods for setting the current level and moving to new levels.
    [System.Serializable]
    public class LevelManager
    {

        // Just an arbitrary float which all cell instances' sizes are multiplied by.
        [SerializeField]
        float cellScale;
        public float CellScale { get { return cellScale; } }

        // The maximum width of the cell grid. This is arbitrary and is only used for defining the indices.
        [SerializeField]
        int maxWidth;
        public int MaxWidth
        {
            get { return maxWidth; }
            set { maxWidth = value; }
        }

        // The maximum depth of the cell grid. This is arbitrary and is only used for defining the indices.
        [SerializeField]
        int maxDepth;
        public int MaxDepth
        {
            get { return maxDepth; }
            set { maxDepth = value; }
        }

        // The index for the current level.
        public Level CurrentLevel { get; private set; }

        // This is a list of all levels. Hub area, biodome area, biodome boss, mechanical bay, mechanical bay boss, final boss.
        [SerializeField] public List<Level> levels;

        // Would work just as well anywhere else.
        public LevelManager()
        {
            CurrentLevel = null;
        }

        public void Reset()
        {
            CurrentLevel = null;
        }

        // Sets the current level index, as long as it's valid, and returns the level.
        public Level SetCurrentLevel(int index)
        {
            if (index < 0 || index >= levels.Count)
                return null;

            CurrentLevel = levels[index];

            return CurrentLevel;
        }

        // Poorly named.
        // Gets the current level, verifies that it's not null and has been initialized,
        // Then gets the index of the Exit cell from the level's list of exits (if it's in the list),
        // Then gets the corresponding next level's index from the level's list of exit destination indices.
        // Then sets the current level and returns the level.
        public Level NextLevel(Cell exit)
        {
            if (CurrentLevel == null || !CurrentLevel.Initialized)
                return null;

            int exitIndex = CurrentLevel.ExitCells.IndexOf(exit);

            if (exitIndex < 0 || exitIndex >= CurrentLevel.ExitCells.Count)
                return null;

            int nextIndex = CurrentLevel.exitList[exitIndex];

            if (nextIndex < 0 || nextIndex >= levels.Count)
                return null;

            return SetCurrentLevel(nextIndex);
        }

        // Returns the Vector3 position for the cell, using the cell's X and Z values and multiplying them by the CellScale.
        // Returns Vector3.zero if the cell is null.
        public Vector3 GetCellPosition(Cell cell)
        {
            if (cell == null)
            {
                Debug.LogError("ERROR: Cannot get cell position, cell is null.");
                return Vector3.zero;
            }
            float cellScale = CellScale;
            return new Vector3(cell.X * cellScale, 0, cell.Z * cellScale);
        }

        public Vector3 GetCellPosition(int x, int z)
        {
            float cellScale = CellScale;
            return new Vector3(x * cellScale, 0f, z * cellScale);
        }

        public GameObject GetRandomPrefab(List<LevelPrefab> prefabList)
        {
            if (prefabList == null || prefabList.Count == 0)
                return null;

            float totalWeight = 0f;

            foreach (LevelPrefab pref in prefabList)
            {
                totalWeight += pref.Weight;
            }

            GameObject prefab = null;
            float choice = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (LevelPrefab pref in prefabList)
            {
                cumulative += pref.Weight;
                if (choice <= cumulative)
                {
                    prefab = pref.Prefab;
                    break;
                }
            }

            // If couldn't find one with the proper weight, just grab the first prefab.
            if (prefab == null)
            {
                prefab = prefabList[0].Prefab;
            }

            return prefab;
        }
    }
}
