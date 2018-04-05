﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityClasses;

namespace MapClasses
{
    // A map class that mainly serves as a container for the base level fields, properties, and methods.
    // Contains a list of Levels, the current level index, and methods for setting the current level and moving to new levels.
    [System.Serializable]
    public class Map
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
        // TODO: Probably just cleaner to keep a reference to the actual level object,
        //       since I doubt we'll be using the index for anything.
        public Level CurrentLevel { get; private set; }

        // This is a list of all levels. Hub area, biodome area, biodome boss, mechanical bay, mechanical bay boss, final boss.
        [SerializeField] public List<Level> levels;

        // I don't remember why I had this here,
        //      probably just wanted to keep a list of all entities in the level for later use.
        public List<IEntity> Entities { get; private set; }

        // Would work just as well anywhere else.
        public Map()
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

        // Returns the Vector3 position for the entity, using the entity's cell's X and Z values and multiplying them by the CellScale.
        // Returns Vector3.zero if the entity or cell are null.
        // TODO: Remove both GetEntityPosition methods and just use GetCellPosition,
        //      with the assumption that the entity's pivot point is at their bottom boundary.
        public Vector3 GetEntityPosition(IEntity entity)
        {
            if (entity == null)
            {
                Debug.LogError("ERROR: Cannot get entity's position, entity is null.");
                return Vector3.zero;
            }

            Cell cell = entity.Cell;

            if (cell == null)
            {
                Debug.LogError("ERROR: Cannot get entity's position, cell is null.");
                return Vector3.zero;
            }
            float cellScale = CellScale;
            return new Vector3(cell.X * cellScale, 0.5f, cell.Z * cellScale);
        }

        // Returns the Vector3 position for an entity, using a cell's X and Z values and multiplying them by the CellScale.
        // Returns Vector3.zero if the cell is null.
        public Vector3 GetEntityPosition(Cell cell)
        {
            if (cell == null)
            {
                Debug.LogError("ERROR: Cannot get entity's position, cell is null.");
                return Vector3.zero;
            }
            float cellScale = CellScale;
            return new Vector3(cell.X * cellScale, 0.5f, cell.Z * cellScale);
        }
    }
}
