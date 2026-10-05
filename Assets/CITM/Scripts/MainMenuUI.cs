using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup menuRoot;     // whole menu (background included)
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup buttonGroup;
    [SerializeField] private Button startButton;

    [Header("AR")]
    [SerializeField] private ARSession arSession;      // component is disabled in the editor

    [Header("Timing")]
    [SerializeField] private float initialDelay = 0.5f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    private void Awake()
    {
        menuRoot.alpha = 1f;
        menuRoot.gameObject.SetActive(true);

        titleGroup.alpha = 0f;
        buttonGroup.alpha = 0f;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;

        if (arSession != null)
            arSession.enabled = false;

        startButton.onClick.AddListener(OnStartPressed);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(initialDelay);

        yield return Fade(titleGroup, 0f, 1f, fadeInDuration);
        yield return Fade(buttonGroup, 0f, 1f, fadeInDuration);

        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;
    }

    private void OnStartPressed()
    {
        startButton.interactable = false;
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        // Start the AR camera while the menu fades out
        if (arSession != null)
            arSession.enabled = true;

        menuRoot.blocksRaycasts = false;
        yield return Fade(menuRoot, 1f, 0f, fadeOutDuration);

        menuRoot.gameObject.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.StartGame();
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        group.alpha = to;
    }
}
