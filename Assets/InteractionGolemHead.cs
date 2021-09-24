using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionGolemHead : MonoBehaviour
{

    [SerializeField] int recoilForce = 65;
    private BossInteractionTheGolem mainInteractionScript;
    void Start()
    {
        mainInteractionScript = GetComponentInParent<BossInteractionTheGolem>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.tag == "Sword" && mainInteractionScript.canTakeDamage == true)
        {
            golemHeadRecoil(col.gameObject);
            mainInteractionScript.golemHeadHit();
        }
    }

    void golemHeadRecoil(GameObject colider)
    {
        Rigidbody2D rb = colider.GetComponent<Rigidbody2D>();

        rb.AddForce(Vector2.left * recoilForce, ForceMode2D.Impulse);
        rb.AddForce(Vector2.up * (recoilForce / 2), ForceMode2D.Impulse);
    }
}
