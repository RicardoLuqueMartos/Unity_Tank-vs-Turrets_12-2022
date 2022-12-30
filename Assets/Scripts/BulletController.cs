using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BulletController : MonoBehaviour
{
    #region Variable
    enum MoveSystemEnum { ByForce, ByTranslate}
    [SerializeField]
    private MoveSystemEnum MoveSystem = new MoveSystemEnum();



    [SerializeField]
    private bool ExplodeByItself = false;

    [SerializeField]
    private int Damages = 1;

    [SerializeField]
    private float MaxDistance = 1;

    [SerializeField]
    private float MoveSpeed = 20;

    bool moving = true;

    [SerializeField]
    private float bounceForce = 20;

    Vector3 StartPosition;

    Rigidbody _rigidBody;

    [SerializeField]
    private GameObject FXSpawnerFireObjPrefab;

    [SerializeField]
    private GameObject FXSpawnerImpactObjPrefab;

 //   [SerializeField]
    public GameObject emiter;
    public GameObject DirObj;
    Collision _collision;

    #endregion Variable

    void OnEnable()
    {
        StartPosition = transform.position;
        _rigidBody = GetComponent<Rigidbody>();
        moving = true;
        PlayFireFXs();
    }

    private void Update()
    {
        if (moving) // the bullet has been shot and a force is applied to it
        {
            if (MoveSystem == MoveSystemEnum.ByForce)
            {
                //apply a force to the bullet to move it
                _rigidBody.AddForce(transform.up * MoveSpeed);
            }
            else if (MoveSystem == MoveSystemEnum.ByTranslate)
            {
                transform.Translate(Vector3.up * (Time.deltaTime * MoveSpeed));
            }
        }

        // get the distance between
        if (GetDistance() >= MaxDistance) DestroySelf();
    }

    float GetDistance() // returns the distance between the Bullet and its starting position 
    {
        float distance = Vector3.Distance(StartPosition, transform.position);

        return distance;
    }

    public int GetDamages() // returns the damages from the bullet to apply to the target
    {
        return Damages;
    }

    void OnCollisionEnter(Collision collision) // when the bullet collides with an object
    {
        if (collision.transform.tag != "NoBulletCollision")
        {
            _collision = collision;

            // destroy the bullet after it to collide with anything       
            DestroySelf();
        }
    }

    void ApplyExplosionForce(Vector3 _position, Transform _target) // Apply an Explosion Force at the collision point
    {
        // get the rigidbody of the collided object to apply the explosion force to
        
        if (_target != null){
            Rigidbody otherRigidBody = _target.GetComponent<Rigidbody>();

            // apply the explosion force to the target rigidbody if relevant
            if (otherRigidBody != null)
                otherRigidBody.AddExplosionForce(bounceForce, _position, 5);
        }
    }

    #region FXs
    void PlayFireFXs()
    {
        if (FXSpawnerFireObjPrefab != null)
        {
            //   GameObject DirObj = emiter.GetComponent<BaseController>().GetFireFXDirectionObj();

            if (DirObj != null)
            {
                // Instantiate the particle system at the impact position
                GameObject spawner = Instantiate<GameObject>(FXSpawnerFireObjPrefab, StartPosition,
                   DirObj.transform.rotation);

                // assign the origin position of the weapon fire               
                //   spawner.GetComponent<FXSpawner>().SourceObjPosition = DirObj.transform.position;


                spawner.GetComponent<FXSpawner>().startPosition = DirObj.transform.position;

                // launch the FXs system
                spawner.GetComponent<FXSpawner>().InitSystem(true);
            }
        }
    }

    void PlayImpactFXs(Vector3 _position)
    {
        if (FXSpawnerImpactObjPrefab != null)
        {
            // Instantiate the particle system at the impact position
            GameObject spawner = Instantiate<GameObject>(FXSpawnerImpactObjPrefab, _position, new Quaternion(0,0,0,0));

            // assign the origin position of the weapon fire
            spawner.GetComponent<FXSpawner>().SourceObjPosition = StartPosition;

            // launch the FXs system
            spawner.GetComponent<FXSpawner>().InitSystem(true);
        }
    }
    #endregion FXs

    void DestroySelf() // Destroy the bullet by itself
    {
        if (ExplodeByItself || _collision != null)
        {
            if (_collision != null)
            {
                PlayImpactFXs(_collision.GetContact(0).point);

                // Apply an Explosion Force at the collision point
                ApplyExplosionForce(_collision.GetContact(0).point, _collision.transform);
            }
            else
            {
                PlayImpactFXs(transform.position);
                // Apply an Explosion Force at the collision point
                ApplyExplosionForce(transform.position, null);
            }
        }

        Destroy(gameObject);
    }
}
