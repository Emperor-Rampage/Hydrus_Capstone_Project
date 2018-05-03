using System.Collections;
using System.Collections.Generic;
using MapClasses;
using Pixelplacement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using AbilityClasses;
using EntityClasses;
using System;
using System.Linq;
using Pixelplacement.TweenSystem;

[RequireComponent(typeof(Canvas))]
public class UIManager : MonoBehaviour
{
    GameManager manager;

    [Header("Main Settings")]
    // Serialized fields.
    [SerializeField]
    CanvasGroup fadePanel;
    [SerializeField] const float defaultFadeTime = 1f;
    AnimationCurve defaultFadeCurve = Tween.EaseInOutStrong;
    [SerializeField] TMP_Text fadeText;

    [Space(2)]
    [Header("Menu Settings")]
    [SerializeField]
    GameObject menuPanel;
    [SerializeField] GameObject[] menuList;

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

    [SerializeField] GameObject enemyInfoPanel;
    [SerializeField] TMP_Text enemyNameText;
    [SerializeField] Image enemyHealthBar;
    [SerializeField] Image enemyCastBackBar;
    [SerializeField] Image enemyCastBackInterruptBar;
    [SerializeField] Image enemyCastBar;
    [SerializeField] TMP_Text enemyCastText;


    [Header("Ability Tree Settings")]
    [SerializeField] RectTransform abilityTreeContentPanel;
    [SerializeField] GameObject abilityTreePrefab;
    [SerializeField] GameObject abilityTierPrefab;
    [SerializeField] GameObject abilityVariantPrefab;

    // Private fields.
    int currentMenu;
    int currentHUD;

    [Header("Settings Menu Items")]
    // Serialized fields.
    [SerializeField]
    Toggle fullscreenToggle;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown antialiasingDropdown;
    [SerializeField] TMP_Dropdown vSyncDropdown;
    [SerializeField] TMP_Dropdown frameRateDropdown;

    // Private fields.
    [HideInInspector] public Resolution[] resolutions;
    List<TMP_Text> effectTextList = new List<TMP_Text>();

    List<GameObject> playerAbilityIcons = new List<GameObject>();

    TweenBase screenHighlightTween;
    TweenBase screenDamageTween;
    TweenBase playerHealthBarTween;
    TweenBase playerCastBarTween;

    public bool Highlighted { get; private set; } = false;


    // TODO: Set up UI to run animations during pause (timeScale = 0)
    //       using https://docs.unity3d.com/ScriptReference/AnimatorUpdateMode.UnscaledTime.html

    // FIXME: During transitions, the user can still continue clicking UI elements, such as in the main menu.
    //        Should be locked out of taking other actions.
    void Start()
    {
        manager = GameManager.Instance;
    }

    public void Initialize_Main()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();
        foreach (Resolution res in resolutions)
        {
            resolutionDropdown.options.Add(new TMP_Dropdown.OptionData(res.ToString()));
        }

        resolutionDropdown.RefreshShownValue();
        AllButtons(menuPanel, true);

