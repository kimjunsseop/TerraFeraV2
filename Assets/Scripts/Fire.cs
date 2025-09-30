using UnityEngine;
public class Fire : MonoBehaviour
{
    private bool playerInside = false;
    public bool IsPlayerInside => playerInside;

    private int playerLayer;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == playerLayer)
        {
            playerInside = false;
        }
    }
}