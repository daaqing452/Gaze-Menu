﻿ //========= Copyright 2018, HTC Corporation. All rights reserved. ===========
using System;
using UnityEngine;

namespace ViveSR.anipal.Eye
{
    public class SRanipal_EyeFocusSample : MonoBehaviour
    {
        private FocusInfo FocusInfo;
        private readonly float MaxDistance = 20;
        private readonly GazeIndex[] GazePriority = new GazeIndex[] { GazeIndex.COMBINE, GazeIndex.LEFT, GazeIndex.RIGHT };

        private void Start()
        {
            if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

            foreach (GazeIndex index in GazePriority)
            {
                Ray GazeRay;
                if (SRanipal_Eye.Focus(index, out GazeRay, out FocusInfo, MaxDistance))
                {
                    DartBoard dartBoard = FocusInfo.transform.GetComponent<DartBoard>();
                    if (dartBoard != null) dartBoard.Focus(FocusInfo.point);
                    Debug.Log("Eye Focused");
                    Renderer[] ren = GameObject.Find("gaze menu").GetComponentsInChildren<Renderer>();
                    for (int j = 0; j < ren.Length; j++) {
                        Material m = ren[j].material;
                        m.color = new Color(m.color.r, m.color.g, m.color.b, 0.02f);
                    }
                    break;
                }
            }
        }
    }
}