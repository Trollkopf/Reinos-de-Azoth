using System.Collections.Generic;
using UnityEngine;

public class OpponentLayoutController : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField]
    private RectTransform leftSlot;

    [SerializeField]
    private RectTransform centerSlot;

    [SerializeField]
    private RectTransform rightSlot;

    [Header("Prefab")]
    [SerializeField]
    private GameObject opponentPrefab;

    [Header("Identities")]
    [SerializeField]
    private List<OpponentIdentity> identities = new List<OpponentIdentity>();

    [Header("Misc")]
    [SerializeField]
    private PlayerTargetSelectionManager selectionManager;

    private Dictionary<PlayerState, OpponentIdentity> assignedIdentities =
        new Dictionary<PlayerState, OpponentIdentity>();

    public void ShowOpponents(List<PlayerState> opponents)
    {
        ClearSlots();

        if (opponents == null || opponents.Count == 0)
        {
            return;
        }

        List<OpponentIdentity> randomIdentities = new List<OpponentIdentity>();

        foreach (PlayerState opponent in opponents)
        {
            randomIdentities.Add(GetOrAssignIdentity(opponent));
        }

        if (opponents.Count == 1)
        {
            CreateOpponent(opponents[0], centerSlot, randomIdentities[0]);

            return;
        }

        if (opponents.Count == 2)
        {
            CreateOpponent(opponents[0], leftSlot, randomIdentities[0]);

            CreateOpponent(opponents[1], rightSlot, randomIdentities[1]);

            return;
        }

        CreateOpponent(opponents[0], leftSlot, randomIdentities[0]);

        CreateOpponent(opponents[1], centerSlot, randomIdentities[1]);

        CreateOpponent(opponents[2], rightSlot, randomIdentities[2]);
    }

    private void CreateOpponent(PlayerState player, RectTransform slot, OpponentIdentity identity)
    {
        GameObject instance = Instantiate(opponentPrefab, slot);

        RectTransform rect = instance.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;

            rect.localScale = Vector3.one;
        }

        OpponentPortraitView portraitView = instance.GetComponent<OpponentPortraitView>();

        if (portraitView != null)
        {
            portraitView.Setup(player, selectionManager, identity.displayName, identity.artwork);
        }
    }

    private void ClearSlots()
    {
        ClearSlot(leftSlot);
        ClearSlot(centerSlot);
        ClearSlot(rightSlot);
    }

    private void ClearSlot(RectTransform slot)
    {
        if (slot == null)
            return;

        for (int i = slot.childCount - 1; i >= 0; i--)
        {
            Destroy(slot.GetChild(i).gameObject);
        }
    }

    private List<OpponentIdentity> GetRandomIdentities(int amount)
    {
        List<OpponentIdentity> available = new List<OpponentIdentity>(identities);

        List<OpponentIdentity> selected = new List<OpponentIdentity>();

        amount = Mathf.Min(amount, available.Count);

        for (int i = 0; i < amount; i++)
        {
            int randomIndex = Random.Range(0, available.Count);

            selected.Add(available[randomIndex]);

            available.RemoveAt(randomIndex);
        }

        return selected;
    }

    private OpponentIdentity GetOrAssignIdentity(PlayerState player)
    {
        if (assignedIdentities.TryGetValue(player, out OpponentIdentity existing))
        {
            return existing;
        }

        List<OpponentIdentity> available = new List<OpponentIdentity>(identities);

        foreach (OpponentIdentity used in assignedIdentities.Values)
        {
            available.Remove(used);
        }

        if (available.Count == 0)
        {
            return identities[Random.Range(0, identities.Count)];
        }

        OpponentIdentity identity = available[Random.Range(0, available.Count)];

        assignedIdentities.Add(player, identity);

        return identity;
    }

    public OpponentIdentity GetIdentity(PlayerState player)
    {
        if (player == null)
            return null;

        if (assignedIdentities.TryGetValue(player, out OpponentIdentity identity))
        {
            return identity;
        }

        return null;
    }
}
