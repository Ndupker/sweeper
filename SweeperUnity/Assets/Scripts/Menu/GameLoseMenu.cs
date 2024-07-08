using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoseMenu : MonoBehaviour, IMineMenu
{
    public GameManager _gameManager;
    public GameObject _loseMenuObject;
    public GameObject _loseMenuVRObject;
    public void Hide()
    {
        if (ViewManager.instance._viewMode == ViewMode.Normal)
            _loseMenuObject.SetActive(false);
        else
            _loseMenuVRObject.SetActive(false);
    }

    public void Show()
    {
        if (ViewManager.instance._viewMode == ViewMode.Normal)
            _loseMenuObject.SetActive(true);
        else
            _loseMenuVRObject.SetActive(true);
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
