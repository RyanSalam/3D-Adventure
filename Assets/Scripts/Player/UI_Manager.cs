using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        else
            Destroy(this);
    }

    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Image healthFill;

    public void TogglePause()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        Time.timeScale = pauseMenu.activeSelf ? 0.0f : 1f;
        Cursor.lockState = pauseMenu.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void GameOverDisplay()
    {
        gameOverScreen.SetActive(true);
        StartCoroutine(ScreenFadein());
        gameOverScreen.transform.GetChild(0).gameObject.SetActive(true);
    }

    private IEnumerator ScreenFadein()
    {
        var g = gameOverScreen.GetComponent<Image>();
        float elapsed = 0f;

        Color current = g.color;
        Color target = Color.black;

        while (elapsed < 4f)
        {
            elapsed += Time.deltaTime;
            g.color = Color.Lerp(current, target, elapsed / 4f);
            yield return null;
        }
    }

    public void ResumeButton()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void UpdateHP()
    {
        DamageAble d = PlayerController.instance.GetComponent<DamageAble>();

        Debug.Log("is this runnning");

        int max = d.maxHP;
        int current = d.currentHP;

        Debug.Log(current);

        float percent = (float)current / (float)max;
        StartCoroutine(ChangeHPBar(percent));
    }

    private IEnumerator ChangeHPBar(float percent)
    {
        float preChange = healthFill.fillAmount;
        float elapsed = 0f;

        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            healthFill.fillAmount = Mathf.Lerp(preChange, percent, elapsed / 0.2f);
            yield return null;
        }
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
