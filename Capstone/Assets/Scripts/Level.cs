using System.Collections;
using System.Collections.Generic;
using Pixelplacement;
using UnityEngine;
using EntityClasses;
using UnityEngine.SceneManagement;

using AudioClasses;
using AbilityClasses;

namespace MapClasses
{
    // Enum that indicates a direction.
    public enum Direction
    {
        Null = -1,
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    }

    // A level class, which contains all information and methods for a specific level.
    // Handles generation of levels, connections, and procedural generation.
    // 
    // TODO: Implement system to instantiate entities.
    //      (Probably a list of EntitySpawn objects, which contain X and Z values and references to ScriptableObject Enemies,
    //      which contain Entity objects.)
    //      Maybe a seperate EntitySpawnManager, called from the GameManager.

    // FIXME: Clean up the validation. Right now every other method independently validates all cells and indices each time they're called.
    //          This results in some checks being performed multiple times down the call stack.
    //          Should remove as much of this as possible, even if some of it is technically unsafe code.
    [System.Serializable]
    public class Level
    {
        // The index for the scene to be loaded for this level.
        [SerializeField] public int sceneIndex;

        // The name of the level.
        [SerializeField] public string name;

        // The bitmap for the level.
        [SerializeField] public Texture2D levelMap;
        [SerializeField] public BackgroundMusic music;

        // A list of indexes for the levels to be loaded from each exit.
        [SerializeField] public List<int> exitList;
        [SerializeField] public List<EnemySpawn> spawnList;
        // A reference to the player entity, because we'll be using it a lot.
        public Player Player { get; private set; }
        public List<Enemy> EnemyList { get; private set; }

        // A list of all cells in the level, because we might need it. But probably not.
        public List<Cell> cells;
        // The player's spawn location.
        public Cell PlayerSpawnCell { get; set; }

        // All cells of with cell.Type == CellType.Exit. These correspond to the exitList of integers.
        //          ExitCell[0] goes to exitList[0], which contains an index pointing to the next level.
        public List<Cell> ExitCells { get; set; }

        // Indicates whether the level object has been initialized yet.
        public bool Initialized { get; private set; }

        // Inidicates whether the player can execute the exit action.
        // This is true if the player is standing on an exit cell. Checks are made whenever an entity has moved.
        public bool CanExit { get; private set; }

        // A 2d matrix that indicates whether or not there is a connection between a given cell and a given direction.
        // connectionMatrix size is [number of cells, number of directions], so number of cells * 4 (four directions).
        // If connectionMatrix[cell.Index, (int)direction] == true,
        //      then there is a connection between the cell and the neighbor in that direction.
        public bool[,] connectionMatrix;

        // The number of indices.
        int numIndexes;

        // A list of connection cells. These cells indicate a connection between a procedurally generated area and a static area.
        List<Cell> connectionCells;

        // Initializes the level,
        // Gets the data from the levelMap bitmap, setting up the MaxWidth, MaxDepth, and numIndexes.
        // Instantiates the list of cells, and creates the connection matrix.
        // Then calls each of the initialization methods: Cell creation, static connection creation, and then procedural generation.
        public void InitializeLevel(Player player = null)
        {
            if (levelMap == null)
                return;

            Debug.Log("Level initialization started..");

            GameManager.Instance.Map.MaxWidth = levelMap.width;
            GameManager.Instance.Map.MaxDepth = levelMap.height;
            numIndexes = GameManager.Instance.Map.MaxWidth * GameManager.Instance.Map.MaxDepth;
            cells = new List<Cell>();
            connectionMatrix = new bool[numIndexes, 4];
            // Get the pixels from the texturemap.
            // Determine from pixels which cells exist, and which are connections.
            InitializeCells();
            Initialized = true;
            InitializeConnections();
            InitializeProcedural();
            InitializeEntities(player);

            Debug.Log("Level initialization finished.");
        }

