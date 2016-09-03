using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LibraryPage : MonoBehaviour
{
    public ScrollRecycler ScrollRecycler;
    public GameObject CardSectionPrefab;
    public Dictionary<string, CardRecord> CardRecordsByUnitInstance = new Dictionary<string, CardRecord>();

    void Start()
    {
    }

    public CardRecord CreateRecord(string unit)
    {
        CardRecord foundRecord = null;

        if (!CardRecordsByUnitInstance.TryGetValue(unit, out foundRecord))
        {
            var cr = new CardRecord();
            cr.Instance = null;
            cr.unitInstance = unit;

            CardRecordsByUnitInstance[unit] = cr;

            return cr;
        }

        return null;
    }
}
