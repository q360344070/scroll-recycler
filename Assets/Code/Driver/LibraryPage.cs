using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LibraryPage : MonoBehaviour
{
    public const int TestNumber = 2;

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

    // #debug - Entire Update is debugging
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Break();
        }
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

        //currLayoutRecycler.AddRecord(CreateRecord("apollo"));
        //currLayoutRecycler.AddRecord(CreateRecord("hades"));
        //currLayoutRecycler.AddRecord(CreateRecord("ganymede"));
        //currLayoutRecycler.AddRecord(CreateRecord("europa"));

        for (int i = 0; i < 1000; ++i)
        {
            currLayoutRecycler.AddRecord(CreateRecord(Guid.NewGuid().ToString()));
        }


        // Add records
    }

    public void Test3()
    {
    }
}
