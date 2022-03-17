using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStats : MonoBehaviour
{
    public int currentHp;
    public int maxHp;
    void Start()
    {
        currentHp = maxHp;
    }

}
