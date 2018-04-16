using System.Collections;
using System.Collections.Generic;
using MapClasses;
using Pixelplacement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using AbilityClasses;
using System;

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
    [SerializeField]
    GameObject menuPanel;
    [SerializeField] GameObject[] menuList;

    [Space(2)]
    [SerializeField]
    GameObject hudPanel;
    [SerializeField] GameObject[] hudList;
    [SerializeField] TMP_Text areaText;
    [SerializeField] TMP_Text promptText;

    [SerializeField] TMP_Text coresText;
    [SerializeField] TMP_Text effectTextPrefab;
    [SerializeField] VerticalLayoutGroup effectGroup;

    [SerializeField] Image playerHealthBar;
    [SerializeField] Image playerHealthMissing;
    [SerializeField] Image playerCastBar;


    [SerializeField] GameObject enemyInfoPanel;
    [SerializeField] TMP_Text enemyNameText;
    [SerializeField] Image enemyHealthBar;
    [SerializeField] Image enemyCastBar;
    [SerializeField] TMP_Text enemyCastText;

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
    List<TMP_Text> effectTextList;


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

        GoToMenu(0);
    }

    public void Initialize_Level()
    {
        effectTextList = new List<TMP_Text>();
        ShowHUD(0);
    }

    public void Play()
    {
        if (manager == null)
            return;

        manager.Map.SetCurrentLevel(0);
        manager.LoadLevel(defaultFadeTime);
    }

    public void QuitGame()
    {
        if (!Application.isEditor)
        {
            FadeOut(0.5f);
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
        foreach (Transform child in hud.transform)
        {
            child.gameObject.SetActive(true);
        }

        // If was previous showing menu.
        hudPanel.SetActive(true);
        menuPanel.SetActive(false);

        hud.SetActive(true);
        currentHUD = index;
    }

    public void TogglePause()
    {
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
            effectText.text = effect.Effect + " - " + effect.Duration;
        }
        else if (effect.Effect == AbilityStatusEff.DoT)
        {
            effectText.text = effect.Effect + " " + effect.Value.ToString("0.0") + "/sec - " + effect.Duration.ToString("0.0");
        }
        else
        {
            effectText.text = effect.Effect + " " + (effect.Value * 100f).ToString("0.0") + "% - " + effect.Duration.ToString("0.0");
        }
    }

    public void UpdatePlayerCores(int newCores)
    {
        coresText.text = "Cores: " + newCores;
    }

    public void UpdatePlayerHealth(float healthPercentage)
    {
        Tween.Stop(playerHealthMissing.GetInstanceID());
        Tween.Value(playerHealthBar.fillAmount, healthPercentage, (value) => playerHealthMissing.fillAmount = value, 0.25f, 0.25f, Tween.EaseInOut);
        playerHealthBar.fillAmount = healthPercentage;
    }

    public void UpdatePlayerCast(float castTime)
    {
        Tween.Finish(playerCastBar.GetInstanceID());
        Tween.Value(0f, 1f, (value) => playerCastBar.fillAmount = value, castTime, 0f, completeCallback: () => playerCastBar.fillAmount = 0f);
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
