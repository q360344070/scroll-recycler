﻿using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class CellData
{
    public GameObject Instance; // Instantiated gameObject
    public RectTransformData RectTransformData = new RectTransformData();
    public LayoutData LayoutData = new LayoutData();

    // TODO Specify if records need more than one Layout calculation per grouping
}
