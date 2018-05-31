using System.Collections;
using System.Collections.Generic;
using MapClasses;
using UnityEngine;

public class MouseLookManager : MonoBehaviour
{
    new Camera camera;
    new Transform transform;

    [Range(0f, 90f)]
    [SerializeField] float directionRange;
    [Range(0f, 90f)]
    [SerializeField] float restrictRange;
    [SerializeField] float minimumY;
    [SerializeField] float maximumY;


    public Direction RestrictDirection { get; set; } = Direction.Null;
    public float SensitivityX { get; set; } = 2f;
    public float SensitivityY { get; set; } = 2f;

    float rotationX = 0f;
    float rotationY = 0f;

    float restrictUpperUp;
    float restrictLowerUp;

    float rangeUpperUp;
    float rangeLowerUp;
    float rangeUpperRight;
    float rangeLowerRight;
    float rangeUpperDown;
    float rangeLowerDown;
    float rangeUpperLeft;
    float rangeLowerLeft;

    void Update()
    {
        if (camera == null || transform == null)
            return;

        rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * SensitivityX;
        // Debug.Log("Rotation X: " + rotationX);
        if (RestrictDirection != Direction.Null)
        {
            float directionRotation = (int)RestrictDirection * 90f;

            float range = restrictRange / 2f;
            Debug.Log("Restricting mouse movement between " + (directionRotation - range) + " and " + (directionRotation + range));

            // Special case for up, since it's the point at which 360 becomes 0.
            // For all other directions, can just use the clamp method.
            if (RestrictDirection == Direction.Up)
            {
                float midPoint = (restrictLowerUp + restrictUpperUp) / 2f;
                if (rotationX < restrictLowerUp && rotationX >= midPoint)
                    rotationX = restrictLowerUp;
                else if (rotationX > restrictUpperUp && rotationX < midPoint)
                    rotationX = restrictUpperUp;
            }
            else
            {
                rotationX = Mathf.Clamp(rotationX, directionRotation - range, directionRotation + range);
            }
        }

        rotationY += Input.GetAxis("Mouse Y") * SensitivityY;
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

        transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0f);
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;

        float restrict = restrictRange / 2f;
        restrictLowerUp = 360f - restrict;
        restrictUpperUp = 0f + restrict;

        float range = directionRange / 2f;
        rangeLowerUp = 360f - range;
        rangeUpperUp = 0f + range;

        rangeLowerRight = 90f - range;
        rangeUpperRight = 90f + range;

        rangeLowerDown = 180f - range;
        rangeUpperDown = 180f + range;

        rangeLowerLeft = 270f - range;
        rangeUpperLeft = 270f + range;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void SetTarget(GameObject target)
    {
        camera = target.GetComponentInChildren<Camera>();
        transform = target.transform;
    }

    public Direction GetDirectionFacing()
    {
        if (IsInUpRange())
        {
            return Direction.Up;
        }
        else if (IsInRightRange())
        {
            return Direction.Right;
        }
        else if (IsInDownRange())
        {
            return Direction.Down;
        }
        else if (IsInLeftRange())
        {
            return Direction.Left;
        }
        return Direction.Null;
    }
    bool IsInUpRange()
    {
        return (rotationX >= rangeLowerUp || rotationX < rangeUpperUp);
    }

    bool IsInRightRange()
    {
        return (rotationX >= rangeLowerRight && rotationX < rangeUpperRight);
    }

    bool IsInDownRange()
    {
        return (rotationX >= rangeLowerDown && rotationX < rangeUpperDown);
    }

    bool IsInLeftRange()
    {
        return (rotationX >= rangeLowerLeft && rotationX < rangeUpperLeft);
    }
}
