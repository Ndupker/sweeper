using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IMineMenu
{
    void Show();
    void Hide();
}
public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        MainTitle,
        InGame,
        Won,
        Lost,
        Generating
    }
    public GameState _gameState = GameState.MainTitle;
    public MainMenu _mainMenu;
    public Map _map;
    public MapSaveLoad _mapSaveLoad;
    public GameWinMenu _winMenu;
    public GameLoseMenu _loseMenu;

    public void Start()
    {
        switch(_gameState)
        {
            case GameState.InGame:
                ReplayGame();
                break;
            case GameState.Lost:
                GameLose();
                break;
            case GameState.Won:
                GameWin();
                break;
            case GameState.MainTitle:
                ShowMainMenu();
                break;
            case GameState.Generating:
                _map.CreateCells();
                _map.Show();
                break;

        }
    }

    public void ReplayGame()
    {
        _gameState = GameState.InGame;
        _mapSaveLoad.LoadAll(_map._mapType, _map._dimentions);
        _map.Show();
    }
    public void ShowGame(MapType mapType, MapDimentionsBombs dimentions)
    {
        _gameState = GameState.InGame;
        _mapSaveLoad.LoadAll(mapType, dimentions);
        _map.Show();
    }
    public void GameWin()
    {
        _gameState = GameState.Won;
        _winMenu.Show();
    }
    public void GameLose()
    {
        _gameState = GameState.Lost;
        _loseMenu.Show();
    }
    public void ShowMainMenu()
    {
        _gameState = GameState.MainTitle;
        _mainMenu.Show();
        _map.HideMap();
    }
}
