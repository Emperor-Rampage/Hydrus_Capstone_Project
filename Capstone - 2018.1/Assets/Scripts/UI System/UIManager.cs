using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Pixelplacement;
using Pixelplacement.TweenSystem;
using TMPro;

using MapClasses;
using AbilityClasses;
using EntityClasses;
using AudioClasses;

// TODO: Use an enum to define ui sounds. Store them in a dictionary with the enum as the key and the SoundEffect as the value.
// TODO: Pass the enum in each ui element's event trigger.

[RequireComponent(typeof(Canvas), typeof(TutorialManager))]
public class UIManager : MonoBehaviour
{
    GameManager manager;
    TutorialManager tutorialManager;

    [Header("Main Settings")]
    // Serialized fields.
    [SerializeField]
    CanvasGroup fadePanel;
    [SerializeField] const float defaultFadeTime = 1f;
    AnimationCurve defaultFadeCurve = Tween.EaseInOutStrong;
    [SerializeField] TMP_Text fadeText;
    [SerializeField] ParticleSystem highlightUIPrefab;
    // TODO: Add support for highlighting ui elements with highlight particle system.

    [Header("Sound Settings")]
    [SerializeField]
    UISound defaultHoverSound = null;
    [SerializeField] UISound defaultClickSound = null;

    [Space(2)]
    [Header("Menu Settings")]
    [SerializeField]
    GameObject menuPanel;
    [SerializeField] GameObject[] menuList;
    [SerializeField] Button continueButton;

    [Header("Class Select Menu Settings")]
    [SerializeField]
    GameObject classMenu;
    [SerializeField] GameObject classButtonPrefab;
    [SerializeField] TMP_Text classNameText;
    [SerializeField] TMP_Text classDescriptionText;
    [SerializeField] TMP_Text[] abilityNameTexts;
    [SerializeField] TMP_Text[] abilityTypeTexts;
    [SerializeField] Image[] abilityIconImages;
    [SerializeField] TMP_Text[] abilityDescriptionTexts;
    [SerializeField] Button confirmButton;

    [Space(2)]
    [Header("HUD Settings")]
    [SerializeField]
    GameObject hudPanel;
    [SerializeField] GameObject[] hudList;
    [SerializeField] CanvasGroup screenHighlightGroup;
    [SerializeField] CanvasGroup screenDamageGroup;

    [SerializeField] TMP_Text areaText;
    [SerializeField] TMP_Text timeLimitText;
    [SerializeField] TMP_Text promptText;

    [SerializeField] TMP_Text coresText;
    [SerializeField] TMP_Text effectTextPrefab;
    [SerializeField] VerticalLayoutGroup effectGroup;

    [SerializeField] Image playerHealthBar;
    [SerializeField] Image playerHealthMissing;
    [SerializeField] Image playerCastBackBar;
    [SerializeField] Image playerCastBackInterruptBar;
    [SerializeField] Image playerCastInterruptImage;
    [SerializeField] Image playerCastBar;

    [SerializeField] GameObject playerAbilityPanel;
    [SerializeField] GameObject playerAbilityIconPrefab;
    [SerializeField] RawImage playerMiniMap;

    [SerializeField] GameObject enemyInfoPanel;
    [SerializeField] TMP_Text enemyNameText;
    [SerializeField] Image enemyHealthBar;
    [SerializeField] Image enemyCastBackBar;
    [SerializeField] Image enemyCastBackInterruptBar;
    [SerializeField] Image enemyCastBar;
    [SerializeField] TMP_Text enemyCastText;

    // HUD Settings Menu
    [SerializeField] SettingsContainer hudSettingsContainer;
    public SettingsContainer HUDSettings { get { return hudSettingsContainer; } }
    int currentHUDSettingsTab;

    [Header("Ability Tree Settings")]
    [SerializeField] TMP_Text treeCoresText;
    [SerializeField] AbilityInfoContainer abilityInfoContainer;
    [SerializeField] RectTransform abilityTreeContentPanel;
    [SerializeField] GameObject abilityTreePrefab;
    [SerializeField] GameObject abilityTierPrefab;
    [SerializeField] GameObject abilityVariantPrefab;
    [SerializeField] GameObject abiltiyLinePrefab;
    AbilityTree abilityTree;
    List<TreeAbility> treeAbilities = new List<TreeAbility>();

    // Private fields.
    int currentMenu;
    int currentHUD;
    [Header("Overlay Map Settings")]
    [SerializeField]
    TMP_Text mapText;
    [SerializeField] float miniMapZoom;
    [SerializeField] float overlayMapZoom;

    [Header("Settings Menu Items")]
    [SerializeField]
    SettingsContainer mainSettingsContainer;
    public SettingsContainer MainMenuSettings { get { return mainSettingsContainer; } }
    int currentSettingsTab;

    // Private fields.
    [HideInInspector] public Resolution[] resolutions;


    List<TMP_Text> effectTextList = new List<TMP_Text>();

    List<GameObject> playerAbilityIcons = new List<GameObject>();

    TweenBase screenHighlightTween;
    TweenBase screenDamageTween;
    TweenBase playerHealthBarTween;
    TweenBase playerCastBarTween;

    public bool Paused { get; private set; }
    public bool ShowingTree { get; private set; }
    public bool ShowingMap { get; private set; }
    public bool Highlighted { get; private set; } = false;


