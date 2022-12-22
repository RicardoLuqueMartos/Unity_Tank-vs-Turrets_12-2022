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
    RectTransform GameUI;

    [SerializeField]
    RectTransform startMenu;

    [SerializeField]
    RectTransform FailMenu;

    [SerializeField]
    RectTransform WinMenu;

    [SerializeField]
    TMP_Text AmmosText;

    [SerializeField]
    TMP_Text MaxAmmosText;

    [SerializeField]
    TMP_Text LifeText;

    [SerializeField]
    TMP_Text MaxLifeText;

    [SerializeField]
    TMP_Text DestroyedTurretsText;

    [SerializeField]
    TMP_Text TurretsAmountText;


    #endregion Variables

    #region Max Values Init
    public void SetMaxLifePointsDisplay(int value)
    {
        MaxLifeText.text = value.ToString();
    }

    public void SetTurretsAmountDisplay(int value)
    {
        TurretsAmountText.text = value.ToString();
    }

    public void SetMaxAmmosDisplay(int value)
    {
        string textToDisplay = value.ToString();

        if (value == -1) textToDisplay = "Inf";

        MaxAmmosText.text = textToDisplay;
    }
    #endregion Max Values Init

    #region Values Update
    public void SetLifePointsDisplay(int value)
    {
        LifeText.text = value.ToString();
    }

    public void SetDestroyedTurretsDisplay(int value)
    {
        DestroyedTurretsText.text = value.ToString();
    }

    public void SetAmmosDisplay(int value)
    {
        string textToDisplay = value.ToString();

        if (MaxAmmosText.text == "Inf") textToDisplay = "Inf";

        AmmosText.text = textToDisplay;
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
        GameUI.gameObject.SetActive(true);
    }

    public void CloseGameUI()
    {
        GameUI.gameObject.SetActive(false);
    }

    public void OpenWinMenu()
    {
        CloseGameUI();
        WinMenu.gameObject.SetActive(true);
    }

    public void OpenFailMenu()
    {
        CloseGameUI();
        FailMenu.gameObject.SetActive(true);
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
