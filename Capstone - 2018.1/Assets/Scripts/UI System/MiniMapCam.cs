using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCam : MonoBehaviour
{
    public Transform player;

    //TO DO: Expose this to the options menu so the player has control over the Minimap
    public bool followRotation { get; set; }

    private void Start()
    {
        if(player == null)
        {
            //TO DO: Update this to grab the player from another container, instead of having to manually look for it.

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) {
                player = playerObject.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (player != null) {
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;

            if (followRotation == true)
            {
                transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
            }
        }
    }

}
