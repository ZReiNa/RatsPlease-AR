using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GuestSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private Camera arCamera;

    [Header("Prefabs")]
    [SerializeField] private GameObject[] guestPrefabs;   // the 9 guests
    [SerializeField] private GameObject heartEffectPrefab;
    [SerializeField] private GameObject skullEffectPrefab;

    [Header("Spawn settings")]
    [SerializeField] private float minDistance = 0.8f;
    [SerializeField] private float maxDistance = 2.5f;
    [SerializeField] private float respawnDelay = 0.6f;
    [SerializeField] private float effectHeight = 0.4f;

    private Guest currentGuest;
    private bool active;

    public void Begin()
    {
        active = true;
        StopAllCoroutines();
        StartCoroutine(SpawnRoutine(0f));
    }

    public void Stop()
    {
        active = false;
        StopAllCoroutines();
        if (currentGuest != null)
            Destroy(currentGuest.gameObject);
        currentGuest = null;
    }

    public void OnGuestHit(Guest guest, CardType card)
    {
        if (guest != currentGuest) return;

        GameSession.Instance.Evaluate(guest.Index, card);

        var fx = card == CardType.Cheese ? heartEffectPrefab : skullEffectPrefab;
        if (fx != null)
        {
            var pos = guest.transform.position + Vector3.up * effectHeight;
            Destroy(Instantiate(fx, pos, Quaternion.identity), 3f);
        }

        Destroy(guest.gameObject);
        currentGuest = null;

        if (active)
            StartCoroutine(SpawnRoutine(respawnDelay));
    }

    private IEnumerator SpawnRoutine(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        // Keep trying until a suitable plane is found
        while (active)
        {
            if (TryGetSpawnPoint(out var point))
            {
                SpawnGuest(point);
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SpawnGuest(Vector3 point)
    {
        int index = Random.Range(0, guestPrefabs.Length);

        Vector3 toCamera = arCamera.transform.position - point;
        toCamera.y = 0f;
        var rot = toCamera.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(toCamera)
            : Quaternion.identity;

        var go = Instantiate(guestPrefabs[index], point, rot);

        var guest = go.GetComponent<Guest>();
        if (guest == null) guest = go.AddComponent<Guest>();
        guest.Init(index, this);

        currentGuest = guest;
    }

    private bool TryGetSpawnPoint(out Vector3 result)
    {
        var candidates = new List<Vector3>();
        Vector3 camPos = arCamera.transform.position;

        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment != PlaneAlignment.HorizontalUp) continue;
            if (plane.trackingState != TrackingState.Tracking) continue;
            if (!plane.boundary.IsCreated || plane.boundary.Length < 3) continue;

            for (int i = 0; i < 6; i++)
            {
                Vector2 edge = plane.boundary[Random.Range(0, plane.boundary.Length)];
                Vector2 local = Vector2.Lerp(plane.center, edge, Random.value);
                Vector3 world = plane.transform.TransformPoint(new Vector3(local.x, 0f, local.y));

                Vector3 flat = world - camPos;
                flat.y = 0f;
                float dist = flat.magnitude;

                if (dist >= minDistance && dist <= maxDistance)
                    candidates.Add(world);
            }
        }

        if (candidates.Count == 0)
        {
            result = default;
            return false;
        }

        result = candidates[Random.Range(0, candidates.Count)];
        return true;
    }
}