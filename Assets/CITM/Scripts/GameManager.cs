using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool GameStarted { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void StartGame()
    {
        GameStarted = true;
    }
}