using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public enum ColorMode { Select, Edit };

public class GlobalController : MonoBehaviour
{
    // Transform and positions
    class TransformInfo
    {
        public Vector3 pos;
        public Quaternion rotate;
        public Vector3 scale;
    }

    private static List<GameObject> bones = new List<GameObject>();
    private static List<TransformInfo> originalTransform = new List<TransformInfo>();
    private static List<GameObject> adjustedBones = new List<GameObject>();
    private static List<TransformInfo> originalTransformAdjusted = new List<TransformInfo>();

    // Colors and opacities
    public static ColorMode gColorMode { get; set; }
    
    public static int numberOfBones = 0, numberOfAdjustedBones = 1;

    // Slider
    private static GameObject slider;

    // Start is called before the first frame update
    void Start()
    {

        //DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/MyDemo/Assets/Meshes");
        //FileInfo[] fis = di.GetFiles();

        //foreach (FileInfo fi in fis)
        //{
        //    // File Name Convensions : 
        //    // Bone_1.obj for base bones,
        //    // Bone_#.obj for fragments,
        //    // Bone_#_aligned.obj for adjusted fragments,
        //    // Bone_{#+1}_aligned.obj for addition artifitial structures.

        //    //Debug.Log(fi.Name);
        //    if (fi.Extension.Contains("obj"))
        //    {
        //        if (fi.Name.Contains("aligned"))
        //            numberOfAdjustedBones++;
        //        else
        //            numberOfBones++;
        //    }
        //}

        numberOfBones = 6;
        numberOfAdjustedBones = 7;

        Debug.Log("Number of bones loaded: " + numberOfBones);
        Debug.Log("Number of adjusted bones loaded: " + numberOfAdjustedBones);

        for (int i = 1; i <= numberOfBones; i++)
        {
            GameObject t = GameObject.Find("Bone_" + i);
            bones.Add(t);
            Transform tr = t.GetComponent<Transform>();
            TransformInfo ti = new TransformInfo
            {
                pos = tr.localPosition,
                rotate = tr.localRotation,
                scale = tr.localScale
            };
            originalTransform.Add(ti);
        }

        for (int i = 1; i <= numberOfAdjustedBones; i++)
        {
            GameObject t = GameObject.Find("Bone_" + i + "_aligned");
            adjustedBones.Add(t);
            Transform tr = t.GetComponent<Transform>();
            TransformInfo ti = new TransformInfo
            {
                pos = tr.localPosition,
                rotate = tr.localRotation,
                scale = tr.localScale
            };
            originalTransformAdjusted.Add(ti);
        }

        gColorMode = ColorMode.Select;

        slider = GameObject.Find("PinchSlider");
    }

    public void ResetPositions()
    {
        Debug.Log("Reset Botton Pressed");
        Debug.Log(originalTransform.Count);

        for (int i = 1; i < originalTransform.Count; i++)
        {
            bones[i].transform.localPosition = originalTransform[i].pos;
            bones[i].transform.localRotation = originalTransform[i].rotate;
            bones[i].transform.localScale = originalTransform[i].scale;
        }

        adjustedBones[0].transform.localPosition = originalTransformAdjusted[0].pos;
        adjustedBones[0].transform.localRotation = originalTransformAdjusted[0].rotate;
        adjustedBones[0].transform.localScale = originalTransformAdjusted[0].scale;

        //for(int i = 0; i<originalTransformAdjusted.Count; i++)
        //{
        //    adjustedBones[i].transform.localPosition =  originalTransformAdjusted[i].pos;
        //    adjustedBones[i].transform.localRotation =  originalTransformAdjusted[i].rotate;
        //    adjustedBones[i].transform.localScale =     originalTransformAdjusted[i].scale;
        //}

    }

    public void ResetColorForAll()
    {
        foreach(GameObject o in bones)
        {
            o.GetComponent<AdjustBoneColor>().ResetColor();
        }
    }

    public void ShowOrHideAdjustments()
    {
        TextMeshPro[] texts = GameObject.Find("ShowAdjustment").GetComponentsInChildren<TextMeshPro>();

        if (adjustedBones[0].activeInHierarchy)
        {
            adjustedBones[0].SetActive(false);
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Show Adjustment";
            }
        }else
        {
            adjustedBones[0].SetActive(true);
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Hide Adjustment";
            }
        }
    }

    public void ShowOrHideSlider()
    {
        TextMeshPro[] texts = slider.GetComponentsInChildren<TextMeshPro>();

        if (slider.activeSelf)
        {
            slider.SetActive(false);
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Show Slider";
            }
        }
        else
        {
            slider.SetActive(true);
            foreach (TextMeshPro tmp in texts)
            {
                tmp.text = "Hide Slider";
            }
        }
    }

    public void ChangeColoringMode()
    {
        gColorMode = 1 - gColorMode;
        TextMeshPro[] texts = GameObject.Find("EditOpacityButton").GetComponentsInChildren<TextMeshPro>();
        foreach(TextMeshPro tmp in texts)
        {
            if (gColorMode == ColorMode.Select)
                tmp.text = "Edit Opacity";
            else
                tmp.text = "Fix Opacity";
        }
    }
}
