// Copyright (c) 2012-2017 Anders Gustafsson, Cureos AB.
// All rights reserved. Any unauthorised reproduction of this 
// material will constitute an infringement of copyright.

#if UNITY_WSA

using System.IO;

using UnityEngine;

using Dicom;
using Dicom.Imaging;
using Dicom.Log;

public class Test : MonoBehaviour
{
    private string _dump;
    private Texture2D _texture;

    // Use this for initialization
    private void Start()
    {
#if UNITY_EDITOR
        using (var stream =
            new MemoryStream(UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/fo-dicom/Demo/Images/image_dfl.bytes")
                .bytes))
#else
        using (var stream = File.OpenRead("E:/Data/DICOM/CT-MONO2-8-abdo.dcm"))
#endif
        {
            var file = DicomFile.Open(stream);
            var anonymizer = new DicomAnonymizer();
            anonymizer.AnonymizeInPlace(file);

            _dump = file.WriteToString();
            _texture = new DicomImage(file.Dataset).RenderImage().AsTexture2D();
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, _texture.width, _texture.height), _texture);
        GUI.Label(new Rect(_texture.width, 0, Screen.width - _texture.width, Screen.height), _dump);
    }
}

#else

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Dicom;
using Dicom.Imaging;
using Dicom.Log;
using Dicom.Network;

using UnityEngine;

/// <summary>
/// Script initializes a simple C-STORE SCP server that accepts single DICOM images to be sent to port 11112.
/// Provided the image is stored in a transfer syntax that is recognized by fo-dicom for Unity, the received image
/// is displayed on screen, together with a text dump of the content.
/// </summary>
public class Test : MonoBehaviour
{
    private const string StoragePath = @".\DICOM";

    private static readonly object _lock = new object();

    private static string _newPath;

    private string _dump;
    private Texture2D _texture;
    private DicomAnonymizer _anonymizer;
    private IDicomServer _server;

    // Use this for initialization
    private void Start()
    {
        Application.targetFrameRate = 1;

        const int port = 11112;

        // preload dictionary to prevent timeouts
        var dict = DicomDictionary.Default;

        // start DICOM server on port 11112
        _server = DicomServer.Create<CStoreSCP>(port);
        Debug.Log($"Starting C-Store SCP server, listening on {_server.IPAddress}:{_server.Port}");

        _anonymizer = new DicomAnonymizer();
        _newPath = null;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_newPath == null) return;
        Debug.Log($"New path: {_newPath}");

        var file = DicomFile.Open(_newPath);
        _anonymizer.AnonymizeInPlace(file);
        Debug.Log($"File format: {file.Format}, contains pixel data: {file.Dataset.Contains(DicomTag.PixelData)}");

        _texture = file.Dataset.Contains(DicomTag.PixelData)
            ? new DicomImage(file.Dataset).RenderImage()?.AsTexture2D()
            : null;

        _dump = file.WriteToString();
        _newPath = null;
    }

    private void OnGUI()
    {
        if (_texture != null)
            GUI.DrawTexture(new Rect(0, 0, _texture.width, _texture.height), _texture);

        if (_dump != null)
            GUI.Label(
                new Rect(_texture != null ? _texture.width : 0, 0,
                    Screen.width - (_texture != null ? _texture.width : 0), Screen.height), _dump);
    }

    private class CStoreSCP : DicomService, IDicomServiceProvider, IDicomCStoreProvider, IDicomCEchoProvider
    {
        private static readonly DicomTransferSyntax[] AcceptedTransferSyntaxes =
        {
            DicomTransferSyntax.ExplicitVRLittleEndian,
            DicomTransferSyntax.ExplicitVRBigEndian,
            DicomTransferSyntax.ImplicitVRLittleEndian
        };

        private static readonly DicomTransferSyntax[] AcceptedImageTransferSyntaxes =
        {
            // Lossless
            DicomTransferSyntax.JPEGLSLossless,
            DicomTransferSyntax.JPEG2000Lossless,
            DicomTransferSyntax.RLELossless,

            // Lossy
            DicomTransferSyntax.JPEGLSNearLossless,
            DicomTransferSyntax.JPEG2000Lossy,
            DicomTransferSyntax.JPEGProcess1,

            // Uncompressed
            DicomTransferSyntax.ExplicitVRLittleEndian,
            DicomTransferSyntax.ExplicitVRBigEndian,
            DicomTransferSyntax.ImplicitVRLittleEndian
        };

        public CStoreSCP(INetworkStream stream, Encoding fallbackEncoding, Dicom.Log.Logger log)
            : base(stream, fallbackEncoding, log)
        {
        }

        public Task OnReceiveAssociationRequestAsync(DicomAssociation association)
        {
            foreach (var pc in association.PresentationContexts)
            {
                if (pc.AbstractSyntax == DicomUID.Verification) pc.AcceptTransferSyntaxes(AcceptedTransferSyntaxes);
                else if (pc.AbstractSyntax.StorageCategory != DicomStorageCategory.None)
                    pc.AcceptTransferSyntaxes(AcceptedImageTransferSyntaxes);
            }

            Debug.LogFormat("{0}", association);

            return SendAssociationAcceptAsync(association);
        }

        public Task OnReceiveAssociationReleaseRequestAsync()
        {
            return SendAssociationReleaseResponseAsync();
        }

        public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
        {
        }

        public void OnConnectionClosed(Exception exception)
        {
        }

        public DicomCStoreResponse OnCStoreRequest(DicomCStoreRequest request)
        {
            Debug.LogFormat("Received request, instance UID: {0}, transfer syntax: {1}", request.SOPInstanceUID.UID,
                request.TransferSyntax);

            var studyUid = request.Dataset.Get<string>(DicomTag.StudyInstanceUID);
            var instUid = request.SOPInstanceUID.UID;

            var path = Path.GetFullPath(StoragePath);
            path = Path.Combine(path, studyUid);

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            path = Path.Combine(path, instUid) + ".dcm";
            request.File.Save(path);

            lock (_lock)
                _newPath = path;

            return new DicomCStoreResponse(request, DicomStatus.Success);
        }

        public void OnCStoreRequestException(string tempFileName, Exception e)
        {
            // let library handle logging and error response
        }

        public DicomCEchoResponse OnCEchoRequest(DicomCEchoRequest request)
        {
            return new DicomCEchoResponse(request, DicomStatus.Success);
        }
    }
}

#endif
