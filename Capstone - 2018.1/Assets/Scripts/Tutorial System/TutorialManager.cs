using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas), typeof(MouseLookManager))]
public class TutorialManager : MonoBehaviour
{
    MouseLookManager mouseLookManager;
    public static UnityEvent SectionDone;
    // UIManager uiManager;
    [SerializeField] GameObject tutorialPanel;
    [SerializeField] TutorialSection introductionSection;
    public TutorialSection Introduction { get { return introductionSection; } }
    [SerializeField] TutorialSection movementSection;
    public TutorialSection Movement { get { return movementSection; } }
    [SerializeField] TutorialSection combatSection;
    public TutorialSection Combat { get { return combatSection; } }
    [SerializeField] TutorialSection upgradeSection;
    public TutorialSection Upgrade { get { return upgradeSection; } }
    TutorialSection currentSection;
    public TutorialSection Current { get { return currentSection; } }

    public bool RunTutorial { get; set; }
    public bool InTutorial { get; private set; }

    // TODO: Implement introduction. Should give a brief description of the goal of the game and some controls.
    // 		 Should run upon beginning a new game and loading into the Hub.
    void Start()
    {
        mouseLookManager = GetComponent<MouseLookManager>();
        if (SectionDone == null)
            SectionDone = new UnityEvent();
        SectionDone.AddListener(EndSection);
    }

    void Run()
    {
        Time.timeScale = 0f;
        mouseLookManager.enabled = false;
        tutorialPanel.SetActive(true);
        currentSection.gameObject.SetActive(true);
        currentSection.Reset();
        InTutorial = true;
    }

    void EndSection()
    {
        Time.timeScale = 1f;
        mouseLookManager.enabled = true;
        currentSection.Complete = true;
        currentSection.gameObject.SetActive(false);
        tutorialPanel.SetActive(false);
        InTutorial = false;
    }

    public void ResetAll()
    {
        RunTutorial = true;
        InTutorial = false;
        Introduction.Complete = false;
        Movement.Complete = false;
        Combat.Complete = false;
        Upgrade.Complete = false;
        currentSection = null;
    }

    public void RunIntroduction()
    {
        currentSection = introductionSection;
        Run();
    }

    // TODO: Implement movement tutorial. Should go over the grid-based movement along with movement controls.
    // 		 Should run upon completing the Introduction.
    public void RunMovementTutorial()
    {
        currentSection = movementSection;
        Run();
    }

    // TODO: Implement combat tutorial. Should go over ability controls, combat mechanics, ability types, effects.
    //		 Should run upon completing the Movement Tutorial.
    public void RunCombatTutorial()
    {
        currentSection = combatSection;
        Run();
    }

    // TODO: Implement upgrade tutorial. Should go over Cores, the Ability Tree Menu, and ability upgrades.
    // 		 Should run upon the player killing their first enemy.
    public void RunUpgradeTutorial()
    {
        currentSection = upgradeSection;
        Run();
    }

    // Goes to the next screen in the current tutorial.
    public void NextScreen()
    {
        if (currentSection == null)
        {
            Debug.LogError("ERROR: Attempting to go to next tutorial screen before assigning the section.");
            return;
        }

        currentSection.NextScreen();
    }
}
