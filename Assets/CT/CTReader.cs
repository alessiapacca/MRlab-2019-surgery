using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public class CTReader : MonoBehaviour {
    public TextAsset ctobj;
    public ComputeShader typeConverter;
    NRRD nrrd;

    Texture2D tex;
    Gradient grad;

    void Start() {
        nrrd = new NRRD(ctobj);
        tex = new Texture2D(nrrd.sizes[0], nrrd.sizes[1], TextureFormat.RGBA32, false);
        GetComponent<Renderer>().material.mainTexture = tex;

        grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.white, 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
    }

    void Update() {
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out MixedRealityPose pose)) {
            int z = (int) (pose.Position.z * nrrd.sizes[2]) % nrrd.sizes[2];
            if (z < 0) z += nrrd.sizes[2];

            var buf = tex.GetRawTextureData<Color32>();
            for (int i = 0; i < nrrd.sizes[0] * nrrd.sizes[1]; i++)
                buf[i] = grad.Evaluate(nrrd.grid[nrrd.sizes[0] * nrrd.sizes[1] * z + i]);
            tex.Apply();
        }
    }
}

public class NRRD {
    readonly public Dictionary<String, String> headers = new Dictionary<String, String>();
    readonly public float[] grid;

    readonly public int[] sizes;
    readonly public float[] origin = { 0, 0, 0 };
    readonly public float[][] directions = {
        new float[] { 1, 0, 0 },
        new float[] { 0, 1, 0 },
        new float[] { 0, 0, 1 } };

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

            sizes = Array.ConvertAll(headers["sizes"].Split(), s => int.Parse(s));
            if (headers.ContainsKey("space origin"))
                origin = Array.ConvertAll(headers["space origin"].Substring(1, headers["space origin"].Length - 2).Split(','), v => float.Parse(v));
            if (headers.ContainsKey("space directions"))
                directions = Array.ConvertAll(headers["space directions"].Split(), s => Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => float.Parse(v)));

            var mem = new MemoryStream();
            using (var stream = new GZipStream(reader.BaseStream, CompressionMode.Decompress)) stream.CopyTo(mem);
            grid = new float[sizes[0] * sizes[1] * sizes[2]];
            Buffer.BlockCopy(mem.ToArray(), 0, grid, 0, grid.Length * sizeof(float));
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
