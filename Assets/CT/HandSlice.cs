using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class HandSlice : MonoBehaviour {
    public CTReader ct;
    public GameObject referencePlane;
    public bool leftHanded;

    Texture2D tex;

    void Start() {
        tex = new Texture2D(512, 512);
        GetComponent<Renderer>().material.mainTexture = tex;
    }

    void Update() {
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
            dx = ct.ScaleVector(dx) / tex.width;
            dy = ct.ScaleVector(dy) / tex.height;

            if (referencePlane) {
                var rp = referencePlane.GetComponent<Transform>();
                rp.up = plane.normal;
                rp.localPosition = p2;
            }

            ct.Slice(orig, dx, dy, tex);
        }
    }
}