        // Creates the list of ExitCells and connectionCells, then gets the data from the levelMap bitmap.
        // Iterates through each pixel, getting its type and
        //      creating a new cell with the index, X, and Z all being assigned via the pixel's index.
        // Then adds it to the necessary lists.
        // Assigns to connectionCells if type Connection, ExitCells if type Exit, and PlayerSpawnCell if type Spawn.
        void InitializeCells()
        {
            ExitCells = new List<Cell>();
            connectionCells = new List<Cell>();
            Color[] levelPixels = levelMap.GetPixels();

            // For each pixel in the texturemap.
            for (int p = 0; p < numIndexes; p++)
            {
                var type = Cell.DetermineCellType(levelPixels[p]);

                Cell cell = new Cell(p, Cell.GetX(p), Cell.GetZ(p));

                cell.Type = type;

                cells.Add(cell);
                if (cell.Type == CellType.Connection)
                {
                    connectionCells.Add(cell);
                }
                else if (cell.Type == CellType.Spawn)
                {
                    PlayerSpawnCell = cell;
                }
                else if (cell.Type == CellType.Exit)
                {
                    ExitCells.Add(cell);
                }
            }
        }

        // Creates all connections between static types.
        // Iterates through each cell in the cells List, checking if it is not of type Procedural or Empty,
        //      and creates a connection with all neighbors that are also not of type Procedural or Empty.
        void InitializeConnections()
        {
            foreach (Cell cell in cells)
            {
                // If it's one of the static types, and not empty, create connections with all other static non-empty types.
                if (cell.Type != CellType.Procedural && cell.Type != CellType.Empty)
                {
                    int[] cellNeighborIndexes = Cell.GetNeighborIndexes(cell.X, cell.Z);

                    for (int i = 0; i < cellNeighborIndexes.Length; i++)
                    {
                        int nCellIndex = cellNeighborIndexes[i];
                        if (!IsValidCell(nCellIndex))
                            continue;

                        var nCell = cells[nCellIndex];

                        if (nCell.Type != CellType.Procedural && nCell.Type != CellType.Empty)
                        {
                            CreateConnection(cell.Index, (Direction)i);
                        }
                    }
                }
            }
        }

        void InitializeProcedural()
        {
            // Iterate through all area connections.
            // For each connection
            //   Traverse through adjacent procedural cells until:
            //     Another area connection is found
            //     An unvisited procedural cell with connections is found - Meaning, has been visited by a previous traversal.

            // This will ensure that all connections to one procedural area connect to every other connection to the same procedural area.

            // Traversal:
            //   Check possible directions, must be:
            //     Procedural
            //     and
            //     Not visited
            //
            //   If connection, will choose.
            //   Otherwise, choose random from possible directions.
            //   Repeat until complete.


            foreach (Cell connection in connectionCells)
            {
                TraverseProcedural(connection);
            }
        }

