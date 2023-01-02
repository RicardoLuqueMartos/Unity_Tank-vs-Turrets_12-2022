using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : BaseController
{
    #region Variables

    [Header("Tank Controller")]    

    [SerializeField]
    float maxVelocity = 1.0f;

    [SerializeField]
    float velocity = 1.0f;

    [SerializeField]
    float rotationSpeed = 1.0f;       

    Rigidbody _rigidBody;       

    [SerializeField]
    GameObject cameraObject;    
    
    public enum StateEnum { Starting, Idle, MoveForward, MoveBackward, RotateInPlace, Destroyed }

    [SerializeField]
    StateEnum state = StateEnum.Starting;
   
    [SerializeField]
    float startingEngineDuration = 1.0f;

    [Serializable]
    public class FXData
    {
        public GameObject smockParticlesPrefab;
       
        public SoundFXData soundFXs = new SoundFXData();
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
    private FXData fXs = new FXData();
    #endregion Variables

    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        cameraObject = FindObjectOfType<Camera>().gameObject;
    }    

    public void StartEngine()
    {
        fXs.soundFXs.MoveSoundPlayer.enabled = true;
        fXs.soundFXs.MoveSoundPlayer.PlayOneShot(fXs.soundFXs.StartingSound);
        Invoke("StartIdleSound", startingEngineDuration/2);
        Invoke("EngineStarted", startingEngineDuration);
    }
    public void StartIdleSound()
    {
        fXs.soundFXs.IdleSoundPlayer.enabled = true;
        fXs.soundFXs.IdleSoundPlayer.loop = true;
        fXs.soundFXs.MoveSoundPlayer.clip = fXs.soundFXs.IdleSound;
        fXs.soundFXs.MoveSoundPlayer.Play();
    }
        void EngineStarted()
    {
        state = StateEnum.Idle;
        fXs.soundFXs.MoveSoundPlayer.loop = true;
    }

    void Update()
    {
        if (!GameStarted)
            return;

        if (state == StateEnum.Starting)
            return;

        UpdateInput();
        UpdateAudio();
    }

    void UpdateAudio()
    {
        if (state != StateEnum.Idle)
        {
            if (state == StateEnum.MoveForward
                && fXs.soundFXs.MoveSoundPlayer.clip != fXs.soundFXs.MoveForwardSound)
            {
                fXs.soundFXs.MoveSoundPlayer.Stop();
                fXs.soundFXs.MoveSoundPlayer.clip = fXs.soundFXs.MoveForwardSound;
                fXs.soundFXs.MoveSoundPlayer.Play();
            }
            else if (state == StateEnum.MoveBackward
                && fXs.soundFXs.MoveSoundPlayer.clip != fXs.soundFXs.MoveBackwardSound)
            {
                fXs.soundFXs.MoveSoundPlayer.Stop();
                fXs.soundFXs.MoveSoundPlayer.clip = fXs.soundFXs.MoveBackwardSound;
                fXs.soundFXs.MoveSoundPlayer.Play();
            }
            else if (state == StateEnum.RotateInPlace
                && fXs.soundFXs.MoveSoundPlayer.clip != fXs.soundFXs.RotateSound)
            {
                fXs.soundFXs.MoveSoundPlayer.Stop();
                fXs.soundFXs.MoveSoundPlayer.clip = fXs.soundFXs.RotateSound;
                fXs.soundFXs.MoveSoundPlayer.Play();
            }
        }
        else
        {
            fXs.soundFXs.MoveSoundPlayer.Stop();
            fXs.soundFXs.MoveSoundPlayer.clip = null;
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
            transform.Translate(Vector3.forward * (Time.deltaTime * maxVelocity));
            state = StateEnum.MoveForward;
        }
        else if (Input.GetAxis("Vertical") < 0 || Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * (Time.deltaTime * maxVelocity));
            state = StateEnum.MoveBackward;
        }
        else
        {
            state = StateEnum.Idle;
        }
    }

    void HandleTankRotation()
    {
        if (Input.GetAxis("Horizontal") != 0)
        {
            // rotate Horizontally the body of the tank by getting the input value and multiplying it by the rotation speed
            transform.Rotate(0.0f, Input.GetAxis("Horizontal") * rotationSpeed, 0.0f, Space.World);

            if (state == StateEnum.Idle && (Input.GetAxis("Vertical") == 0 || Input.GetKey(KeyCode.None)))
                state = StateEnum.RotateInPlace;
        }
        else if (Input.GetAxis("Vertical") == 0 || Input.GetKey(KeyCode.None) )
        {
            state = StateEnum.Idle;
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
        if (fXs.soundFXs.TurretRotateSoundPlayer.clip != fXs.soundFXs.TurretRotateSound)
        {
            fXs.soundFXs.TurretRotateSoundPlayer.enabled = true;
            fXs.soundFXs.TurretRotateSoundPlayer.loop = true;
            fXs.soundFXs.TurretRotateSoundPlayer.clip = fXs.soundFXs.TurretRotateSound;
            fXs.soundFXs.TurretRotateSoundPlayer.Play();
        }
    }

    public void StopTurretRotateSound()
    {
        fXs.soundFXs.TurretRotateSoundPlayer.Stop();
        fXs.soundFXs.TurretRotateSoundPlayer.clip = null;
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
