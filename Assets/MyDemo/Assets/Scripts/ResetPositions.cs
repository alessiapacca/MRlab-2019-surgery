using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;

public class ResetPositions : MonoBehaviour
{
    private List<GameObject> bones = new List<GameObject>();
    private List<Transform> transformOfBones = new List<Transform>();
    private ReadOnlyCollection<Transform> transformOfBonesCopy;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 1; i <= 6; i++)
        {
            GameObject t = GameObject.Find("Bone_" + i);
            bones.Add(t);
            Transform tr = t.GetComponent<Transform>();
            //Debug.Log(tr);
            transformOfBones.Add(tr);
        }
        Debug.Log("Init: #bones, " + bones.Count);
        Debug.Log("Init: #transforms, " + transformOfBones.Count);

        //transformOfBonesCopy = new ReadOnlyCollection<Transform>(transformOfBones);
        //Debug.Log("Num of Bones1 , " + transformOfBonesCopy.Count);

    }

    void Update()
    {

    }

    public void resetPositions()
    {
        //    StartMethod example = new StartMethod();
        //    bones = example.getBones();
        //    transformOfBones = example.getTransform();

        Debug.Log("Botton Pressed");

        Debug.Log("Num of Bones, " + transformOfBones.Count);

        for (int i = 1; i <=6 ; i++)
        {
            //Debug.Log(transformOfBones[i]);
            Vector3 pos = GameObject.Find("BoneCollection").transform.position;
            GameObject.Find("Bone_"+i).transform.SetPositionAndRotation(pos, new Quaternion(0, 0, 0, 1));
        }

    }
}
