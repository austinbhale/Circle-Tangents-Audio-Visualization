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

public class TangentCircles : CircleTangent
{
    [Header("Setup")]
    public GameObject circlePrefab;
    private GameObject innerCircleGO, outerCircleGO;
    private Vector4 innerCircle, outerCircle;
    public float innerCircleRadius, outerCircleRadius;
    private Vector4[] tangentCircle;
    private GameObject[] tangentObject;
    [Range(1, 64)]
    public int circleAmount;

    [Header("Input")]
    [Range(0, 1)]
    public float distanceToOuterTangent; // 0: hit tangent of outer, 1: won't move at all
    [Range(0, 1)]
    public float movementSmooth;
    [Range(0.1f, 10f)]
    public float radiusChangeSpeed;
    private Vector2 thumbstickLeft, thumbStickLeftSmooth;
    private float radiusChange;

    [Header("Audio Visuals")]
    public AudioPeer64 audioPeer;
    public Material materialBase;
    private Material[] materials;
    public Gradient gradient;
    public float emissionMultiplier;
    public bool emissionBuffer;
    [Range(0, 1)]
    public float thresholdEmission;

    public bool scaleYOnAudio;
    public bool scaleXOnAudio;
    public bool scaleZOnAudio;
    public bool scaleBuffer;
    [Range(0, 1)]
    public float scaleThreshold;
    public float scaleStart;
    public Vector2 scaleMinMax;

    // Start is called before the first frame update
    void Start()
    {
        innerCircle = new Vector4(0, 0, 0, innerCircleRadius);
        outerCircle = new Vector4(0, 0, 0, outerCircleRadius);

        tangentCircle = new Vector4[circleAmount];
        tangentObject = new GameObject[circleAmount];
        materials = new Material[circleAmount];
        for (int i = 0; i < circleAmount; i++)
        {
            GameObject tangentInstance = (GameObject)Instantiate(circlePrefab);
            tangentObject[i] = tangentInstance;
            tangentObject[i].transform.parent = this.transform;
            materials[i] = new Material(materialBase);
            materials[i].EnableKeyword("_EMISSION");
            materials[i].SetColor("_Color", new Color(0, 0, 0));
            tangentObject[i].GetComponent<MeshRenderer>().material = materials[i];
        }
    }

    void PlayerInput()
    {
        thumbstickLeft = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        thumbStickLeftSmooth = new Vector2(
            thumbStickLeftSmooth.x * (1 - movementSmooth) + thumbstickLeft.x * movementSmooth,
            thumbStickLeftSmooth.y * (1 - movementSmooth) + thumbstickLeft.y * movementSmooth);

        radiusChange = Input.GetAxis("TriggerL") - Input.GetAxis("TriggerR");

        innerCircle = new Vector4(
            (thumbStickLeftSmooth.x * (outerCircle.w - innerCircle.w) * (1 - distanceToOuterTangent)) + outerCircle.x,
            0.0f,
            (thumbStickLeftSmooth.y * (outerCircle.w - innerCircle.w) * (1 - distanceToOuterTangent)) + outerCircle.z,
            innerCircle.w + (radiusChange * Time.deltaTime * radiusChangeSpeed)
        );
    }

    // Update is called once per frame
    void Update()
    {
        // PlayerInput();

        for (int i = 0; i < circleAmount; i++)
        {
            // If we have 20 circles, each circle will be 18 degrees more than its previous one
            tangentCircle[i] = FindTangentCircle(outerCircle, innerCircle, (360f / circleAmount) * i);
            tangentObject[i].transform.position = new Vector3(tangentCircle[i].x + this.transform.position.x, tangentCircle[i].y + this.transform.position.y, tangentCircle[i].z + this.transform.position.z);
            tangentObject[i].transform.rotation = new Quaternion(this.transform.rotation.w, this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z);

            tangentObject[i].transform.localScale = (scaleYOnAudio || scaleXOnAudio || scaleZOnAudio) ? scaleOnAudio(i) : tangentObject[i].transform.localScale = new Vector3(tangentCircle[i].w, tangentCircle[i].w, tangentCircle[i].w) * 2;

            if (audioPeer.audioBandBuffer64[i] > thresholdEmission)
            {
                if (emissionBuffer)
                {
                    materials[i].SetColor("_EmissionColor", gradient.Evaluate((1f / circleAmount) * i) * audioPeer.audioBandBuffer64[i] * emissionMultiplier);
                }
                else
                {
                    materials[i].SetColor("_EmissionColor", gradient.Evaluate((1f / circleAmount) * i) * audioPeer.audioBand64[i] * emissionMultiplier);

                }
            }
            else
            {
                materials[i].SetColor("_EmissionColor", new Color(0, 0, 0));
            }

            tangentObject[i].transform.localScale = tangentObject[i].transform.localScale + (tangentObject[i].transform.localScale - this.transform.localScale);
        }
    }

    Vector3 scaleOnAudio(int i)
    {
        Vector3 scaledOut;
        if (audioPeer.audioBandBuffer64[i] > scaleThreshold)
        {
            float x, y, z;
            x = tangentCircle[i].w;
            y = tangentCircle[i].w;
            z = tangentCircle[i].w;

            if (scaleYOnAudio)
            {
                // tangentObject[i].transform.localScale = (scaleBuffer) ? new Vector3(tangentCircle[i].w, scaleStart + Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, audioPeer.audioBandBuffer64[i]), tangentCircle[i].w) * 2
                //                                                   : new Vector3(tangentCircle[i].w, scaleStart + Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, audioPeer.audioBand64[i]), tangentCircle[i].w) * 2;
                y = (scaleBuffer) ? scaleStart + Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, audioPeer.audioBandBuffer64[i]) : scaleStart + Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, audioPeer.audioBand64[i]);
            }

            if (scaleXOnAudio)
            {
                x = (scaleBuffer) ? scaleStart + Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, audioPeer.audioBandBuffer64[i]) : scaleStart + Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, audioPeer.audioBand64[i]);
            }

            if (scaleZOnAudio)
            {
                z = (scaleBuffer) ? scaleStart + Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, audioPeer.audioBandBuffer64[i]) : scaleStart + Mathf.Lerp(scaleMinMax.x, scaleMinMax.y, audioPeer.audioBand64[i]);
            }

            scaledOut = new Vector3(x, y, z) * 2;

        }
        else
        {
            float x, y, z;
            x = (scaleXOnAudio) ? scaleStart : tangentCircle[i].w;
            y = (scaleYOnAudio) ? scaleStart : tangentCircle[i].w;
            z = (scaleZOnAudio) ? scaleStart : tangentCircle[i].w;
            scaledOut = new Vector3(x, y, z) * 2;
        }

        return scaledOut;
    }
}
