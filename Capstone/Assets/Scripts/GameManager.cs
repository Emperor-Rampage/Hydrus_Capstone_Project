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

    [SerializeField]
    GameObject blockPrefab;
    // The prefab used for generic entities during debug build.
    [SerializeField] GameObject entityPrefab;
    // For testing the AudioManager.
    [SerializeField] SoundEffect testSound;
    // For testing ability casting.
    [SerializeField] AbilityObject testAbility;

    [Space(10)]
    [Header("Setup")]
    // The player prefab. Likely will only contain a camera.
    [SerializeField]
    GameObject playerPrefab;
    // A list of GameObjects that need to be instantiated and defined as DontDestroyOnLoad.
    [SerializeField]
    List<GameObject> DoNotDestroyList;

    [Space(10)]
    [Header("Game")]

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
        foreach (var gm in DoNotDestroyList)
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

        UpdateHUD();
        HandlePlayerInput();

        foreach (Enemy enemy in level.EnemyList)
        {
            if (enemy.State == EntityState.Idle)
            {
                var action = aiManager.ExecuteAIOnEnemy(enemy);
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
            }
        }
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
        // Check to see if we're loading a level.
        level = Map.CurrentLevel;

        // If there is a level to load, load it in.
        // Otherwise, we're loading the title screen.
        if (level != null)
        {
            // Sets up cells, connections, player spawn, and generates procedural areas.
            level.InitializeLevel();
            level.Player.Abilities.Add(testAbility);
            UnityEngine.Debug.Log(level.connectionMatrix.GetLength(1));
            // Creates debug instances for the cells and connections.
            // TODO: Create procedural area generation.
            //          Possibly use prefab "templates" or cell chunks, remove walls where there are connections between adjacent cells.
            //          For corner pieces, iterate through each cell, from left to right, bottom to top,
            //          checking connections with adjacent cells,
            //
            //          Create local list of corners to create, 0 = up right, 1 = down right, 2 = down left, 3 = up left
            //
            //          IF creating all corners, then fill completely,
            //          ELSE IF creating outer edges, then only fill with corners in which there is no connection.
            //
            //          Check all connections, if procedural, if that cell has been visited,
            //            don't instantiate any corners in that direction by removing the corresponding corners from the list.
            //
            //              IF connection with up,
            //                  IF has been visited, don't create up left or up right corners - remove 3 and 0.
            //                  IF has not been visited, create both both up left and up right corners - do nothing.
            //              IF connection with right,
            //                  IF has been visited, don't create up right or down right corners - remove 3 and 1.
            //                  IF has not been visited, create both both up right and down right corners - do nothing.
            //              IF connection with down,
            //                  IF has been visited, don't create down left or down right corners - remove 2 and 1.
            //                  IF has not been visited, create both both down left and down right corners - do nothing.
            //              IF connection with left,
            //                  IF has been visited, don't create up left or down left corners - remove 3 and 2.
            //                  IF has not been visited, create both both up left and down left corners - do nothing.
            //
            //              Iterate through all remaining corners to create and instantiate them.
            //
            //
            BuildLevel_Procedural();

            //            BuildLevel_Debug(level);

            // Create the player. Set the instance to a new instantiated playerPrefab.
            level.Player.Instance = GameObject.Instantiate(playerPrefab);
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
            uiManager.Initialize_Level();
            // Display the area text.
            uiManager.DisplayAreaText(level.name);
            // Fade in with the area name.
            uiManager.FadeIn(level.name, 2f);

            audioManager.FadeInMusic(level.music, 1f);

            inGame = true;
        }
        else
        {
            // Do stuff to load main menu.
            inGame = false;
            uiManager.Initialize_Main();
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
                GameObject cellInstance = GameObject.Instantiate(cellPrefabDebug, center, Quaternion.identity);
                cellInstance.transform.localScale = Vector3.one * cellScale;

                for (int d = 0; d < 4; d++)
                {
                    Direction direction = (Direction)d;
                    if (level.HasConnection(cell, direction))
                    {
                        RemoveParts(cellInstance, direction);
                    }
                }
            }
        }
    }

    // Destroys pieces of a cell instance in a given direction.
    // Used for procedural, to remove walls and connecting pieces between cells that are connected.
    // Iterates through each child of the instance, and if the piece contains the direction in its name, it will be removed.
    void RemoveParts(GameObject cellInstance, Direction direction)
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
        // If standing at an exit, give option to go through exit.
        ExitPrompt(level.CanExit);
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
    void HandlePlayerInput()
    {
        if (level.Player.State == EntityState.Idle)
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
                bool alive = level.Player.Damage(25);
                if (!alive)
                {
                    Debug.Log("Entity is dead!");
                    StartCoroutine(level.RemoveEntity(level.Player));
                }
                uiManager.UpdatePlayerHealth((float)level.Player.CurrentHealth / level.Player.MaxHealth);
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                bool full = level.Player.Heal(25);
                uiManager.UpdatePlayerHealth((float)level.Player.CurrentHealth / level.Player.MaxHealth);
            }
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
                return level.Player.ToAbsoluteDirection(Direction.Right);
            else if (h < 0)
                return level.Player.ToAbsoluteDirection(Direction.Left);
        }
        else if (magH < magV)
        {
            if (v > 0)
                return level.Player.ToAbsoluteDirection(Direction.Up);
            else if (v < 0)
                return level.Player.ToAbsoluteDirection(Direction.Down);
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

    // TODO: Adjust speed to account for any movement slows.
    void MoveEntityLocation(Entity entity, Direction direction)
    {
        Cell neighbor = level.GetDestination(entity, direction);
        if (neighbor == null)
            return;

        entity.State = EntityState.Moving;

        Tween.Position(entity.Instance.transform, Map.GetCellPosition(neighbor), Movespeed, 0f, Tween.EaseLinear, completeCallback: () => entity.State = EntityState.Idle);
        StartCoroutine(MoveEntityLocation_Coroutine(entity, neighbor, Movespeed * 0.75f));
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
        AbilityObject ability = entity.CastAbility(index);
        if (ability == null)
            return;

        uiManager.UpdatePlayerCast(ability.CastTime);
        // Get the cells to highlight and display them.
        List<Cell> affected = level.GetAffectedCells_Highlight(entity, ability);
    }

    void CastEnemyAbility(Entity entity, int index)
    {
        AbilityObject ability = entity.CastAbility(index);
        if (ability == null)
            return;

        // Get the cells to highlight and display them.
        List<Cell> affected = level.GetAffectedCells_Highlight(entity, ability);
    }

    public void PerformAbility(Entity entity, AbilityObject ability)
    {
        // Play sounds and animations.
        // Deal damage to entities in the cells.

        if (ability.SoundEffect != null)
        {
            audioManager.PlaySoundEffect(new SoundEffect(ability.SoundEffect, entity.Instance.transform.position));
        }

        List<Cell> affected = level.GetAffectedCells(entity, ability);
        Debug.Log(entity.Name + " casting " + ability.SoundEffect);

        foreach (Cell cell in affected)
        {
            //            Debug.Log("Performing " + ability.abilityName + " at " + cell.X + ", " + cell.Z);
            Entity target = cell.Occupant;

            if (target != null)
            {
                bool alive = target.Damage(ability.Damage);

                if (!alive)
                {
                    Debug.Log("Entity is dead!");
                    StartCoroutine(level.RemoveEntity(target));
                }

                Debug.Log("-- Affecting " + target.Name + " .. Dealing " + ability.Damage + " damage.");
            }
        }
    }
}
