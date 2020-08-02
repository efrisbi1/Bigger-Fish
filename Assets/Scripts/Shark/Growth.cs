using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Growth : MonoBehaviour
{
    private GameObject shark;
    private Vector3 scaleChange;

    // Start is called before the first frame update
    void Start()
    {
        shark = GameObject.Find("WhiteShark");
        scaleChange = new Vector3(0.00001f, 0.00001f, 0.00001f);
    }

    // Update is called once per frame
    void Update()
    {
        shark.transform.localScale += scaleChange;
    }
}
