using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.ReplayKit;

public class ReplayManager : MonoBehaviour
{
    public delegate void SimpleCallback();
    public SimpleCallback RecordingFailedCallback;
    public SimpleCallback RecordingPreviewReadyCallback;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        ReplayKitManager.DidInitialise += onInitialize;
        ReplayKitManager.DidRecordingStateChange += onRecordStateChange;
    }

    void Start()
    {
        ReplayKitManager.Initialise();
    }

    void OnDestroy()
    {
        ReplayKitManager.DidInitialise -= onInitialize;
        ReplayKitManager.DidRecordingStateChange -= onRecordStateChange;
    }

    private void onInitialize(ReplayKitInitialisationState state, string message)
    {
        Debug.Log("ReplayManager::onInitialize -> Received Event Callback : DidInitialise [State:" + state.ToString() + " " + "Message:" + message);
        switch (state)
        {
            case ReplayKitInitialisationState.Success:
                Debug.Log("ReplayManager::onInitialize -> ReplayKitManager.DidInitialise : Initialisation Success");
                break;
            case ReplayKitInitialisationState.Failed:
                Debug.Log("ReplayManager::onInitialize -> ReplayKitManager.DidInitialise : Initialisation Failed with message[" + message + "]");
                break;
            default:
                Debug.Log("ReplayManager::onInitialize -> Unknown State");
                break;
        }
    }

    private void onRecordStateChange(ReplayKitRecordingState state, string message)
    {
        Debug.Log("ReplayManager::onRecordStateChange -> Received Event Callback : DidRecordingStateChange [State:" + state.ToString() + " " + "Message:" + message);
        switch (state)
        {
            case ReplayKitRecordingState.Started:
                Debug.Log("ReplayManager::onRecordStateChange -> ReplayKitManager.DidRecordingStateChange : Video Recording Started");
                break;
            case ReplayKitRecordingState.Stopped:
                Debug.Log("ReplayManager::onRecordStateChange -> ReplayKitManager.DidRecordingStateChange : Video Recording Stopped");
                break;
            case ReplayKitRecordingState.Failed:
                Debug.Log("ReplayManager::onRecordStateChange -> ReplayKitManager.DidRecordingStateChange : Video Recording Failed with message [" + message + "]");

                if (RecordingFailedCallback != null)
                {
                    RecordingFailedCallback();
                }

                break;
            case ReplayKitRecordingState.Available:
                Debug.Log("ReplayManager::onRecordStateChange -> ReplayKitManager.DidRecordingStateChange : Video Recording available for preview / file access");

                if (RecordingPreviewReadyCallback != null)
                {
                    RecordingPreviewReadyCallback();
                }

                break;
            default:
                Debug.Log("ReplayManager::onRecordStateChange -> Unknown State");
                break;
        }
    }

    public bool IsAvailable()
    {
        bool isRecordingAPIAvailable = ReplayKitManager.IsRecordingAPIAvailable();
        string message = isRecordingAPIAvailable ? "Replay Kit recording API is available!" : "Replay Kit recording API is not available.";
        Debug.Log("ReplayManager::IsAvailable -> message: " + message);

        return isRecordingAPIAvailable;
    }

    public bool IsRecording()
    {
        return ReplayKitManager.IsRecording();
    }

    public bool IsPreviewAvailable()
    {
        bool previewAvailable = false;

        if (ReplayKitManager.IsPreviewAvailable())
        {
            previewAvailable = true;
        }

        print("ReplayManager::IsPreviewAvailable -> previewAvailable: " + previewAvailable.ToString());
        return previewAvailable;
    }

    public bool PlayPreview()
    {
        if (ReplayKitManager.IsPreviewAvailable())
        {
            return ReplayKitManager.Preview();
        }
        // Still preview is not available. Make sure you call preview after you receive ReplayKitRecordingState.Available status from DidRecordingStateChange
        return false;
    }

    public bool DiscardPreview()
    {
        if (ReplayKitManager.IsPreviewAvailable())
        {
            return ReplayKitManager.Discard();
        }

        return false;
    }

    public void StartRecording(bool enableMic)
    {
        ReplayKitManager.SetMicrophoneStatus(enable: enableMic);
        ReplayKitManager.StartRecording();
    }

    public void StopRecording()
    {
        ReplayKitManager.StopRecording((filePath, error) => 
        {
            Debug.Log("ReplayManager::StopRecording -> File path available : " + ReplayKitManager.GetPreviewFilePath());
        });
    }

    public string GetRecordingFile()
    {
        if (ReplayKitManager.IsPreviewAvailable())
        {
            return ReplayKitManager.GetPreviewFilePath();
        }
        else
        {
            Debug.LogError("ReplayManager::GetRecordingFile -> File not yet available. Please wait for ReplayKitRecordingState.Available status");
            return null;
        }
    }

    public void SavePreview() //Saves preview to gallery
    {
        if (ReplayKitManager.IsPreviewAvailable())
        {
            ReplayKitManager.SavePreview((error) =>
            {
                Debug.Log("ReplayManager::SavePreview -> Saved preview to gallery with error : " + ((error == null) ? "null" : error));
            });
        }
        else
        {
            Debug.LogError("ReplayManager::SavePreview -> Recorded file not yet available. Please wait for ReplayKitRecordingState.Available status");
        }
    }

    public void SharePreview()
    {
        if (ReplayKitManager.IsPreviewAvailable())
        {
            ReplayKitManager.SharePreview();
            Debug.Log("ReplayManager::SharePreview -> Shared video preview");
        }
        else
        {
            Debug.LogError("ReplayManager::SharePreview -> Recorded file not yet available. Please wait for ReplayKitRecordingState.Available status");
        }
    }
}
