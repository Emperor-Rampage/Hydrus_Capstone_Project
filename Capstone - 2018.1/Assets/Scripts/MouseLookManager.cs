using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLookManager : MonoBehaviour {
	new Camera camera;
	new Transform transform;

	[SerializeField] float minimumY;
	[SerializeField] float maximumY;


	public float SensitivityX { get; set; } = 10f;
	public float SensitivityY { get; set; } = 10f;

	float rotationY = 0f;

	public void SetTarget(GameObject target) {
		camera = target.GetComponentInChildren<Camera>();
		transform = target.transform;
	}

	void Update() {
		if (camera == null || transform == null)
			return;

		float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * SensitivityX;
		
		rotationY += Input.GetAxis("Mouse Y") * SensitivityY;
		rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

		transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0f);
	}

	void OnEnable() {
		Cursor.lockState = CursorLockMode.Locked;
	}

	void OnDisable() {
		Cursor.lockState = CursorLockMode.None;
	}
}
