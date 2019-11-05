// Copyright (c) 2012-2017 Anders Gustafsson, Cureos AB.
// All rights reserved. Any unauthorised reproduction of this 
// material will constitute an infringement of copyright.

using Dicom;
using Dicom.Network;

using UnityEngine;

public class StoreScu : MonoBehaviour
{
#if !UNITY_WSA

    private const string FileLocation = "Assets/fo-dicom/Demo/Images/";

    private readonly string[] _fileNames = { "image_dfl", "US-RGB-8-epicard", "VL3_J2KI" };

    private readonly DicomClient _client = new DicomClient { Linger = 0 };

    private int _currIdx = 0;

    // Use this for initialization
    private void Start()
    {
        _client.AssociationAccepted += (sender, args) => Debug.Log("Association accepted");
        _client.AssociationRejected += (sender, args) => Debug.Log("Association rejected");
        _client.AssociationReleased += (sender, args) => Debug.Log("Association released");
    }

    // Update is called once per frame
    private void Update()
    {
        var fileName = _fileNames[_currIdx++];
        if (_currIdx == _fileNames.Length) _currIdx = 0;

        var path = $"{FileLocation}{fileName}.bytes";
        Debug.LogFormat("Path: {0}", path);

        var file = DicomFile.Open(path);
        Debug.LogFormat("File format: {0}", file.Format);

        var request = new DicomCStoreRequest(file);
        request.OnResponseReceived = (rq, rsp) =>
        {
            Debug.LogFormat("Transfer syntax: {0}, SOP instance UID: {1}", rq.TransferSyntax, rq.SOPInstanceUID.UID);
            Debug.LogFormat("Response status: {0}", rsp.Status);
        };

        Debug.LogFormat("Request created, instance UID: {0}, transfer syntax: {1}", request.SOPInstanceUID.UID,
            request.TransferSyntax);

        _client.AddRequest(request);
        _client.Send("127.0.0.1", 11112, false, "STORESCU", "STORESCP");
    }

#endif
}
