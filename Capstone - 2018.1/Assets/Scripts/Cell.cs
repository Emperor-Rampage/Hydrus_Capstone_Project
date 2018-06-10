using System.Collections;
using System.Collections.Generic;
using MapClasses;
using UnityEngine;
using EntityClasses;
using AbilityClasses;
using AStar;

namespace MapClasses
{
    public enum CellType
    {
        Empty = 0,
        Static = 1,
        Procedural = 2,
        Connection = 3,
        Door = 4,
        Spawn = 5,
        Exit = 6
    }


    public class Cell : INode
    {
        public static Color EmptyColor = new Color(0, 0, 0, 0);
        public static Color StaticColor = new Color(0, 1, 0, 1);
        public static Color ProceduralColor = new Color(0, 0, 1, 1);
        public static Color ConnectionColor = new Color(1, 0, 0, 1);
        public static Color DoorColor = new Color(1, 1, 0, 1);
        public static Color SpawnColor = new Color(1, 1, 1, 1);
        public static Color ExitColor = new Color(0, 0, 0, 1);

        public int Index { get; private set; }
        public int X { get; private set; }
        public int Z { get; private set; }
        public NodeInfo NodeInfo { get; set; } = new NodeInfo();
        public CellType Type { get; set; }
        public bool Locked { get; set; }
        public Entity Occupant { get; set; }

        public Item Item { get; set; }

        public List<Indicator> Indicators { get; set; } = new List<Indicator>();


        /*
        ---------------------------------------------------------------------
            Constructors
        */

        #region Constructors

        public Cell() { }
        public Cell(int index, int x, int z)
        {
            Index = index;
            X = x;
            Z = z;
        }

        #endregion

        /*
        ---------------------------------------------------------------------
            Methods
        */
        #region Methods

        public int GetIndex()
        {
            return ((Z * GameManager.Instance.LevelManager.MaxWidth) + X);
        }
        public bool AreNeighbors(Cell cell2)
        {
            // If they are on the same column,
            if (X == cell2.X)
            {
                // If cell2 is directly above or below cell1.
                if (Extensions.IntegerDifference(Z, cell2.Z) == 1)
                    return true;
                else
                    return false;
            }

            // If they are on the same row,
            if (Z == cell2.Z)
            {
                // If cell2 is directly to the right or left of cell1.
                if (Extensions.IntegerDifference(X, cell2.X) == 1)
                    return true;
                else
                    return false;
            }
            return false;
        }
        /// <summary>
        /// Gets the index of a neighboring cell in a passed in direction.
        /// Direction: 0 = up, 1 = right, 2 = down, 3 = left.
        /// Returns -1 on failure.
        /// </summary>
        /// <returns>int</returns>
        public int GetNeighborIndex(Direction direction)
        {
            // If the direction passed in is invalid, return false.
            if (direction == Direction.Null)
                return -1;

            int destX = X;
            int destZ = Z;

            // Up, right, down, left.
            if (direction == Direction.Up)
                destZ += 1;
            else if (direction == Direction.Right)
                destX += 1;
            else if (direction == Direction.Down)
                destZ -= 1;
            else if (direction == Direction.Left)
                destX -= 1;

            int destIndex = Cell.GetIndex(destX, destZ);

            return destIndex;
        }

        /// <summary>
        /// Gets the index of a neighboring cell in a passed in direction.
        /// Direction: 0 = up, 1 = right, 2 = down, 3 = left.
        /// Returns -1 on failure.
        /// </summary>
        /// <returns>int</returns>
        public int GetNeighborIndex(int direction)
        {

            // If the direction passed in is invalid, return false.
            if (direction < 0 || direction > 3)
                return -1;

            if (direction == (int)Direction.Null)
                return -1;

            int destX = X;
            int destZ = Z;

            // Up, right, down, left.
            if (direction == (int)Direction.Up)
                destZ += 1;
            else if (direction == (int)Direction.Right)
                destX += 1;
            else if (direction == (int)Direction.Down)
                destZ -= 1;
            else if (direction == (int)Direction.Left)
                destX -= 1;

            int destIndex = Cell.GetIndex(destX, destZ);

            return destIndex;
        }

