using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionGolemHead : MonoBehaviour
{

    private BossInteractionTheGolem mainInteractionScript;
    void Start()
    {
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.tag == "Sword" && mainInteractionScript.canTakeDamage == true)
        {
            mainInteractionScript.golemHeadHit();
        }
    }
}
