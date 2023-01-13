using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BaseController : MonoBehaviour
{
    #region Variables
    [Header("Base Controller")]
    public bool GameStarted = false;
    
    enum HowDestroyEnum { DestroyObject, DisableComponent }
    [SerializeField]
    private HowDestroyEnum HowDestroy = new HowDestroyEnum();

    [Header("Player Only")]

    [SerializeField]
    protected int TurretAmount = 10;

    [SerializeField]
    protected int DestroyedTurrets = 0;

    [SerializeField]
    public bool IsPlayer = false;

    [SerializeField]
    public TankController tankController;

    [SerializeField]
    protected GameObject AimedDestroyableObject;
    [SerializeField]
    protected GameObject PreviouslyAimedDestroyableObject;

    [Header("Life Points & Heal")]
    [SerializeField]
    protected int MaxLifePoint = 1; // -1 means infinite

    [SerializeField]
    protected int LifePoint = 1;

    [SerializeField]
    protected float HealingRate = 1.0f;

    [SerializeField]
    protected int HealingAmount = 1;

    [Header("Ammo & Firing")]
    [SerializeField]
    private int MaxAmmo = -1; // -1 means infinite

    [SerializeField]
    protected int Ammo = 20;

    [SerializeField]
    private GameObject BulletPrefab;

    [SerializeField]
    protected BulletSpawnerData BulletSpawner;

    [Serializable]
    public class BulletSpawnerData
    {
        public GameObject BulletSpawner;
        public GameObject FireFXDirection;
    }

    [SerializeField]
    protected bool Firing = false;

    [SerializeField]
    protected ParticleSystem FiringParticleSystem;

    [SerializeField]
    protected List<BulletSpawnerData> BulletSpawnersList = new List<BulletSpawnerData>();

    [SerializeField]
    private float FireRate = 0.5f;

    [SerializeField]
    private bool CanonLocked = true;

    RaycastHit hit;
    RaycastHit hitAim;

    [Header("Canon, Turrets & body")]

    [SerializeField]
    private float CanonRotationSpeed = 0.5f;

    [SerializeField]
    protected GameObject TurretParent;

    [SerializeField]
    protected GameObject CanonTurret;

    [SerializeField]
    protected GameObject Canon;

    [SerializeField]
    protected float CanonEulerAnglesOffset = 0f;

    [SerializeField]
    protected float CanonMinEulerAngle = 100f;

    [Header("UI")]

    protected UIManager uiManager;

    [SerializeField]
    protected TurretHealthBar healthbar;

    [Header("UI: Aim Icon")]
    [SerializeField]
    GameObject AimIcon;

    [SerializeField]
    GameObject BaseAimIconPosition;

    [SerializeField]
    LayerMask AimLayer;

    [SerializeField]
    float AimingIconMoveSpeed = 3f;

    [SerializeField]
    float AimingIconOffset = 0.5f;

    [Header("FX: visual")]
    [SerializeField]
    private GameObject FXSpawnerDestroyedObjPrefab;

    [SerializeField]
    private GameObject DestroyedFXPositionObj;


    [Header("FX: sound")]
    [SerializeField]
    private AudioClip DestroyedSound;

    [SerializeField]
    private AudioSource DestroyedSoundPlayer;

    public delegate void MessageEvent();
    public static event MessageEvent TankDestroyed;
    public static event MessageEvent TurretDestroyed;
    #endregion Variables

    #region Inits
    private void Start()
    {
        uiManager = FindObjectOfType<UIManager>();
        tankController = FindObjectOfType<TankController>();
    }

    private void OnEnable()
    {
        tankController = FindObjectOfType<TankController>();
        uiManager = FindObjectOfType<UIManager>();
        DetectTurretsAmount();
        InitMaxValuesDisplays();

        if (healthbar != null)
            healthbar.maxHealthPoints = MaxLifePoint;        
    }

    public void StartGame()
    {
        OnEnable();
     //   InitMaxValuesDisplays();
        uiManager.CloseStartMenu();
        GameStarted = true;
        tankController.StartEngine();        
    }

    #endregion Inits

    public int GetLifePoints()
    {
        return LifePoint;
    }

    public int GetMaxLifePoints()
    {
        return MaxLifePoint;
    }

    #region About Enemy Turrets
    void DetectTurretsAmount()
    {
        List<TurretController> turretsList = new List<TurretController>();
        turretsList = FindObjectsOfType<TurretController>().ToList();
        TurretAmount = turretsList.Count;
    }
    
    #endregion About Enemy Turrets

    #region Updates
    private void Update()
    {
        if (!GameStarted)
            return;

        if (!IsPlayer && BulletSpawner.BulletSpawner != null)
            TraceBulletTrajectory(BulletSpawner);       
    }
    private void LateUpdate()
    {
        if (!GameStarted)
            return;

        if (IsPlayer/* && BulletSpawner != null*/)
        {
            TraceTrajectoryForAimingIcon(BulletSpawner.BulletSpawner);
        }
    }
    #endregion Updates

    #region Firing
    protected void Fire()
    {
        // if the canon is not locked by fire rate and has some remaining ammo / infinite ammo
        if (!CanonLocked && (Ammo > 0 || MaxAmmo == -1))
        {
            // verify the trajectory of the future bullet
            if (BulletSpawner.BulletSpawner != null)         
                TraceBulletTrajectory(BulletSpawner);

            // if the object is a turret
            if (!IsPlayer)
            {
                if (BulletSpawner.BulletSpawner != null)
                {
                    // Use a raycast to detect if the target is the player's tank and is aimed and no obstacle is on the way, return the aimed object
                    if (Physics.Raycast(BulletSpawner.BulletSpawner.transform.position, BulletSpawner.BulletSpawner.transform.up, out hit)){

                        // Draw a line in the Editor window from the canon on the trajectory of the future bullet
                        Debug.DrawRay(BulletSpawner.BulletSpawner.transform.position, BulletSpawner.BulletSpawner.transform.up * 20f, Color.white);


                        if (hit.collider.transform.GetComponent<TankController>())
                        {
                            // prepare the fire rate to be applyed
                            LockCanon();

                            if (Firing == false && FiringParticleSystem != null)
                            {
                                Firing = true;
                                FiringParticleSystem.Play(true);
                            }

                            // prepare the bullet to be created
                            InstantiateBulletPrefab(BulletSpawner);
                        }
                        else if (Firing == true && FiringParticleSystem != null)
                        {

                            Firing = false;
                            FiringParticleSystem.Stop(true);
                        }
                    }
                    else if (Firing == true && FiringParticleSystem != null)
                    {
                      
                        Firing = false;
                        FiringParticleSystem.Stop(true);
                    }
                }
                else
                {
                    // shoot with every spawner
                    for ( int i = 0; i < BulletSpawnersList.Count; i++)
                    {
                        if (BulletSpawnersList[i].BulletSpawner != null)
                        {
                            // for trap turrets
                            if (BulletSpawnersList[i].BulletSpawner.GetComponentInParent<DestroyMeByBullets>() == false
                                || (BulletSpawnersList[i].BulletSpawner.GetComponentInParent<DestroyMeByBullets>().enabled == true))
                            // prepare the bullet to be created
                            InstantiateBulletPrefab(BulletSpawnersList[i]);
                        }
                    }
                    // prepare the fire rate to be applyed
                    LockCanon();
                }
            }
            // if the object is the player's tank
            else if (IsPlayer)
            {
                // prepare the fire rate to be applyed
                LockCanon();
                // prepare the bullet to be created
                InstantiateBulletPrefab(BulletSpawner);
            }            
        }
    }

    protected void LockCanon() // lock the canon for the duration of the fire rate timer
    {
        // lock the engine for the FireRate duration to avoid it to shoot constantly
        CanonLocked = true;
        // unlock the canon after the fire rate timer to be ended
        Invoke("UnlockCanon", FireRate);
    }

    protected void UnlockCanon() // unlock the canon at the end of the duration of the fire rate timer
    {
        // unlock the canon for it to be able to shoot
        CanonLocked = false;
    }

    void TraceBulletTrajectory(BulletSpawnerData bulletSpawner)
    {     
        // Use a raycast to detect if the target is aimed and no obstacle on the way, return the aimed object
        if (Physics.Raycast(bulletSpawner.BulletSpawner.transform.position, bulletSpawner.BulletSpawner.transform.up, out hit))
        {
            // Draw a line in the Editor window from the canon on the trajectory of the future bullet
            Debug.DrawRay(bulletSpawner.BulletSpawner.transform.position, bulletSpawner.BulletSpawner.transform.up * 20f);
        }
    }

    protected void InstantiateBulletPrefab(BulletSpawnerData bulletSpawner) // create the bullet
    {
        if (bulletSpawner != null)
        {            
            // Create a bullet and place it on the correct trajectory
            GameObject bullet = Instantiate<GameObject>(BulletPrefab, bulletSpawner.BulletSpawner.transform.position, bulletSpawner.BulletSpawner.transform.rotation);
            BulletController bulletController = bullet.transform.GetComponent<BulletController>();
            bulletController.emiter = bulletSpawner.BulletSpawner;
            bulletController.dirObj = bulletSpawner.FireFXDirection;

            bullet.gameObject.SetActive(true);
            
            // remove one ammo from inventory
            Ammo = Ammo - 1;

            // display ammo count on the UI
            if (IsPlayer)
            {
                uiManager.SetAmmosDisplay(Ammo);

                bulletController.FiredBy = BulletController.FiredByEnum.Player;
            }
            else bulletController.FiredBy = BulletController.FiredByEnum.Enemy;
        }
    }
    
    public GameObject GetFireFXDirectionObj()
    {
        return BulletSpawner.FireFXDirection;
    }
    #endregion Firing

    #region Aiming Icon
    void TraceTrajectoryForAimingIcon(GameObject bulletSpawner)
    {
        // Use a raycast to detect if the target is aimed and no obstacle on the way, return the aimed object
        if (Physics.Raycast(bulletSpawner.transform.position, bulletSpawner.transform.up, out hitAim, 50f, AimLayer))
        {
            // Draw a line in the Editor window from the canon on the trajectory of the future bullet
            Debug.DrawRay(bulletSpawner.transform.position, bulletSpawner.transform.up * 20f);

            HandleAimIcon(hitAim);
        }
        else
        {
            // move the aiming icon to default position
            ResetAimIcon();
        }
    }
    void HandleAimIcon(RaycastHit aimed) 
    {
        #region Draw Aiming Icon for player on target 
        
        // show the icon
        AimIcon.SetActive(true);
        // Smoothly move the aiming Icon To the hit.point position
        AimIcon.transform.position = Vector3.Lerp(AimIcon.transform.position, aimed.point, Time.deltaTime * AimingIconMoveSpeed);
        // rotate the icon to face the camera
        AimIcon.transform.LookAt(Camera.main.transform);

        if (IsPlayer && aimed.transform.GetComponent<TurretController>() == true)
        {
            if (AimedDestroyableObject != null)
            {
                if (PreviouslyAimedDestroyableObject != null 
                    && PreviouslyAimedDestroyableObject.GetComponent<TurretController>())                
                    PreviouslyAimedDestroyableObject.GetComponent<TurretController>().NoMoreAimed();
                PreviouslyAimedDestroyableObject = AimedDestroyableObject;
            }
            AimedDestroyableObject = aimed.transform.gameObject;
            AimedDestroyableObject.GetComponent<TurretController>().SetAsAimed();
        }
        else if (IsPlayer && aimed.transform.GetComponent<DestroyMeByBullets>() == true)
        {
            if (AimedDestroyableObject != null 
                && AimedDestroyableObject.GetComponent<DestroyMeByBullets>())
            {
                if (PreviouslyAimedDestroyableObject != null
                    && PreviouslyAimedDestroyableObject.GetComponent<DestroyMeByBullets>())
                    PreviouslyAimedDestroyableObject.GetComponent<DestroyMeByBullets>().NoMoreAimed();
                PreviouslyAimedDestroyableObject = AimedDestroyableObject;
            }
            AimedDestroyableObject = aimed.transform.gameObject;
            AimedDestroyableObject.GetComponent<DestroyMeByBullets>().SetAsAimed();
        }
        else HideEnemyHealthbar();

        #endregion Draw Aiming Icon for player on target
    }

    void ResetAimIcon()
    {
        // hide the icon
        AimIcon.SetActive(false);
        // hide enemy healthbar
        HideEnemyHealthbar();
    }

    void HideEnemyHealthbar()
    {       
        // hide enemy healthbar
        if (PreviouslyAimedDestroyableObject != null
            && PreviouslyAimedDestroyableObject.transform.GetComponent<TurretController>())
            PreviouslyAimedDestroyableObject.GetComponent<TurretController>().NoMoreAimed();

        else if (AimedDestroyableObject != null
            && AimedDestroyableObject.transform.GetComponent<TurretController>())
            AimedDestroyableObject.GetComponent<TurretController>().NoMoreAimed();

        else if (PreviouslyAimedDestroyableObject != null
            && PreviouslyAimedDestroyableObject.transform.GetComponent<DestroyMeByBullets>())
            PreviouslyAimedDestroyableObject.GetComponent<DestroyMeByBullets>().NoMoreAimed();

        else if (AimedDestroyableObject != null
            && AimedDestroyableObject.transform.GetComponent<DestroyMeByBullets>())
            AimedDestroyableObject.GetComponent<DestroyMeByBullets>().NoMoreAimed();
    }
    #endregion Aiming Icon

    #region Damages & death
    void OnCollisionEnter(Collision collision) // object is collided by anther object, verify if the other is an ennemy bullet
    {
        BulletController bulletController = collision.transform.GetComponent<BulletController>();
        
        if (LifePoint > 0) {
            // verify if the collision is an entering ennemy bullet
            if (bulletController  != null)
            {

           /*     if (!IsPlayer && bulletController.FiredBy == BulletController.FiredByEnum.Enemy)
                { }
                else */

                // Receive damages from the bullet                  
                ReceiveDamages(bulletController.GetDamages());
            }
            // verify is it is an ammo box
            else if (IsPlayer && collision.transform.GetComponent<AmmoBoxManager>())
            {
                if (Ammo < MaxAmmo)
                {
                    // Receive damages from the bullet
                    AddAmmos(collision.transform.GetComponent<AmmoBoxManager>().GetAmmos());
                    Destroy(collision.gameObject);

                    if (IsPlayer)
                        uiManager.SetAmmosDisplay(Ammo);
                }
            }
        }
        else
        {
            // resurrection
        }
    }

    protected void ReceiveDamages(int damages) // the object receives damages from a colliding ennemy bullet
    {
        // verify if the object is not invincible
        if (MaxLifePoint != -1)
        {
            // kill / destroy self at 0 life point left
            if (LifePoint - damages <= 0)
            {
                LifePoint = 0;

                if (tankController == null)
                    tankController = FindObjectOfType<TankController>();

                // verify if the object is a turret
                if (transform != null
                    && tankController != null
                    && tankController.transform != null
                    && tankController.transform != transform)
                {
                    TurretDestroyed?.Invoke();
                   
                }
                DestroySelf();
            }
            // apply damages amount
            else
            {
                LifePoint = LifePoint - damages;
            }

            // update UI about life points
            if (IsPlayer)
                uiManager.SetLifePointsDisplay(LifePoint);
            else if (healthbar != null)
                healthbar.UpdateHealthBar(LifePoint);
        }
    }

    void DestroySelf() // destroy itself and depending objects
    {
        if (FiringParticleSystem != null)
            FiringParticleSystem.Stop();

        if (HowDestroy == HowDestroyEnum.DestroyObject) {
            // destroy the object
            Destroy(CanonTurret);
            Destroy(gameObject);
    
        }
        else if (HowDestroy == HowDestroyEnum.DisableComponent)
        {
            // disable the object
            this.enabled = false;       
        }

        InstantiateFXForDestruction();
        PlayDestructionSound();

        // Open the Win Menu if the Player's tank is destroyed 
        if (IsPlayer)
        {
            TankDestroyed?.Invoke();
            tankController.GameStarted = false;
        }
    }

    void InstantiateFXForDestruction()
    {
        if (FXSpawnerDestroyedObjPrefab != null)
        {
            // Instantiate the particle system at the impact position
            GameObject spawner = Instantiate<GameObject>(FXSpawnerDestroyedObjPrefab, DestroyedFXPositionObj.transform.position,
               DestroyedFXPositionObj.transform.rotation);
        }
    }

    void PlayDestructionSound()
    {
        if (DestroyedSoundPlayer != null)
        {
            DestroyedSoundPlayer.enabled = true;
            DestroyedSoundPlayer.Stop();
            DestroyedSoundPlayer.loop = false;
            DestroyedSoundPlayer.PlayOneShot(DestroyedSound);
        }

    }
    #endregion Damages & death

    void AddAmmos(int amount)
    {
        Ammo = Ammo + amount;
        if (Ammo > MaxAmmo) Ammo = MaxAmmo;


    }

    #region UI
    void InitMaxValuesDisplays() // Assign the max values to the UI at game init
    {        
        // Set initially all the UI about Life points, ammos and turrets
        uiManager.SetLifePointsDisplay(LifePoint);
        uiManager.SetMaxLifePointsDisplay(MaxLifePoint);

        uiManager.SetDestroyedTurretsDisplay(DestroyedTurrets);
        uiManager.SetTurretsAmountDisplay(TurretAmount);

        uiManager.SetAmmosDisplay(Ammo);
        uiManager.SetMaxAmmosDisplay(MaxAmmo);        
    }   
    #endregion UI
}
