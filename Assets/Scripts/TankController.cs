using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : BaseController
{
    #region Variables

    [Header("Tank Controller")]

    

    [SerializeField]
    float MaxVelocity = 1.0f;

    [SerializeField]
    float Velocity = 1.0f;

    [SerializeField]
    float RotationSpeed = 1.0f;
       

    Rigidbody _rigidBody;
       

    [SerializeField]
    GameObject cameraObject;
    #endregion Variables

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        cameraObject = FindObjectOfType<Camera>().gameObject;
    }

    void Update()
    {
        if (!GameStarted)
            return;

        UpdateInput();       
    }
    void UpdateInput()
    {
        HandleTankVelocity();
        HandleTankRotation();
        HandleTankCanonRotation();
        HandleFire();
    }
    
    #region Movement & rotation
    void HandleTankVelocity()
    {
        // get the input and move the tank forward and backward depending of the max velocity
        if (Input.GetAxis("Vertical") > 0 || Input.GetKey(KeyCode.Z))
        {
            transform.Translate(Vector3.forward * (Time.deltaTime * MaxVelocity));
        }
        else if (Input.GetAxis("Vertical") < 0 || Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * (Time.deltaTime * MaxVelocity));
        }
    }

    void HandleTankRotation()
    {
        if (Input.GetAxis("Horizontal") != 0)
        {
            // rotate Horizontally the body of the tank by getting the input value and multiplying it by the rotation speed
            transform.Rotate(0.0f, Input.GetAxis("Horizontal")*RotationSpeed, 0.0f, Space.World);
        }

    }

    public GameObject GetTurretObj()
    {
        return CanonTurret;
    }

    public void HandleTankCanonRotation()
    {
        // Stick the position of the tank turret to its position on the tank
        CanonTurret.transform.position = TurretParent.transform.position;

        // Synchronize the rotation of the turret and the camera lookat direction
        // by calculating the degrees to rotate arround the 3 axes from x and z of the turret rotation
        // and the y of the main camera.

    /*        CanonTurret.transform.localRotation = 
                Quaternion.Euler(TurretParent.transform.localRotation.x, 
                Camera.main.transform.eulerAngles.y, TurretParent.transform.localRotation.z);
      */  

        
        CanonTurret.transform.localRotation =
            Quaternion.Euler(Camera.main.transform.eulerAngles.x -22.5f,
            Camera.main.transform.eulerAngles.y, TurretParent.transform.localRotation.z);
        
    }

    #endregion Movement & rotation

    #region Firing
    void HandleFire()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Fire();
        }        
    }
    
    #endregion Firing
    
    #region Heal
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "HealPlatform")
        {
            StartHeal();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "HealPlatform")
        {
            EndHeal();
        }
    }

    void StartHeal()
    {
        InvokeRepeating("Healing", 0, HealingRate);
    }

    void Healing()
    {
        int tmpLife = LifePoint;
        if (LifePoint < MaxLifePoint)
        {
            tmpLife = LifePoint + HealingAmount;            
        }
        
        if (tmpLife > MaxLifePoint)
        {
            tmpLife = MaxLifePoint;
        }
        LifePoint = tmpLife;

        // update UI about life points
        if (IsPlayer)
            uiManager.SetLifePointsDisplay(LifePoint);
    }

    void EndHeal()
    {
        CancelInvoke("Healing");
    }
    #endregion Heal
}
