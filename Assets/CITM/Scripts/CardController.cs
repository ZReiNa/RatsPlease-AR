using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CardController : MonoBehaviour
{
    public static CardController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private Camera arCamera;

    [Header("Toggles")]
    [SerializeField] private Toggle cheeseToggle;
    [SerializeField] private Toggle trapToggle;

    [Header("Card prefabs")]
    [SerializeField] private GameObject cheesePrefab;
    [SerializeField] private GameObject trapPrefab;

    [Header("Guest rotation")]
    [SerializeField] private float rotateSpeed = 0.4f;

    private static readonly List<ARRaycastHit> hits = new();

    private CardType? selected;
    private CardItem currentCard;
    private CardItem draggedCard;
    private Guest rotatedGuest;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        cheeseToggle.onValueChanged.AddListener(OnCheeseToggled);
        trapToggle.onValueChanged.AddListener(OnTrapToggled);

        GameManager.Instance.OnGameStarted += ResetState;
        GameManager.Instance.OnGameEnded += ResetState;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStarted -= ResetState;
            GameManager.Instance.OnGameEnded -= ResetState;
        }
    }

    private void OnCheeseToggled(bool on)
    {
        if (on) trapToggle.SetIsOnWithoutNotify(false);
        UpdateSelected();
    }

    private void OnTrapToggled(bool on)
    {
        if (on) cheeseToggle.SetIsOnWithoutNotify(false);
        UpdateSelected();
    }

    private void UpdateSelected()
    {
        if (cheeseToggle.isOn) selected = CardType.Cheese;
        else if (trapToggle.isOn) selected = CardType.Trap;
        else selected = null;
    }

    private void ResetState()
    {
        cheeseToggle.SetIsOnWithoutNotify(false);
        trapToggle.SetIsOnWithoutNotify(false);

        selected = null;
        draggedCard = null;
        rotatedGuest = null;

        ClearCard();
    }

    public void ClearCard()
    {
        if (currentCard != null)
            Destroy(currentCard.gameObject);

        currentCard = null;
        draggedCard = null;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.GameStarted)
            return;

        if (Touch.activeTouches.Count == 0)
            return;

        Touch touch = Touch.activeTouches[0];

        switch (touch.phase)
        {
            case UnityEngine.InputSystem.TouchPhase.Began:
                OnTouchBegan(touch);
                break;

            case UnityEngine.InputSystem.TouchPhase.Moved:
            case UnityEngine.InputSystem.TouchPhase.Stationary:
                OnTouchHeld(touch);
                break;

            case UnityEngine.InputSystem.TouchPhase.Ended:
            case UnityEngine.InputSystem.TouchPhase.Canceled:
                draggedCard = null;
                rotatedGuest = null;
                break;
        }
    }

    private void OnTouchBegan(Touch touch)
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(touch.finger.index))
            return;

        Ray ray = arCamera.ScreenPointToRay(touch.screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            var card = hit.collider.GetComponentInParent<CardItem>();
            if (card != null)
            {
                draggedCard = card;
                return;
            }

            var guest = hit.collider.GetComponentInParent<Guest>();
            if (guest != null)
            {
                rotatedGuest = guest;
                return;
            }
        }

        if (selected.HasValue && TryGetPlanePoint(touch.screenPosition, out Vector3 point))
        {
            SpawnCard(selected.Value, point);
            draggedCard = currentCard;
        }
    }

    private void OnTouchHeld(Touch touch)
    {
        if (draggedCard != null)
        {
            if (TryGetPlanePoint(touch.screenPosition, out Vector3 point))
                draggedCard.transform.position = point;
        }
        else if (rotatedGuest != null)
        {
            rotatedGuest.transform.Rotate(0f, -touch.delta.x * rotateSpeed, 0f, Space.Self);
        }
    }

    private bool TryGetPlanePoint(Vector2 screenPos, out Vector3 point)
    {
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            point = hits[0].pose.position;
            return true;
        }

        point = default;
        return false;
    }

    private void SpawnCard(CardType type, Vector3 point)
    {
        ClearCard();

        var prefab = type == CardType.Cheese ? cheesePrefab : trapPrefab;

        Vector3 toCam = arCamera.transform.position - point;
        toCam.y = 0f;

        Quaternion rot = toCam.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(toCam)
            : Quaternion.identity;

        var go = Instantiate(prefab, point, rot);

        var item = go.GetComponent<CardItem>();
        if (item == null)
            item = go.AddComponent<CardItem>();

        item.Type = type;

        var rb = go.GetComponent<Rigidbody>();
        if (rb == null)
            rb = go.AddComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;

        currentCard = item;
    }
}