        GoToMenu(0);
    }

    public void Initialize_Level(float interruptPercent = 0f)
    {
        playerCastBackBar.fillAmount = interruptPercent;
        playerCastBackInterruptBar.fillAmount = 1f - interruptPercent;
        enemyCastBackBar.fillAmount = interruptPercent;
        enemyCastBackInterruptBar.fillAmount = 1f - interruptPercent;

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
            foreach (Transform child in abilityIconObject.transform)
            {
                if (child.name == "Icon")
                {
                    child.GetComponent<Image>().sprite = ability.Icon;
                }
            }
            playerAbilityIcons.Add(abilityIconObject);
        }
        UpdatePlayerAbilityHUD(player.GetCooldownsList(), player.GetCooldownRemainingList(), player.CurrentAbility, player.CastProgress);
    }

    public void SetUpAbilityTreeMenu(AbilityTree tree)
    {
        // TODO: Switch to a panel for each ability with a vertical layout group.
        //       Populate with panels with horizontal layout groups with each ability's tier.

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

        for (int a = 0; a < tree.Player.Class.BaseAbilities.Count; a++) {
            
            GameObject abilityPanel = GameObject.Instantiate(abilityTreePrefab, abilityTreeContentPanel);
            // TODO: Switch to calculating the width. Should be number of leaves + any padding and spacing.
            abilityPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(500f, 0f);

            for (int t = 0; t < tree.NumTiers; t++) {

                GameObject tierPanel = GameObject.Instantiate(abilityTierPrefab, abilityPanel.transform);
                // TODO: Switch to calculating the height. Should be cell height.
                tierPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 100f);

                var abilityTier = tree.GetAbilityTier(a, t);

                for (int i = 0; i < abilityTier.Count; i++) {

                    GameObject cell = GameObject.Instantiate(abilityVariantPrefab, tierPanel.transform);
                    cell.GetComponent<RectTransform>().sizeDelta = new Vector2(tree.CellWidth, tree.CellHeight);
                    cell.name = "TreeIcon_" + abilityTier[i].Name;
                    cell.transform.GetChild(0).GetComponent<Image>().sprite = abilityTier[i].Icon;
                }

            }

        } 



        // for (int t = tree.NumTiers - 1; t >= 0; t--)
        // {
        //     Debug.Log("---- Tier " + t);
        //     int rowCount = 0;
        //     for (int a = 0; a < tree.Player.Class.BaseAbilities.Count; a++)
        //     {
        //         Debug.Log("-- Ability " + t);
        //         var abilityTier = tree.GetAbilityTier(a, t);
        //         int leafCount = tree.GetAbilityLeafs(a);
        //         int median = Mathf.FloorToInt(leafCount / 2f);

        //         int tierSpacing = 0;
        //         // var nextAbilityTier = tree.GetAbilityTier(a, t + 1);
        //         // if (nextAbilityTier != null)
        //         // {
        //         //     tierSpacing = Mathf.FloorToInt((nextAbilityTier.Count / abilityTier.Count) / 2f);
        //         // }

        //         tierSpacing = Mathf.FloorToInt((leafCount / abilityTier.Count) / 2f);

        //         /*
                
        //         count1 = 4
        //         count2 = 8
        //         count3 = 11
                
        //         spacing1 = (count2 / count1) / 2 = (8 / 4) / 2 = 1
        //         spacing2 = (count3 / count2) / 2 = (11 / 8) / 2 = 1.375 / 2 = 0.6875
                
        //          O O O O
        //         OOOOOOOO
        //         OOOOOOOOOOO
        //         */

        //         for (int c = 0; c < abilityTier.Count; c++)
        //         {

        //             Debug.Log(" Cell " + c);
        //             GameObject cell = GameObject.Instantiate(abilityTreePrefab, abilityTreeContentPanel);
        //             cell.name = "TreeIcon_" + abilityTier[c].Name;
        //             cell.transform.GetChild(0).GetComponent<Image>().sprite = abilityTier[c].Icon;
        //             rowCount++;

        //             for (int s = 0; s < tierSpacing; s++)
        //             {
        //                 GameObject blank = GameObject.Instantiate(abilityTreePrefab, abilityTreeContentPanel);
        //                 blank.name = "Spacer";
        //                 rowCount++;
        //             }
        //         }
        //     }
        //     if (rowCount < tree.TotalNumLeafs)
        //     {
        //         for (int r = 0; r < (tree.TotalNumLeafs - rowCount); r++)
        //         {
        //             GameObject blank = GameObject.Instantiate(abilityTreePrefab, abilityTreeContentPanel);
        //             blank.name = "Spacer";
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

    public void SetConfirmButton(bool active)
    {
        // Debug.Log("Setting confirm button to " + active);
        confirmButton.enabled = active;
    }
    public void Play()
    {
        if (manager == null)
            return;

        AllButtons(menuPanel);
        manager.Map.SetCurrentLevel(0);
        manager.LoadLevel(defaultFadeTime);
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
        Debug.Log("Going to menu " + index + " from menu " + currentMenu);
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

    public void ShowHUD(int index)
    {
        Debug.Log("Showing HUD " + index + " and hiding HUD " + currentHUD);

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

    public void TogglePause()
    {
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
            Debug.Log("Cancelling UI cast tween!");
            playerCastBarTween.Stop();
            playerCastBarTween = null;

        }
        Color startColor = new Color(1f, 0f, 0f, 0f);
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
        foreach (Transform child in abilityIconObject.transform)
        {
            if (child.name == "CooldownTimerImage")
            {
                child.GetComponent<Image>().fillAmount = cooldownRemaining / cooldown;
            }
            else if (child.name == "CooldownTimerText")
            {
                if (cooldownRemaining <= 0f)
                    child.GetComponent<TMP_Text>().text = "";
                else
                    child.GetComponent<TMP_Text>().text = cooldownRemaining.ToString("0.0");
            }
            else if (child.name == "CastTimerImage")
            {
                if (casting)
                {
                    child.GetComponent<Image>().fillAmount = castProgress;
                }
                else
                {
                    child.GetComponent<Image>().fillAmount = 0f;
                }
            }
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

    public void FadeOut(string text = "", float speed = defaultFadeTime)
    {
        Tween.CanvasGroupAlpha(fadePanel, 1f, speed, 0f, defaultFadeCurve);
        fadeText.text = text;
    }

    public void FadeOut(float speed = defaultFadeTime)
    {
        Tween.CanvasGroupAlpha(fadePanel, 1f, speed, 0f, defaultFadeCurve);
        fadeText.text = "";
    }

    public void FadeIn(string text = "", float speed = defaultFadeTime)
    {
        Tween.CanvasGroupAlpha(fadePanel, 0f, speed, 0f, defaultFadeCurve);
        fadeText.text = text;
    }
    public void FadeIn(float speed = defaultFadeTime)
    {
        Tween.CanvasGroupAlpha(fadePanel, 0f, speed, 0f, defaultFadeCurve);
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
        areaText.text = text;
    }
}
