using System;
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

    public enum StateEnum { Starting, Idle, MoveForward, MoveBackward, RotateInPlace, Destroyed }

    [SerializeField]
    StateEnum State = StateEnum.Starting;
   
    [SerializeField]
    float StartingEngineDuration = 1.0f;

    [Serializable]
    public class FXData
    {
        public GameObject SmockParticlesPrefab;
       
        public SoundFXData SoundFXs = new SoundFXData();
    }

    [Serializable]
    public class SoundFXData
    {
        public AudioSource IdleSoundPlayer;
        public AudioClip IdleSound;
        public AudioSource MoveSoundPlayer;
        public AudioClip StartingSound;
        public AudioClip MoveForwardSound;
        public AudioClip MoveBackwardSound;
        public AudioClip RotateSound;
        public AudioSource TurretRotateSoundPlayer;
        public AudioClip TurretRotateSound;
       
        //   public List<AudioClip> IdleSoundsList = new List<AudioClip>();
        //   public List<AudioClip> MoveForwardSoundsList = new List<AudioClip>();
        //   public List<AudioClip> MoveBackwardSoundsList = new List<AudioClip>();
        //   public List<AudioClip> MoveRotateInPlaceSoundsList = new List<AudioClip>();
    }
    [SerializeField]
    private FXData FXs = new FXData();
    #endregion Variables

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        cameraObject = FindObjectOfType<Camera>().gameObject;
    }    

    public void StartEngine()
    {
        FXs.SoundFXs.MoveSoundPlayer.enabled = true;
        FXs.SoundFXs.MoveSoundPlayer.PlayOneShot(FXs.SoundFXs.StartingSound);
        Invoke("StartIdleSound", StartingEngineDuration/2);
        Invoke("EngineStarted", StartingEngineDuration);
    }
    public void StartIdleSound()
    {
        FXs.SoundFXs.IdleSoundPlayer.enabled = true;
        FXs.SoundFXs.IdleSoundPlayer.loop = true;
        FXs.SoundFXs.MoveSoundPlayer.clip = FXs.SoundFXs.IdleSound;
        FXs.SoundFXs.MoveSoundPlayer.Play();
    }

    void EngineStarted()
    {
        State = StateEnum.Idle;
        FXs.SoundFXs.MoveSoundPlayer.loop = true;
    }

    void Update()
    {
        if (!GameStarted)
            return;

        if (State == StateEnum.Starting)
            return;

        UpdateInput();
        UpdateAudio();
    }

    void UpdateAudio()
    {
        if (State != StateEnum.Idle)
        {
            if (State == StateEnum.MoveForward
                && FXs.SoundFXs.MoveSoundPlayer.clip != FXs.SoundFXs.MoveForwardSound)
            {
                FXs.SoundFXs.MoveSoundPlayer.Stop();
                FXs.SoundFXs.MoveSoundPlayer.clip = FXs.SoundFXs.MoveForwardSound;
                FXs.SoundFXs.MoveSoundPlayer.Play();
            }
            else if (State == StateEnum.MoveBackward
                && FXs.SoundFXs.MoveSoundPlayer.clip != FXs.SoundFXs.MoveBackwardSound)
            {
                FXs.SoundFXs.MoveSoundPlayer.Stop();
                FXs.SoundFXs.MoveSoundPlayer.clip = FXs.SoundFXs.MoveBackwardSound;
                FXs.SoundFXs.MoveSoundPlayer.Play();
            }
            else if (State == StateEnum.RotateInPlace
                && FXs.SoundFXs.MoveSoundPlayer.clip != FXs.SoundFXs.RotateSound)
            {
                FXs.SoundFXs.MoveSoundPlayer.Stop();
                FXs.SoundFXs.MoveSoundPlayer.clip = FXs.SoundFXs.RotateSound;
                FXs.SoundFXs.MoveSoundPlayer.Play();
            }
        }
        else
        {
            FXs.SoundFXs.MoveSoundPlayer.Stop();
            FXs.SoundFXs.MoveSoundPlayer.clip = null;
        }
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
            State = StateEnum.MoveForward;
        }
        else if (Input.GetAxis("Vertical") < 0 || Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * (Time.deltaTime * MaxVelocity));
            State = StateEnum.MoveBackward;
        }
        else
        {
            State = StateEnum.Idle;
        }
    }

    void HandleTankRotation()
    {
        if (Input.GetAxis("Horizontal") != 0)
        {
            // rotate Horizontally the body of the tank by getting the input value and multiplying it by the rotation speed
            transform.Rotate(0.0f, Input.GetAxis("Horizontal") * RotationSpeed, 0.0f, Space.World);

            if (State == StateEnum.Idle && (Input.GetAxis("Vertical") == 0 || Input.GetKey(KeyCode.None)))
                State = StateEnum.RotateInPlace;
        }
        else if (Input.GetAxis("Vertical") == 0 || Input.GetKey(KeyCode.None) )
        {
            State = StateEnum.Idle;
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
        TurretParent.transform.rotation = transform.rotation;
        // Synchronize the rotation of the turret and the camera lookat direction
        // by calculating the degrees to rotate arround the 3 axes from x and z of the turret rotation
        // and the y of the main camera.        
        CanonTurret.transform.localRotation =
            Quaternion.Euler(TurretParent.transform.localRotation.x,
            Camera.main.transform.eulerAngles.y, TurretParent.transform.localRotation.z);

        float CanonEulerAngle = Camera.main.transform.eulerAngles.x - CanonEulerAnglesOffset;

        if (Camera.main.transform.eulerAngles.x < CanonMinEulerAngle)
        {
            Canon.transform.localRotation = Quaternion.Euler(Camera.main.transform.eulerAngles.x - CanonEulerAnglesOffset,
                   Canon.transform.localRotation.y, Canon.transform.localRotation.z);
        }
    }

    public void HandleTurretRotateSound()
    {
        if (FXs.SoundFXs.TurretRotateSoundPlayer.clip != FXs.SoundFXs.TurretRotateSound)
        {
            FXs.SoundFXs.TurretRotateSoundPlayer.enabled = true;
            FXs.SoundFXs.TurretRotateSoundPlayer.loop = true;
            FXs.SoundFXs.TurretRotateSoundPlayer.clip = FXs.SoundFXs.TurretRotateSound;
            FXs.SoundFXs.TurretRotateSoundPlayer.Play();
        }
    }

    public void StopTurretRotateSound()
    {
        FXs.SoundFXs.TurretRotateSoundPlayer.Stop();
        FXs.SoundFXs.TurretRotateSoundPlayer.clip = null;
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
