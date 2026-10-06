using UnityEngine;

public class Guest : MonoBehaviour
{
    public int Index { get; private set; }

    private GuestSpawner spawner;
    private bool resolved;

    public void Init(int index, GuestSpawner owner)
    {
        Index = index;
        spawner = owner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (resolved) return;
        if (GameManager.Instance == null || !GameManager.Instance.GameStarted) return;

        var card = other.GetComponentInParent<CardItem>();
        if (card == null) return;

        resolved = true;
        spawner.OnGuestHit(this, card.Type);
    }
}