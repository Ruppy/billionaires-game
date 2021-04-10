using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    public Vector2 minConstraint;
    public Vector2 maxConstraint;

    public Vector3 desiredPosition;

    // Update is called once per frame
    void FixedUpdate()
    {
      desiredPosition = target.position + offset;
      float contrainedX = Mathf.Max(target.position.x, minConstraint.x);
      contrainedX = Mathf.Min(contrainedX, maxConstraint.x);
      float contrainedY = Mathf.Max(target.position.y, minConstraint.y);
      contrainedY = Mathf.Min(contrainedY, maxConstraint.y);

      Vector3 constrainedPosition = new Vector3(contrainedX, contrainedY, -10f);
      Vector3 smoothedPosition = Vector3.Lerp(transform.position, constrainedPosition, smoothSpeed);
      transform.position = smoothedPosition;
    }
}