        public int[] GetNeighborIndexes()
        {
            int up = Cell.GetIndex(X, Z + 1);
            int right = Cell.GetIndex(X + 1, Z);
            int down = Cell.GetIndex(X, Z - 1);
            int left = Cell.GetIndex(X - 1, Z);

            return new int[] { up, right, down, left };
        }

        public int GetDistance(int x2, int z2)
        {
            return Mathf.Abs(X - x2) + Mathf.Abs(Z - z2);
        }

        public int GetDistance(Cell cell2)
        {
            return Mathf.Abs(X - cell2.X) + Mathf.Abs(Z - cell2.Z);
        }

        public Direction GetNeighborDirection(Cell cell2)
        {
            if (cell2 == null)
            {
                // Debug.LogError("ERROR: Passed null cell2 into Cell.GetNeighborDirection.");
                return Direction.Null;
            }
            int x2 = cell2.X;
            int z2 = cell2.Z;

            if (X == x2)
            {
                // If it's up.
                // Or if it's down.
                if (z2 - Z == 1)
                    return Direction.Up;
                else if (z2 - Z == -1)
                    return Direction.Down;
            }
            else if (Z == z2)
            {
                // If it's right.
                // Or if it's left.
                if (x2 - X == 1)
                    return Direction.Right;
                else if (x2 - X == -1)
                    return Direction.Left;
            }

            return Direction.Null;
        }

        #endregion

        /*
        --------------------------------------------------------------------------------------------
            Static Methods
        */
        #region Static Methods

        public static int GetIndex(int x, int z, int maxWidth, int maxDepth)
        {
            if (x >= maxWidth || z >= maxDepth)
            {
                return -1;
            }
            return ((z * maxWidth) + x);
        }

        public static int GetIndex(int x, int z)
        {
            LevelManager levelManager = GameManager.Instance.LevelManager;
            if (x >= levelManager.MaxWidth || z >= levelManager.MaxDepth)
            {
                return -1;
            }
            return ((z * GameManager.Instance.LevelManager.MaxWidth) + x);
        }
        public static int GetX(int index, int maxWidth)
        {
            return index % maxWidth;
        }
        public static int GetX(int index)
        {
            return index % GameManager.Instance.LevelManager.MaxWidth;
        }
        public static int GetZ(int index, int maxWidth)
        {
            return (int)(index / maxWidth);
        }

        public static int GetZ(int index)
        {
            return (int)(index / GameManager.Instance.LevelManager.MaxWidth);
        }

        /// <summary>
        /// Gets the index of a neighboring cell in a passed in direction.
        /// Direction: 0 = up, 1 = right, 2 = down, 3 = left.
        /// </summary>
        /// <returns>Boolean</returns>
        public static int GetNeighborIndex(int index, Direction direction)
        {
            if (direction == Direction.Null)
                return -1;

            int destX = Cell.GetX(index);
            int destZ = Cell.GetZ(index);

            // Up, right, down, left.
            if (direction == Direction.Up)
                destZ += 1;
            else if (direction == Direction.Right)
                destX += 1;
            else if (direction == Direction.Down)
                destZ -= 1;
            else if (direction == Direction.Left)
                destX -= 1;

            int destIndex = Cell.GetIndex(destX, destZ);

            return destIndex;
        }
        public static int[] GetNeighborIndexes(int x, int z)
        {
            int up = Cell.GetIndex(x, z + 1);
            int right = Cell.GetIndex(x + 1, z);
            int down = Cell.GetIndex(x, z - 1);
            int left = Cell.GetIndex(x - 1, z);

            return new int[] { up, right, down, left };
        }
        public static bool AreNeighbors(int x1, int z1, int x2, int z2)
        {
            // If they are on the same column,
            if (x1 == x2)
            {
                // If cell2 is directly above or below cell1.
                if (Extensions.IntegerDifference(z1, z2) == 1)
                    return true;
                else
                    return false;
            }

            // If they are on the same row,
            if (z1 == z2)
            {
                // If cell2 is directly to the right or left of cell1.
                if (Extensions.IntegerDifference(x1, x2) == 1)
                    return true;
                else
                    return false;
            }

            return false;
        }