        // Creates a list to keep track of visited procedural cells, sets the current cell to the origin, then begins the loop.
        //      Adds the cell to the list of visited, then get all neighbors and either goes to an end case or chooses a random direction.
        //      Then sets the cell variable to the new cell.
        // After the loop has completed, iterates through all of the visited cells again and creates connections between any neighboring ones.
        // This generates rooms. Remove this last step to generate hallways.
        void TraverseProcedural(Cell origin)
        {
            Debug.Log("Traversing..");
            List<Cell> visitedProcedural = new List<Cell>();

            Cell cell = origin;
            int iter = 0;
            while (true)
            {
                iter++;
                int cellIndex = cell.Index;
                visitedProcedural.Add(cell);
                //                Debug.Log(".. " + cellIndex + " is type " + cell.Type + " .. Visited count is " + visitedProcedural.Count);

                int[] nIndexes = cell.GetNeighborIndexes();

                List<int> possIndexes = new List<int>();
                bool definiteFound = false;
                int definiteIndex = -1;

                // Iterate through all neighbors and get choices.
                // If an end is found, set it to the definite choice.
                // Otherwise, if a valid choice, add it to the list.
                for (int n = 0; n < nIndexes.Length; n++)
                {
                    int nIndex = nIndexes[n];
                    if (!IsValidCell(nIndex))
                        continue;

                    var nCell = cells[nIndex];

                    // If the neighbor is a procedural with existing connections that has not been visited in this traversal, make the connection and end the traversal.
                    // Else
                    // If the neighbor is a connection that has no been visited in this traversal, make the connection and end the traversal.
                    // Else
                    // If the neighbor is procedural with no existing connections that has not been visited in this traversal, make the connection and set it to the new cell.
                    bool neighborHasConnections = HasConnections(nIndex);
                    bool hasBeenVisited = visitedProcedural.Contains(nCell);
                    if (nCell.Type == CellType.Procedural && neighborHasConnections && !hasBeenVisited && !definiteFound)
                    {
                        Debug.Log("Unvisited procedural found with existing connections.");
                        definiteFound = true;
                        definiteIndex = nIndex;
                    }
                    else if (nCell.Type == CellType.Connection && !hasBeenVisited && !definiteFound)
                    {
                        Debug.Log("Unvisited connection found.");
                        definiteFound = true;
                        definiteIndex = nIndex;
                    }
                    else if (nCell.Type == CellType.Procedural && !neighborHasConnections && !hasBeenVisited)
                    {
                        possIndexes.Add(nIndex);
                    }
                }

                // If definiteFound, make a connecton with the definite and break.
                // If not, iterate through all possible choices and select one at random to create a connection with. Set that cell as the new cell.
                // If there are no possible choices, set the cell to a random one from the list of visited cells.

                if (definiteFound && definiteIndex != -1)
                {
                    Direction direction = Cell.GetNeighborDirection(cellIndex, definiteIndex);

                    CreateConnection(cellIndex, direction);
                    break;
                }

                if (possIndexes.Count > 0)
                {
                    int selectionIndex = Random.Range(0, possIndexes.Count);
                    int selection = possIndexes[selectionIndex];

                    Direction direction = Cell.GetNeighborDirection(cellIndex, selection);
                    CreateConnection(cellIndex, direction);

                    cell = cells[selection];
                }
                else
                {
                    int selectionIndex = Random.Range(0, visitedProcedural.Count);

                    Cell selection = visitedProcedural[selectionIndex];

                    cell = selection;
                }

                if (cell.Index == origin.Index && possIndexes.Count < 0)
                {
                    Debug.LogError("Error: Connection has no possible neighbors.");
                    break;
                }

                // Just to avoid an infinite loop if a bug is introduced.
                if (iter >= 1000)
                {
                    Debug.LogError("Error: Reached max iterations.");
                    break;
                }
            }

            // Note: Comment out this section to only generate hallways instead of rooms.
            // TODO: Determine whether or not to use hallways or rooms.
            //       Alternatively, create separate methods and pixel designation for hallway and room generation areas.
            foreach (Cell visitedCell in visitedProcedural)
            {
                int[] cellNeighborIndexes = Cell.GetNeighborIndexes(visitedCell.X, visitedCell.Z);

                for (int i = 0; i < cellNeighborIndexes.Length; i++)
                {
                    int nCellIndex = cellNeighborIndexes[i];
                    if (!IsValidCell(nCellIndex))
                        continue;

                    var nCell = cells[nCellIndex];

                    if (nCell.Type == CellType.Procedural && visitedProcedural.Contains(nCell))
                    {
                        CreateConnection(visitedCell.Index, (Direction)i);
                    }
                }
            }
        }

