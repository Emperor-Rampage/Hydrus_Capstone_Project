using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class MiniMapCam : MonoBehaviour
{
    public Transform player;
    new Camera camera;

    //TO DO: Expose this to the options menu so the player has control over the Minimap
    public bool FollowRotation { get; set; }

    void Start() {
        camera = GetComponent<Camera>();
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelLoaded;
    }

    // Remove OnLevelLoaded
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode) {
    }

    private void LateUpdate()
    {
        if(player == null)
        {
            //TODO: Update this to grab the player from another container, instead of having to manually look for it.

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) {
                player = playerObject.transform;
            }
        } else {
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;

            if (FollowRotation == true)
            {
                transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
            }
        }
    }

    public void SetZoom(float zoom) {
        camera.orthographicSize = Mathf.Clamp(zoom, 0f, 100f);
    }
}
