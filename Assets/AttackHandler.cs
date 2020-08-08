using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHandler : MonoBehaviour
{
    public GameObject mouth;

    public void MegAttackEvent()
    {
        mouth.SetActive(true);
    }

    public void MegCoolEvent()
    {
        mouth.SetActive(false);
    }
}
