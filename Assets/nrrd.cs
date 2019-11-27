using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public class nrrd : MonoBehaviour
{
    public TextAsset ctobj;

    Dictionary<String, String> headers = new Dictionary<String, String>();
    int sampleSize;
    int[] dims;
    short[] ct;

    Texture2D tex;

    // Start is called before the first frame update
    void Start()
    {
        using (var reader = new BinaryReader(new MemoryStream(ctobj.bytes)))
        {
            for (string line = reader.ReadLine(); line.Length > 0; line = reader.ReadLine())
            {
                if (line.StartsWith("#") || !line.Contains(":")) continue;
                var tokens = line.Split(':');
                var key = tokens[0].Trim();
                var value = tokens[1].Trim();
                headers.Add(key, value);

                switch (key)
                {
                    case "type":
                        switch (value)
                        {
                            case "short":
                                sampleSize = 2;
                                break;
                            default:
                                throw new System.ArgumentException("Unknown header value (" + key + ": " + value);
                        }
                        break;
                    case "sizes":
                        dims = Array.ConvertAll(value.Split(), s => int.Parse(s));
                        break;
                }
            }

            using (var gzip = new GZipStream(reader.BaseStream, CompressionMode.Decompress))
            {
                var mem = new MemoryStream();
                gzip.CopyTo(mem);
                var bytes = mem.ToArray();
                ct = new short[bytes.Length / 2];
                Buffer.BlockCopy(bytes, 0, ct, 0, bytes.Length);
            }

            tex = new Texture2D(dims[0], dims[1], TextureFormat.RGBA32, false);
            GameObject.Find("Quad1").GetComponent<Renderer>().material.mainTexture = tex;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Palm, Handedness.Right, out MixedRealityPose pose))
        {
            int z = (int)(pose.Position.z * dims[2]) % dims[2];
            if (z < 0) z += dims[2];

            var slice = new short[dims[0] * dims[1]];
            Buffer.BlockCopy(ct, z * dims[0] * dims[1], slice, 0, dims[0] * dims[1]);

            var buf = tex.GetRawTextureData<Color32>();
            for (int i = 0; i < slice.Length; i++)
            {
                var v = (1 << 16) - slice[i];
                buf[i] = new Color32((byte)(v >> 8), (byte)(v >> 8), (byte)v, 255);
            }
            tex.Apply();
        }
    }
}

public static class BinaryReaderExtension
{
    // https://www.codeproject.com/Articles/996254/BinaryReader-ReadLine-extension
    public static string ReadLine(this BinaryReader reader)
    {
        var result = new StringBuilder();
        bool foundEndOfLine = false;
        char ch;
        while (!foundEndOfLine)
        {
            try
            {
                ch = reader.ReadChar();
            }
            catch (EndOfStreamException)
            {
                if (result.Length == 0) return null;
                else break;
            }

            switch (ch)
            {
                case '\r':
                    if (reader.PeekChar() == '\n') reader.ReadChar();
                    foundEndOfLine = true;
                    break;
                case '\n':
                    foundEndOfLine = true;
                    break;
                default:
                    result.Append(ch);
                    break;
            }
        }
        return result.ToString();
    }
}
