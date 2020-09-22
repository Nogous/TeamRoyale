﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public string mainMenuSceneName;

    public void ResumeGame()
    {
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().inPaused = false;
        Time.timeScale = 1;
    }
    public void QuitToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
