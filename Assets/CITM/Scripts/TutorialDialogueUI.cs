using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialDialogueUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines =
    {
        "Hello, my lackey! Today you have an important job to do.",
        "My royal party is about to start, and I have invited many of my 'friends.'",
        "Your mission is to serve delicious cheese to those who are dressed up for the occasion.",
        "And for those who aren't, give 'em the trap.",
        "Just move the pertinent card towards the guest.",
        "You have 3 minutes to do your job; I expect great results from you."
    };

    [Header("Typing")]
    [SerializeField] private float charactersPerSecond = 35f;
    [SerializeField] private float fadeDuration = 0.4f;

    private int currentLineIndex;
    private bool waitingForNext;
    private bool skippingTyping;
    private System.Action onFinished;

    private void Awake()
    {
        nextButton.gameObject.SetActive(false);
        nextButton.onClick.AddListener(OnNextPressed);

        rootGroup.alpha = 0f;
        rootGroup.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        nextButton.onClick.RemoveListener(OnNextPressed);
    }

    public void PlayTutorial(System.Action finishedCallback)
    {
        onFinished = finishedCallback;
        currentLineIndex = 0;
        rootGroup.gameObject.SetActive(true);
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        yield return Fade(0f, 1f);

        while (currentLineIndex < dialogueLines.Length)
        {
            yield return TypeLine(dialogueLines[currentLineIndex]);

            waitingForNext = true;
            nextButton.gameObject.SetActive(true);

            while (waitingForNext)
                yield return null;

            nextButton.gameObject.SetActive(false);
            currentLineIndex++;
        }

        yield return Fade(1f, 0f);
        rootGroup.gameObject.SetActive(false);

        onFinished?.Invoke();
    }

    private IEnumerator TypeLine(string line)
    {
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalCharacters = dialogueText.textInfo.characterCount;
        float shownCharacters = 0f;
        skippingTyping = false;

        while (shownCharacters < totalCharacters && !skippingTyping)
        {
            shownCharacters += charactersPerSecond * Time.unscaledDeltaTime;
            dialogueText.maxVisibleCharacters = Mathf.Min(Mathf.FloorToInt(shownCharacters), totalCharacters);
            yield return null;
        }

        dialogueText.maxVisibleCharacters = totalCharacters;
    }

    private void OnNextPressed()
    {
        if (!waitingForNext)
            return;

        waitingForNext = false;
    }

    public void SkipTyping()
    {
        skippingTyping = true;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            rootGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        rootGroup.alpha = to;
    }
}