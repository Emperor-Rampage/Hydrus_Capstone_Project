using System.Collections;
using System.Collections.Generic;
using MapClasses;
using Pixelplacement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] Image playerHealthBar;
    [SerializeField] Image playerHealthMissing;
    [SerializeField] Image playerCastBar;

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
    //    Tween currentMissingHealthTween;


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

        if (index < 0 || index >= hudList.Length)
            return;

        GameObject hud = hudList[index];

        if (hud == null)
            return;

        if (currentHUD >= 0 && currentHUD < hudList.Length)
        {
            GameObject current = hudList[currentHUD];
            if (current != null)
            {
                current.SetActive(false);
            }
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

    public void UpdatePlayerHealth(float newPercentage)
    {
        Tween.Stop(playerHealthMissing.GetInstanceID());
        Tween.Value(playerHealthBar.fillAmount, newPercentage, (value) => playerHealthMissing.fillAmount = value, 0.25f, 0.25f, Tween.EaseInOut);
        playerHealthBar.fillAmount = newPercentage;
    }

    public void UpdatePlayerCast(float castTime)
    {
        Tween.Stop(playerCastBar.GetInstanceID());
        Tween.Value(0f, 1f, (value) => playerCastBar.fillAmount = value, castTime, 0f, completeCallback: () => playerCastBar.fillAmount = 0f);
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
