using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour, IMineMenu
{
    public GameManager _gameManager;
    public GameObject _mainMenuObject;
    public GameObject _mainMenuVRObject;
    public void Hide()
    {
        if(ViewManager.instance._viewMode == ViewMode.Normal)
            _mainMenuObject.SetActive(false);
        else
            _mainMenuVRObject.SetActive(false);
    }

    public void Show()
    {
        if (ViewManager.instance._viewMode == ViewMode.Normal)
            _mainMenuObject.SetActive(true);
        else
            _mainMenuVRObject.SetActive(true);
    }

    public void Play2DButtonPressed()
    {
        _gameManager.ShowGame(MapType._2D, MapDimentionsBombs._12x22_99);
        Hide();
    }
    public void PlayEasy3DButtonPressed()
    {
        _gameManager.ShowGame(MapType._3D, MapDimentionsBombs._4x4x4_6);
        Hide();
    }
    public void Play3DButtonPressed()
    {
        _gameManager.ShowGame(MapType._3D, MapDimentionsBombs._6x6x6_20);
        Hide();
    }
}
