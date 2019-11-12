using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorMode { Select, Edit };

public class GlobalController : MonoBehaviour
{
    public static ColorMode gColorMode { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        gColorMode = ColorMode.Select;
    }

    public void ChangeColoringMode()
    {
        gColorMode = 1 - gColorMode;
    }
}
