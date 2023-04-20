using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using Microsoft;
using System.Diagnostics;
using System.Text;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

public class Pinch : MonoBehaviour, IMixedRealityPointerHandler
{
    private List<Vector3> trackedPositions = new List<Vector3>();
    private List<List<Vector3>> sequences;
    private List<Vector3> currentSequence;

    private IMixedRealityInputSystem inputSystem;

    public GameObject sphereMarker;

    GameObject thumbObject;
    GameObject indexObject;


    MixedRealityPose pose;

    void Start()
    {
        thumbObject = Instantiate(sphereMarker, this.transform);
        indexObject = Instantiate(sphereMarker, this.transform);
        sequences = new List<List<Vector3>>();
        currentSequence = new List<Vector3>();


    }

    void Awake()
    {
        inputSystem = CoreServices.InputSystem;

        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);

    }

    void Update()
    {
        //nothing
    }

    void IMixedRealityPointerHandler.OnPointerUp(MixedRealityPointerEventData eventData)
    {
        // Disable tracking spheres
        UnityEngine.Debug.Log("Sequence " + sequences.Count + " done");
        thumbObject.GetComponent<Renderer>().enabled = false;
        indexObject.GetComponent<Renderer>().enabled = false;

        // Output tracked data as a CSV file
        sequences.Add(new List<Vector3>(currentSequence));
        currentSequence.Clear();

    }
    //save data out

    void IMixedRealityPointerHandler.OnPointerDown(
         MixedRealityPointerEventData eventData)
    {
        // Requirement for implementing the interface
       // UnityEngine.Debug.Log("Pointer down");
        // change prefab
        

    }

    void IMixedRealityPointerHandler.OnPointerDragged(
         MixedRealityPointerEventData eventData)
    {
        // Requirement for implementing the interface
        //UnityEngine.Debug.Log("Dragged");
        
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

        // output dat
        // output dat

    }
    void OnApplicationQuit()
    {
        string filePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "/tracking_data/tracked_sketches.csv";
        SaveSequencesToCSV(filePath);
    }

    void SaveSequencesToCSV(string filePath)
    {
        StringBuilder csv = new StringBuilder();

        for (int i = 0; i < sequences.Count; i++)
        {
            for (int j = 0; j < sequences[i].Count; j++)
            {
                Vector3 pos = sequences[i][j];
                csv.AppendLine($"{i},{pos.x},{pos.y},{pos.z}");
            }
        }
        UnityEngine.Debug.Log("Saving out " + sequences.Count + " sequences of data");

        File.WriteAllText(filePath, csv.ToString());
    }
}