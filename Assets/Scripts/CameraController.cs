using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    #region Variables
    [SerializeField]
    BaseController player;

    [SerializeField]
    public Transform targetTransform;

    [SerializeField]
    public Transform cameraPivot;

    [SerializeField]
    private Transform cameraTransform;

    [SerializeField]
    Transform playerTransform;
    
    [SerializeField]
    float CameraRotationSpeed;

    #region Orbit Variables
    [Header("Orbit Variables")]

    [SerializeField]
    private float PivotHeightOffset = 2.0f;

    [SerializeField]
    private float maxDistance = 2f;

    [SerializeField]
    private float xSpeed = 250.0f;
    [SerializeField]
    private float ySpeed = 120.0f;

    [SerializeField]
    private float yMinLimit = -20;
    [SerializeField]
    private float yMaxLimit = 80;

    [SerializeField]
    private float xMinLimit = -35;
    [SerializeField]
    private float xMaxLimit = 35;

    float x = 0.0f;
    float y = 0.0f;

    float prevDistance;
    bool OrbitTarget = true;

    [Header("Camera Look Type Collision")]
    public LayerMask collisionLayers; // camera will collide with this
    private float defaultPosition;
    private Vector3 cameraFollowVelocity = Vector3.zero;
    private Vector3 cameraVectorPosition;
    public float minimumCollisionOffset = 0.2f;
    public float cameraCollisionOffset = 0.2f; // how much the camera will jump of its colliding
    public float cameraCollisionRadius = 0.2f;

    #endregion Orbit Variables

    #endregion Variables

    private void Awake()
    {
        player = FindObjectOfType<TankController>();
        playerTransform = player.transform;
        cameraTransform = Camera.main.transform;

        #region Orbit
        var angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        #endregion Orbit

        defaultPosition = transform.localPosition.z;
    }

    // Update is called once per frame      
    void Update()
    {
        if (!player)
            return;

        if (player.GameStarted)
        {
            RotateCamera();
            HandleCameraCollisions();
        }
    }   

    private void RotateCamera()
    {
        if (playerTransform && OrbitTarget)
        {
            if (cameraPivot == null)
                cameraPivot = cameraTransform.parent;

            // get mouse input and apply the mouse speed offset

            // get position gape between Mouse X and Mouse Y on screen points every frame 
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            // limit rotation of the camera
            x = ClampAngle(x, xMinLimit, xMaxLimit);
            y = ClampAngle(y, yMinLimit, yMaxLimit);

            #region apply rotation and position of the camera
            // calculate the degrees to rotate arround the 3 axes 
            var rotation = Quaternion.Euler(y, x, 0);

            // calculate a position from a rotation multiplied by the position of the targeted object
            // minus the distance of the camera
            var position = rotation * new Vector3(0.0f, 0.0f, -maxDistance) + playerTransform.transform.position;

            cameraPivot.rotation = rotation;
        //    cameraPivot.position = position;
            cameraPivot.position = new Vector3( playerTransform.position.x, playerTransform.position.y +PivotHeightOffset, playerTransform.position.z );
            #endregion apply rotation and position of the camera
        }
    }

    float ClampAngle(float angle, float min, float max)
    {
        // limit the canera angle values to - and + 360 degrees
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;

        // return camera rotation depending on min and max values 
        return Mathf.Clamp(angle, min, max);
    }

    #region Buggé donc désactivé
    private void HandleCameraCollisions()
    {
        if (cameraPivot == null)        
            cameraPivot = cameraTransform.parent;

        float targetPosition = defaultPosition;
        RaycastHit hitObstacle;
        Vector3 direction = cameraTransform.position - cameraPivot.position /*new Vector3(cameraTransform.position.x, cameraTransform.position.y, cameraTransform.position.z-maxDistance)*/;
     //   direction.Normalize();

        //   bool SomethingOnTheWay = false;



        // use a raycasr to detect a collision on the way between the camera and the player object
        if (Physics.SphereCast(player.transform.position, 
            cameraCollisionRadius, direction, out hitObstacle,  maxDistance, collisionLayers))
        {
            Debug.DrawLine(player.transform.position, hitObstacle.point, Color.blue);

            Debug.DrawLine(player.transform.position, cameraTransform.position, Color.red);

            if (player != null)
            {

                // Determine the distance from the camera pivot object to the collided object
                float distanceTo = Vector3.Distance(cameraPivot.position, hitObstacle.point);

                // calculate the target position from distance to the collision and applying the offset
                targetPosition = -(distanceTo - cameraCollisionOffset);
            }
            else player = FindObjectOfType<TankController>();
        }

        if (Mathf.Abs(targetPosition) < minimumCollisionOffset)
        {
            targetPosition = targetPosition - minimumCollisionOffset;
        }

        cameraVectorPosition.z = Mathf.Lerp(cameraTransform.localPosition.z, targetPosition, 0.2f);

        // verify the distance comparing the max camera distance, assign clamp cameraVectorPosition.z to max camera distance if exceded  
        if (cameraVectorPosition.z < -maxDistance)
        {
            cameraVectorPosition.z = -maxDistance;
        }

        cameraTransform.localPosition = cameraVectorPosition;
    }
    #endregion Buggé donc désactivé

}
