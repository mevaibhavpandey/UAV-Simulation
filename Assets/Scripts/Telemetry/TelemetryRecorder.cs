using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ASTRA.UAV.Interfaces;

namespace ASTRA.UAV.Telemetry
{
    /// <summary>
    /// Records streaming telemetry frames to memory and persists to JSON/CSV flight log files for analysis.
    /// </summary>
    public class TelemetryRecorder : MonoBehaviour
    {
        [Header("Recorder Configuration")]
        [SerializeField] private bool recordOnStart = false;
        [SerializeField] private float recordingIntervalSeconds = 0.1f; // 10 Hz sampling
        [SerializeField] private string logSubfolder = "FlightLogs";

        [Header("Runtime Status")]
        [SerializeField] private bool isRecording = false;
        [SerializeField] private int recordedFrameCount = 0;

        private readonly List<TelemetryData> recordedFrames = new List<TelemetryData>();
        private float lastRecordTime = 0f;
        private ITelemetryProvider telemetryProvider;

        /// <summary>Gets whether recording is active.</summary>
        public bool IsRecording => isRecording;

        /// <summary>Gets read-only list of recorded telemetry frames.</summary>
        public IReadOnlyList<TelemetryData> RecordedFrames => recordedFrames;

        private void Awake()
        {
            telemetryProvider = GetComponent<ITelemetryProvider>();
            if (telemetryProvider == null)
            {
                telemetryProvider = FindAnyObjectByType<MockTelemetryProvider>();
            }
        }

        private void Start()
        {
            if (recordOnStart)
            {
                StartRecording();
            }
        }

        private void Update()
        {
            if (!isRecording || telemetryProvider == null) return;

            if (Time.time - lastRecordTime >= recordingIntervalSeconds)
            {
                lastRecordTime = Time.time;
                recordedFrames.Add(telemetryProvider.CurrentTelemetry);
                recordedFrameCount = recordedFrames.Count;
            }
        }

        /// <summary>
        /// Begins recording telemetry snapshots.
        /// </summary>
        public void StartRecording()
        {
            recordedFrames.Clear();
            recordedFrameCount = 0;
            isRecording = true;
            Debug.Log("[TelemetryRecorder] Recording STARTED.");
        }

        /// <summary>
        /// Stops recording telemetry snapshots.
        /// </summary>
        public void StopRecording()
        {
            isRecording = false;
            Debug.Log($"[TelemetryRecorder] Recording STOPPED. Total frames captured: {recordedFrames.Count}");
        }

        /// <summary>
        /// Saves recorded telemetry sequence to JSON format.
        /// </summary>
        /// <param name="filename">Optional custom filename.</param>
        /// <returns>Full file path saved to.</returns>
        public string ExportToJson(string filename = "")
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = $"FlightLog_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            }

            string dir = Path.Combine(Application.persistentDataPath, logSubfolder);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string filePath = Path.Combine(dir, filename);
            string json = JsonUtility.ToJson(new TelemetryContainer { Frames = recordedFrames }, true);
            File.WriteAllText(filePath, json);

            Debug.Log($"[TelemetryRecorder] Exported telemetry JSON to: {filePath}");
            return filePath;
        }

        [Serializable]
        private class TelemetryContainer
        {
            public List<TelemetryData> Frames;
        }
    }
}


