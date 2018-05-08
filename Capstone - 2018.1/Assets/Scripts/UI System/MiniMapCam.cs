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

            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y;
        transform.position = newPosition;

        if (followRotation == true)
        {
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }

    }

}
