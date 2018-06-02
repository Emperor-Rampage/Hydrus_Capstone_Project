using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.Rendering.PostProcessing;
using Pixelplacement;

using MapClasses;
using EntityClasses;
using AudioClasses;
using AIClasses;
using AbilityClasses;
using ParticleClasses;

// The main game manager. Is a singleton, and contains the general settings as well as references to other systems.
// Contains fields and properties for:
//   Debug section with placeholder variables.
//   Setup section with the instances that need to be set up when the game starts.
//   Game sections with private references to necessary objects and general stats.
//
// Should be the main root object, and generally should be the only thing calling the other systems (UIManager, AIManager, Map, Level, etc.)
[SelectionBase]
public class GameManager : Pixelplacement.Singleton<GameManager>
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
    [SerializeField] GameObject testIndicatorPlayer;
    [SerializeField] GameObject testIndicatorEnemy;
    [SerializeField] CameraShakeObject testShakeEvent;

    [Space(10)]
    [Header("Setup")]
    // The player prefab. Likely will only contain a camera.
    [SerializeField]
    GameObject playerPrefab;
    // A list of GameObjects that need to be instantiated and defined as DontDestroyOnLoad.
    [SerializeField] List<GameObject> doNotDestroyList;
    [SerializeField] AudioMixer mixer;

    [Header("Level Pieces")]
    [SerializeField]
    List<LevelPrefab> wallPrefabs;
    [SerializeField] List<LevelPrefab> floorPrefabs;
    [SerializeField] List<LevelPrefab> ceilingPrefabs;
    [SerializeField] List<LevelPrefab> cornerPrefabs;
    [SerializeField] List<LevelPrefab> doorPrefabs;

    [Space(10)]
    [Header("Game")]
    // TODO: Move MouseLookManager from its own singleton to the UIManager.
    //       This is to allow it to be enabled and disabled more easily on pause and other ui events.
    MouseLookManager mouseLookManager;
    SettingsManager settingsManager;

    [SerializeField] AbilityTree abilityTree;
    public AbilityTree AbilityTree { get { return abilityTree; } private set { abilityTree = value; } }
    [SerializeField] List<PlayerClass> classes;
    // [SerializeField] List<AbilityObject> playerAbilities;
    PlayerClass selectedClass;

    // A reference to the Map object, which handles the general level management.
    [SerializeField] LevelManager levelManager;
    // A reference to the current level, because caching is more efficient.
    Level level;
    PlayerData playerData;

    //A reference to the particle effect manager that handles all particle system work
    ParticleSystemController particleManager;

    // A reference to the UIManager instance, which is created at runtime, and handles all user interface actions.
    UIManager uiManager;
    TutorialManager tutorialManager;
    // A reference to the AudioManager instance, which is created at runtime, and handles all audio.
    AudioManager audioManager;
    public AudioManager AudioManager { get { return audioManager; } }
    AIManager aiManager;
    public MiniMapCam MiniMapCam { get; private set; }

    [SerializeField] BackgroundMusic titleMusic;

    // How long it takes an entity to move from one square to another.
    [SerializeField] float movespeed;
    [SerializeField] AnimationCurve moveCurve;
    [SerializeField] AnimationCurve turnCurve;
    // How long it takes an entity to turn 90 degrees.
    [SerializeField] float turnspeed;
    [SerializeField] float tickRate;
    [SerializeField] int enemyAggroDistance;
    [SerializeField] float interruptPercentage;

    // Whether or not we're in-game.
    bool inGame;
    bool paused;
    bool useTimeLimit;
    bool useMouse;
    float timeRemaining;
    Coroutine gradualEffectsCoroutine;
    Coroutine playerMovementCoroutine;

    // Properties: Because why not do things all proper-like.
    public LevelManager LevelManager
    {
        get { return levelManager; }
        private set { levelManager = value; }
    }

    public float Movespeed { get { return movespeed; } }

    public float Turnspeed { get { return turnspeed; } }

    public void NewGame()
    {
        List<int> tiers = new List<int>();
        List<int> indexes = new List<int>();
        foreach (AbilityObject ability in selectedClass.BaseAbilities)
        {
            tiers.Add(ability.Tier);
            indexes.Add(ability.Index);
        }
        playerData = new PlayerData
        {
            classIndex = classes.IndexOf(selectedClass),
            abilityTiers = tiers,
            abilityIndexes = indexes,
            cores = 0
        };
        tutorialManager.RunTutorial = true;
        LevelManager.SetCurrentLevel(0);
        LoadLevel(1f);
    }
    public void Continue()
    {
        playerData = settingsManager.LoadGame();
        if (playerData.classIndex < 0 || playerData.classIndex >= classes.Count)
        {
            Debug.LogError("ERROR: Loaded player does not have a selected class.");
            return;
        }

        selectedClass = classes[playerData.classIndex];
        tutorialManager.RunTutorial = false;
        LevelManager.SetCurrentLevel(0);
        LoadLevel(1f);
    }

    public void SaveGame()
    {
        if (level != null && level.Player != null)
        {
            List<int> tiers = new List<int>();
            List<int> indexes = new List<int>();
            foreach (AbilityObject ability in level.Player.Abilities)
            {
                tiers.Add(ability.Tier);
                indexes.Add(ability.Index);
                // indexes.Add(ability.Index);
            }
            PlayerData data = new PlayerData()
            {
                classIndex = classes.IndexOf(level.Player.Class),
                abilityTiers = tiers,
                abilityIndexes = indexes,
                cores = level.Player.Cores
            };
            settingsManager.SaveGame(data);
        }
    }

    public void RevertSettings()
    {
        settingsManager.LoadSettings();
        uiManager.SetAllSettingsElements(settingsManager);
    }

    public void ResetSettingsToDefault()
    {
        settingsManager = new SettingsManager();
        uiManager.SetAllSettingsElements(settingsManager);
    }

    public void ApplySettings()
    {
        Debug.Log("Applying player settings.");
        settingsManager.SetSettings(uiManager);

        // Gameplay
        ApplyAdjustedHealth();
        ApplyUseTimeLimit();

        // Graphics
        Resolution targetRes;
        if (settingsManager.ResolutionIndex > 0 && settingsManager.ResolutionIndex < uiManager.resolutions.Length)
            targetRes = uiManager.resolutions[settingsManager.ResolutionIndex];
        else if (uiManager.resolutions.Length > 0)
            targetRes = uiManager.resolutions[0];
        else
            targetRes = Screen.currentResolution;

        Screen.SetResolution(targetRes.width, targetRes.height, Screen.fullScreen, targetRes.refreshRate);

        Screen.fullScreen = settingsManager.Fullscreen;

        QualitySettings.masterTextureLimit = settingsManager.TextureQualityIndex;

        ApplyAntialiasingSettings();

        QualitySettings.vSyncCount = settingsManager.VSyncIndex;

        // 0 = -1 = Uncapped, 1 = 30, 2 = 60, 3 = 80, 4 = 120, 5 = 144
        if (settingsManager.FrameRateIndex == 0)
        {
            Application.targetFrameRate = -1;
        }
        else if (settingsManager.FrameRateIndex == 1)
        {
            Application.targetFrameRate = 30;
        }
        else if (settingsManager.FrameRateIndex == 2)
        {
            Application.targetFrameRate = 60;
        }
        else if (settingsManager.FrameRateIndex == 3)
        {
            Application.targetFrameRate = 80;
        }
        else if (settingsManager.FrameRateIndex == 4)
        {
            Application.targetFrameRate = 120;
        }
        else if (settingsManager.FrameRateIndex == 5)
        {
            Application.targetFrameRate = 144;
        }

        // Sound
        Debug.Log("Master volume: " + settingsManager.MasterVolume);
        float masterDBValue = 20f * Mathf.Log10(settingsManager.MasterVolume);
        float systemDBValue = 20f * Mathf.Log10(settingsManager.SystemVolume);
        float musicDBValue = 20f * Mathf.Log10(settingsManager.MusicVolume);
        float fxDBValue = 20f * Mathf.Log10(settingsManager.FXVolume);
        float ambientDBValue = 20f * Mathf.Log10(settingsManager.AmbientVolume);

        mixer.SetFloat("MasterVolume", masterDBValue);
        mixer.SetFloat("SystemVolume", systemDBValue);
        mixer.SetFloat("MusicVolume", musicDBValue);
        mixer.SetFloat("FXVolume", fxDBValue);
        mixer.SetFloat("AmbientVolume", ambientDBValue);

        // Controls
        mouseLookManager.SensitivityX = settingsManager.XSensitivity;
        mouseLookManager.SensitivityY = settingsManager.YSensitivity;

        settingsManager.SaveSettings();
    }

    void ApplyAdjustedHealth()
    {
        if (level != null && level.Player != null)
        {
            // Player player = level.Player;
            int adjustedMaxHealth = (int)(level.Player.Class.Health * settingsManager.HealthPercent);
            // float adjustedPerc = (player.CurrentHealth / (float)player.MaxHealth);
            // int adjustedCurrent = (int)(adjustedMaxHealth * adjustedPerc);

            // Debug.Log("Adjusting player max health of " + player.Class.Health + " by " + settingsManager.HealthPercent + " and getting " + adjustedMaxHealth);
            // Debug.Log("-- Adjust player current health of " + player.CurrentHealth + " to " + adjustedCurrent);
            level.Player.MaxHealth = adjustedMaxHealth;
            level.Player.CurrentHealth = adjustedMaxHealth;
        }
    }

    void ApplyUseTimeLimit()
    {
        useTimeLimit = (settingsManager.TimeLimit != 0f);
    }

    void ApplyAntialiasingSettings()
    {
        if (settingsManager.AntialiasingIndex == 0)
        {
            Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.None;
        }
        else if (settingsManager.AntialiasingIndex == 1)
        {
            Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
        }
        else if (settingsManager.AntialiasingIndex == 2)
        {
            Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.TemporalAntialiasing;
        }
        else if (settingsManager.AntialiasingIndex == 3)
        {
            Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
        }
        else if (settingsManager.AntialiasingIndex == 4)
        {
            Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
        }
        else if (settingsManager.AntialiasingIndex == 5)
        {
            Camera.main.GetComponent<PostProcessLayer>().antialiasingMode = PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing;
        }
    }

    // Called at the start of the game.
    // Iterates through the DoNotDestroyList List, instantiates each GameObject, it to DontDestroyOnLoad, and sets up any references necessary.
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
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
                        tutorialManager = gmInstance.GetComponent<TutorialManager>();
                        mouseLookManager = gmInstance.GetComponent<MouseLookManager>();
                    }
                    else if (gmInstance.GetComponent<AudioManager>() != null && audioManager == null)
                    {
                        audioManager = gmInstance.GetComponent<AudioManager>();
                    }
                    else if (gmInstance.GetComponent<AIManager>() != null && aiManager == null)
                    {
                        aiManager = gmInstance.GetComponent<AIManager>();
                    }
                    else if (gmInstance.GetComponent<MiniMapCam>() != null && MiniMapCam == null)
                    {
                        MiniMapCam = gmInstance.GetComponent<MiniMapCam>();
                    }
                    else if (gmInstance.GetComponent<ParticleSystemController>() != null && particleManager == null)
                    {
                        particleManager = gmInstance.GetComponent<ParticleSystemController>();
                    }
                }
            }
            uiManager.Initialize();
            SetUpClassMenu();

            settingsManager = new SettingsManager();
            settingsManager.LoadSettings();
            // Save it after attempting to load, just to generate the config file on the first play.
            settingsManager.SaveSettings();
        }
    }

    // Nothing yet.
    void Start()
    {
        // Have to do this hear instead of in OnLevelLoaded or Awake because AudioMixer.SetFloat doesn't work in either method.
        // Because Unity is the best.
        ApplySettings();
    }

    // Executes every frame.
    // If a level has not been loaded and setup (inGame), then just return out.
    // Otherwise, update the player's hud and handle the player's input.
    void Update()
    {
        if (inGame)
        {
            if (useTimeLimit)
            {
                timeRemaining = Mathf.Clamp(timeRemaining - Time.deltaTime, 0f, settingsManager.TimeLimit);
                // If the player is out of time. KILL THEM.
                if (timeRemaining <= 0)
                {
                    if (level != null && level.Player != null)
                        PerformEntityDeathCheck(level.Player, false);
                }
            }
            // Update all HUD elements.
            UpdateHUD();

            if (tutorialManager.InTutorial)
            {
                HandleTutorialInput();
            }
            else
            {
                if (mouseLookManager.enabled)
                {
                    if (mouseLookManager.RestrictDirection != Direction.Null)
                    {
                        if (level.Player.Facing != mouseLookManager.RestrictDirection)
                            level.Player.Facing = mouseLookManager.RestrictDirection;
                    }
                    else
                    {
                        Direction facingDirection = mouseLookManager.GetDirectionFacing();
                        if (facingDirection != Direction.Null)
                        {
                            level.Player.Facing = facingDirection;
                        }
                    }
                }
                // Get player input and do stuff.
                HandlePlayerInput();
                // Let the enemy's do stuff.
                HandleEnemyAI();
            }
        }
        else
        {
            // Updates title screen elements.
            UpdateUI();
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

    public void SetUpClassMenu()
    {
        uiManager.SetConfirmButtonActive(false);
        uiManager.SetUpClassButtons(classes);
        if (classes.Count > 0)
        {
            SelectClass(classes[0]);
        }
    }

    public void SelectClass(PlayerClass playerClass)
    {
        if (playerClass == null)
            return;

        selectedClass = playerClass;
        uiManager.SelectClass(selectedClass);
        uiManager.SetConfirmButtonActive(true);
    }

    public void LoadMainMenu(float delay = 0f)
    {
        uiManager.FadeOut("Loading...", delay);
        audioManager.FadeOutMusic(delay);
        StartCoroutine(LoadMainMenu_Coroutine(delay));
    }

    IEnumerator LoadMainMenu_Coroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (inGame && level != null)
        {
            foreach (Enemy enemy in level.EnemyList)
            {
                enemy.Kill();
            }
        }

        inGame = false;
        int sceneIndex = 0;
        level = null;
        LevelManager.Reset();
        SceneManager.LoadScene(sceneIndex);
    }

    // Loads a level and scene with a delay in seconds.
    public void LoadLevel(float delay = 0f)
    {
        uiManager.FadeOut("Loading...", delay);
        audioManager.FadeOutMusic(delay);
        StartCoroutine(LoadLevel_Coroutine(delay));
    }

    // The coroutine for loading a level scene, waits the delay, then gets the current level's sceneIndex and loads the scene.
    IEnumerator LoadLevel_Coroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerMovementCoroutine != null)
            StopCoroutine(playerMovementCoroutine);
        if (inGame && level != null)
        {
            foreach (Enemy enemy in level.EnemyList)
            {
                enemy.Kill();
            }
        }

        int sceneIndex = LevelManager.CurrentLevel.sceneIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    // Main setup method. Executes when a scene is loaded.
    // Gets a reference to the current level from the Map and assigns it to the local level variable. If it is null, it initializes the level as the main menu, including the HUD.
    // If not null, initializes the level, generates the level, and adds in the player entity.
    //  Also calls the intialization for the HUD, displays the level text, and updates the appropriate UI.
    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        // FIXME: When an ability executed right as the level transitions, an exception is thrown and the game stops working.
        Player player = null;
        if (level != null && player == null)
        {
            player = level.Player;
        }
        // Check to see if we're loading a level.
        level = LevelManager.CurrentLevel;

        // If there is a level to load, load it in.
        // Otherwise, we're loading the title screen.
        if (level != null)
        {
            ApplyUseTimeLimit();
            // If it's the hub, don't use a time limit.
            if (scene.buildIndex == 1)
            {
                useTimeLimit = false;
            }
            timeRemaining = settingsManager.TimeLimit;
            AbilityTree.Initialize(selectedClass);
            // Sets up cells, connections, player spawn, and generates procedural areas.
            level.InitializeLevel(player, playerData, selectedClass);
            player = level.Player;
            ApplyAdjustedHealth();

            AbilityTree.Player = player;

            GameObject levelContainer = new GameObject("_Level");
            BuildLevel_Procedural(levelContainer);
            BuildLevel_Procedural_Corners(levelContainer);

            // Create the player. Set the instance to a new instantiated playerPrefab.
            player.Instance = GameObject.Instantiate(player.Class.ClassCamera);
            mouseLookManager.SetTarget(player.Instance);
            // Manually set the position.
            SetEntityInstanceLocation(player);
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
            uiManager.SetUpAbilityIcons(player);
            uiManager.SetUpAbilityTreeMenu(AbilityTree);
            // Display the area text.
            uiManager.DisplayAreaText(level.name);
            uiManager.SetTimeTextActive(useTimeLimit);
            // Fade in with the area name.
            uiManager.FadeIn(level.name, 2f);
            uiManager.UpdatePlayerHealth(player.CurrentHealth / player.MaxHealth);

            audioManager.FadeInMusic(level.music, 1f);

            inGame = true;
            if (gradualEffectsCoroutine != null)
                StopCoroutine(gradualEffectsCoroutine);
            gradualEffectsCoroutine = StartCoroutine(ApplyGradualEffects_Coroutine());

            if (tutorialManager.RunTutorial && !tutorialManager.Introduction.Complete)
                tutorialManager.RunIntroduction();
        }
        else
        {
            // Do stuff to load main menu.
            inGame = false;
            uiManager.Initialize_Main();
            if (settingsManager.LoadGame() != null)
                uiManager.SetContinueButtonActive(true);
            else
                uiManager.SetContinueButtonActive(false);

            uiManager.SetAllSettingsElements(settingsManager);
            uiManager.FadeIn("Hydrus", 3f);
            audioManager.FadeInMusic(titleMusic, 0f);
        }
        ApplyAntialiasingSettings();
    }

    void BuildLevel_Procedural(GameObject levelContainer)
    {
        if (levelContainer == null)
        {
            levelContainer = new GameObject("_LevelCells");
        }
        float cellScale = LevelManager.CellScale;
        foreach (Cell cell in level.cells)
        {
            // TODO: Switch to cell.Type == CellType.Procedural, instead of != CellType.Empty.
            // Currently generates for all cells, but we may want to only generate for the procedural sections,
            // and manually place the static areas.
            if (cell.Type != CellType.Empty && level.HasConnections(cell))
            {
                Vector3 center = LevelManager.GetCellPosition(cell);
                GameObject cellInstance = new GameObject("Cell_" + cell.Index);
                cellInstance.transform.SetParent(levelContainer.transform);
                cellInstance.transform.position = center;
                GameObject.Instantiate(LevelManager.GetRandomPrefab(floorPrefabs), cellInstance.transform);
                GameObject.Instantiate(LevelManager.GetRandomPrefab(ceilingPrefabs), cellInstance.transform);
                List<GameObject> wallInstances = new List<GameObject>();
                for (int w = 0; w < 4; w++)
                {
                    GameObject wallInstance;
                    if (cell.Type != CellType.Exit)
                    {
                        wallInstance = GameObject.Instantiate(LevelManager.GetRandomPrefab(wallPrefabs), cellInstance.transform);
                    }
                    else
                    {
                        // Debug.Log("Connecton cell! Adding a door prefab to all walls.");
                        wallInstance = GameObject.Instantiate(LevelManager.GetRandomPrefab(doorPrefabs), cellInstance.transform);
                    }
                    wallInstance.name = "Wall_" + ((Direction)w).ToString();
                    wallInstance.transform.rotation = Quaternion.Euler(0f, 90f * w, 0f);
                    wallInstances.Add(wallInstance);
                }

                cellInstance.transform.localScale = Vector3.one * cellScale;

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

    void BuildLevel_Procedural_Corners(GameObject levelContainer)
    {
        if (levelContainer == null)
        {
            levelContainer = new GameObject("_LevelCorners");
        }

        float cellScale = LevelManager.CellScale;
        Vector3 offset = new Vector3(0.5f * cellScale, 0f, 0.5f * cellScale);
        // Iterate through each corner. Of cell up left, up right, down left, down right,
        //      IF, none are connected and at least one has connections, create CORNER
        //      OR, two are connected (one connection), create WALL CONNECTION
        //      OR, three are connected (two connections), create CORNER
        //      OR, four are connected (three connections), create CORNER
        //      OR, are all connected (four connections), do not create CORNER

        // Iterating through corners. Corner would be +0.5,+0.5. Actually iterating through down-left cell indices.
        for (int x = -1; x < LevelManager.MaxWidth; x++)
        {
            for (int z = -1; z < LevelManager.MaxDepth; z++)
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
                        Vector3 center = LevelManager.GetCellPosition(x, z) + offset;
                        GameObject cornerInstance = GameObject.Instantiate(LevelManager.GetRandomPrefab(cornerPrefabs), center, Quaternion.identity, levelContainer.transform);
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
                        Vector3 center = LevelManager.GetCellPosition(x, z) + offset;
                        GameObject cornerInstance = GameObject.Instantiate(LevelManager.GetRandomPrefab(cornerPrefabs), center, Quaternion.identity, levelContainer.transform);
                        // GameObject cornerInstance = GameObject.Instantiate(cornerPrefabDebug, center, Quaternion.identity);
                        cornerInstance.transform.localScale = cornerInstance.transform.localScale * cellScale;
                    }
                }
                else if (numConnections == 3)
                {
                    // Create corner
                    Vector3 center = LevelManager.GetCellPosition(x, z) + offset;
                    GameObject cornerInstance = GameObject.Instantiate(LevelManager.GetRandomPrefab(cornerPrefabs), center, Quaternion.identity, levelContainer.transform);
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
        float cellScale = LevelManager.CellScale;
        // Main iteration.
        foreach (Cell cell in level.cells)
        {
            // If it's not empty and has connections.
            if (cell.Type != CellType.Empty && level.HasConnections(cell))
            {
                // Gets the correct positions, sizes, and colors and sets up the block with the determined values.
                Vector3 center = LevelManager.GetCellPosition(cell);
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

    void UpdateUI()
    {
        uiManager.UpdateSettingsElements(settingsManager);
    }

    // General update HUD method.
    void UpdateHUD()
    {
        Player player = level.Player;
        // If standing at an exit, give option to go through exit.
        ExitPrompt(level.CanExit);
        uiManager.UpdateTimeText(timeRemaining);
        uiManager.UpdatePlayerCores(player.Cores);
        uiManager.UpdateEffectList(player.StatusEffects);
        //uiManager.UpdatePlayerAbilityHUD(player.Cooldowns.Values.ToList(), player.CooldownsRemaining.Values.ToList(), player.CurrentAbility, player.CastProgress);
        uiManager.UpdatePlayerAbilityHUD(player.GetCooldownsList(), player.GetCooldownRemainingList(), player.CurrentAbility, player.CastProgress);

        if (uiManager.Paused)
        {
            uiManager.UpdateHUDSettingsElements(settingsManager);
        }

        if (player.Cell.Indicators.Count > 0 && !uiManager.Highlighted)
        {
            // Debug.Log("Player cell is being targeted.");
            uiManager.ToggleBorderHighlight();
            // uiManager.HighlightBorder();
        }
        else if (player.Cell.Indicators.Count <= 0 && uiManager.Highlighted)
        {
            // Debug.Log("Player cell is NOT being targeted.");
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
        Player player = level.Player;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiManager.TogglePause();
        }
        if (!uiManager.Paused)
        {
            if (Input.GetKeyDown(settingsManager.MapMenuKey))
            {
                uiManager.ToggleMap();
            }
            else if (Input.GetKeyDown(settingsManager.TreeMenuKey))
            {
                uiManager.ToggleTree();
            }

            if (!uiManager.ShowingTree && !uiManager.ShowingMap && player.State == EntityState.Idle && !player.StatusEffects.Stunned)
            {
                Direction inputDir = GetInputDirection();
                if (Input.GetKeyDown(settingsManager.InteractKey) && level.CanExit)
                {
                    ExitLevel();
                }

                if (inputDir != Direction.Null)
                {
                    MoveEntityLocation(player, inputDir);
                    //                MoveEntityInstance(player, inputDir);
                }
                else if (Input.GetKey(settingsManager.TurnLeftKey))
                {
                    TurnEntityInstanceLeft(player);
                }
                else if (Input.GetKey(settingsManager.TurnRightKey))
                {
                    TurnEntityInstanceRight(player);
                }

                if (Input.GetKey(settingsManager.Ability1Key))
                {
                    CastPlayerAbility(player, 0);
                }
                else if (Input.GetKey(settingsManager.Ability2Key))
                {
                    CastPlayerAbility(player, 1);
                }
                else if (Input.GetKey(settingsManager.Ability3Key))
                {
                    CastPlayerAbility(player, 2);
                }
                else if (Input.GetKey(settingsManager.Ability4Key))
                {
                    CastPlayerAbility(player, 3);
                }
                else if (Input.GetKeyDown(KeyCode.Minus))
                {
                    bool alive = player.Damage(25, (player.CastProgress >= interruptPercentage));
                    PerformEntityDeathCheck(player, alive);

                    uiManager.UpdatePlayerHealth(player.CurrentHealth / player.MaxHealth);
                }
                else if (Input.GetKeyDown(KeyCode.Equals))
                {
                    player.Heal(25);
                    uiManager.UpdatePlayerHealth(player.CurrentHealth / player.MaxHealth);
                }
            }
        }
    }

    void HandleTutorialInput()
    {
        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape))
        {
            tutorialManager.NextScreen();
            if (tutorialManager.Current.Complete)
            {
                if (tutorialManager.Current == tutorialManager.Introduction)
                    tutorialManager.RunMovementTutorial();
                else if (tutorialManager.Current == tutorialManager.Movement)
                    tutorialManager.RunCombatTutorial();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            TutorialManager.SectionDone.Invoke();
            tutorialManager.RunTutorial = false;
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
                bool alive = player.Damage(player.StatusEffects.DamageRate * tickRate);
                uiManager.UpdatePlayerHealth(player.CurrentHealth / player.MaxHealth);
                PerformEntityDeathCheck(player, alive);
            }

            foreach (Enemy enemy in level.EnemyList)
            {
                if (enemy.StatusEffects.HealRate > 0f)
                    enemy.Heal(enemy.StatusEffects.HealRate * enemy.MaxHealth * tickRate);

                if (enemy.StatusEffects.DamageRate > 0f)
                {
                    bool alive = enemy.Damage(enemy.StatusEffects.DamageRate * tickRate);
                    PerformEntityDeathCheck(enemy, alive);
                }
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
        LevelManager.NextLevel(level.Player.Cell);
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
        // Player player = level.Player;
        Cell neighbor = level.GetDestination(entity, direction);
        if (neighbor == null)
            return;

        if (entity.StatusEffects.Stunned || entity.StatusEffects.Rooted)
            return;

        entity.State = EntityState.Moving;

        // float adjustedMovespeed = Movespeed / entity.StatusEffects.MovementScale;
        float adjustedMovespeed = entity.GetAdjustedMoveSpeed(Movespeed);

        //Probably going to make a separate method to handle all this.
        Tween.Position(entity.Instance.transform, LevelManager.GetCellPosition(neighbor), adjustedMovespeed, 0f, moveCurve, completeCallback: () => entity.State = EntityState.Idle);
        Coroutine movementCoroutine = StartCoroutine(MoveEntityLocation_Coroutine(entity, neighbor, adjustedMovespeed * 0.75f));
        if (entity.IsPlayer)
        {
            playerMovementCoroutine = movementCoroutine;
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
        // entity.RemoveMovementCoroutine();
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
        Tween.Rotate(entity.Instance.transform, new Vector3(0f, -90f, 0f), Space.World, Turnspeed, 0f, turnCurve, completeCallback: (() => FinishTurning(entity)));
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
        Tween.Rotate(entity.Instance.transform, new Vector3(0f, 90f, 0f), Space.World, Turnspeed, 0f, turnCurve, completeCallback: (() => FinishTurning(entity)));
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
        eTransform.position = LevelManager.GetCellPosition(entity.Cell);
        eTransform.rotation = Quaternion.Euler(0f, 90f * (int)entity.Facing, 0f);
    }

    void CastPlayerAbility(Entity entity, int index)
    {
        if (entity.StatusEffects.Stunned || entity.StatusEffects.Silenced)
            return;

        AbilityObject ability = entity.CastAbility(index);
        if (ability == null)
            return;

        mouseLookManager.RestrictDirection = entity.Facing;

        uiManager.UpdatePlayerCast(entity.CurrentCastTime);
        // Get the cells to highlight and display them.
        List<Cell> affected = level.GetAffectedCells_Highlight(entity, ability);
        foreach (Cell cell in affected)
        {
            AddIndicator(testIndicatorPlayer, cell, entity);
        }
        //Setting the cast time scale to the current cast time scale... Blegh
        SetPlayerCastAnimation("Cast", level.Player.Abilities[index].CastTime);
    }

    public void CancelPlayerAbility()
    {
        uiManager.CancelPlayerCast();
        mouseLookManager.RestrictDirection = Direction.Null;
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
            AddIndicator(testIndicatorEnemy, cell, entity);
        }
    }

    public void PerformAbility(Entity entity, AbilityObject ability)
    {
        if (entity == null || entity.Instance == null || ability == null)
            return;

        // Play sounds and animations.
        // Deal damage to entities in the cells.

        if (ability.SoundEffect != null)
        {
            audioManager.PlaySoundEffect(new SoundEffect(ability.SoundEffect, entity.Instance.transform.position));
        }

        if (entity.IsPlayer == true)
        {
            mouseLookManager.RestrictDirection = Direction.Null;
            SetPlayerAnimation("CastActivate", 1.0f);
        }

        List<Cell> affected = level.GetAffectedCells(entity, ability);
        // Debug.Log(entity.Name + " casting " + ability.SoundEffect);

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
        // Debug.Log("-- Affecting " + target.Name + " .. Dealing " + ability.Damage + " damage.");

        foreach (AbilityEffect effect in ability.StatusEffects)
        {
            AbilityEffect effectInstance = new AbilityEffect(caster.Index, effect.Effect, effect.Duration, effect.Value);
            target.StatusEffects.AddEffect(effectInstance);
        }
        bool alive = target.Damage(ability.Damage, (target.CastProgress >= interruptPercentage));

        if (target.IsPlayer)
        {
            uiManager.UpdatePlayerHealth(target.CurrentHealth / target.MaxHealth);
            if (ability.Damage > 0)
            {
                uiManager.FlashPlayerDamage();
                target.Instance.GetComponentInChildren<ShakeTransform>().AddShakeEvent(testShakeEvent);
            }
        }
        else if (ability.Type != AbilityType.Self && target.IsPlayer == false)
        {
            particleManager.PlayHitSpark(target);
        }

        PerformEntityDeathCheck(target, alive);
    }

    void ApplyZoneAbility(Entity target, AbilityObject ability, Entity caster)
    {
        foreach (AbilityEffect effect in ability.StatusEffects)
        {
            AbilityEffect effectInstance = null;
            if (effect.Effect == AbilityStatusEff.Heal || effect.Effect == AbilityStatusEff.DoT)
            {
                effectInstance = new AbilityEffect(caster.Index, effect.Effect, tickRate, (effect.Value / effect.Duration) * tickRate);
            }
            else
            {
                effectInstance = new AbilityEffect(caster.Index, effect.Effect, tickRate, effect.Value);
            }
            target.StatusEffects.AddEffect(effectInstance);
        }
        // bool alive = target.Damage(0);
        // // if (target.GetType() == typeof(Player))
        // if (target.IsPlayer)
        // {
        //     uiManager.UpdatePlayerHealth(target.CurrentHealth / target.MaxHealth);
        // }

        // PerformEntityDeathCheck(target, alive);
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
        Player player = level.Player;
        if (!alive && entity.IsPlayer)
        {
            inGame = false;
            // Do player death stuff.
            if (playerMovementCoroutine != null)
                StopCoroutine(playerMovementCoroutine);

            player.State = EntityState.Null;
            player.Cores = 0;
            player.CurrentHealth = player.MaxHealth;
            if (gradualEffectsCoroutine != null)
                StopCoroutine(gradualEffectsCoroutine);
            audioManager.FadeOutMusic(1f);
            LevelManager.SetCurrentLevel(0);
            LoadLevel(0.5f);
        }
        else if (!alive && !entity.IsPlayer)
        {
            player.Cores += entity.Cores;
            // Regenerate 20% of missing health on kill.
            player.Heal((player.MaxHealth - player.CurrentHealth) * 0.2f);
            uiManager.UpdatePlayerHealth(player.CurrentHealth / player.MaxHealth);

            if (tutorialManager.RunTutorial && !tutorialManager.Upgrade.Complete)
            {
                if (player.Cores < 75) player.Cores = 75;
                tutorialManager.RunUpgradeTutorial();
            }
            //Play Core collection VFX
            //particleManager.PlayCoreGather(entity);
            //Play Dissolve Effect
            //particleManager.DissolveEnemy(entity);
            DestroyEnemy(entity);
            StartCoroutine(level.RemoveEntity(entity));

        }
    }

    public void DestroyEnemy(Entity target)
    {
        particleManager.PlayCoreGather(target);
        particleManager.DissolveEnemy(target, level);
    }

    public void AddIndicator(GameObject indicatorPrefab, Cell cell, Entity entity)
    {
        GameObject indicatorInstance = GameObject.Instantiate(indicatorPrefab, LevelManager.GetCellPosition(cell), indicatorPrefab.transform.rotation);
        indicatorInstance.transform.localScale *= LevelManager.CellScale;
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

    // public AbilityObject GetPlayerAbility(int index)
    // {
    //     return playerAbilities.SingleOrDefault((abil) => abil.Index == index);
    // }
}
