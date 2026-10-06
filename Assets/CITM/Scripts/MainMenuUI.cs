using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup menuRoot;
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup buttonGroup;
    [SerializeField] private Button startButton;

    [Header("Timing")]
    [SerializeField] private float titleFadeDelay = 0.5f;
    [SerializeField] private float fadeDuration = 0.75f;

    private Coroutine menuRoutine;

    private void Awake()
    {
        
        menuRoot.alpha = 1f;
        menuRoot.gameObject.SetActive(true);

        titleGroup.alpha = 0f;
        buttonGroup.alpha = 0f;

        startButton.interactable = true;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        startButton.onClick.AddListener(OnStartPressed);
        ShowMenu();
    }

    private void OnDestroy()
    {
        startButton.onClick.RemoveListener(OnStartPressed);
    }

    private void OnStartPressed()
    {
        startButton.interactable = false;
        GameManager.Instance.StartGame();
        FadeOutMenu();
    }

    public void ShowMenu()
    {
        if (menuRoutine != null)
            StopCoroutine(menuRoutine);

        menuRoot.gameObject.SetActive(true);
        menuRoot.alpha = 1f;
        menuRoot.blocksRaycasts = true;

        titleGroup.alpha = 0f;
        buttonGroup.alpha = 0f;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;
        startButton.interactable = true;

        menuRoutine = StartCoroutine(FadeMenuIn());
    }

    public void FadeOutMenu()
    {
        if (menuRoutine != null)
            StopCoroutine(menuRoutine);

        menuRoutine = StartCoroutine(FadeMenuOut());
    }

    private IEnumerator FadeMenuIn()
    {
        yield return new WaitForSecondsRealtime(titleFadeDelay);

        yield return FadeCanvasGroup(titleGroup, 0f, 1f, fadeDuration);
        yield return FadeCanvasGroup(buttonGroup, 0f, 1f, fadeDuration);

        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;
    }

    private IEnumerator FadeMenuOut()
{
    buttonGroup.interactable = false;
    buttonGroup.blocksRaycasts = false;
    menuRoot.blocksRaycasts = false;

    
    yield return FadeCanvasGroup(menuRoot, menuRoot.alpha, 0f, fadeDuration);

    menuRoot.gameObject.SetActive(false);
}

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        group.alpha = to;
    }
}