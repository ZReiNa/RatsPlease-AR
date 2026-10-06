using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class RuleCard
{
    public Sprite sprite;
    public int[] cheeseGuestIndices;
}

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [Header("Game")]
    [SerializeField] private float gameDuration = 180f;
    [SerializeField] private int pointsForCorrect = 1;
    [SerializeField] private int penaltyForWrong = 0;
    [SerializeField] private RuleCard[] rules;

    [Header("References")]
    [SerializeField] private GuestSpawner spawner;

    [Header("HUD")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Image ruleImage;
    [SerializeField] private Button exitButton;

    [Header("Results")]
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private Button resultsMenuButton;

    private RuleCard currentRule;
    private float remaining;
    private int score;
    private bool running;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        hudRoot.SetActive(false);
        resultsPanel.SetActive(false);

        exitButton.onClick.AddListener(OnExitPressed);
        resultsMenuButton.onClick.AddListener(OnResultsMenuPressed);

        GameManager.Instance.OnGameStarted += HandleGameStarted;
        GameManager.Instance.OnGameEnded += HandleGameEnded;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= HandleGameStarted;
            GameManager.Instance.OnGameEnded -= HandleGameEnded;
        }
    }

    private void HandleGameStarted()
    {
        score = 0;
        remaining = gameDuration;
        running = true;

        currentRule = rules[UnityEngine.Random.Range(0, rules.Length)];
        ruleImage.sprite = currentRule.sprite;

        resultsPanel.SetActive(false);
        hudRoot.SetActive(true);
        UpdateScoreText();
        UpdateTimerText();

        spawner.Begin();
    }

    private void HandleGameEnded()
    {
        running = false;
        spawner.Stop();
        hudRoot.SetActive(false);
    }

    private void Update()
    {
        if (!running) return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f)
        {
            remaining = 0f;
            UpdateTimerText();
            Finish();
            return;
        }

        UpdateTimerText();
    }

    private void Finish()
    {
        GameManager.Instance.EndGame();

        finalScoreText.text = $"Score: {score}";
        resultsPanel.SetActive(true);
    }

    public bool Evaluate(int guestIndex, CardType card)
    {
        if (!running) return false;

        bool guestWantsCheese = Array.IndexOf(currentRule.cheeseGuestIndices, guestIndex) >= 0;
        bool correct = (card == CardType.Cheese) == guestWantsCheese;

        if (correct)
            score += pointsForCorrect;
        else
            score = Mathf.Max(0, score - penaltyForWrong);

        UpdateScoreText();
        return correct;
    }

    private void OnExitPressed()
    {
        GameManager.Instance.ReturnToMenu();
    }

    private void OnResultsMenuPressed()
    {
        resultsPanel.SetActive(false);
        GameManager.Instance.ReturnToMenu();
    }

    private void UpdateTimerText()
    {
        int total = Mathf.CeilToInt(remaining);
        timerText.text = $"{total / 60}:{total % 60:00}";
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score}";
    }
}