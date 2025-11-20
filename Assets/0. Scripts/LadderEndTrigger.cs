using UnityEngine;

public class LadderEndTrigger : MonoBehaviour
{
    public enum EndType { Top, Bottom }
    public EndType type;

    public Ladder ladder;   // Ladder 프리팹 연결해두기

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;
        ladder.SetActiveChildCollider(false);
        if (type == EndType.Top)
            pc.OnClimbEndTopEnter();
        else
            pc.OnClimbEndBottomEnter();
    }
}
