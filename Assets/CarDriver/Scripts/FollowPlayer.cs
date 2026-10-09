using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;

    // Camera position relative to the player
    private Vector3 offset = new Vector3(0, 5, -7);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // LateUpdate runs after Update, so the camera moves after the vehicle (no jitter)
    void LateUpdate()
    {
        // Offset the camera behind the player by adding to the player's position
        transform.position = player.transform.position + offset;
    }
}
