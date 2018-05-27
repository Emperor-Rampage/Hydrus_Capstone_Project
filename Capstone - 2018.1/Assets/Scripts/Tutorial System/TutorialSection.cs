using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSection : MonoBehaviour
{
    [SerializeField] GameObject[] screens;
    int iter = 0;

    public bool Complete { get; set; }

    public void NextScreen()
    {
        for (int i = 0; i < screens.Length; i++)
        {
            if (i == iter)
            {
                screens[i].SetActive(true);
            }
            else
            {
                screens[i].SetActive(false);
            }
        }
        if (iter >= screens.Length)
        {
            gameObject.SetActive(false);
            TutorialManager.SectionDone.Invoke();
            return;
        }
        iter++;
    }

    public void Reset()
    {
        iter = 0;
        Complete = false;
        NextScreen();
    }
}
