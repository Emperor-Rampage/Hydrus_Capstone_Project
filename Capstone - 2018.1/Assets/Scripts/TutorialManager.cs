using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class TutorialManager : MonoBehaviour
{
    // UIManager uiManager;
    public bool RunTutorial { get; set; }
    public bool InTutorial { get; private set; }

    // TODO: Implement introduction. Should give a brief description of the goal of the game and some controls.
    // 		 Should run upon beginning a new game and loading into the Hub.
    public void RunIntroduction()
    {
        Time.timeScale = 0f;
    }

    // TODO: Implement movement tutorial. Should go over the grid-based movement along with movement controls.
    // 		 Should run upon completing the Introduction.
    public void RunMovementTutorial()
    {
        Time.timeScale = 0f;
    }

    // TODO: Implement combat tutorial. Should go over ability controls, combat mechanics, ability types, effects.
    //		 Should run upon completing the Movement Tutorial.
    public void RunCombatTutorial()
    {
        Time.timeScale = 0f;
    }

    // TODO: Implement upgrade tutorial. Should go over Cores, the Ability Tree Menu, and ability upgrades.
    // 		 Should run upon the player killing their first enemy.
    public void RunUpgradeTutorial()
    {
        Time.timeScale = 0f;
    }

    // Goes to the next screen in the current tutorial.
    public void NextScreen()
    {

    }
}
