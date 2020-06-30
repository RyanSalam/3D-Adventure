using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;

        else
            Destroy(this);
    }

    public bool gameOver;

    public int CurrentSceneIndex { get; private set; }

    private void OnLevelWasLoaded(int level)
    {
        CurrentSceneIndex = level;
    }

    private void Update()
    {
        if (gameOver)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UI_Manager.instance.TogglePause();
        }
    }

    public void Gameover()
    {
        gameOver = true;

        UI_Manager.instance.GameOverDisplay();
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void Restart()
    {
        LoadScene(CurrentSceneIndex);
    }
}
