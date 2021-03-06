﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class BossNPC : MonoBehaviour
{
    public SharkController shark;
    private Animator anim;
    private NavMeshAgent npcNav;
    public Transform sharkTrans, npcTrans;

    [Header("NPC Stats")]
    public DamageSystem npcHp;
    [SerializeField] double npcHealth;
    [SerializeField] double npcDamage;
    [SerializeField] GameObject hitmark, feedText, aggro,spin, comp;
    [SerializeField] int feedCount;

    // Start is called before the first frame update
    void Start()
    {
        npcHp = new DamageSystem(npcHealth, npcDamage);
        anim = GetComponent<Animator>();
        npcNav= GetComponent<NavMeshAgent>();
        npcNav.SetDestination(GetRandomLocation());
        this.GetComponent<BoxCollider>().enabled = false;
        npcTrans = this.transform;
        hitmark.SetActive(false);
        feedText.SetActive(false);
        spin.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (npcTrans.position.y > 110)
        {
            npcTrans.position = new Vector3(npcTrans.position.x, 77, npcTrans.position.z);
            npcNav.baseOffset=25f;
            npcNav.height= 30f;

        }

        if (npcHp.getHp() != 0)
        {
            if (!npcNav.pathPending)
            {
                if (npcNav.remainingDistance <= npcNav.stoppingDistance)
                {
                    if (!npcNav.hasPath || npcNav.velocity.sqrMagnitude == 0f)
                    {
                        npcNav.SetDestination(GetRandomLocation());
                    }
                }
            }
        }
        if (npcHp.getHp()==0.0)
        {
            Destroy(anim);
            Destroy(npcNav);
            comp.SetActive(true);

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene("Menu");
            }

        }
    }
    private void OnTriggerEnter(Collider sMouth)
    {
        if (npcHp.getHp()!=0.0 && sMouth.gameObject.tag == "SharkMouth")
        {
            npcHp.damage(shark.sharkStat.getDam());
            StartCoroutine(hitmarker());
            Debug.Log("NPC HP: " + npcHp.getHp());
        }
    }

    private void OnTriggerStay(Collider sharkCol)
    {
        shark.Bnpc = this;
        if (npcHp.getHp() == 0.0 && sharkCol.gameObject.tag== "Shark")
        {
            feedText.SetActive(true);
            shark.Feed();
        }
    }
    private void OnTriggerExit(Collider sharkCol)
    {
        if (npcHp.getHp() == 0.0 && sharkCol.gameObject.tag == "Shark")
        {
            feedText.SetActive(false);
            shark.Bnpc = null;
        }
    }
    public void Fed()
    {
        Debug.Log("NPC Food Left: " + feedCount);
        if (feedCount > 0)
        {
            feedCount -= 1;
        }
        else if(feedCount<=0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator hitmarker()
    {
        hitmark.SetActive(true);
        yield return new WaitForSeconds(.25f);
        hitmark.SetActive(false);
    }

    private Vector3 GetRandomLocation()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        // Pick the first indice of a random triangle in the nav mesh
        int t = Random.Range(0, navMeshData.indices.Length - 3);

        // Select a random point on it
        Vector3 point = Vector3.Lerp(navMeshData.vertices[navMeshData.indices[t]], navMeshData.vertices[navMeshData.indices[t + 1]], Random.value);
        Vector3.Lerp(point, navMeshData.vertices[navMeshData.indices[t + 2]], Random.value);

        return point;
    }
    private bool pathComplete()
    {
        if (Vector3.Distance(npcNav.destination, npcNav.transform.position) <= npcNav.stoppingDistance)
        {
            if (!npcNav.hasPath || npcNav.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }

        return false;
    }

    float targetY;
    float targetOff;
    float targetHeight;

    public void Target()
    {
        npcNav.SetDestination(sharkTrans.position);

        targetY = sharkTrans.position.y-1f;
        targetHeight = targetY * .6f;
        targetOff = targetHeight-5.0f;

        if (npcTrans.position.y > targetY)
            npcTrans.Translate(0f, -.2f, 0f);
        if (npcTrans.position.y < targetY)
            npcTrans.Translate(0f, .2f, 0f);
        if (npcNav.baseOffset > targetOff)
            npcNav.baseOffset -= .2f;
        if (npcNav.baseOffset < targetOff)
            npcNav.baseOffset += .2f;
        if (npcNav.height > targetHeight)
            npcNav.height -= .2f;
        if (npcNav.height < targetHeight)
            npcNav.height += .2f;

        if (npcNav.remainingDistance <= 15f)
        {
            npcNav.SetDestination(npcTrans.position);
            anim.SetBool("isAttack", true);
        }
        else
            anim.SetBool("isAttack", false);
    }

    bool isCom = true;
}
