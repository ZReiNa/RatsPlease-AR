using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool GameStarted { get; private set; }

    // Resets when the app process is closed and reopened
    private static bool tutorialHasPlayedThisAppSession = false;

    public event Action OnGameStarted;
    public event Action OnGameEnded;

    [Header("References")]
    [SerializeField] private MainMenuUI mainMenuUI;
    [SerializeField] private TutorialDialogueUI tutorialDialogueUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartGame()
    {
        if (!tutorialHasPlayedThisAppSession && tutorialDialogueUI != null)
        {
            tutorialHasPlayedThisAppSession = true;
            tutorialDialogueUI.PlayTutorial(BeginGameplay);
        }
        else
        {
            BeginGameplay();
        }
    }

    private void BeginGameplay()
    {
        GameStarted = true;
        OnGameStarted?.Invoke();
    }

    // Stops the game (timer, spawning, HUD) without touching the menu
    public void EndGame()
    {
        if (!GameStarted) return;
        GameStarted = false;
        OnGameEnded?.Invoke();
    }

    public void ReturnToMenu()
    {
        EndGame();
        if (mainMenuUI != null)
            mainMenuUI.ShowMenu();
    }
}