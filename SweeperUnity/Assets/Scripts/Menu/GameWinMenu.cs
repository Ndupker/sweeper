using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWinMenu : MonoBehaviour, IMineMenu
{
    public GameManager _gameManager;
    public GameObject _winMenuObject;
    public GameObject _winMenuVRObject;
    public void Hide()
    {
        if (ViewManager.instance._viewMode == ViewMode.Normal)
            _winMenuObject.SetActive(false);
        else
            _winMenuVRObject.SetActive(false);

    }

    public void Show()
    {
        if (ViewManager.instance._viewMode == ViewMode.Normal)
            _winMenuObject.SetActive(true);
        else
            _winMenuVRObject.SetActive(true);
    }
    public void PlayAgain()
    {
        Hide();
        _gameManager.ReplayGame();
    }
    public void GoBack()
    {
        Hide();
        _gameManager.ShowMainMenu();
    }
}