        public static Direction GetNeighborDirection(int index1, int index2)
        {
            int x1 = Cell.GetX(index1);
            int z1 = Cell.GetZ(index1);

            int x2 = Cell.GetX(index2);
            int z2 = Cell.GetZ(index2);

            if (x1 == x2)
            {
                // If it's up.
                // Or if it's down.
                if (z2 - z1 == 1)
                    return Direction.Up;
                else if (z2 - z1 == -1)
                    return Direction.Down;
            }
            else if (z1 == z2)
            {
                // If it's right.
                // Or if it's left.
                if (x2 - x1 == 1)
                    return Direction.Right;
                else if (x2 - x1 == -1)
                    return Direction.Left;
            }

            return Direction.Null;
        }
        public static Direction GetNeighborDirection(Cell cell1, Cell cell2)
        {
            int x1 = cell1.X;
            int z1 = cell1.Z;
            int x2 = cell2.X;
            int z2 = cell2.Z;

            if (x1 == x2)
            {
                // If it's up.
                // Or if it's down.
                if (z2 - z1 == 1)
                    return Direction.Up;
                else if (z2 - z1 == -1)
                    return Direction.Down;
            }
            else if (z1 == z2)
            {
                // If it's right.
                // Or if it's left.
                if (x2 - x1 == 1)
                    return Direction.Right;
                else if (x2 - x1 == -1)
                    return Direction.Left;
            }

            return Direction.Null;
        }

        public static int GetDistance(int x1, int z1, int x2, int z2)
        {
            return Mathf.Abs(x1 - x2) + Mathf.Abs(z1 - z2);
        }
        public static int GetDistance(Cell cell1, Cell cell2)
        {
            return Mathf.Abs(cell1.X - cell2.X) + Mathf.Abs(cell1.Z - cell2.Z);
        }

        public static CellType DetermineCellType(Color pixelColor)
        {
            if (pixelColor == SpawnColor)
                // If white, is the player spawn.
                return CellType.Spawn;
            else if (pixelColor == ExitColor)
                // If black, is the level's exit.
                return CellType.Exit;
            else if (pixelColor == StaticColor)
                // If green, is a static room cell.
                return CellType.Static;
            else if (pixelColor == ProceduralColor)
                // If blue, is a procedural hallway cell.
                return CellType.Procedural;
            else if (pixelColor == ConnectionColor)
                // If red, is an area connection.
                return CellType.Connection;
            else if (pixelColor == DoorColor)
                // If yellow, is a locked door.
                return CellType.Door;
            else if (pixelColor == EmptyColor)
                // If clear, is empty.
                return CellType.Empty;

            // If not recognized, is empty.
            return CellType.Empty;
        }

        public static Color DetermineColor(CellType cellType)
        {
            if (cellType == CellType.Spawn)
                return SpawnColor;
            else if (cellType == CellType.Exit)
                return ExitColor;
            else if (cellType == CellType.Door)
                return DoorColor;
            else if (cellType == CellType.Connection)
                return ConnectionColor;
            else if (cellType == CellType.Procedural)
                return ProceduralColor;
            else if (cellType == CellType.Static)
                return StaticColor;
            else if (cellType == CellType.Empty)
                return EmptyColor;
            else
                return EmptyColor;
        }

        #endregion
    }
}