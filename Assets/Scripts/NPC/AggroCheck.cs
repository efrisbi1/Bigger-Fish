using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroCheck : MonoBehaviour
{
    [SerializeField] patrolNPC pnpc;

    private void OnTriggerStay(Collider sharkCol)
    {
        if (sharkCol.gameObject.tag == "Shark")
        {
            pnpc.Target();
        }
    }
}
