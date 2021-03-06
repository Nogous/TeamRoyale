﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    private Transform origin;
    [HideInInspector]
    public List<Transform> destination;
    private GameObject destinationList;

    [Header("Speed")]
    public float speed = 3f;

    private GameObject detectionZone;
    private int i = 0;

    public Animator ennemycontroller;

    // Start is called before the first frame update
    void Start()
    {
        origin = this.transform;
        detectionZone = transform.GetChild(1).gameObject;
        destinationList = transform.parent.GetChild(1).gameObject;

        for(int i = 0; i < destinationList.transform.childCount; i++) 
        {
            destination.Add(destinationList.transform.GetChild(i));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (origin.transform.position.x < destination[i].transform.position.x - 0.5f)
        {
            ennemycontroller.SetInteger("leftrightint", 1);
            ennemycontroller.SetInteger("updownint", 0);
        }

        if (origin.transform.position.x > destination[i].transform.position.x + 0.5f)
        {
            ennemycontroller.SetInteger("leftrightint", -1);
            ennemycontroller.SetInteger("updownint", 0);
        }

        if (origin.transform.position.y > destination[i].transform.position.y + 0.5f)
        {
            ennemycontroller.SetInteger("updownint", -1);
            ennemycontroller.SetInteger("leftrightint", 0);
        }

        if (origin.transform.position.y < destination[i].transform.position.y - 0.5f)
        {
            ennemycontroller.SetInteger("updownint", 1);
            ennemycontroller.SetInteger("leftrightint", 0);
        }

        if (speed > 0)
        {
            float ratio = Time.deltaTime * speed;
            transform.position = Vector2.MoveTowards(origin.position, destination[i].position, ratio);

            if (Vector2.Distance(this.transform.position, destination[i].position) < 0.5f)
            {
                origin = this.transform;
                i++;
                if (i >= destination.Count)
                {
                    i = 0;
                }
            }
        }

        Vector3 difPos = destination[i].position - transform.position;
        float rotationZ = Mathf.Atan2(difPos.y, difPos.x) * Mathf.Rad2Deg;
        detectionZone.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
    }
}
