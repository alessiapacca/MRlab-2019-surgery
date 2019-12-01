using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public class CTReader : MonoBehaviour {
    public GameObject referenceCube;
    public GameObject referencePlane;
    public TextAsset ct;
    public ComputeShader slicer;

    NRRD nrrd;

    int kernel;
    RenderTexture rtex;
    Texture2D tex;

    Transform tf;

    void Start() {
        tf = referenceCube.GetComponent<Transform>();
        nrrd = new NRRD(ct);
        kernel = slicer.FindKernel("CSMain");

        tf.localPosition = nrrd.origin;
        tf.localScale = Vector3.Scale(nrrd.scale, new Vector3(nrrd.dims[0], nrrd.dims[1], nrrd.dims[2]));

        var buf = new ComputeBuffer(nrrd.data.Length, sizeof(float));
        buf.SetData(nrrd.data);
        slicer.SetBuffer(kernel, "data", buf);
        slicer.SetInts("dims", nrrd.dims);

        rtex = new RenderTexture(512, 512, 1);
        rtex.enableRandomWrite = true;
        rtex.Create();
        slicer.SetTexture(kernel, "slice", rtex);
        slicer.SetInts("outDims", new int[] { rtex.width, rtex.height });

        tex = new Texture2D(rtex.width, rtex.height);
        GetComponent<Renderer>().material.mainTexture = tex;
    }

    void Update() {
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, Handedness.Right, out MixedRealityPose po1) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out MixedRealityPose po2) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, Handedness.Right, out MixedRealityPose po3)) {
            var p1 = tf.InverseTransformPoint(po1.Position);
            var p2 = tf.InverseTransformPoint(po2.Position);
            var p3 = tf.InverseTransformPoint(po3.Position);
            var plane = new Plane(p1, p2, p3);

            var orig = plane.ClosestPointOnPlane(Vector3.zero);
            var dy = (p2 - p1).normalized;
            var dx = Vector3.Cross(dy, plane.normal);
            dy /= rtex.height / 2;
            dx /= rtex.width / 2;

            var rp = referencePlane.GetComponent<Transform>();
            rp.up = plane.normal;
            rp.localPosition = p2;

            slicer.SetFloats("orig", new float[] { orig.x, orig.y, orig.z });
            slicer.SetFloats("dx", new float[] { dx.x, dx.y, dx.z });
            slicer.SetFloats("dy", new float[] { dy.x, dy.y, dy.z });
            slicer.Dispatch(kernel, (rtex.width + 7) / 8, (rtex.height + 7) / 8, 1);

            RenderTexture.active = rtex;
            tex.ReadPixels(new Rect(0, 0, rtex.width, rtex.height), 0, 0);
            tex.Apply();
        }
    }
}

public class NRRD {
    readonly public Dictionary<String, String> headers = new Dictionary<String, String>();
    readonly public float[] data;
    readonly public int[] dims;

    readonly public Vector3 origin = new Vector3(0, 0, 0);
    readonly public Vector3 scale = new Vector3(1, 1, 1);

    public NRRD(TextAsset asset) {
        using (var reader = new BinaryReader(new MemoryStream(asset.bytes))) {
            for (string line = reader.ReadLine(); line.Length > 0; line = reader.ReadLine()) {
                if (line.StartsWith("#") || !line.Contains(":")) continue;
                var tokens = line.Split(':');
                var key = tokens[0].Trim();
                var value = tokens[1].Trim();
                headers.Add(key, value);
            }

            if (headers["dimension"] != "3") throw new ArgumentException("NRRD is not 3D");
            if (headers["type"] != "float") throw new ArgumentException("NRRD is not of type float");
            if (headers["endian"] != "little") throw new ArgumentException("NRRD is not little endian");
            if (headers["encoding"] != "gzip") throw new ArgumentException("NRRD is not gzip encoded");

            dims = Array.ConvertAll(headers["sizes"].Split(), s => int.Parse(s));
            if (headers.ContainsKey("space origin")) {
                var origin = Array.ConvertAll(headers["space origin"].Substring(1, headers["space origin"].Length - 2).Split(','), v => float.Parse(v));
                this.origin = new Vector3(origin[0], origin[1], origin[2]);
            }
            if (headers.ContainsKey("space directions")) {
                var scale = Array.ConvertAll(headers["space directions"].Split(), s => Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => float.Parse(v)));
                if (scale[0][0] == 0 || scale[1][1] == 0 || scale[2][2] == 0) throw new ArgumentException("NRRD has 0 scale value");
                if (scale[0][1] != 0 || scale[1][0] != 0 || scale[2][0] != 0 ||
                    scale[0][2] != 0 || scale[1][2] != 0 || scale[2][1] != 0) throw new ArgumentException("NRRD is not axis-aligned");
                this.scale = new Vector3(scale[0][0], scale[1][1], scale[2][2]);
            }

            var mem = new MemoryStream();
            using (var stream = new GZipStream(reader.BaseStream, CompressionMode.Decompress)) stream.CopyTo(mem);
            data = new float[dims[0] * dims[1] * dims[2]];
            Buffer.BlockCopy(mem.ToArray(), 0, data, 0, data.Length * sizeof(float));
        }
    }
}

public static class BinaryReaderExtension {
    public static string ReadLine(this BinaryReader reader) {
        var line = new StringBuilder();
        for (bool done = false; !done;) {
            var ch = reader.ReadChar();
            switch (ch) {
                case '\r':
                    if (reader.PeekChar() == '\n') reader.ReadChar();
                    done = true;
                    break;
                case '\n':
                    done = true;
                    break;
                default:
                    line.Append(ch);
                    break;
            }
        }
        return line.ToString();
    }
}
