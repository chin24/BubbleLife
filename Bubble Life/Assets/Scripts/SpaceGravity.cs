﻿using UnityEngine;
using System.Collections;

public class SpaceGravity : MonoBehaviour {

    private GameObject[] objs;
    internal int simCount = 1000;
    public float simScale = 1f;

    float M1 = 0;
    int M1PosInd = 0;

    // Use this for initialization
    void Start () {
        objs = GameObject.FindGameObjectsWithTag("food");

        //Figure out "Sun Object" (largest mass)
        
        for (int b = 0; b < objs.Length; b++)
        {
            if (objs[b].GetComponent<Rigidbody2D>().mass > M1)
            {
                M1 = objs[b].GetComponent<Rigidbody2D>().mass;
                M1PosInd = b;
            }
        }

        //Figure out initial SOI

        for (int b = 0; b < objs.Length; b++)
        {
            float m1 = objs[b].GetComponent<Rigidbody2D>().mass;

            objs[b].GetComponent<SpaceTrajectory>().SOI = CalcSOI(m1, M1, Vector3.Distance(objs[M1PosInd].transform.position, objs[b].transform.position));

            //Vector3 force = Vector3.zero;
        }

        //Initial velocities

        for (int i = 0; i < 2; i++)
        {
            for (int b = 0; b < objs.Length; b++)
            {
                float m1 = objs[b].GetComponent<Rigidbody2D>().mass;

                float m2 = M1;
                Vector3 m2Pos = objs[M1PosInd].transform.position;
                Vector2 m2Vel = objs[M1PosInd].GetComponent<Rigidbody2D>().velocity;
                Vector3 distance = m2Pos - objs[b].transform.position;

                Vector3 force = univGrav(m1, m2, distance) * Time.deltaTime;

                for (int c = 0; c < objs.Length; c++)
                {
                    if (c != b)
                    {
                        Vector3 otherDist = objs[c].transform.position - objs[b].transform.position;

                        if (otherDist.sqrMagnitude < distance.sqrMagnitude && (otherDist.magnitude - objs[c].GetComponent<SpaceTrajectory>().SOI) < 0)
                        {
                            m2Pos = objs[c].transform.position;
                            m2 = objs[c].GetComponent<Rigidbody2D>().mass;
                            m2Vel += objs[c].GetComponent<Rigidbody2D>().velocity;
                            distance = m2Pos - objs[b].transform.position;

                            force += univGrav(m1, m2, distance) * Time.deltaTime;
                        }
                    }


                }

                //Apply Velocity
                //Vector3 force = univGrav(m1, m2, distance) * Time.deltaTime;
                Vector3 vel = CentripicalForceVel(m1, distance.magnitude, force.magnitude) * Tangent(force.normalized) + new Vector3(m2Vel.x, m2Vel.y);
                objs[b].GetComponent<Rigidbody2D>().velocity = vel;

            }
        }
        

    }
	
	// Update is called once per frame
	void Update () {

        //Update Sun Object
        for (int b = 0; b < objs.Length; b++)
        {
            float m1 = objs[b].GetComponent<Rigidbody2D>().mass;
            if (m1 > M1)
            {
                M1 = m1;
                M1PosInd = b;
            }

            objs[b].GetComponent<SpaceTrajectory>().SOI = CalcSOI(m1, M1, Vector3.Distance(objs[M1PosInd].transform.position, objs[b].transform.position));
        }

        //Draw Sphere of Influence


        //Forces
        for (int b = 0; b < objs.Length; b++)
        {
            float m1 = objs[b].GetComponent<Rigidbody2D>().mass;

            float m2 = M1;
            Vector3 m2Pos = objs[M1PosInd].transform.position;
            Vector2 m2Vel = objs[M1PosInd].GetComponent<Rigidbody2D>().velocity;
            Vector3 distance = m2Pos - objs[b].transform.position;
            int m2PosInd = M1PosInd;

            Vector3 force = univGrav(m1, m2, distance);
            

            for (int c = 0; c < objs.Length; c++)
            {
                if (c != b)
                {
                    Vector3 otherDist = objs[c].transform.position - objs[b].transform.position;

                    if (otherDist.sqrMagnitude < distance.sqrMagnitude && (otherDist.magnitude - objs[c].GetComponent<SpaceTrajectory>().SOI) < 0)
                    {
                        m2Pos = objs[c].transform.position;
                        m2 = objs[c].GetComponent<Rigidbody2D>().mass;
                        m2Vel += objs[M1PosInd].GetComponent<Rigidbody2D>().velocity;
                        distance = m2Pos - objs[b].transform.position;
                        m2PosInd = c;
                        force += univGrav(m1, m2, distance);
                    }
                }
                
                
            }

            //Apply Force
            //Vector3 force = univGrav(m1, m2, distance);
            //Vector3 vel = CentripicalForceVel(m1, distance.magnitude, force.magnitude) * Tangent(force.normalized).normalized;
            //objs[b].GetComponent<Rigidbody2D>().velocity = vel;
            objs[b].GetComponent<Rigidbody2D>().AddForce(force * Time.deltaTime);
            objs[b].GetComponent<SpaceTrajectory>().m2Pos = m2Pos;
            objs[b].GetComponent<SpaceTrajectory>().m2Vel = objs[m2PosInd].GetComponent<Rigidbody2D>().velocity;
            objs[b].GetComponent<SpaceTrajectory>().m2 = m2;
            objs[b].GetComponent<SpaceTrajectory>().distance = distance;

        }
    }

    void OnDrawGizmos()
    {
        for (int b = 0; b < objs.Length; b++)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(objs[b].transform.position, objs[b].GetComponent<SpaceTrajectory>().SOI);
        }

            
        
    }

    public float CalcSOI(float m1, float M1, float r)
    {
        float rSOI = r * Mathf.Pow(m1 / M1, 0.4f);
        return rSOI;
    }

    public Vector3 univGrav(float m1, float m2, Vector3 distance)
    {
        if (distance == Vector3.zero)
            return Vector3.zero;
        float G = 10f;//.0000000000667f;

        float r3 = Mathf.Pow(distance.sqrMagnitude, 1.5f);

        Vector3 force = (G * m1 * m2 * distance) / r3;
        //print("Force Added: " + force);
        return force;
    }

    public Vector3 Tangent(Vector3 normal)
    {
        Vector3 tangent = Vector3.Cross(normal, Vector3.forward);

        if (tangent.magnitude == 0)
        {
            tangent = Vector3.Cross(normal, Vector3.up);
        }

        return tangent;
    }

    public float CentripicalForceVel( float m1, float r, float force)
    {
        return Mathf.Sqrt((force * r) / m1);
    }
}
