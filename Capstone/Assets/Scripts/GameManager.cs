using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

using MapClasses;
using EntityClasses;
using AudioClasses;
using AIClasses;
using AbilityClasses;
using UnityEngine.SceneManagement;
using System.Linq;


// The main game manager. Is a singleton, and contains the general settings as well as references to other systems.
// Contains fields and properties for:
//   Debug section with placeholder variables.
//   Setup section with the instances that need to be set up when the game starts.
//   Game sections with private references to necessary objects and general stats.
//
// Should be the main root object, and generally should be the only thing calling the other systems (UIManager, AIManager, Map, Level, etc.)
[SelectionBase]
public class GameManager : Singleton<GameManager>
{
    [Space(10)]
    [Header("Debug")]
    // The prefab used to fill in the cells during debug build.
    [SerializeField]
    GameObject cellPrefabDebug;
    [SerializeField] GameObject cornerPrefabDebug;

    [SerializeField] GameObject blockPrefab;
    // The prefab used for generic entities during debug build.
    [SerializeField] GameObject entityPrefab;
    // For testing the AudioManager.
    [SerializeField] SoundEffect testSound;
    // For testing ability casting.
    [SerializeField] AbilityObject testAbility;
    [SerializeField] PlayerClass testClass;
    [SerializeField] GameObject testIndicator;
    [SerializeField] CameraShakeObject testShakeEvent;

    [Space(10)]
    [Header("Setup")]
    // The player prefab. Likely will only contain a camera.
    [SerializeField]
    GameObject playerPrefab;
    // A list of GameObjects that need to be instantiated and defined as DontDestroyOnLoad.
    [SerializeField] List<GameObject> doNotDestroyList;

    [Header("Level Pieces")]
    [SerializeField]
    List<MapPrefab> wallPrefabs;
    [SerializeField] List<MapPrefab> floorPrefabs;
    [SerializeField] List<MapPrefab> ceilingPrefabs;
    [SerializeField] List<MapPrefab> cornerPrefabs;

    [Space(10)]
    [Header("Game")]

    [SerializeField]
    List<PlayerClass> classes;
    PlayerClass selectedClass;

    // A reference to the Map object, which handles the general level management.
    [SerializeField]
    Map map;
    // A reference to the current level, because caching is more efficient.
    Level level;

    // A reference to the UIManager instance, which is created at runtime, and handles all user interface actions.
    UIManager uiManager;
    // A reference to the AudioManager instance, which is created at runtime, and handles all audio.
    AudioManager audioManager;
    AIManager aiManager;

    [SerializeField] BackgroundMusic titleMusic;

    // How long it takes an entity to move from one square to another.
    [SerializeField] float movespeed;
    // How long it takes an entity to turn 90 degrees.
    [SerializeField] float turnspeed;
    [SerializeField] float tickRate;
    [SerializeField] int enemyAggroDistance;
    [SerializeField] float interruptPercentage;

    // Whether or not we're in-game.
    bool inGame;

    // Properties: Because why not do things all proper-like.
    public Map Map
    {
        get { return map; }
        private set { map = value; }
    }

    public float Movespeed { get { return movespeed; } }

    public float Turnspeed { get { return turnspeed; } }

    // Called at the start of the game.
    // Iterates through the DoNotDestroyList List, instantiates each GameObject, it to DontDestroyOnLoad, and sets up any references necessary.
    void Awake()
    {
        Debug.Log("GameManager Awake function executing..");
        foreach (var gm in doNotDestroyList)
        {
            if (gm != null)
            {
                Debug.Log("Instantiating " + gm.name + " and setting to do not destroy.");
                var gmInstance = Instantiate(gm);
                DontDestroyOnLoad(gmInstance);
                if (gmInstance.GetComponent<UIManager>() != null && uiManager == null)
                {
                    uiManager = gmInstance.GetComponent<UIManager>();
                }
                else if (gmInstance.GetComponent<AudioManager>() != null && audioManager == null)
                {
                    audioManager = gmInstance.GetComponent<AudioManager>();
                }
                else if (gmInstance.GetComponent<AIManager>() != null && aiManager == null)
                {
                    aiManager = gmInstance.GetComponent<AIManager>();
                }
            }
        }
    }

    // Nothing yet.
    void Start()
    {
    }

    // Executes every frame.
    // If a level has not been loaded and setup (inGame), then just return out.
    // Otherwise, update the player's hud and handle the player's input.
    void Update()
    {
        if (!inGame)
            return;

        // Update all HUD elements.
        UpdateHUD();
        // Get player input and do stuff.
        HandlePlayerInput();
        // Let the enemy's do stuff.
        HandleEnemyAI();
    }

