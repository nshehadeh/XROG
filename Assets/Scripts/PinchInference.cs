using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using Microsoft;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Linq;

using static System.Net.Mime.MediaTypeNames;

public class PinchInference : MonoBehaviour, IMixedRealityPointerHandler
{
    private List<Vector3> trackedPositions = new List<Vector3>();
    private List<Vector3> currentSequence;
    public SVMClient svmClient;
    private int predictionResult;

    private IMixedRealityInputSystem inputSystem;

    public GameObject sphereMarker;

    GameObject thumbObject;
    GameObject indexObject;


    MixedRealityPose pose;

    void Start()
    {
        thumbObject = Instantiate(sphereMarker, this.transform);
        indexObject = Instantiate(sphereMarker, this.transform);
        currentSequence = new List<Vector3>();
    }
    void Awake()
    {
        inputSystem = CoreServices.InputSystem;
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
    {
        // Disable tracking spheres
        thumbObject.GetComponent<Renderer>().enabled = false;
        indexObject.GetComponent<Renderer>().enabled = false;
        //make sure count is enough
        if(currentSequence.Count > 30)
        {
            UnityEngine.Debug.Log("Attempting inference on " + currentSequence.Count + " frames");
            int prediction = 0;
            StartCoroutine(svmClient.GetPrediction(currentSequence, result =>
            {
                prediction = result;
                UnityEngine.Debug.Log("PREDICTION: " + prediction);
            }));

        }
        //get prediction
        //clear sequence
        currentSequence.Clear();

    }
    void IMixedRealityPointerHandler.OnPointerDown(
         MixedRealityPointerEventData eventData)
    {
        // Requirement for implementing the interface

    }

    void IMixedRealityPointerHandler.OnPointerDragged(
         MixedRealityPointerEventData eventData)
    {

        // change pre fab to show that tracking is enabled
        thumbObject.GetComponent<Renderer>().enabled = false;
        indexObject.GetComponent<Renderer>().enabled = false;

        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, Handedness.Right, out pose))
        {
            thumbObject.GetComponent<Renderer>().enabled = true;
            Vector3 thumbPosition = pose.Position;
            thumbObject.transform.position = thumbPosition;

        }
        if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out pose))
        {
            indexObject.GetComponent<Renderer>().enabled = true;
            Vector3 indexPosition = pose.Position;

            indexObject.transform.position = indexPosition;
            currentSequence.Add(indexPosition);

        }

        //save data
    }

    // Detecting the air tap gesture
    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        // Requirement for implementing the interface
    }
}