        void InitializeEntities(Player player)
        {
            // First initialize the player entity.
            // Then iterate through all enemy spawns and initialize each of them

            EnemyList = new List<Enemy>();

            Player = player;
            // Create the player.
            if (player == null)
            {
                Player = new Player { Index = -1, Name = "Player", Facing = Direction.Up, State = EntityState.Idle, MaxHealth = 100, CurrentHealth = 100 };
            }
            Player.Facing = Direction.Up;
            Player.State = EntityState.Idle;
            //Player.InitializeCooldownsList();
            // Set the player's location to the player spawn.
            SetEntityLocation(Player, PlayerSpawnCell);

            int enemyIndex = 0;
            foreach (var spawn in spawnList)
            {
                Enemy enemy = new Enemy(spawn.EnemyObject.Enemy);
                //enemy.InitializeCooldownsList();
                enemy.Index = enemyIndex++;
                enemy.Target = Direction.Null;
                enemy.InCombat = false;
                SetEntityLocation(enemy, spawn.X, spawn.Z);
                EnemyList.Add(enemy);
            }
        }

        public IEnumerator RemoveEntity(Entity entity)
        {
            Debug.Log("Removing entity..");
            if (entity != null)
            {
                Debug.Log("-- Entity is not null.");
                while (entity.State != EntityState.Idle)
                {
                    Debug.Log("-- Entity is not idle, waiting a frame. Instead is " + entity.State);
                    yield return null;
                }

                Debug.Log("-- Entity is idle. Destroying everything.");
                // If the entity is present in the enemy list, it will be removed.
                GameObject.Destroy(entity.Instance);
                if (entity.GetType() == typeof(Enemy))
                {
                    EnemyList.Remove((Enemy)entity);
                }

                if (entity.Cell != null)
                {
                    entity.Cell.Occupant = null;
                }
            }
        }

        public List<Cell> GetAffectedCells_Highlight(Entity entity, AbilityObject ability)
        {
            List<Cell> highlight = new List<Cell>();
            Cell cell = entity.Cell;
            if (cell == null)
                return highlight;

            if (ability.Type == AbilityType.None)
            {
                // Do nothing.
            }
            else if (ability.Type == AbilityType.Melee)
            {
                // Return the cell in front of the entity.
                Cell neighborCell = GetNeighbor(entity.Cell, entity.Facing);
                if (neighborCell != null)
                {
                    highlight.Add(neighborCell);
                }
            }
            else if (ability.Type == AbilityType.Ranged)
            {
                // Return all cells in a line in the direction the entity is facing, starting with the cell in front of the entity.
                Cell current = cell;
                for (int r = 0; r < ability.Range; r++)
                {
                    Cell next = GetNeighbor(current, entity.Facing);
                    if (next != null)
                    {
                        highlight.Add(next);
                    }
                    current = next;
                }
            }
            else if (ability.Type == AbilityType.AreaOfEffect || ability.Type == AbilityType.Zone)
            {
                // Get all pixels, return relative cells.
                Texture2D sprite = ability.AOESprite;
                if (sprite == null)
                    return highlight;

                int width = sprite.width;
                int height = sprite.height;
                int entityX = 0;
                int entityY = 0;
                Color[] aoePixels = sprite.GetPixels();
                for (int i = 0; i < aoePixels.Length; i++)
                {
                    if (aoePixels[i] == Color.white)
                    {
                        entityX = GetSpriteX(i, width);
                        entityY = GetSpriteY(i, width);
                    }
                }

                for (int p = 0; p < aoePixels.Length; p++)
                {
                    if (aoePixels[p] == Color.black)
                    {
                        int pixelX = GetSpriteX(p, width);
                        int pixelY = GetSpriteY(p, width);

                        int offsetX = (pixelX - entityX);
                        int offsetZ = (pixelY - entityY);

                        Vector2 relativeOffset = new Vector2(offsetX, offsetZ).Rotate(entity.GetDirectionDegrees());
                        Vector2Int relativeIntOffset = new Vector2Int(Mathf.RoundToInt(relativeOffset.x), Mathf.RoundToInt(relativeOffset.y));

                        int index = Cell.GetIndex(cell.X + (int)relativeIntOffset.x, cell.Z + (int)relativeIntOffset.y);
                        if (IsValidCell(index))
                        {
                            Cell target = cells[index];
                            if (target != null)
                            {
                                highlight.Add(target);
                            }
                        }
                    }
                }
            }
            else if (ability.Type == AbilityType.Self)
            {
                // Return the entity's cell.
                highlight.Add(entity.Cell);
            }

            return highlight;
        }

