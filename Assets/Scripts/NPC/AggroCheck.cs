using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AggroCheck : MonoBehaviour
{
    [SerializeField] patrolNPC pnpc;
    [SerializeField] BossNPC Bnpc;

    private void OnTriggerStay(Collider sharkCol)
    {
        if (sharkCol.gameObject.tag == "Shark")
        {
            try
            {
                pnpc.Target();
            }
            catch(Exception e)
            {

            }
            try
            {
                Bnpc.Target();
            }
            catch (Exception e)
            {

            }
        }
    }
}
