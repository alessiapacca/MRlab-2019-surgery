using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class HandSlice : MonoBehaviour {
    public CTReader ct;
    public int width, height;
    public int interval;

    public bool enableReferencePlane;
    public bool leftHanded;

    Texture2D tex;
    GameObject referencePlane;

    int curInterval = 0;

    void Start() {
        tex = new Texture2D(width, height);
        GetComponent<Renderer>().material.mainTexture = tex;
    }

    void Update() {
        if (curInterval++ < interval) return;
        curInterval = 0;
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexKnuckle, leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po1) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po2) &&
            HandJointUtils.TryGetJointPose(TrackedHandJoint.PinkyKnuckle, leftHanded ? Handedness.Left : Handedness.Right, out MixedRealityPose po3)) {
            var p1 = ct.TransformWorldCoords(po1.Position);
            var p2 = ct.TransformWorldCoords(po2.Position);
            var p3 = ct.TransformWorldCoords(po3.Position);
            var plane = leftHanded ? new Plane(p1, p3, p2) : new Plane(p1, p2, p3);

            var orig = plane.ClosestPointOnPlane(Vector3.zero);
            var dy = (p2 - p1).normalized;
            var dx = Vector3.Cross(dy, plane.normal);

            if (enableReferencePlane) {
                if (!referencePlane) {
                    referencePlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                    referencePlane.GetComponent<Transform>().SetParent(ct.gameObject.GetComponent<Transform>());
                    referencePlane.GetComponent<Transform>().localScale = new Vector3(0.02f, 0.02f, 0.02f);
                }
                referencePlane.GetComponent<Transform>().up = plane.normal;
                referencePlane.GetComponent<Transform>().localPosition = p2;
            }

            ct.Slice(orig, dx, dy, tex);
        }
    }
}