    // TODO: Set up UI to run animations during pause (timeScale = 0)
    //       using https://docs.unity3d.com/ScriptReference/AnimatorUpdateMode.UnscaledTime.html

    // FIXME: During transitions, the user can still continue clicking UI elements, such as in the main menu.
    //        Should be locked out of taking other actions.

    public void Initialize()
    {
        manager = GameManager.Instance;

        // Connect settings buttons with GameManager methods.
        mainSettingsContainer.backButton.onClick.AddListener(manager.RevertSettings);
        mainSettingsContainer.resetButton.onClick.AddListener(manager.ResetSettingsToDefault);
        mainSettingsContainer.applyButton.onClick.AddListener(manager.ApplySettings);

        // Connect hud settings buttons with GameManager methods.
        hudSettingsContainer.backButton.onClick.AddListener(manager.RevertSettings);
        hudSettingsContainer.resetButton.onClick.AddListener(manager.ResetSettingsToDefault);
        hudSettingsContainer.applyButton.onClick.AddListener(manager.ApplySettings);
    }
    public void Initialize_Main()
    {
        resolutions = Screen.resolutions;

        mainSettingsContainer.resolutionDropdown.ClearOptions();
        hudSettingsContainer.resolutionDropdown.ClearOptions();
        for (int r = 0; r < resolutions.Length; r++)
        {
            mainSettingsContainer.resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolutions[r].ToString()));
            hudSettingsContainer.resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(resolutions[r].ToString()));
        }
        mainSettingsContainer.resolutionDropdown.RefreshShownValue();
        hudSettingsContainer.resolutionDropdown.RefreshShownValue();

        if (screenHighlightTween != null)
            screenHighlightTween.Stop();
        screenHighlightTween = null;
        screenHighlightGroup.alpha = 0f;
        if (screenDamageTween != null)
            screenDamageTween.Stop();
        screenDamageTween = null;
        screenDamageGroup.alpha = 0f;

        AllButtons(menuPanel, true);
        GoToMenu(0);
    }

    public void Initialize_Level(float interruptPercent = 0f)
    {
        playerCastBackBar.fillAmount = interruptPercent;
        playerCastBackInterruptBar.fillAmount = 1f - interruptPercent;
        enemyCastBackBar.fillAmount = interruptPercent;
        enemyCastBackInterruptBar.fillAmount = 1f - interruptPercent;

        if (screenHighlightTween != null)
            screenHighlightTween.Stop();
        screenHighlightTween = null;
        screenHighlightGroup.alpha = 0f;
        if (screenDamageTween != null)
            screenDamageTween.Stop();
        screenDamageTween = null;
        screenDamageGroup.alpha = 0f;

        AllButtons(hudPanel, true);
        ShowHUD(0);
    }

    void AllButtons(GameObject parent, bool interactable = false)
    {
        Button[] buttons = parent.transform.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            button.interactable = interactable;
        }
    }

    public void SetUpClassButtons(List<PlayerClass> playerClasses)
    {
        // Debug.Log("Adding " + playerClasses.Count + " class buttons.");
        foreach (PlayerClass playerClass in playerClasses)
        {
            GameObject buttonObject = GameObject.Instantiate(classButtonPrefab, classMenu.transform, false);
            buttonObject.GetComponentInChildren<TMP_Text>().text = playerClass.Name;
            buttonObject.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.SelectClass(playerClass));

            EventTrigger trigger = buttonObject.GetComponent<EventTrigger>();

            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((data) => OnButtonHover());
            trigger.triggers.Add(enterEntry);

            EventTrigger.Entry clickEntry = new EventTrigger.Entry();
            clickEntry.eventID = EventTriggerType.PointerClick;
            clickEntry.callback.AddListener((data) => OnButtonClick());
            trigger.triggers.Add(clickEntry);
        }
    }

    public void SetUpAbilityIcons(Player player)
    {
        // First, delete all children of the panel.
        //        playerAbilityIcons.Clear();
        playerAbilityIcons = new List<GameObject>();
        foreach (Transform child in playerAbilityPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (AbilityObject ability in player.Abilities)
        {
            GameObject abilityIconObject = GameObject.Instantiate(playerAbilityIconPrefab, playerAbilityPanel.transform);
            PlayerHUDAbility container = abilityIconObject.GetComponent<PlayerHUDAbility>();
            if (container != null)
            {
                container.Icon.sprite = ability.Icon;
            }
            // ParticleSystem highlight = ParticleSystem.Instantiate(highlightUIPrefab, abilityIconObject.GetComponent<RectTransform>().position, Quaternion.identity, gameObject.transform);
            // highlight.transform.SetAsFirstSibling();
            playerAbilityIcons.Add(abilityIconObject);
        }
        UpdatePlayerAbilityHUD(player.GetCooldownsList(), player.GetCooldownRemainingList(), player.CurrentAbility, player.CastProgress);
    }

    public void SetUpAbilityTreeMenu(AbilityTree tree)
    {
        foreach (Transform child in abilityTreeContentPanel.transform)
        {
            Destroy(child.gameObject);
        }
        treeAbilities.Clear();
        // TODO: Switch to a panel for each ability with a vertical layout group.
        //       Populate with panels with horizontal layout groups with each ability's tier.
        abilityTree = tree;

        LayoutElement layoutElement = abilityTreeContentPanel.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = tree.Width;
        layoutElement.preferredHeight = tree.Height;

        HorizontalLayoutGroup horizontalLayoutGroup = abilityTreeContentPanel.GetComponent<HorizontalLayoutGroup>();
        horizontalLayoutGroup.spacing = tree.Spacing;
        horizontalLayoutGroup.padding.top = tree.Padding;
        horizontalLayoutGroup.padding.right = tree.Padding;
        horizontalLayoutGroup.padding.bottom = tree.Padding;
        horizontalLayoutGroup.padding.left = tree.Padding;

        // For each of the abilities. Create a panel with a VerticalLayoutGroup for each.
        //  For each tier of each ability. Create a panel with a HorizontalLayoutGroup for each.
        //   For each ability in the tier. Create an instance of the icon prefab and set the icon.

        for (int a = 0; a < tree.Player.Class.BaseAbilities.Count; a++)
        {
            GameObject abilityPanel = GameObject.Instantiate(abilityTreePrefab, abilityTreeContentPanel);
            // TODO: Switch to calculating the width. Should be number of leaves + any padding and spacing.
            abilityPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 0f);

            for (int t = tree.NumTiers - 1; t >= 0; t--)
            {

                GameObject tierPanel = GameObject.Instantiate(abilityTierPrefab, abilityPanel.transform);
                // TODO: Switch to calculating the height. Should be cell height.
                tierPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 100f);

                var abilityTier = tree.GetAbilityTier(a, t);

                for (int i = 0; i < abilityTier.Count; i++)
                {
                    AbilityObject ability = abilityTier[i];

                    GameObject cell = GameObject.Instantiate(abilityVariantPrefab, tierPanel.transform);
                    cell.GetComponent<RectTransform>().sizeDelta = new Vector2(tree.CellWidth, tree.CellHeight);
                    cell.name = "TreeIcon_" + ability.Name;

                    TreeAbility container = cell.GetComponent<TreeAbility>();
                    container.Tier = ability.Tier;
                    container.Index = ability.Index;
                    // container.Index = abilityTree.GetTreeAbilityIndex(ability.Tier - 1, ability);
                    container.Icon.sprite = ability.Icon;

                    if (tree.IsCurrentAbility(ability) || tree.IsAvailable(ability))
                    {
                        container.DimObject.SetActive(false);
                    }
                    else
                    {
                        container.DimObject.SetActive(true);
                    }

                    EventTrigger trigger = cell.GetComponent<EventTrigger>();

                    EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                    enterEntry.eventID = EventTriggerType.PointerEnter;
                    enterEntry.callback.AddListener((data) => OnTreeAbilityHover((PointerEventData)data));
                    trigger.triggers.Add(enterEntry);

                    EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                    exitEntry.eventID = EventTriggerType.PointerExit;
                    exitEntry.callback.AddListener((data) => OnTreeAbilityUnHover((PointerEventData)data));
                    trigger.triggers.Add(exitEntry);

                    EventTrigger.Entry clickEntry = new EventTrigger.Entry();
                    clickEntry.eventID = EventTriggerType.PointerClick;
                    clickEntry.callback.AddListener((data) => OnTreeAbilityClicked((PointerEventData)data));
                    trigger.triggers.Add(clickEntry);


                    // cell.transform.GetChild(0).GetComponent<Image>().sprite = ability.Icon;

                    treeAbilities.Add(container);
                    // tree.AddAbilityUI(t, i, cell);
                    // tree.AddAbilityUI(t, tree.GetTreeAbilityIndex(t, ability), cell);
                }

            }
        }

        foreach (TreeAbility treeAbility in treeAbilities)
        {
            AbilityObject ability = abilityTree.GetTreeAbility(treeAbility.Tier, treeAbility.Index);
            foreach (AbilityObject nextAbility in ability.NextTier)
            {
                TreeAbility nextTreeAbility = treeAbilities.FirstOrDefault((tAbil) => tAbil.Index == nextAbility.Index && tAbil.Tier == nextAbility.Tier);
            }
        }

        // For each base ability.
        //  For each tier of the ability
        //   If there is a next tier,
        //    compare with each of the next abilities.
        //    If contained
        //     Get both UI elements and draw a line between them.

        // for (int a = 0; a < tree.Player.Class.BaseAbilities.Count; a++)
        // {
        //     for (int t = 0; t < tree.NumTiers; t++)
        //     {

        //         var abilityTier = tree.GetAbilityTier(a, t);

        //         var nextTier = tree.GetAbilityTier(a, t + 1);

        //         if (nextTier != null)
        //         {
        //             for (int i = 0; i < abilityTier.Count; i++)
        //             {
        //                 AbilityObject ability = abilityTier[i];
        //                 for (int n = 0; n < nextTier.Count; n++)
        //                 {
        //                     AbilityObject nextAbility = nextTier[n];
        //                     if (ability.NextTier.Contains(nextAbility))
        //                     {
        //                         int ability1Index = ability.Index;
        //                         int ability2Index = nextAbility.Index;
        //                         // int ability1Index = tree.GetTreeAbilityIndex(t, ability);
        //                         // int ability2Index = tree.GetTreeAbilityIndex(t + 1, nextAbility);

        //                         GameObject ability1UI = tree.UITiers[t][i];
        //                         GameObject ability2UI = tree.UITiers[t + 1][n];

        //                         TreeAbility container1 = ability1UI.GetComponent<TreeAbility>();
        //                         TreeAbility container2 = ability2UI.GetComponent<TreeAbility>();

        //                         // Debug.Log(container1.Icon.rectTransform.localPosition);

        //                         Vector2 ability1Position = ability1UI.GetComponent<RectTransform>().anchoredPosition;
        //                         Vector2 ability2Position = ability2UI.GetComponent<RectTransform>().position;

        //                         Vector3 position = new Vector3((ability1Position.x + ability2Position.x) / 2f, (ability1Position.y + ability2Position.y) / 2f, 0f);
        //                         Quaternion rotation = Quaternion.identity;
        //                         int length = 50;

        //                         GameObject line = GameObject.Instantiate(abiltiyLinePrefab, ability1UI.transform);

        //                         RectTransform lineTransform = line.GetComponent<RectTransform>();

        //                         // Debug.Log("Placing line at " + position + " between " + ability1Position + " and " + ability2Position + " .. " + abilityTreeContentPanel.transform.position);

        //                         lineTransform.anchoredPosition = position;
        //                         lineTransform.rotation = rotation;
        //                         lineTransform.sizeDelta = new Vector2(lineTransform.sizeDelta.x, length);

        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }
    }

    public void SelectClass(PlayerClass selected)
    {
        classNameText.text = selected.Name;
        classDescriptionText.text = selected.Description;
        for (int a = 0; a < 4; a++)
        {
            abilityNameTexts[a].text = selected.BaseAbilities[a].Name;
            abilityTypeTexts[a].text = selected.BaseAbilities[a].Type.ToString();
            abilityDescriptionTexts[a].text = selected.BaseAbilities[a].ToolTip;
            abilityIconImages[a].sprite = selected.BaseAbilities[a].Icon;
        }
    }

    public void SetContinueButtonActive(bool active)
    {
        continueButton.interactable = active;
    }

    public void SetConfirmButtonActive(bool active)
    {
        confirmButton.interactable = active;
    }

    public void SetTimeTextActive(bool active) {
        timeLimitText.gameObject.SetActive(active);
    }

    public void Play()
    {
        if (manager == null)
            return;

        AllButtons(menuPanel);
        manager.NewGame();
    }

    public void Continue()
    {
        if (manager == null)
            return;

        AllButtons(menuPanel);
        manager.Continue();
    }

    public void TogglePause()
    {
        Paused = !Paused;
        ShowingTree = false;
        ShowingMap = false;

        if (Paused)
        {
            Time.timeScale = 0f;
            ShowingMap = false;
            ShowingTree = false;
            manager.MiniMapCam.SetZoom(miniMapZoom);
            ShowHUD(1);
        }
        else
        {
            Time.timeScale = 1f;
            manager.MiniMapCam.SetZoom(miniMapZoom);
            ShowHUD(0);
        }
    }

    public void ToggleTree()
    {
        ShowingTree = !ShowingTree;
        ShowingMap = false;
        if (ShowingTree)
        {
            ShowHUD(4);
        }
        else
        {
            Time.timeScale = 1f;
            manager.MiniMapCam.SetZoom(miniMapZoom);
            ShowHUD(0);
        }
    }

    public void ToggleMap()
    {
        ShowingMap = !ShowingMap;
        ShowingTree = false;
        if (ShowingMap)
        {
            manager.MiniMapCam.SetZoom(overlayMapZoom);
            ShowHUD(5);
        }
        else
        {
            Time.timeScale = 1f;
            manager.MiniMapCam.SetZoom(miniMapZoom);
            ShowHUD(0);
        }
    }

    public void ExitGame()
    {
        manager.MiniMapCam.SetZoom(10f);
        Paused = false;
        ShowingTree = false;
        ShowingMap = false;
        Time.timeScale = 1f;
        manager.SaveGame();
        manager.LoadMainMenu(defaultFadeTime);
    }

    public void QuitGame()
    {
        if (!Application.isEditor)
        {
            FadeOut(0.5f);
            AllButtons(menuPanel);
            Application.Quit();
        }
    }

    public void GoToMenu(int index)
    {
        // Debug.Log("Going to menu " + index + " from menu " + currentMenu);
        // If the index is invalid, return.
        if (index < 0 || index >= menuList.Length)
            return;

        // Get the new menu, if it is null, return.
        GameObject menu = menuList[index];
        if (menu == null)
            return;

        // If the CurrentMenu index is valid,
        if (currentMenu >= 0 && currentMenu < menuList.Length)
        {
            // Get the current menu, if it is not null,
            GameObject current = menuList[currentMenu];
            if (current != null)
            {
                current.SetActive(false);
            }
        }

        hudPanel.SetActive(false);
        menuPanel.SetActive(true);

        menu.SetActive(true);
        currentMenu = index;
    }

    public void SetAllSettingsElements(SettingsManager settings)
    {
        SetSettingsElements(settings);
        SetHUDSettingsElements(settings);
    }

    public void ShowSettingsTab(int index)
    {
        if (currentHUDSettingsTab >= 0 && currentHUDSettingsTab < mainSettingsContainer.tabs.Length)
        {
            Tab currentTab = mainSettingsContainer.tabs[currentHUDSettingsTab];
            if (currentTab != null)
            {
                currentTab.gameObject.SetActive(false);
            }
        }

        if (index < 0 || index >= hudSettingsContainer.tabs.Length)
            return;

        Tab tab = mainSettingsContainer.tabs[index];
        if (tab == null)
            return;

        mainSettingsContainer.tabTitle.text = tab.Title;
        tab.gameObject.SetActive(true);
        currentHUDSettingsTab = index;
    }

    public void SetSettingsElements(SettingsManager settings)
    {
        mainSettingsContainer.maxHealthSlider.value = settings.HealthPercent;
        hudSettingsContainer.timeLimitToggle.isOn = (settings.TimeLimit != 0f);
        mainSettingsContainer.timeLimitSlider.value = settings.TimeLimit;

        mainSettingsContainer.fullscreenToggle.isOn = settings.Fullscreen;
        mainSettingsContainer.resolutionDropdown.value = settings.ResolutionIndex;
        mainSettingsContainer.antialiasingDropdown.value = settings.AntialiasingIndex;
        mainSettingsContainer.vSyncDropdown.value = settings.VSyncIndex;
        mainSettingsContainer.frameRateDropdown.value = settings.FrameRateIndex;

        mainSettingsContainer.masterSlider.value = settings.MasterVolume;
        mainSettingsContainer.systemSlider.value = settings.SystemVolume;
        mainSettingsContainer.musicSlider.value = settings.MusicVolume;
        mainSettingsContainer.fxSlider.value = settings.FXVolume;
        mainSettingsContainer.ambientSlider.value = settings.AmbientVolume;

        UpdateSettingsElements(settings);
    }

    public void UpdateSettingsElements(SettingsManager settings)
    {
        mainSettingsContainer.maxHealthText.text = mainSettingsContainer.maxHealthSlider.value.ToString("0%");
        TimeSpan time = TimeSpan.FromSeconds(mainSettingsContainer.timeLimitSlider.value);
        string timeString = String.Format("{0:00}:{1:00}", (int)time.TotalMinutes, time.Seconds);
        mainSettingsContainer.timeLimitText.text = timeString;

        mainSettingsContainer.masterValueText.text = mainSettingsContainer.masterSlider.value.ToString("0%");
        mainSettingsContainer.systemValueText.text = mainSettingsContainer.systemSlider.value.ToString("0%");
        mainSettingsContainer.musicValueText.text = mainSettingsContainer.musicSlider.value.ToString("0%");
        mainSettingsContainer.fxValueText.text = mainSettingsContainer.fxSlider.value.ToString("0%");
        mainSettingsContainer.ambientValueText.text = mainSettingsContainer.ambientSlider.value.ToString("0%");
    }

    public void ShowHUD(int index)
    {
        // Debug.Log("Showing HUD " + index + " and hiding HUD " + currentHUD);

        // If invalid index, return.
        if (index < 0 || index >= hudList.Length)
            return;

        GameObject hud = hudList[index];
        // If the hud element is null, return.
        if (hud == null)
            return;
        // If currently showing a hud element, stop showing it.
        if (currentHUD >= 0 && currentHUD < hudList.Length)
        {
            GameObject current = hudList[currentHUD];
            if (current != null)
            {
                current.SetActive(false);
            }
        }

        // Just for now, since the UI is buggy in the editor.
        // TODO: Remove this when the bug is fixed that jumbles up the UI when applying to the prefab.
        // foreach (Transform child in hud.transform)
        // {
        //     child.gameObject.SetActive(true);
        // }

        // If was previous showing menu.
        hudPanel.SetActive(true);
        menuPanel.SetActive(false);

        hud.SetActive(true);
        currentHUD = index;
    }

    public void ShowHUDSettingsTab(int index)
    {
        if (currentHUDSettingsTab >= 0 && currentHUDSettingsTab < hudSettingsContainer.tabs.Length)
        {
            Tab currentTab = hudSettingsContainer.tabs[currentHUDSettingsTab];
            if (currentTab != null)
            {
                currentTab.gameObject.SetActive(false);
            }
        }

        if (index < 0 || index >= hudSettingsContainer.tabs.Length)
            return;

        Tab tab = hudSettingsContainer.tabs[index];
        if (tab == null)
            return;

        hudSettingsContainer.tabTitle.text = tab.Title;
        tab.gameObject.SetActive(true);
        currentHUDSettingsTab = index;
    }

    public void SetHUDSettingsElements(SettingsManager settings)
    {
        hudSettingsContainer.maxHealthSlider.value = settings.HealthPercent;
        hudSettingsContainer.maxHealthText.text = hudSettingsContainer.maxHealthSlider.value.ToString("0%");

        hudSettingsContainer.timeLimitToggle.isOn = (settings.TimeLimit != 0f);

        hudSettingsContainer.timeLimitSlider.value = settings.TimeLimit;
        TimeSpan time = TimeSpan.FromSeconds(hudSettingsContainer.timeLimitSlider.value);
        string timeString = String.Format("{0:00}:{1:00}", (int)time.TotalMinutes, time.Seconds);
        hudSettingsContainer.timeLimitText.text = timeString;

        hudSettingsContainer.fullscreenToggle.isOn = settings.Fullscreen;
        hudSettingsContainer.resolutionDropdown.value = settings.ResolutionIndex;
        hudSettingsContainer.antialiasingDropdown.value = settings.AntialiasingIndex;
        hudSettingsContainer.vSyncDropdown.value = settings.VSyncIndex;
        hudSettingsContainer.frameRateDropdown.value = settings.FrameRateIndex;

        hudSettingsContainer.masterSlider.value = settings.MasterVolume;
        hudSettingsContainer.systemSlider.value = settings.SystemVolume;
        hudSettingsContainer.musicSlider.value = settings.MusicVolume;
        hudSettingsContainer.fxSlider.value = settings.FXVolume;
        hudSettingsContainer.ambientSlider.value = settings.AmbientVolume;

        UpdateHUDSettingsElements(settings);
    }

    public void UpdateHUDSettingsElements(SettingsManager settings)
    {
        hudSettingsContainer.masterValueText.text = hudSettingsContainer.masterSlider.value.ToString("0%");
        hudSettingsContainer.systemValueText.text = hudSettingsContainer.systemSlider.value.ToString("0%");
        hudSettingsContainer.musicValueText.text = hudSettingsContainer.musicSlider.value.ToString("0%");
        hudSettingsContainer.fxValueText.text = hudSettingsContainer.fxSlider.value.ToString("0%");
        hudSettingsContainer.ambientValueText.text = hudSettingsContainer.ambientSlider.value.ToString("0%");
    }

    public void ToggleBorderHighlight()
    {
        if (screenHighlightTween != null)
            screenHighlightTween.Stop();

        if (Highlighted)
            screenHighlightTween = Tween.CanvasGroupAlpha(screenHighlightGroup, 0f, 0.15f, 0f);
        else
            screenHighlightTween = Tween.CanvasGroupAlpha(screenHighlightGroup, 1f, 0.15f, 0f);

        Highlighted = !Highlighted;
    }

    public void FlashPlayerDamage()
    {
        if (screenDamageTween != null)
            screenDamageTween.Stop();
        screenDamageTween = Tween.CanvasGroupAlpha(screenDamageGroup, 1f, 0.15f, 0f, Tween.EaseWobble);
    }

    public void UpdateEffectList(EffectDictionary effectDictionary)
    {
        List<AbilityEffect> abilityEffects = new List<AbilityEffect>();
        foreach (AbilityStatusEff type in Enum.GetValues(typeof(AbilityStatusEff)))
        {
            List<AbilityEffect> typeList = effectDictionary.GetEffectList(type);
            if (typeList != null)
                abilityEffects.AddRange(typeList);
        }

        // Iterate through all passed in AbilityEffects. Update the corresponding texts and create new texts if there aren't enough.
        int i = 0;
        for (; i < abilityEffects.Count; i++)
        {
            AbilityEffect effect = abilityEffects[i];
            // If there is a corresponding effect text, set the text.
            // Otherwise, create a new one, add it to the effectTextList, and set the text.
            if (i < effectTextList.Count)
            {
                TMP_Text effectText = effectTextList[i];
                DisplayEffectInList(effectText, effect);
            }
            else
            {
                TMP_Text effectText = GameObject.Instantiate(effectTextPrefab, effectGroup.transform, false);
                effectTextList.Add(effectText);
                DisplayEffectInList(effectText, effect);
            }

        }

        // Iterate through all excess ability texts, removing them.
        for (; i < effectTextList.Count; i++)
        {
            TMP_Text effectText = effectTextList[i];
            effectTextList.Remove(effectText);
            Destroy(effectText);
        }
    }

    void DisplayEffectInList(TMP_Text effectText, AbilityEffect effect)
    {
        if (effect.Effect == AbilityStatusEff.Root || effect.Effect == AbilityStatusEff.Silence || effect.Effect == AbilityStatusEff.Stun)
        {
            effectText.text = effect.Effect + " - " + effect.Remaining.ToString("0.0");
        }
        else if (effect.Effect == AbilityStatusEff.DoT)
        {
            effectText.text = effect.Effect + " " + effect.Value + " / " + effect.Duration + " " + (effect.Value / effect.Duration).ToString("0.0") + "/sec - " + effect.Remaining.ToString("0.0");
        }
        else if (effect.Effect == AbilityStatusEff.Heal)
        {
            effectText.text = effect.Effect + " " + (effect.Value * 100f / effect.Duration).ToString("0.0") + "%/sec - " + effect.Remaining.ToString("0.0");
        }
        else
        {
            effectText.text = effect.Effect + " " + (effect.Value * 100f).ToString("0.0") + "% - " + effect.Remaining.ToString("0.0");
        }
    }

    public void UpdatePlayerCores(int newCores)
    {
        coresText.text = "Cores: " + newCores;
        treeCoresText.text = "Cores: " + newCores;
    }

    public void UpdatePlayerHealth(float healthPercentage)
    {
        if (playerHealthBarTween != null)
            playerHealthBarTween.Stop();
        playerHealthBarTween = Tween.Value(playerHealthBar.fillAmount, healthPercentage, (value) => playerHealthMissing.fillAmount = value, 0.25f, 0.25f, Tween.EaseInOut);

        playerHealthBar.fillAmount = healthPercentage;
    }

    public void UpdatePlayerCast(float castTime)
    {
        if (playerCastBarTween != null)
            playerCastBarTween.Stop();
        playerCastBarTween = Tween.Value(0f, 1f, (value) => playerCastBar.fillAmount = value, castTime, 0f, completeCallback: () => playerCastBar.fillAmount = 0f);
    }

    public void CancelPlayerCast()
    {
        if (playerCastBarTween != null)
        {
            // Debug.Log("Cancelling UI cast tween!");
            playerCastBarTween.Stop();
            playerCastBarTween = null;

        }
        Color targetColor = new Color(1f, 0f, 0f, 1f);
        Tween.Color(playerCastInterruptImage, targetColor, 0.5f, 0f, Tween.EaseWobble);
        playerCastBar.fillAmount = 0f;
    }

    public void UpdatePlayerAbilityHUD(List<float> cooldowns, List<float> cooldownsRemaining, int currentCast = -1, float castProgress = 0f)
    {
        for (int i = 0; i < cooldowns.Count; i++)
        {
            SetPlayerAbilityIcon(i, cooldowns[i], cooldownsRemaining[i], (currentCast == i), castProgress);
        }
    }

    void SetPlayerAbilityIcon(int index, float cooldown, float cooldownRemaining, bool casting = false, float castProgress = 0f)
    {
        GameObject abilityIconObject = playerAbilityIcons[index];
        PlayerHUDAbility container = abilityIconObject.GetComponent<PlayerHUDAbility>();
        if (container != null)
        {
            container.CooldownTimer.fillAmount = cooldownRemaining / cooldown;
            if (cooldownRemaining == 0f)
                container.CooldownTimer.fillAmount = 0f;
            container.CooldownText.text = (cooldownRemaining <= 0f) ? "" : cooldownRemaining.ToString("0.0");
            container.CastTimer.fillAmount = (casting) ? castProgress : 0f;
        }
    }

    public void UpdateEnemyInfo(bool adjacentEnemy = false, string name = "", float healthPercentage = 0f, float castProgress = 0f, float castTime = 0f)
    {
        if (adjacentEnemy)
        {
            enemyInfoPanel.SetActive(true);
            enemyNameText.text = name;
            UpdateEnemyHealth(healthPercentage);
            UpdateEnemyCast(castProgress, castTime);
        }
        else
        {
            enemyInfoPanel.SetActive(false);
        }
    }

    public void UpdateEnemyHealth(float healthPercentage)
    {
        //        Tween.Stop(enemyHealthMissing.GetInstanceID());
        //        Tween.Value(enemyHealthBar.fillAmount, healthPercentage, (value) => enemyHealthMissing.fillAmount = value, 0.25f, 0.25f, Tween.EaseInOut);
        enemyHealthBar.fillAmount = healthPercentage;
    }

    public void UpdateEnemyCast(float castProgress, float castTime)
    {
        enemyCastBar.fillAmount = castProgress;
        float castRemaining = (castTime * (1f - castProgress));
        if (castRemaining < Mathf.Epsilon)
            enemyCastText.text = "";
        else
            enemyCastText.text = (castTime * (1 - castProgress)).ToString("0.0");
        //        Tween.Stop(enemyCastBar.GetInstanceID());
        //        Tween.Value(0f, 1f, (value) => enemyCastBar.fillAmount = value, castTime, 0f, completeCallback: () => enemyCastBar.fillAmount = 0f);
    }

    void RefreshAbilityTree()
    {
        foreach (TreeAbility treeAbility in treeAbilities)
        {
            AbilityObject ability = abilityTree.GetTreeAbility(treeAbility.Tier, treeAbility.Index);
            if (ability != null)
            {
                if (abilityTree.IsCurrentAbility(ability) || abilityTree.IsAvailable(ability))
                {
                    treeAbility.DimObject.SetActive(false);
                }
                else
                {
                    treeAbility.DimObject.SetActive(true);
                }
            }
        }
    }

    public void OnTreeAbilityHover(PointerEventData data)
    {
        abilityInfoContainer.gameObject.SetActive(true);

        TreeAbility treeAbility = data.pointerEnter.GetComponentInParent<TreeAbility>();
        if (treeAbility == null)
            return;

        treeAbility.HighlightObject.SetActive(true);

        AbilityObject ability = abilityTree.GetTreeAbility(treeAbility.Tier, treeAbility.Index);

        if (ability != null)
        {
            Debug.Log("Hovered over " + ability.Name);
            abilityInfoContainer.NameText.text = ability.Name;
            abilityInfoContainer.TypeText.text = ability.Type.ToString();
            abilityInfoContainer.TooltipText.text = ability.ToolTip;
            abilityInfoContainer.CooldownText.text = "Cooldown: " + ability.Cooldown;
            abilityInfoContainer.CastTimeText.text = "Cast Time: " + ability.CastTime;
            abilityInfoContainer.DamageText.text = "Damage: " + ability.Damage;
            if (ability.Type == AbilityType.Ranged)
            {
                abilityInfoContainer.AreaText.text = "Range: " + ability.Range;
                abilityInfoContainer.AOEImage.enabled = false;
            }
            else if (ability.Type == AbilityType.AreaOfEffect || ability.Type == AbilityType.Zone)
            {
                abilityInfoContainer.AreaText.text = "Area Shape";
                abilityInfoContainer.AOEImage.texture = ability.AOESprite;
                abilityInfoContainer.AOEImage.enabled = true;
            }
            else
            {
                abilityInfoContainer.AreaText.text = "";
                abilityInfoContainer.AOEImage.enabled = false;
            }

            GameObject effectsObject = abilityInfoContainer.EffectsObject;

            // Clear the list of effects.
            foreach (Transform child in effectsObject.transform)
            {
                Destroy(child.gameObject);
            }
            // Display all of the effects.
            foreach (AbilityEffect effect in ability.StatusEffects)
            {
                TMP_Text effectText = GameObject.Instantiate(effectTextPrefab, effectsObject.transform, false);
                DisplayEffectInTree(effectText, effect);
            }
            // If the ability has no effects, display None.
            if (ability.StatusEffects.Count <= 0)
            {
                TMP_Text effectText = GameObject.Instantiate(effectTextPrefab, effectsObject.transform, false);
                effectText.text = "None";
            }

            abilityInfoContainer.CostText.text = "Cost: " + ability.Cost.ToString();
        }
        OnButtonHover();
    }

    public void OnTreeAbilityUnHover(PointerEventData data)
    {
        abilityInfoContainer.gameObject.SetActive(false);

        TreeAbility treeAbility = data.pointerEnter.GetComponentInParent<TreeAbility>();
        if (treeAbility == null)
            return;
        treeAbility.HighlightObject.SetActive(false);
    }

    public void OnTreeAbilityClicked(PointerEventData data)
    {
        TreeAbility treeAbility = data.pointerPress.GetComponentInParent<TreeAbility>();
        AbilityObject ability = abilityTree.GetTreeAbility(treeAbility.Tier, treeAbility.Index);

        abilityTree.UpgradeAbility(ability);
        RefreshAbilityTree();
        SetUpAbilityIcons(abilityTree.Player);
        OnButtonClick();
    }

    void DisplayEffectInTree(TMP_Text effectText, AbilityEffect effect)
    {
        if (effect.Effect == AbilityStatusEff.Root || effect.Effect == AbilityStatusEff.Silence || effect.Effect == AbilityStatusEff.Stun)
        {
            effectText.text = effect.Effect + " - " + effect.Duration.ToString("0.0");
        }
        else if (effect.Effect == AbilityStatusEff.DoT)
        {
            effectText.text = effect.Effect + " " + effect.Value + " / " + effect.Duration + " " + (effect.Value / effect.Duration).ToString("0.0") + "/sec - " + effect.Duration.ToString("0.0");
        }
        else if (effect.Effect == AbilityStatusEff.Heal)
        {
            effectText.text = effect.Effect + " " + (effect.Value * 100f / effect.Duration).ToString("0.0") + "%/sec - " + effect.Duration.ToString("0.0");
        }
        else
        {
            effectText.text = effect.Effect + " " + (effect.Value * 100f).ToString("0.0") + "% - " + effect.Duration.ToString("0.0");
        }
    }

    public void OnButtonHover(UISound uiSound = null)
    {
        if (uiSound == null)
            uiSound = defaultHoverSound;
        manager.AudioManager.PlayUISound(uiSound);
    }

    public void OnButtonClick(UISound uiSound = null)
    {
        if (uiSound == null)
            uiSound = defaultClickSound;
        manager.AudioManager.PlayUISound(uiSound);
    }

    public void FadeOut(string text = "", float speed = defaultFadeTime)
    {
        Tween.CanvasGroupAlpha(fadePanel, 1f, speed, 0f, defaultFadeCurve, obeyTimescale: false);
        fadeText.text = text;
    }

    public void FadeOut(float speed = defaultFadeTime)
    {
        Tween.CanvasGroupAlpha(fadePanel, 1f, speed, 0f, defaultFadeCurve, obeyTimescale: false);
        fadeText.text = "";
    }

    public void FadeIn(string text = "", float speed = defaultFadeTime)
    {
        Tween.CanvasGroupAlpha(fadePanel, 0f, speed, 0f, defaultFadeCurve, obeyTimescale: false);
        fadeText.text = text;
    }
    public void FadeIn(float speed = defaultFadeTime)
    {
        Tween.CanvasGroupAlpha(fadePanel, 0f, speed, 0f, defaultFadeCurve, obeyTimescale: false);
        fadeText.text = "";
    }

    public void DisplayText(string text)
    {
        promptText.text = text;
    }

    public void DisplayText(string text, float duration)
    {
        StartCoroutine(DisplayText_Coroutine(text, duration));
    }

    IEnumerator DisplayText_Coroutine(string text, float duration)
    {
        promptText.text = text;
        yield return new WaitForSeconds(duration);
        promptText.text = "";
    }

    public void DisplayAreaText(string text)
    {
        mapText.text = text;
        areaText.text = text;
    }

    public void UpdateTimeText(float secondsRemaining) {
        TimeSpan time = TimeSpan.FromSeconds(secondsRemaining);
        timeLimitText.text = String.Format("{0:00}:{1:00}.{2:000}", (int)time.TotalMinutes, time.Seconds, time.Milliseconds);
    }
}
