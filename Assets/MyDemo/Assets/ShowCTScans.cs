using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

using Dicom;
using Dicom.Imaging;
using Dicom.Log;

using Microsoft.MixedReality.Toolkit.UI;

public class ShowCTScans : MonoBehaviour
{
    int currentValue = 1;

    // Start is called before the first frame update
    void Start()
    {
        updateImage();
    }

    // Update is called once per frame
    public void Update()
    {
        PinchSlider slider = GameObject.Find("PinchSlider").GetComponent<PinchSlider>();
        int value = (int) (slider.SliderValue * 2000) + 3;
        Debug.Log("Update, " + value);

        if (value != currentValue)
        {
            currentValue = value;
            updateImage();
        }
    }

    private void updateImage()
    {
        var image = new DicomImage("E:\\ETH\\Mixed Reality Lab\\Data\\pt2\\pt2\\DICOM\\" + currentValue);
        var m_Renderer = GetComponent<Renderer>();
        m_Renderer.material.SetTexture("_MainTex", image.RenderImage().AsTexture2D());
    }
}
