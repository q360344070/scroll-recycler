using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LibraryPage : MonoBehaviour
{
    public const int TestNumber = 1;

    public ScrollRecycler ScrollRecycler;
    public GameObject CardSectionPrefab;
    public Dictionary<string, CardRecord> CardRecordsByUnitInstance
        = new Dictionary<string, CardRecord>();

    void Start()
    {
        switch (TestNumber)
        {
            case 1:
            Test1();
            break;
            case 2:
            Test2();
            break;
        }
    }

    public CardRecord CreateRecord(string unit)
    {
        CardRecord foundRecord = null;
        CardRecordsByUnitInstance.TryGetValue(unit, out foundRecord);

        if (foundRecord != null)
        {
            var cr = new CardRecord();
            cr.Instance = null;
            cr.unitInstance = "";

            CardRecordsByUnitInstance[unit] = cr;

            return cr;
        }

        return null;
    }

    public void Test1()
    {
        Action createCardSmall = () =>
        {
            GameObject cardSmallGO = ResourceCache.Inst.Create("CardSmall");
            cardSmallGO.transform.SetParent(ScrollRecycler.ScrollRect.content, false);
        };

        for (int i = 0; i < 6; ++i)
        {
            createCardSmall();
        }
    }

    public void Test2()
    {
        LayoutRecycler currLayoutRecycler = null;

        // Instantiate recycler layouts
        currLayoutRecycler =
            ScrollRecycler.InstantiateRecyclerLayout(CardSectionPrefab, ScrollRecycler.ScrollRect.content);

        currLayoutRecycler.AddRecord(CreateRecord("apollo"));

        // Add records
    }

    public void Test3()
    {
    }
}
