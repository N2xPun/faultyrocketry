using UnityEngine;

public class PlayerCamFollower : MonoBehaviour
{
    private Transform cam;

    private void Start()
    {
        cam = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().Camera;
    }

    private void Update()
    {
        transform.rotation = cam.transform.rotation;
    }
}