        public List<Cell> GetAffectedCells(Entity entity, AbilityObject ability)
        {
            List<Cell> affected = new List<Cell>();
            Cell cell = entity.Cell;
            if (cell == null)
                return affected;

            if (ability.Type == AbilityType.None)
            {
                // Do nothing.
            }
            else if (ability.Type == AbilityType.Melee)
            {
                // Return the cell in front of the entity.
                Cell neighborCell = GetNeighbor(entity.Cell, entity.Facing);
                if (neighborCell != null)
                {
                    affected.Add(GetNeighbor(entity.Cell, entity.Facing));
                }
            }
            else if (ability.Type == AbilityType.Ranged)
            {
                // Return all cells in a line in the direction the entity is facing, starting with the cell in front of the entity.
                Cell current = cell;
                for (int r = 0; r < ability.Range; r++)
                {
                    Cell next = GetNeighbor(current, entity.Facing);
                    if (next != null && next.Occupant != null)
                    {
                        affected.Add(next);
                        break;
                    }
                    current = next;
                }
            }
            else if (ability.Type == AbilityType.AreaOfEffect || ability.Type == AbilityType.Zone)
            {
                // Get all pixels, return relative cells.
                Texture2D sprite = ability.AOESprite;
                if (sprite == null)
                    return affected;

                int width = sprite.width;
                int height = sprite.height;
                int entityX = 0;
                int entityY = 0;
                Color[] aoePixels = sprite.GetPixels();
                for (int i = 0; i < aoePixels.Length; i++)
                {
                    if (aoePixels[i] == Color.white)
                    {
                        entityX = GetSpriteX(i, width);
                        entityY = GetSpriteY(i, width);
                    }
                }

                for (int p = 0; p < aoePixels.Length; p++)
                {
                    if (aoePixels[p] == Color.black)
                    {
                        int pixelX = GetSpriteX(p, width);
                        int pixelY = GetSpriteY(p, width);

                        int offsetX = (pixelX - entityX);
                        int offsetZ = (pixelY - entityY);

                        Vector2 relativeOffset = new Vector2(offsetX, offsetZ).Rotate(entity.GetDirectionDegrees());
                        Vector2Int relativeIntOffset = new Vector2Int(Mathf.RoundToInt(relativeOffset.x), Mathf.RoundToInt(relativeOffset.y));

                        int index = Cell.GetIndex(cell.X + (int)relativeIntOffset.x, cell.Z + (int)relativeIntOffset.y);
                        if (IsValidCell(index))
                        {
                            Cell target = cells[index];
                            if (target != null)
                            {
                                affected.Add(target);
                            }
                        }
                    }
                }
            }
            else if (ability.Type == AbilityType.Self)
            {
                // Return the entity's cell.
                affected.Add(entity.Cell);
            }

            return affected;
        }


        // Returns the destination cell for an entity that intends to move in Direction direction.
        // Performs a lot of validation, making sure everything is !null, !Locked, and the destination does not have an entity already.
        public Cell GetDestination(Entity entity, Direction direction)
        {
            if (!Initialized)
                return null;

            if (!HasConnection(entity.Cell, direction))
                return null;

            Cell eCell = entity.Cell;
            if (eCell == null)
                return null;

            Cell neighbor = GetNeighbor(entity.Cell, direction);

            if (neighbor == null)
                return null;

            if (neighbor.Locked || neighbor.Occupant != null)
                return null;

            return neighbor;
            //            return SetEntityLocation(entity, nCell);
        }

        // Not using anymore.
        // Moved logic to the GameManager to avoid references to the GameManager from the level object.
        public IEnumerator MoveEntityLocation_Coroutine(Entity entity, Cell cell, float delay)
        {
            cell.Locked = true;
            yield return new WaitForSeconds(delay);
            SetEntityLocation(entity, cell);
            cell.Locked = false;
        }
        // Also not using anymore.
        void FinishMoving(Entity entity, GameManager manager)
        {
            entity.State = EntityState.Idle;
        }

