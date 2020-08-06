using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MenuNPC : MonoBehaviour
{
    private Animator anim;
    private NavMeshAgent npcNav;

    void Start()
    {
        anim = GetComponent<Animator>();
        npcNav= GetComponent<NavMeshAgent>();
        npcNav.SetDestination(GetRandomLocation());
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
