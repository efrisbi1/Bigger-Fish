using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCcontrol : MonoBehaviour
{
    public SharkController shark;
    private Animator anim;
    private NavMeshAgent npcNav;
    public Transform sharkTrans;

    [Header("NPC Stats")]
    public DamageSystem npcHp;
    [SerializeField] double npcHealth;
    [SerializeField] double npcDamage;
    [SerializeField] GameObject hitmark;

    // Start is called before the first frame update
    void Start()
    {
        npcHp = new DamageSystem(100.0, 20.0);
        npcHealth = npcHp.getHp();
        npcDamage = npcHp.getDam();
        anim = GetComponent<Animator>();
        npcNav= GetComponent<NavMeshAgent>();
        npcNav.SetDestination(GetRandomLocation());
        hitmark.SetActive(false);
    }

    // Update is called once per frame
    void Update()
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
        if (npcHp.getHp()==0.0)
        {
            Destroy(GetComponent<CapsuleCollider>());
            Destroy(this);
            Destroy(npcNav);
            anim.SetBool("isSwim", false);
            anim.SetBool("isDead", true);
        }
    }
    private void OnTriggerEnter(Collider sMouth)
    {
        if (sMouth.gameObject.tag == "SharkMouth")
        {
            npcHp.damage(shark.sharkStat.getDam());
            StartCoroutine(hitmarker());
            Debug.Log("NPC HP: " + npcHp.getHp());
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
}