    // Add OnLevelLoaded
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelLoaded;
    }

    // Remove OnLevelLoaded
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }

    public void SetUpClassMenu()
    {
        uiManager.SetConfirmButton(false);
        uiManager.SetUpClassButtons(classes);
        if (classes.Count > 0)
        {
            uiManager.SelectClass(classes[0]);
        }
    }

    public void SelectClass(PlayerClass playerClass)
    {
        Debug.Log("Selecting class " + playerClass.Name);
        //        if (index < 0 || index >= classes.Count)
        if (playerClass == null)
            return;

        selectedClass = playerClass;
        uiManager.SelectClass(selectedClass);
        uiManager.SetConfirmButton(true);
    }

    // Loads a level and scene with a delay in seconds.
    public void LoadLevel(float delay = 0f)
    {
        uiManager.FadeOut("Loading..", delay);
        audioManager.FadeOutMusic(delay);
        StartCoroutine(LoadLevel_Coroutine(delay));
    }

    // The coroutine for loading a level scene, waits the delay, then gets the current level's sceneIndex and loads the scene.
    IEnumerator LoadLevel_Coroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        int sceneIndex = Map.CurrentLevel.sceneIndex;
        Debug.Log("Loading level " + sceneIndex);
        SceneManager.LoadScene(sceneIndex);
    }

    // Main setup method. Executes when a scene is loaded.
    // Gets a reference to the current level from the Map and assigns it to the local level variable. If it is null, it initializes the level as the main menu, including the HUD.
    // If not null, initializes the level, generates the level, and adds in the player entity.
    //  Also calls the intialization for the HUD, displays the level text, and updates the appropriate UI.
    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        // FIXME: When an ability executed right as the level transitions, an exception is thrown and the game stops working.
        Player player = null;
        if (level != null)
        {
            player = level.Player;
        }
        // Check to see if we're loading a level.
        level = Map.CurrentLevel;

        // If there is a level to load, load it in.
        // Otherwise, we're loading the title screen.
        if (level != null)
        {
            // Sets up cells, connections, player spawn, and generates procedural areas.
            level.InitializeLevel(player);
            level.Player.Class = selectedClass;
            level.Player.SetupBaseAbilities();
            level.Player.CurrentAbility = -1;

            BuildLevel_Procedural();
            BuildLevel_Procedural_Corners();

            //            BuildLevel_Debug(level);

            // Create the player. Set the instance to a new instantiated playerPrefab.
            level.Player.Instance = GameObject.Instantiate(level.Player.Class.ClassCamera);
            // Manually set the position.
            SetEntityInstanceLocation(level.Player);
            // Loop through all of the enemies and spawn their instances.
            foreach (var enemy in level.EnemyList)
            {
                // Instantiate the prefab to an instance.
                enemy.Instance = GameObject.Instantiate(enemy.Instance);
                // Set the enemy instance's position.
                SetEntityInstanceLocation(enemy);
            }

            // Initialize the UI for the level.
            uiManager.Initialize_Level(interruptPercentage);
            uiManager.SetUpAbilityIcons(level.Player);
            // Display the area text.
            uiManager.DisplayAreaText(level.name);
            // Fade in with the area name.
            uiManager.FadeIn(level.name, 2f);

            audioManager.FadeInMusic(level.music, 1f);

            inGame = true;
            StartCoroutine(ApplyGradualEffects_Coroutine());
        }
        else
        {
            // Do stuff to load main menu.
            inGame = false;
            uiManager.Initialize_Main();
            SetUpClassMenu();
            uiManager.FadeIn("Hydrus");
            audioManager.FadeInMusic(titleMusic, 0f);
        }
    }

    void BuildLevel_Procedural()
    {
        float cellScale = Map.CellScale;
        foreach (Cell cell in level.cells)
        {
            // TODO: Switch to cell.Type == CellType.Procedural, instead of != CellType.Empty.
            // Currently generates for all cells, but we may want to only generate for the procedural sections,
            // and manually place the static areas.
            if (cell.Type != CellType.Empty && level.HasConnections(cell))
            {
                Vector3 center = Map.GetCellPosition(cell);
                GameObject cellInstance = new GameObject("Cell_" + cell.Index);
                cellInstance.transform.position = center;
                GameObject floorInstance = GameObject.Instantiate(Map.GetRandomPrefab(floorPrefabs), cellInstance.transform);
                GameObject ceilingInstance = GameObject.Instantiate(Map.GetRandomPrefab(ceilingPrefabs), cellInstance.transform);
                List<GameObject> wallInstances = new List<GameObject>();
                for (int w = 0; w < 4; w++)
                {
                    GameObject wallInstance = GameObject.Instantiate(Map.GetRandomPrefab(wallPrefabs), cellInstance.transform);
                    wallInstance.name = "Wall_" + ((Direction)w).ToString();
                    wallInstance.transform.rotation = Quaternion.Euler(0f, 90f * w, 0f);
                    wallInstances.Add(wallInstance);
                }

                cellInstance.transform.localScale = Vector3.one * cellScale;


                //                GameObject cellInstance = GameObject.Instantiate(cellPrefabDebug, center, Quaternion.identity);
                //                cellInstance.transform.localScale = Vector3.one * cellScale;

                for (int d = 0; d < 4; d++)
                {
                    Direction direction = (Direction)d;
                    if (level.HasConnection(cell, direction))
                    {
                        RemoveWall(cellInstance, direction);
                    }
                }
            }
        }
    }

    void BuildLevel_Procedural_Corners()
    {
        float cellScale = Map.CellScale;
        Vector3 offset = new Vector3(0.5f * cellScale, 0f, 0.5f * cellScale);
        // Iterate through each corner. Of cell up left, up right, down left, down right,
        //      IF, none are connected and at least one has connections, create CORNER
        //      OR, two are connected (one connection), create WALL CONNECTION
        //      OR, three are connected (two connections), create CORNER
        //      OR, four are connected (three connections), create CORNER
        //      OR, are all connected (four connections), do not create CORNER

        // Iterating through corners. Corner would be +0.5,+0.5. Actually iterating through down-left cell indices.
        for (int x = -1; x < Map.MaxWidth; x++)
        {
            for (int z = -1; z < Map.MaxDepth; z++)
            {
                // Get the indexes for surrounding cells.
                List<int> cornerIndexes = new List<int>();
                cornerIndexes.Add(Cell.GetIndex(x, z));
                cornerIndexes.Add(Cell.GetIndex(x + 1, z));
                cornerIndexes.Add(Cell.GetIndex(x, z + 1));
                cornerIndexes.Add(Cell.GetIndex(x + 1, z + 1));

                // Iterate through and gather the valid cells.
                List<Cell> cornerCells = new List<Cell>();
                foreach (int cornerIndex in cornerIndexes)
                {
                    if (level.IsValidCell(cornerIndex))
                    {
                        cornerCells.Add(level.cells[cornerIndex]);
                    }
                }

                // Next, check their connections to each other.
                int numConnections = 0;
                bool connections = false;
                List<Direction> connectionDirections = new List<Direction>();

                for (int i = 0; i < cornerCells.Count; i++)
                {
                    Cell cell1 = cornerCells[i];
                    if (level.HasConnections(cell1))
                        connections = true;
                    for (int j = i; j < cornerCells.Count; j++)
                    {
                        Cell cell2 = cornerCells[j];
                        if (cell1 != cell2)
                        {
                            if (level.HasConnection(cell1, cell2))
                            {
                                connectionDirections.Add(Cell.GetNeighborDirection(cell1, cell2));
                                numConnections += 1;
                            }
                        }
                    }
                }

                // foreach (Cell cell1 in cornerCells)
                // {
                //     if (level.HasConnections(cell1))
                //         connections = true;

                //     foreach (Cell cell2 in cornerCells)
                //     {
                //         if (cell1 != cell2)
                //         {
                //             if (level.HasConnection(cell1, cell2))
                //             {
                //                 numConnections += 1;
                //             }
                //         }
                //     }
                // }

                // Debug.Log("Number of connections is " + numConnections + " with " + cornerCells.Count + " cells.");
                // Do stuff based on how many connections.
                if (numConnections == 0)
                {
                    if (connections)
                    {
                        // Create corner.
                        Vector3 center = Map.GetCellPosition(x, z) + offset;
                        GameObject cornerInstance = GameObject.Instantiate(Map.GetRandomPrefab(cornerPrefabs), center, Quaternion.identity);
                        // GameObject cornerInstance = GameObject.Instantiate(cornerPrefabDebug, center, Quaternion.identity);
                        cornerInstance.transform.localScale = cornerInstance.transform.localScale * cellScale;
                    }
                }
                else if (numConnections == 1)
                {
                    // Create wall connection piece.
                }
                else if (numConnections == 2)
                {
                    bool parallelWalls = false;
                    Direction direction1 = connectionDirections[0];
                    Direction direction2 = connectionDirections[1];
                    Direction inverseDirection1 = level.GetInverseDirection(direction1);
                    if (direction1 == direction2 || inverseDirection1 == direction2)
                        parallelWalls = true;

                    if (parallelWalls)
                    {
                        // Create wall connection piece.
                    }
                    else
                    {
                        // Create corner
                        Vector3 center = Map.GetCellPosition(x, z) + offset;
                        GameObject cornerInstance = GameObject.Instantiate(Map.GetRandomPrefab(cornerPrefabs), center, Quaternion.identity);
                        // GameObject cornerInstance = GameObject.Instantiate(cornerPrefabDebug, center, Quaternion.identity);
                        cornerInstance.transform.localScale = cornerInstance.transform.localScale * cellScale;
                    }
                }
                else if (numConnections == 3)
                {
                    // Create corner
                    Vector3 center = Map.GetCellPosition(x, z) + offset;
                    GameObject cornerInstance = GameObject.Instantiate(Map.GetRandomPrefab(cornerPrefabs), center, Quaternion.identity);
                    // GameObject cornerInstance = GameObject.Instantiate(cornerPrefabDebug, center, Quaternion.identity);
                    cornerInstance.transform.localScale = cornerInstance.transform.localScale * cellScale;
                }
                else if (numConnections == 4)
                {
                    // Do not create a corner.
                }
            }
        }
    }

    // Destroys wall pieces of a cell instance in a given direction.
    // Used for procedural, to remove walls pieces between cells that are connected.
    // Iterates through each child of the instance, and if the piece contains the direction in its name, it will be removed.
    void RemoveWall(GameObject cellInstance, Direction direction)
    {
        foreach (Transform piece in cellInstance.transform)
        {
            if (piece.name.Contains(direction.ToString()))
            {
                Destroy(piece.gameObject);
            }
        }
    }

    // A bunch of silly code.
    // Iterates through each cell in the passed-in level, if it's not empty and has connections it gets the appropriate position and size for the debug block, then instantiates it.
    // Sets the color to the respective color for the CellType.
    // Then iterates through each direction and checks if there is a connection in that direction. 
    //   If there is a connection, creates a block to indicate the connection.
    void BuildLevel_Debug(Level level)
    {
        float cellScale = Map.CellScale;
        // Main iteration.
        foreach (Cell cell in level.cells)
        {
            // If it's not empty and has connections.
            if (cell.Type != CellType.Empty && level.HasConnections(cell))
            {
                // Gets the correct positions, sizes, and colors and sets up the block with the determined values.
                Vector3 center = Map.GetCellPosition(cell);
                Vector3 size = new Vector3(0.5f, 0.1f, 0.5f) * cellScale;
                GameObject block = GameObject.Instantiate(blockPrefab, center, Quaternion.identity);
                block.transform.localScale = size;

                block.GetComponent<Renderer>().material.color = Cell.DetermineColor(cell.Type);

                Vector3 offset = Vector3.zero;
                Vector3 connectionSize = new Vector3(0.1f, 0.1f, 0.1f) * cellScale;

                // Iterates through all connections and sets up a block for each.
                int index = cell.Index;
                for (int d = 0; d < level.connectionMatrix.GetLength(1); d++)
                {
                    if (level.connectionMatrix[index, d] == true)
                    {
                        if (d == (int)Direction.Up)
                        {
                            offset = new Vector3(0f, 0f, 0.5f) * cellScale;
                            connectionSize = new Vector3(0.1f, 0.1f, 1f);
                        }
                        else if (d == (int)Direction.Right)
                        {
                            offset = new Vector3(0.5f, 0f, 0f) * cellScale;
                            connectionSize = new Vector3(1f, 0.1f, 0.1f);
                        }
                        else if (d == (int)Direction.Down)
                        {
                            offset = new Vector3(0, 0f, -0.5f) * cellScale;
                            connectionSize = new Vector3(0.1f, 0.1f, 1f);
                        }
                        else if (d == (int)Direction.Left)
                        {
                            offset = new Vector3(-0.5f, 0f, 0f) * cellScale;
                            connectionSize = new Vector3(1f, 0.1f, 0.1f);
                        }
                        GameObject connectionBlock = GameObject.Instantiate(blockPrefab, center + offset, Quaternion.identity);
                        connectionBlock.transform.localScale = connectionSize;
                        connectionBlock.GetComponent<Renderer>().material.color = Color.white;
                    }
                }
            }
        }
    }

    // General update HUD method.
    void UpdateHUD()
    {
        Player player = level.Player;
        // If standing at an exit, give option to go through exit.
        ExitPrompt(level.CanExit);
        uiManager.UpdatePlayerCores(player.Cores);
        uiManager.UpdateEffectList(player.StatusEffects);
        //uiManager.UpdatePlayerAbilityHUD(player.Cooldowns.Values.ToList(), player.CooldownsRemaining.Values.ToList(), player.CurrentAbility, player.CastProgress);
        uiManager.UpdatePlayerAbilityHUD(player.GetCooldownsList(), player.GetCooldownRemainingList(), player.CurrentAbility, player.CastProgress);

        if (player.Cell.Indicators.Count > 0 && !uiManager.Highlighted)
        {
            Debug.Log("Player cell is being targeted.");
            uiManager.ToggleBorderHighlight();
            // uiManager.HighlightBorder();
        }
        else if (player.Cell.Indicators.Count <= 0 && uiManager.Highlighted)
        {
            Debug.Log("Player cell is NOT being targeted.");
            uiManager.ToggleBorderHighlight();
            // uiManager.UnHighlightBorder();
        }

        //        Direction playerDirection = level.Player.ToAbsoluteDirection(Direction.Up);
        bool forwardConnection = level.HasConnection(player.Cell, player.Facing);
        Cell forwardCell = level.GetNeighbor(player.Cell, player.Facing);
        if (forwardConnection && forwardCell != null)
        {
            Enemy enemy = (Enemy)forwardCell.Occupant;
            if (enemy != null)
            {
                uiManager.UpdateEnemyInfo(true, enemy.Name, enemy.CurrentHealth / enemy.MaxHealth, enemy.CastProgress, enemy.CurrentCastTime);
            }
            else
            {
                uiManager.UpdateEnemyInfo(false);
            }
        }
        // If standing on an item, give option to collect item.
        /*        if (level.CanExit)
                {
                    ExitPrompt(true);
                }
                else if (ItemCheck())
                {
                    ItemPrompt();
                }
                */
    }

    // If true, displays the text with the option to exit the level.
    // If false, displays nothing.
    void ExitPrompt(bool check)
    {
        if (check)
        {
            // If standing at an exit, display a text prompt.
            uiManager.DisplayText("Press Space to exit the level");
        }
        else
        {
            uiManager.DisplayText("");
        }
    }

    // Returns true if the player is on a cell with an item.
    // Otherwise, returns false.
    bool ItemCheck()
    {
        Cell cell = level.Player.Cell;
        if (cell != null && cell.Item != null)
        {
            return true;
        }
        return false;
    }

    // Nothing yet. Will display
    void ItemPrompt()
    {
        // If standing on an item, display text prompt.
    }

    // Checks player input, mainly key presses.
    // The player is only able to input commands while they are not already performing other actions. (State must be Idle)
    // Key Presses:
    //              Space - If the player can exit the level, exits the level.
    //              Input Direction - Gets the player's input direction, if there is any input (WASD) - Forward, left, backwards, right
    //              Q - Turns the player left.
    //              E - Turns the player right.
    //TO DO: Add checking for the animation state, so it will know when the player steps left, right, and backwards.
    void HandlePlayerInput()
    {
        if (level.Player.State == EntityState.Idle && !level.Player.StatusEffects.Stunned)
        {
            Direction inputDir = GetInputDirection();
            if (Input.GetKeyDown(KeyCode.Space) && level.CanExit)
            {
                ExitLevel();
            }
            else if (inputDir != Direction.Null)
            {
                MoveEntityLocation(level.Player, inputDir);
                //                MoveEntityInstance(player, inputDir);
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                TurnEntityInstanceLeft(level.Player);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                TurnEntityInstanceRight(level.Player);
            }
            else if (Input.GetKey(KeyCode.Alpha1))
            {
                CastPlayerAbility(level.Player, 0);
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                CastPlayerAbility(level.Player, 1);
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                CastPlayerAbility(level.Player, 2);
            }
            else if (Input.GetKey(KeyCode.Alpha4))
            {
                CastPlayerAbility(level.Player, 3);
            }
            else if (Input.GetKeyDown(KeyCode.M)) // Plays the test sound.
            {
                //                testSound.Position = Vector3.zero; //level.Player.Instance.transform.position;
                audioManager.PlaySoundEffect(testSound);
            }
            else if (Input.GetKeyDown(KeyCode.N)) // Casts the test ability.
            {
                CastPlayerAbility(level.Player, 0);
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                bool alive = level.Player.Damage(25, (level.Player.CastProgress >= interruptPercentage));
                PerformEntityDeathCheck(level.Player, alive);

                uiManager.UpdatePlayerHealth(level.Player.CurrentHealth / level.Player.MaxHealth);
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                level.Player.Heal(25);
                uiManager.UpdatePlayerHealth(level.Player.CurrentHealth / level.Player.MaxHealth);
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                AbilityEffect effect = new AbilityEffect(-1, (AbilityStatusEff)Random.Range(0, 10), Random.Range(0, 10), Random.Range(0f, 1f));
                level.Player.StatusEffects.AddEffect(effect);
            }
        }
    }

    void HandleEnemyAI()
    {
        foreach (Enemy enemy in level.EnemyList)
        {
            enemy.InCombat = (level.GetDistance(enemy.Cell, level.Player.Cell) <= enemyAggroDistance);
            if (enemy.State == EntityState.Idle)
            {
                var action = aiManager.ExecuteAIOnEnemy(enemy, level);
                //                action = new EnemyAction();

                if (action.Movement != Movement.Null)
                {
                    if (action.Movement == Movement.Nothing)
                    {
                        // Do nothing.
                    }
                    else if (action.Movement == Movement.Forward)
                    {
                        MoveEntityLocation(enemy, enemy.Facing);
                    }
                    else if (action.Movement == Movement.TurnLeft)
                    {
                        TurnEntityInstanceLeft(enemy);
                    }
                    else if (action.Movement == Movement.TurnRight)
                    {
                        TurnEntityInstanceRight(enemy);
                    }
                }
                else if (action.AbilityIndex != -1)
                {
                    CastEnemyAbility(enemy, action.AbilityIndex);
                }
            }
        }
    }

    IEnumerator ApplyGradualEffects_Coroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(tickRate);
        Player player = level.Player;
        while (inGame)
        {
            // Debug.Log("Player damage rate is " + player.StatusEffects.DamageRate);

            if (player.StatusEffects.HealRate > 0f)
            {
                player.Heal(player.StatusEffects.HealRate * player.MaxHealth * tickRate);
                uiManager.UpdatePlayerHealth(player.CurrentHealth / player.MaxHealth);
            }

            if (player.StatusEffects.DamageRate > 0f)
            {
                player.Damage(player.StatusEffects.DamageRate * tickRate);
                uiManager.UpdatePlayerHealth(player.CurrentHealth / player.MaxHealth);
            }

            foreach (Enemy enemy in level.EnemyList)
            {
                if (enemy.StatusEffects.HealRate > 0f)
                    enemy.Heal(enemy.StatusEffects.HealRate * enemy.MaxHealth * tickRate);
                if (enemy.StatusEffects.DamageRate > 0f)
                    enemy.Damage(enemy.StatusEffects.DamageRate * tickRate);
            }
            yield return wait;
        }
    }

    // Calls the GetNextLevel method in the Map object, updating the current level with the level corresponding to the exit the player is on.
    // Loads the new level with a 0.5 second fade time.
    void ExitLevel()
    {
        level.Player.State = EntityState.Null;
        audioManager.FadeOutMusic(1f);
        // If the entity has moved to an exit cell and the entity is the player.
        Map.NextLevel(level.Player.Cell);
        LoadLevel(0.5f);
    }

    // Gets the axis inputs, checks which is the dominant direction (forward, right, left, backwards),
    // and converts this relative direction into the corresponding absolute direction. (Left moves the player Up if the player is facing Right).
    Direction GetInputDirection()
    {
        // Left/right
        float h = Input.GetAxisRaw("Horizontal");
        // Forward/backward
        float v = Input.GetAxisRaw("Vertical");

        // If Left/right input is greater than forward/backward input, move left/right
        // Otherwise, move forward/backward
        float magH = Mathf.Abs(h);
        float magV = Mathf.Abs(v);

        if (magH > magV)
        {
            if (h > 0)
                return level.Player.GetDirection(Direction.Right);
            else if (h < 0)
                return level.Player.GetDirection(Direction.Left);
        }
        else if (magH < magV)
        {
            if (v > 0)
                return level.Player.GetDirection(Direction.Up);
            else if (v < 0)
                return level.Player.GetDirection(Direction.Down);
        }

        return Direction.Null;
    }

    // Moves an Entity entity in Direction direction.
    // This is absolute direction, not relative.
    //      Calls the GetDestination method in the level object, which returns a neighbor after a bunch of probably unnecessary validation.
    //      If any of the validation fails, it returns null, which we check for.
    //      Sets the entity's state to Moving to prevent any other actions.
    //      Calls for a tween of the entity's instance GameObject position from its current position to the destination's position.
    //          Passes in the Movespeed for the time parameter, with no delay, an EaseOut animation curve, and a callback method upon completion of the tween.
    //      Starts the MoveEntityLocation_Coroutine coroutine, passing in the entity, neighbor,
    //           and Movespeed * 0.75 for the delay (So the entity is considered on the new cell after 75% of the movement is completed)
    void MoveEntityLocation(Entity entity, Direction direction)
    {
        Player player = level.Player;
        Cell neighbor = level.GetDestination(entity, direction);
        if (neighbor == null)
            return;

        if (entity.StatusEffects.Stunned || entity.StatusEffects.Rooted)
            return;

        entity.State = EntityState.Moving;

        // float adjustedMovespeed = Movespeed / entity.StatusEffects.MovementScale;
        float adjustedMovespeed = entity.GetAdjustedMoveSpeed(Movespeed);

        //Probably going to make a separate method to handle all this.
        if (entity.IsPlayer == true)
        {
            if (level.Player.Facing == direction)
            {
                SetPlayerAnimation("MoveForward", adjustedMovespeed);
            }
            else if (level.Player.GetRight() == direction)
            {
                SetPlayerAnimation("MoveRight", adjustedMovespeed);
            }
            else if (level.Player.GetLeft() == direction)
            {
                SetPlayerAnimation("MoveLeft", adjustedMovespeed);
            }
            else if (level.Player.GetBackward() == direction)
            {
                SetPlayerAnimation("MoveBack", adjustedMovespeed);
            }
        }
        Tween.Position(entity.Instance.transform, Map.GetCellPosition(neighbor), adjustedMovespeed, 0f, Tween.EaseLinear, completeCallback: () => entity.State = EntityState.Idle);
        StartCoroutine(MoveEntityLocation_Coroutine(entity, neighbor, adjustedMovespeed * 0.75f));
    }

    // Sets the destination cell to a locked state (to prevent any other entities to attempt to move to this cell)
    // Waits for (delay) seconds,
    // Then sets the entity's location by calling the SetEntityLocation method in the level object and passing in the entity and cell.
    // Finally, sets the cell to an unlucky state. (Entities will still be prevented from moving there, since an entity is already there)
    public IEnumerator MoveEntityLocation_Coroutine(Entity entity, Cell cell, float delay)
    {
        cell.Locked = true;
        yield return new WaitForSeconds(delay);
        level.SetEntityLocation(entity, cell);
        cell.Locked = false;
    }

    // NOTE: NOT USING.
    // Callback method for entity instance GameObject position tween.
    // Just sets the passed-in entity's state to Idle to indicate the completion of their movement.
    void FinishMoving(Entity entity)
    {
        entity.State = EntityState.Idle;
    }

    // Turns an entity left.
    // Sets the entity's state to Moving,
    // Then executes a tween on the rotation of the entity's instance GameObject, rotating by -90 degrees on the Y-axis,
    //      in world space, TurnSpeed as the speed, no delay, and assigning FinishTurning as the callback method on completion.
    // Then calls TurnLeft on the entity.
    // Note: The change of the entity's Facing property can be instant,
    //      since the entity cannot take any other actions until the rotation is complete.
    void TurnEntityInstanceLeft(Entity entity)
    {
        entity.State = EntityState.Moving;
        Tween.Rotate(entity.Instance.transform, new Vector3(0f, -90f, 0f), Space.World, Turnspeed, 0f, completeCallback: (() => FinishTurning(entity)));
        entity.TurnLeft();
    }

    // Turns an entity right.
    // Sets the entity's state to Moving,
    // Then executes a tween on the rotation of the entity's instance GameObject, rotating by 90 degrees on the Y-axis,
    //      in world space, TurnSpeed as the speed, no delay, and assigning FinishTurning as the callback method on completion.
    // Then calls TurnRight on the entity.
    // Note: The change of the entity's Facing property can be instant,
    //      since the entity cannot take any other actions until the rotation is complete.
    void TurnEntityInstanceRight(Entity entity)
    {
        entity.State = EntityState.Moving;
        Tween.Rotate(entity.Instance.transform, new Vector3(0f, 90f, 0f), Space.World, Turnspeed, 0f, completeCallback: (() => FinishTurning(entity)));
        entity.TurnRight();
    }


    // Callback method for entity instance GameObject rotation tween.
    // Just sets the passed-in entity's state to Idle to indicate the completion of their rotation.
    void FinishTurning(Entity entity)
    {
        entity.State = EntityState.Idle;
    }

    // Sets the entity's instance GameObject position and rotation. Used only during the initial setup of the level.
    void SetEntityInstanceLocation(Entity entity)
    {
        var eTransform = entity.Instance.transform;
        eTransform.position = Map.GetCellPosition(entity.Cell);
        eTransform.rotation = Quaternion.Euler(0f, 90f * (int)entity.Facing, 0f);
    }

    void CastPlayerAbility(Entity entity, int index)
    {
        if (entity.StatusEffects.Stunned || entity.StatusEffects.Silenced)
            return;

        AbilityObject ability = entity.CastAbility(index);
        if (ability == null)
            return;

        uiManager.UpdatePlayerCast(entity.CurrentCastTime);
        // Get the cells to highlight and display them.
        List<Cell> affected = level.GetAffectedCells_Highlight(entity, ability);
        foreach (Cell cell in affected)
        {
            AddIndicator(testIndicator, cell, entity);
        }
        //Setting the cast time scale to the current cast time scale... Blegh
        SetPlayerCastAnimation("Cast", level.Player.Abilities[index].CastTime);
    }

    public void CancelPlayerAbility()
    {
        uiManager.CancelPlayerCast();
        SetPlayerAnimation("Interrupt", 1.0f);
    }

    void CastEnemyAbility(Entity entity, int index)
    {
        if (entity.StatusEffects.Stunned || entity.StatusEffects.Silenced)
            return;

        AbilityObject ability = entity.CastAbility(index);
        if (ability == null)
            return;

        // Get the cells to highlight and display them.
        List<Cell> affected = level.GetAffectedCells_Highlight(entity, ability);
        foreach (Cell cell in affected)
        {
            AddIndicator(testIndicator, cell, entity);
        }
    }

    public void PerformAbility(Entity entity, AbilityObject ability)
    {

        // Play sounds and animations.
        // Deal damage to entities in the cells.

        if (ability.SoundEffect != null)
        {
            audioManager.PlaySoundEffect(new SoundEffect(ability.SoundEffect, entity.Instance.transform.position));

        }

        if (entity.IsPlayer == true)
        {
            SetPlayerAnimation("CastActivate", 1.0f);
        }

        List<Cell> affected = level.GetAffectedCells(entity, ability);
        Debug.Log(entity.Name + " casting " + ability.SoundEffect);

        foreach (Cell cell in affected)
        {
            //            Debug.Log("Performing " + ability.abilityName + " at " + cell.X + ", " + cell.Z);
            if (ability.Type != AbilityType.Zone)
            {
                Entity target = cell.Occupant;

                if (target != null)
                {
                    ApplyAbility(target, ability, entity);
                }
            }
            else
            {
                StartCoroutine(ApplyZoneAbility_Coroutine(cell, ability, entity));
            }
        }
    }

    void ApplyAbility(Entity target, AbilityObject ability, Entity caster)
    {
        Debug.Log("-- Affecting " + target.Name + " .. Dealing " + ability.Damage + " damage.");

        foreach (AbilityEffect effect in ability.StatusEffects)
        {
            AbilityEffect effectInstance = new AbilityEffect(caster.Index, effect.Effect, effect.Duration, effect.Value);
            target.StatusEffects.AddEffect(effectInstance);
        }
        bool alive = target.Damage(ability.Damage, (target.CastProgress >= interruptPercentage));

        if (target.IsPlayer)
        {
            uiManager.UpdatePlayerHealth(target.CurrentHealth / target.MaxHealth);
            uiManager.FlashPlayerDamage();

            if (ability.Damage > 0)
                target.Instance.GetComponentInChildren<ShakeTransform>().AddShakeEvent(testShakeEvent);
        }

        PerformEntityDeathCheck(target, alive);
    }

    void ApplyZoneAbility(Entity target, AbilityObject ability, Entity caster)
    {
        foreach (AbilityEffect effect in ability.StatusEffects)
        {
            AbilityEffect effectInstance = new AbilityEffect(caster.Index, effect.Effect, tickRate, effect.Value);
            target.StatusEffects.AddEffect(effectInstance);
        }

        bool alive = target.Damage(0);
        // if (target.GetType() == typeof(Player))
        if (target.IsPlayer)
        {
            uiManager.UpdatePlayerHealth(target.CurrentHealth / target.MaxHealth);
        }

        PerformEntityDeathCheck(target, alive);
    }

    IEnumerator ApplyZoneAbility_Coroutine(Cell cell, AbilityObject ability, Entity caster)
    {
        if (ability.StatusEffects.Count > 0)
        {
            float timer = 0f;
            WaitForSeconds tick = new WaitForSeconds(tickRate);
            float duration = ability.StatusEffects[0].Duration;

            while (timer < duration)
            {
                Entity target = cell.Occupant;

                if (target != null && target != caster)
                {
                    ApplyZoneAbility(target, ability, caster);
                }

                timer += tickRate;
                yield return tick;
            }
        }
    }

    public void PerformEntityDeathCheck(Entity entity, bool alive)
    {
        if (!alive && entity.IsPlayer)
        {
            // Do player death stuff.
        }
        else if (!alive && !entity.IsPlayer)
        {
            Debug.Log("Enemy is dead!");
            level.Player.Cores += entity.Cores;
            StartCoroutine(level.RemoveEntity(entity));
        }
    }

    public void AddIndicator(GameObject indicatorPrefab, Cell cell, Entity entity)
    {
        GameObject indicatorInstance = GameObject.Instantiate(indicatorPrefab, Map.GetCellPosition(cell), indicatorPrefab.transform.rotation);
        indicatorInstance.transform.localScale *= Map.CellScale;
        Indicator indicator = new Indicator { Instance = indicatorInstance, Cell = cell, Entity = entity };
        indicator.AddIndicator();
    }

    // Method for changing the animation of the player
    public void SetPlayerAnimation(string setter, float scale)
    {
        Animator playerAnim = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        if (setter == "Cast")
        {
            playerAnim.SetFloat("CastTimeScale", scale);
        }
        else
        {
            playerAnim.SetFloat("MoveSpeedScale", scale);
        }

        playerAnim.SetTrigger(setter);

    }

    public void SetPlayerCastAnimation(string setter, float scale)
    {
        Animator playerAnim = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();

        playerAnim.SetFloat("CastTimeScale", scale);
        playerAnim.SetTrigger(setter);

    }
}