        // Moves an Entity entity to Cell cell.
        // Validates everything, making sure the entity can be moved there, then moved the entity.
        // If the entity is the player's entity, sets CanExit to true or false, depending on if the cell is of type Exit.
        // Returns true on success and false on failure.
        public bool SetEntityLocation(Entity entity, Cell cell)
        {
            if (!Initialized)
                return false;

            if (cell == null)
                return false;

            if (cell.Occupant != null)
                return false;

            cell.Occupant = entity;

            if (entity != null)
            {
                if (entity.Cell != null)
                    entity.Cell.Occupant = null;

                if (entity.GetType() == typeof(Player) && IsExit(cell))
                {
                    CanExit = true;
                }
                else if (entity.GetType() == typeof(Player) && !IsExit(cell))
                {
                    CanExit = false;
                }

                if (entity.GetType() == typeof(Enemy))
                {
                    //                    Debug.Log("Entity is an enemy, setting target to null.");
                    ((Enemy)entity).Target = Direction.Null;
                }

                entity.Cell = cell;
            }

            return true;
        }

        // Overload methods we'll probably never use, that just call the first implementation with slightly more validation.
        public bool SetEntityLocation(Entity entity, int index)
        {
            if (!Initialized)
                return false;

            if (!IsValidCell(index))
                return false;

            Cell cell = cells[index];

            return SetEntityLocation(entity, cell);
        }

        // Overload methods we'll probably never use, that just call the first implementation with slightly more validation.
        public bool SetEntityLocation(Entity entity, int X, int Z)
        {
            if (!Initialized)
                return false;

            int index = Cell.GetIndex(X, Z);

            return SetEntityLocation(entity, index);
        }

        /// <summary>
        /// Attempts to create a connection between a cell and the cell in the passed in direction.
        /// Direction: 0 = up, 1 = right, 2 = down, 3 = left.
        /// Returns false on failure.
        /// </summary>
        /// <returns>Boolean</returns>
        public bool CreateConnection(int cellIndex, Direction direction)
        {
            if (!Initialized)
                return false;

            // Get both indexes.
            int destIndex = Cell.GetNeighborIndex(cellIndex, direction);

            // If either index is invalid, return false.
            if (!IsValidCell(cellIndex) || !IsValidCell(destIndex))
                return false;

            // The direction from the second cell to the first.
            Direction inverseDirection = direction;

            if (direction == Direction.Up)
                inverseDirection = Direction.Down;
            else if (direction == Direction.Right)
                inverseDirection = Direction.Left;
            else if (direction == Direction.Down)
                inverseDirection = Direction.Up;
            else if (direction == Direction.Left)
                inverseDirection = Direction.Right;

            // Create connections for each cell to the other.
            connectionMatrix[cellIndex, (int)direction] = true;
            connectionMatrix[destIndex, (int)inverseDirection] = true;

            return true;
        }

        // Attempts to delete all connections between cell1 and cell2.
        // Returns true on successful deletion, returns false on failure.
        public bool DeleteConnection(Cell cell1, Cell cell2)
        {
            if (!Initialized)
                return false;

            // If either is null, return false.
            if (cell1 == null || cell2 == null)
                return false;

            int index1 = cell1.Index;
            int index2 = cell2.Index;
            // If either is outside the index range, return false.
            if (!IsValidCell(index1) || !IsValidCell(index2))
                return false;

            // Break the connection and return true.
            connectionMatrix[index1, index2] = false;
            connectionMatrix[index2, index1] = false;
            return true;
        }

        // Returns true if the cell has a connection in Direction direction.
        public bool HasConnection(Cell cell, Direction direction)
        {
            if (!Initialized)
                return false;

            if (direction == Direction.Null)
                return false;

            if (connectionMatrix[cell.Index, (int)direction])
                return true;

            return false;
        }

