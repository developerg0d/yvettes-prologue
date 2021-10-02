using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{



    void Start()
    {
        StartCoroutine("f");
    }

    IEnumerator f()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(3f);
            yield return new WaitForFixedUpdate();
        }
    }
}
