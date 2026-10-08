using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GuestSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARTrackedImageManager imageManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] guestPrefabs;
    [SerializeField] private GameObject heartEffectPrefab;
    [SerializeField] private GameObject skullEffectPrefab;

    [Header("Settings")]
    [SerializeField] private float respawnDelay = 0.6f;
    [SerializeField] private float effectHeight = 0.4f;

    private ARTrackedImage marker;
    private Guest currentGuest;
    private bool active;
    private float cooldown;

    private void OnEnable()
    {
        imageManager.trackablesChanged.AddListener(OnImagesChanged);
    }

    private void OnDisable()
    {
        imageManager.trackablesChanged.RemoveListener(OnImagesChanged);
    }

    private void OnImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var image in args.added)
            marker = image;

        foreach (var image in args.updated)
            marker = image;

        foreach (var removed in args.removed)
        {
            if (marker != null && marker.trackableId == removed.Key)
            {
                marker = null;

                if (currentGuest != null)
                    Destroy(currentGuest.gameObject);

                currentGuest = null;
            }
        }
    }

    public void Begin()
    {
        active = true;
        cooldown = 0f;
    }

    public void Stop()
    {
        active = false;

        if (currentGuest != null)
            Destroy(currentGuest.gameObject);

        currentGuest = null;
    }

    private void Update()
    {
        if (!active || marker == null) return;

        bool tracking = marker.trackingState == TrackingState.Tracking;

        if (currentGuest != null)
        {
            currentGuest.gameObject.SetActive(tracking);
            return;
        }

        if (!tracking) return;

        cooldown -= Time.deltaTime;
        if (cooldown <= 0f)
            SpawnGuest();
    }

    private void SpawnGuest()
{
    int index = Random.Range(0, guestPrefabs.Length);

    var go = Instantiate(guestPrefabs[index], marker.transform);
    go.transform.localPosition = Vector3.zero;
    go.transform.localRotation = Quaternion.identity;

    foreach (var col in go.GetComponentsInChildren<Collider>(true))
    {
        col.enabled = true;
        col.isTrigger = true;

        if (col is MeshCollider mc)
            mc.convex = true;
    }

    var rb = go.GetComponent<Rigidbody>();
    if (rb == null)
        rb = go.AddComponent<Rigidbody>();
    rb.isKinematic = true;
    rb.useGravity = false;

    var guest = go.GetComponent<Guest>();
    if (guest == null)
        guest = go.AddComponent<Guest>();

    guest.Init(index, this);
    currentGuest = guest;
}

public void OnGuestHit(Guest guest, CardItem card)
{
    if (guest != currentGuest) return;

    bool correct = GameSession.Instance.Evaluate(guest.Index, card.Type);

    var fx = correct ? heartEffectPrefab : skullEffectPrefab;
    if (fx != null)
    {
        var pos = guest.transform.position + guest.transform.up * effectHeight;
        Destroy(Instantiate(fx, pos, Quaternion.identity), 3f);
    }

    
    Destroy(guest.gameObject);
    currentGuest = null;
    cooldown = respawnDelay;

    if (CardController.Instance != null)
        CardController.Instance.ClearCard(); 

    if (card != null)
        Destroy(card.gameObject); 
}
}