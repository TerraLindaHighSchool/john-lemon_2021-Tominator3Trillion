﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveFromParent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
