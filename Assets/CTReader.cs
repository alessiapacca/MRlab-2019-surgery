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
    NRRD nrrd;

    Texture2D tex;
    Gradient grad;

    void Start() {
        Debug.Log("load nrrd");
        nrrd = new NRRD(ctobj);
        Debug.Log("done loading nrrd");
        tex = new Texture2D(nrrd.sizes[0], nrrd.sizes[1], TextureFormat.RGBA32, false);
        GameObject.Find("Quad1").GetComponent<Renderer>().material.mainTexture = tex;

        grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.black, 0), new GradientColorKey(Color.white, 1) },
            new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
    }

    void Update() {
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out MixedRealityPose pose)) {
            int z = (int) (pose.Position.z * nrrd.sizes[2]) % nrrd.sizes[2];
            if (z < 0) z += nrrd.sizes[2];

            var slice = new float[nrrd.sizes[0] * nrrd.sizes[1]];
            Buffer.BlockCopy(nrrd.grid, z * slice.Length * sizeof(float), slice, 0, slice.Length * sizeof(float));

            var buf = tex.GetRawTextureData<Color32>();
            for (int i = 0; i < slice.Length; i++) buf[i] = grad.Evaluate(slice[i]);
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
            int bytelen;
            switch (headers["type"]) {
                case "byte": bytelen = 1; break;
                case "short": bytelen = 2; break;
                case "float": bytelen = 4; break;
                case "int": bytelen = 4; break;
                case "double": bytelen = 8; break;
                case "long": bytelen = 8; break;
                default: throw new ArgumentException("Unknown type: " + headers["type"]);
            };
            var endianMatch = true;
            if (headers.ContainsKey("endian"))
                endianMatch = (headers["endian"] == "little") == BitConverter.IsLittleEndian;

            sizes = Array.ConvertAll(headers["sizes"].Split(), s => int.Parse(s));
            if (headers.ContainsKey("space origin"))
                origin = Array.ConvertAll(headers["space origin"].Substring(1, headers["space origin"].Length - 2).Split(','), v => float.Parse(v));
            if (headers.ContainsKey("space directions"))
                directions = Array.ConvertAll(headers["space directions"].Split(), s => Array.ConvertAll(s.Substring(1, s.Length - 2).Split(','), v => float.Parse(v)));

            var stream = reader.BaseStream;
            try {
                if (headers["encoding"] == "gzip") stream = new GZipStream(stream, CompressionMode.Decompress);
                else if (headers["encoding"] != "raw") throw new ArgumentException("Unknown encoding: " + headers["encoding"]);

                grid = new float[sizes[0] * sizes[1] * sizes[2]];
                float maxValue = float.MinValue;
                float minValue = float.MaxValue;
                for (int i = 0; i < grid.Length; i++) {
                    var buf = new byte[bytelen];
                    stream.Read(buf, 0, bytelen);
                    if (!endianMatch) Array.Reverse(buf);
                    switch (headers["type"]) {
                        case "byte": grid[i] = Convert.ToSingle(BitConverter.ToChar(buf, 0)); break;
                        case "short": grid[i] = Convert.ToSingle(BitConverter.ToInt16(buf, 0)); break;
                        case "float": grid[i] = Convert.ToSingle(BitConverter.ToSingle(buf, 0)); break;
                        case "int": grid[i] = Convert.ToSingle(BitConverter.ToInt32(buf, 0)); break;
                        case "double": grid[i] = Convert.ToSingle(BitConverter.ToDouble(buf, 0)); break;
                        case "long": grid[i] = Convert.ToSingle(BitConverter.ToInt64(buf, 0)); break;
                    }
                    if (grid[i] > maxValue) maxValue = grid[i];
                    if (grid[i] < minValue) minValue = grid[i];
                }
                for (int i = 0; i < grid.Length; i++) grid[i] = (grid[i] - minValue) / (maxValue - minValue);
            } finally {
                stream.Close();
            }
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