        public bool HasConnection(Cell cell1, Cell cell2)
        {
            if (!Initialized)
                return false;

            Direction neighborDirection = Cell.GetNeighborDirection(cell1, cell2);
            if (neighborDirection == Direction.Null)
                return false;

            if (connectionMatrix[cell1.Index, (int)neighborDirection])
                return true;

            return false;
        }

        // Returns true if the cell with the given index has any connections.
        public bool HasConnections(int index)
        {
            if (!Initialized)
                return false;

            if (!IsValidCell(index))
                return false;

            for (int d = 0; d < 4; d++)
            {
                if (connectionMatrix[index, d] == true)
                    return true;
            }

            return false;
        }

        // Returns true if the cell has any connections.
        public bool HasConnections(Cell cell)
        {
            if (!Initialized)
                return false;

            if (cell == null)
                return false;

            int index = cell.Index;
            return HasConnections(index);
        }

        public List<Cell> GetNeighbors(Cell cell)
        {
            if (!Initialized)
                return null;

            if (cell == null)
                return null;

            if (!HasConnections(cell))
                return null;

            List<Cell> neighbors = new List<Cell>();
            int[] neighborIndexes = cell.GetNeighborIndexes();

            for (int n = 0; n < neighborIndexes.Length; n++)
            {
                if (IsValidCell(neighborIndexes[n]))
                    neighbors.Add(cells[neighborIndexes[n]]);
            }

            return neighbors;
        }

        // Gets the neighboring cell of Cell cell in Direction direction.
        public Cell GetNeighbor(Cell cell, Direction direction)
        {
            if (!Initialized)
                return null;

            if (cell == null)
                return null;

            if (direction == Direction.Null)
                return null;

            if (!HasConnection(cell, direction))
                return null;

            int destX = cell.X;
            int destZ = cell.Z;

            // Up, right, down, left.
            if (direction == Direction.Up)
            {
                destZ += 1;
            }
            else if (direction == Direction.Right)
            {
                destX += 1;
            }
            else if (direction == Direction.Down)
            {
                destZ -= 1;
            }
            else if (direction == Direction.Left)
            {
                destX -= 1;
            }


            int destIndex = Cell.GetIndex(destX, destZ);
            if (!IsValidCell(destIndex))
                return null;

            return cells[destIndex];
        }

        // Returns a random cell that is not null, does not have an occupant, and has connections.
        public Cell GetRandomCell()
        {
            if (!Initialized)
                return null;

            int index;
            Cell cell;

            while (true)
            {
                index = Random.Range(0, cells.Count);
                cell = cells[index];

                if (cell == null)
                    continue;

                if (cell.Occupant != null)
                    continue;

                if (HasConnections(cell))
                    break;
            }
            return cell;
        }

        // Returns true if the cell is of type Exit.
        public bool IsExit(Cell cell)
        {
            if (cell != null && cell.Type == CellType.Exit)
                return true;
            return false;

        }

        // Returns true if the index is within bounds.
        public bool IsValidCell(int index)
        {
            if (!Initialized)
                return false;

            if (index < 0 || index >= numIndexes)
                return false;

            return true;
        }

        public Direction GetInverseDirection(Direction direction)
        {
            int newDir = (int)direction - 2;
            if (newDir < 0)
                newDir += 4;

            return (Direction)newDir;
        }

        public int GetDistance(int x1, int z1, int x2, int z2)
        {
            return Mathf.Abs(x1 - x2) + Mathf.Abs(z1 - z2);
        }

        public int GetDistance(Cell cell1, Cell cell2)
        {
            return Mathf.Abs(cell1.X - cell2.X) + Mathf.Abs(cell1.Z - cell2.Z);
        }

        int GetSpriteX(int index, int width)
        {
            return index % width;
        }

        int GetSpriteY(int index, int width)
        {
            return (int)(index / width);
        }
    }
}
