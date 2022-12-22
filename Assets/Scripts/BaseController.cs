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

    [SerializeField]
    int TurretAmount = 10;

    [SerializeField]
    protected int DestroyedTurrets = 0;

    [SerializeField]
    public bool IsPlayer = false;

    [SerializeField]
    public TankController tankController;

    [SerializeField]
    protected int MaxLifePoint = 1; // -1 means infinite

    [SerializeField]
    protected int LifePoint = 1;

    [SerializeField]
    protected float HealingRate = 1.0f;

    [SerializeField]
    protected int HealingAmount = 1;

    [SerializeField]
    private int MaxAmmo = -1; // -1 means infinite

    [SerializeField]
    protected int Ammo = 20;

    [SerializeField]
    private GameObject BulletPrefab;

    [SerializeField]
    private GameObject BulletSpawner;

    [SerializeField]
    protected List<GameObject> BulletSpawnersList = new List<GameObject>();

    [SerializeField]
    private float CanonRotationSpeed = 0.5f;

    [SerializeField]
    private float FireRate = 0.5f;

    [SerializeField]
    private bool CanonLocked = true;

    [SerializeField]
    protected GameObject TurretParent;

    [SerializeField]
    protected GameObject CanonTurret;

    RaycastHit hit;
    RaycastHit hitAim;

    protected UIManager uiManager;
   
    [SerializeField]
    GameObject AimIcon;


    [SerializeField]
    GameObject BaseAimIconPosition;

    [SerializeField]
    LayerMask AimLayer;

    [SerializeField]
    float AimingIconMoveSpeed = 3f;
    #endregion Variables

    private void OnEnable()
    {
        tankController = FindObjectOfType<TankController>();
        uiManager = FindObjectOfType<UIManager>();
        DetectTurretsAmount();
    }

    public void StartGame()
    {
        InitMaxValuesDisplays();
        uiManager.CloseStartMenu();
        GameStarted = true;
    }

    void DetectTurretsAmount()
    {
        List<TurretController> turretsList = new List<TurretController>();
        turretsList = FindObjectsOfType<TurretController>().ToList();
        TurretAmount = turretsList.Count;
    }

    private void Update()
    {
        if (!GameStarted)
            return;

        

        if (!IsPlayer && BulletSpawner != null)
            TraceBulletTrajectory(BulletSpawner);       
    }
    private void LateUpdate()
    {
        if (!GameStarted)
            return;

        if (IsPlayer/* && BulletSpawner != null*/)
        {
            TraceTrajectoryForAimingIcon(BulletSpawner);
        }
    }
    protected void Fire()
    {
        // if the canon is not locked by fire rate and has some remaining ammo / infinite ammo
        if (!CanonLocked && (Ammo > 0 || MaxAmmo == -1))
        {
            // verify the trajectory of the future bullet
            if (BulletSpawner != null)         
                TraceBulletTrajectory(BulletSpawner);

            // if the object is a turret
            if (!IsPlayer)
            {
                if (BulletSpawner != null)
                {
                    // Use a raycast to detect if the target is the player's tank and is aimed and no obstacle is on the way, return the aimed object
                    if (Physics.Raycast(BulletSpawner.transform.position, BulletSpawner.transform.up, out hit)
                    && hit.collider.transform.GetComponent<TankController>())
                    {
                        // Draw a line in the Editor window from the canon on the trajectory of the future bullet
                        Debug.DrawRay(BulletSpawner.transform.position, BulletSpawner.transform.up * 20f);

                        // prepare the fire rate to be applyed
                        LockCanon();
                        // prepare the bullet to be created
                        InstantiateBulletPrefab(BulletSpawner);
                    }
                }
                else
                {
                    // shoot with every spawner
                    for ( int i = 0; i < BulletSpawnersList.Count; i++)
                    {
                        Debug.Log(i.ToString());
                        // prepare the bullet to be created
                        InstantiateBulletPrefab(BulletSpawnersList[i]);
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

    void TraceBulletTrajectory(GameObject bulletSpawner)
    {     
        // Use a raycast to detect if the target is aimed and no obstacle on the way, return the aimed object
        if (Physics.Raycast(bulletSpawner.transform.position, bulletSpawner.transform.up, out hit))
        {
            // Draw a line in the Editor window from the canon on the trajectory of the future bullet
            Debug.DrawRay(bulletSpawner.transform.position, bulletSpawner.transform.up * 20f);

         //   HandleAimIcon(hit.point);
        }
    }
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

        /*    AimIcon.transform.position = BaseAimIconPosition.transform.position;
            AimIcon.transform.LookAt(Camera.main.transform);
        */
        }
    }
    void HandleAimIcon(RaycastHit aimed) 
    {
        #region Draw Aiming Icon for player on target    
        
    //    AimIcon.transform.position = aimed.point;
        AimIcon.transform.position = Vector3.Lerp(AimIcon.transform.position, aimed.point, Time.deltaTime * AimingIconMoveSpeed);
        AimIcon.transform.LookAt(Camera.main.transform);
   
        #endregion Draw Aiming Icon for player on target
    }

    void ResetAimIcon()
    {
        #region Draw Aiming Icon for player on target    

    //    AimIcon.transform.position = BaseAimIconPosition.transform.position;
        AimIcon.transform.position = Vector3.Lerp(AimIcon.transform.position, BaseAimIconPosition.transform.position, Time.deltaTime * AimingIconMoveSpeed);
        AimIcon.transform.LookAt(Camera.main.transform);

        #endregion Draw Aiming Icon for player on target
    }

    protected void InstantiateBulletPrefab(GameObject bulletSpawner) // create the bullet
    {
        // Create a bullet and place it on the correct trajectory
        Instantiate<GameObject>(BulletPrefab, bulletSpawner.transform.position, bulletSpawner.transform.rotation);

        // remove one ammo from inventory
        Ammo = Ammo - 1;

        // display ammo count on the UI
        if (IsPlayer) 
            uiManager.SetAmmosDisplay(Ammo);
    }

    void OnCollisionEnter(Collision collision) // object is collided by anther object, verify if the other is an ennemy bullet
    {
        // verify if the collision is an entering ennemy bullet
        if (collision.transform.GetComponent<BulletController>())
        {
            // Receive damages from the bullet
            ReceiveDamages(collision.transform.GetComponent<BulletController>().GetDamages());
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
                    // add one destroyed turret to counter
                    tankController.DestroyedTurrets = tankController.DestroyedTurrets + 1;

                    // update UI about destroyed turrets
                    if (!IsPlayer)
                        uiManager.SetDestroyedTurretsDisplay(tankController.DestroyedTurrets);

                    // verify if all turrets are destroyed
                    VerifyDestroyedTurretsAmount();
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
        }
    }

    void VerifyDestroyedTurretsAmount() // verify if all ennemy turrets are destroyed
    {
        // verify if all turrets are destroyed
        if (tankController.DestroyedTurrets == TurretAmount)
        {
            // open win menu if all ennemies are destroyed
            uiManager.OpenWinMenu();
        }
    }

    void DestroySelf() // destroy itself and depending objects
    {
        // destroy the object regardless it is the player's tank or a turret
        Destroy(CanonTurret);
        Destroy(gameObject);
        
        // Open the Win Menu if the Player's tank is destroyed 
        if (IsPlayer)
            uiManager.OpenFailMenu();
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
