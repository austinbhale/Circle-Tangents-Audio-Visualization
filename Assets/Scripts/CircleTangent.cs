//Copyright (c) 2018 Peter Olthof, Peer Play
//http://www.peerplay.nl, info AT peerplay.nl 
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

// Following Peer Play's Circle Tangent Visuals Tutorial
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleTangent : MonoBehaviour
{
    protected Vector3 GetRotatedTangent(float degree, float radius)
    {
        double angle = degree * Mathf.Deg2Rad;
        float x = radius * (float)System.Math.Sin(angle);
        float z = radius * (float)System.Math.Cos(angle);
        return new Vector3(x, 0, z);
    }

    // A is the outer circle, B is the inner circle, C is the tangent circle.
    protected Vector4 FindTangentCircle(Vector4 A, Vector4 B, float degree)
    {
        Vector3 C = GetRotatedTangent(degree, A.w);
        float AB_dist = Mathf.Max(Vector3.Distance(new Vector3(A.x, A.y, A.z), new Vector3(B.x, B.y, B.z)), 0.1f);
        float AC_dist = Vector3.Distance(new Vector3(A.x, A.y, A.z), C);
        float BC_dist = Vector3.Distance(new Vector3(B.x, B.y, B.z), C);
        float angleCAB = ((AC_dist * AC_dist) + (AB_dist * AB_dist) - (BC_dist * BC_dist)) / (2 * AC_dist * AB_dist);
        float radius = (((A.w * A.w) - (B.w * B.w) + (AB_dist * AB_dist)) - (2 * A.w * AB_dist * angleCAB))
                                        / (2 * (A.w + B.w - AB_dist * angleCAB));
        C = GetRotatedTangent(degree, A.w - radius);
        return new Vector4(C.x, C.y, C.z, radius);
    }
}
