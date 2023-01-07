using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class TurretController : BaseController
{
    #region Variables
    enum rotationDirectionEnum {Random, Left, Right }

    [Header("Turret Controller")]

    [SerializeField]
    rotationDirectionEnum rotationDirection = rotationDirectionEnum.Random;

    [SerializeField]
    float rotationSpeed = 5;
    float rotateValue;

    [SerializeField]
    bool playerDetected = false;

    [SerializeField]
    TankController target;

    [SerializeField]
    float TurretFireRange = 10f;

    [SerializeField]
    float viewRadius = 10f;

    [SerializeField]
    float viewAngle = 10f;    

    #endregion Variables

    private void Awake()
    {
        tankController = FindObjectOfType<TankController>();
        RandomizeRotationDirection();
    }

    void RandomizeRotationDirection()
    {
        float ran = Random.Range(-10.0f, 10.0f);
        if (ran < 0)
            rotationDirection = rotationDirectionEnum.Left;
        else rotationDirection = rotationDirectionEnum.Right;
    }

    private void Update()
    {
        if (!tankController.GameStarted)
            return;


        if (target != null) AimPlayer();
        else LurkForPlayer();
    }

    void LurkForPlayer()
    {
        RotateTurretCanon();
    }

    void RotateTurretCanon()
    {
        if (rotationDirection == rotationDirectionEnum.Left)
            rotateValue = 0 - rotationSpeed;
        else rotateValue = rotationSpeed;

        CanonTurret.transform.Rotate(0.0f, rotateValue, 0.0f, Space.Self);
    }

    void AimPlayer()
    {
        // calculate relative position between target position and turret position
    //    Vector3 relativePos = target.transform.position - CanonTurret.transform.position;
        Vector3 relativePos = target.transform.position - BulletSpawner.BulletSpawner.transform.position;

        // transform to rotation the look direction of relativePosition
        Quaternion toRotation = Quaternion.LookRotation(relativePos);

        // apply interpolation between the rotation of the turret and the rotation the look direction of relativePosition
        CanonTurret.transform.rotation = Quaternion.Lerp(CanonTurret.transform.rotation, toRotation, 2.5f * Time.deltaTime);

        if (GetDistanceToPlayer() <= TurretFireRange) // fire a projectile if the target is in range
            HandleFire();
                 
        else if (Firing == true && FiringParticleSystem != null)
        {

            Firing = false;
            FiringParticleSystem.Stop(true);
        }
        
    }

    public void SetTarget(TankController tankController) // receive the target detected by TurretDetectorTrigger
    {
        target = tankController;
    }

    float GetDistanceToPlayer() // Return the calculated distance between the Turret and the target
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        return distance;
    }

    void HandleFire()
    {
        Fire();
    }

    public void SetAsAimed()
    {
        if (healthbar != null) healthbar.gameObject.SetActive(true);
    }

    public void NoMoreAimed()
    {
        if (healthbar != null) healthbar.gameObject.SetActive(false);
    }
}
