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
    [SerializeField] float restrictStrength;
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
    float midpointUpRight;
    float midpointDownRight;
    float midpointDownLeft;
    float midpointUpLeft;

    float velocity = 0f;

    void LateUpdate()
    {
        if (camera == null || transform == null)
            return;

        Direction currentDirection = GetDirectionFacing();
        float directionAngle = (int)currentDirection * 90f;
        float angleDistance = Mathf.Abs(Mathf.DeltaAngle(rotationX, directionAngle));
        float sensitivityXModifier = 0.25f + (angleDistance / 22.5f);

        rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * (SensitivityX * sensitivityXModifier);

        if (RestrictDirection != Direction.Null)
        {
            float directionRotation = (int)RestrictDirection * 90f;

            float range = restrictRange / 2f;

            // Special case for up, since it's the point at which 360 becomes 0.
            // For all other directions, can just use the clamp method.

            float restrictLower = Mathf.Repeat(directionRotation - range, 360f);
            float restrictUpper = Mathf.Repeat(directionRotation + range, 360f);
            float distanceLower = Mathf.Abs(Mathf.DeltaAngle(rotationX, restrictLower));
            float distanceUpper = Mathf.Abs(Mathf.DeltaAngle(rotationX, restrictUpper));

            float speed = restrictStrength * Time.deltaTime;

            if (distanceLower >= restrictRange)
            {
                rotationX = Mathf.MoveTowardsAngle(rotationX, restrictUpper, speed);
            }
            else if (distanceUpper >= restrictRange)
            {
                rotationX = Mathf.MoveTowardsAngle(rotationX, restrictLower, speed);
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
