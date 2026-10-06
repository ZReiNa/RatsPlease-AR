using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[Serializable]
public class CardEntry
{
    public string imageName;      // must match the name in the Reference Image Library
    public CardType type;
    public GameObject modelPrefab;
}

public class TrackedCardManager : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private CardEntry[] cards;

    private readonly Dictionary<TrackableId, GameObject> spawned = new();

    private void OnEnable()  { imageManager.trackablesChanged.AddListener(OnChanged); }
    private void OnDisable() { imageManager.trackablesChanged.RemoveListener(OnChanged); }

    private void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var image in args.added)
            Create(image);

        foreach (var image in args.updated)
        {
            if (spawned.TryGetValue(image.trackableId, out var go))
                go.SetActive(image.trackingState == TrackingState.Tracking);
        }

        foreach (var removed in args.removed)
        {
            if (spawned.TryGetValue(removed.Key, out var go))
            {
                Destroy(go);
                spawned.Remove(removed.Key);
            }
        }
    }

    private void Create(ARTrackedImage image)
    {
        foreach (var entry in cards)
        {
            if (entry.imageName != image.referenceImage.name) continue;

            var go = Instantiate(entry.modelPrefab, image.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            var item = go.GetComponent<CardItem>();
            if (item == null) item = go.AddComponent<CardItem>();
            item.Type = entry.type;

            // A kinematic Rigidbody is required for trigger events to fire
            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            spawned[image.trackableId] = go;
            return;
        }
    }
}