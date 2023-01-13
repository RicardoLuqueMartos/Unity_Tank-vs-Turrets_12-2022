using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Newtonsoft.Json.Linq;

public class UIManager : MonoBehaviour
{
    #region Variables
    [SerializeField]
    RectTransform gameUI;

    [SerializeField]
    RectTransform startMenu;

    [SerializeField]
    RectTransform failMenu;

    [SerializeField]
    RectTransform winMenu;

    [SerializeField]
    TMP_Text ammosText;

    [SerializeField]
    TMP_Text maxAmmosText;

    [SerializeField]
    TMP_Text lifeText;

    [SerializeField]
    TMP_Text maxLifeText;

    [SerializeField]
    TMP_Text destroyedTurretsText;

    [SerializeField]
    TMP_Text turretsAmountText;

    [SerializeField]
    PlayerHealthBar playerHealthBar;

    #endregion Variables

    private void OnEnable()
    {
        BaseController.TankDestroyed += OpenFailMenu;
        TankController.WinGame += OpenWinMenu;
    }

    private void OnDestroy()
    {
        BaseController.TankDestroyed -= OpenFailMenu;
        TankController.WinGame -= OpenWinMenu;

    }

    #region Max Values Init
    public void SetMaxLifePointsDisplay(int value)
    {
        maxLifeText.text = value.ToString();
        playerHealthBar.maxHealthPoints = value;
    }

    public void SetTurretsAmountDisplay(int value)
    {
        turretsAmountText.text = value.ToString();
    }

    public void SetMaxAmmosDisplay(int value)
    {
        string textToDisplay = value.ToString();

        if (value == -1) textToDisplay = "Inf";

        maxAmmosText.text = textToDisplay;
    }
    #endregion Max Values Init

    #region Values Update
    public void SetLifePointsDisplay(int value)
    {
        lifeText.text = value.ToString();
        UpdateLifebar(value);
    }

    void UpdateLifebar(int value)
    {
        playerHealthBar.UpdateHealthBar(value);
    }

    public void SetDestroyedTurretsDisplay(int value)
    {
        destroyedTurretsText.text = value.ToString();
    }

    public void SetAmmosDisplay(int value)
    {
        string textToDisplay = value.ToString();

        if (maxAmmosText.text == "Inf") textToDisplay = "Inf";

        ammosText.text = textToDisplay;
    }
    #endregion Values Update

    public void CloseStartMenu()
    {
        startMenu.gameObject.SetActive(false);
        OpenGameUI();
    }

    #region Open / Close Menus
    public void OpenGameUI()
    {
        gameUI.gameObject.SetActive(true);
    }

    public void CloseGameUI()
    {
        gameUI.gameObject.SetActive(false);
    }

    public void OpenWinMenu()
    {
        CloseGameUI();
        winMenu.gameObject.SetActive(true);
    }

    public void OpenFailMenu()
    {
        CloseGameUI();
        failMenu.gameObject.SetActive(true);
    }

    #endregion Open / Close Menus

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
