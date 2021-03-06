﻿using UnityEngine;
using System.Collections;

public class BaseObject : MonoBehaviour {

    public float health = 100;
    public AudioClip laserHit;
    public AudioClip crashSound;

    protected LevelScript levelScript;
    protected Rigidbody2D rigid;
    protected AudioSource source;

    protected float volLow = .5f;
    protected float volHigh = 1f;
    protected float freqLow = .75f;
    protected float freqHigh = 1.25f;

    protected float velMod = .1f;

    // Use this for initialization
    void Awake () {
        source = GetComponent<AudioSource>();
        levelScript = GameObject.FindGameObjectWithTag("LevelController").GetComponent<LevelScript>();
        rigid = gameObject.GetComponent<Rigidbody2D>();

    }
	
	// Update is called once per frame
	protected void CheckBorder () {

        if (transform.position.x < levelScript.minWorldPosX)
        {
            transform.position = new Vector3(levelScript.maxWorldPosX, transform.position.y, transform.position.z);
        }
        else if (transform.position.y < levelScript.minWorldPosY)
        {
            transform.position = new Vector3(transform.position.x, levelScript.maxWorldPosY, transform.position.z);
        }
        else if (transform.position.x > levelScript.maxWorldPosX)
        {
            transform.position = new Vector3(levelScript.minWorldPosX, transform.position.y, transform.position.z);
        }
        else if (transform.position.y > levelScript.maxWorldPosY)
        {
            transform.position = new Vector3(transform.position.x, levelScript.minWorldPosY, transform.position.z);
        }

    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Rigidbody2D colRigid = col.transform.GetComponent<Rigidbody2D>();
        Vector2 finalVel = InelasticCollision(rigid.mass, rigid.velocity, colRigid.mass, colRigid.velocity);

        float damage = Mathf.Abs(rigid.velocity.magnitude - finalVel.magnitude);

        health -= damage * damage;

        if (CompareTag("AI"))
            transform.GetComponent<AI>().fitness -= damage * damage;

        if (col.transform.tag == "Projectile")
        {
            source.pitch = 1;
            source.PlayOneShot(laserHit, Random.Range(.5f, 1f));
        }
        else if (col.transform.tag == "Asteroid")
        {
            source.pitch = Random.Range(freqLow, freqHigh);
            float vol = Random.Range(volLow, volHigh) * rigid.velocity.sqrMagnitude * velMod;
            source.PlayOneShot(crashSound, vol);
        }

    }

    public static Vector2 InelasticCollision(float mass1, Vector2 vel1, float mass2, Vector2 vel2)
    {

        Vector2 finalVel = (mass1 * vel1 + mass2 * vel2) / (mass1 + mass2);
        return finalVel;
    }
}
