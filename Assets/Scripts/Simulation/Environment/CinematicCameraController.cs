using System.Collections.Generic;
using UnityEngine;

namespace ASTRA.UAV.Simulation.Environment
{
    /// <summary>
    /// Definition of a single cinematic camera presentation viewpoint.
    /// </summary>
    [System.Serializable]
    public class CinematicWaypoint
    {
        public string label;
        public Vector3 position;
        public Vector3 rotationEuler;
        public float holdTimeDuration = 4.0f;
        public float transitionTime = 5.0f;
    }

    /// <summary>
    /// Smooth cinematic camera sequence player for funding presentations.
    /// Interpolates through facility highlights: Runway, Hangar, Mission Control, Helipad, and Aerial Overview.
    /// </summary>
    public class CinematicCameraController : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private List<CinematicWaypoint> waypoints = new List<CinematicWaypoint>();

        [Header("State")]
        [SerializeField] private bool isPlaying = false;
        [SerializeField] private bool loopSequence = true;
        [SerializeField] private int currentWaypointIndex = 0;

        private float timer = 0f;
        private bool isTransitioning = false;
        private Vector3 startPosition;
        private Quaternion startRotation;

        public bool IsPlaying => isPlaying;
        public string CurrentLabel => waypoints.Count > 0 && currentWaypointIndex < waypoints.Count ? waypoints[currentWaypointIndex].label : "";

        private void Awake()
        {
            InitializeDefaultFacilityWaypoints();
        }

        private void InitializeDefaultFacilityWaypoints()
        {
            if (waypoints.Count > 0) return;

            waypoints.Add(new CinematicWaypoint
            {
                label = "Facility Aerial Overview",
                position = new Vector3(0f, 60f, -120f),
                rotationEuler = new Vector3(25f, 0f, 0f),
                holdTimeDuration = 3f,
                transitionTime = 6f
            });

            waypoints.Add(new CinematicWaypoint
            {
                label = "UAV Main Runway Threshold",
                position = new Vector3(-80f, 12f, -10f),
                rotationEuler = new Vector3(8f, 90f, 0f),
                holdTimeDuration = 4f,
                transitionTime = 5f
            });

            waypoints.Add(new CinematicWaypoint
            {
                label = "Mission Control Center & Helipad",
                position = new Vector3(45f, 15f, 30f),
                rotationEuler = new Vector3(15f, -135f, 0f),
                holdTimeDuration = 4f,
                transitionTime = 5f
            });

            waypoints.Add(new CinematicWaypoint
            {
                label = "UAV Maintenance Hangar",
                position = new Vector3(-35f, 10f, 50f),
                rotationEuler = new Vector3(10f, 45f, 0f),
                holdTimeDuration = 4f,
                transitionTime = 6f
            });

            waypoints.Add(new CinematicWaypoint
            {
                label = "Security Gate & Access Road",
                position = new Vector3(110f, 8f, 90f),
                rotationEuler = new Vector3(12f, -110f, 0f),
                holdTimeDuration = 3f,
                transitionTime = 6f
            });
        }

        private void OnEnable()
        {
            StartCinematicSequence();
        }

        public void StartCinematicSequence()
        {
            if (waypoints.Count == 0) InitializeDefaultFacilityWaypoints();
            isPlaying = true;
            currentWaypointIndex = 0;
            PrepareWaypointTransition(0);
        }

        public void StopCinematicSequence()
        {
            isPlaying = false;
        }

        private void Update()
        {
            if (!isPlaying || waypoints.Count == 0) return;

            CinematicWaypoint current = waypoints[currentWaypointIndex];
            timer += Time.deltaTime;

            if (isTransitioning)
            {
                float t = Mathf.Clamp01(timer / current.transitionTime);
                // Smooth Step ease curve
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                transform.position = Vector3.Lerp(startPosition, current.position, smoothT);
                transform.rotation = Quaternion.Slerp(startRotation, Quaternion.Euler(current.rotationEuler), smoothT);

                if (t >= 1.0f)
                {
                    isTransitioning = false;
                    timer = 0f;
                }
            }
            else
            {
                // Hold position with subtle panning drift
                transform.Rotate(Vector3.up, 1.5f * Time.deltaTime, Space.World);

                if (timer >= current.holdTimeDuration)
                {
                    AdvanceToNextWaypoint();
                }
            }
        }

        private void AdvanceToNextWaypoint()
        {
            int nextIndex = currentWaypointIndex + 1;
            if (nextIndex >= waypoints.Count)
            {
                if (loopSequence) nextIndex = 0;
                else
                {
                    isPlaying = false;
                    return;
                }
            }

            PrepareWaypointTransition(nextIndex);
        }

        private void PrepareWaypointTransition(int targetIndex)
        {
            currentWaypointIndex = targetIndex;
            startPosition = transform.position;
            startRotation = transform.rotation;
            timer = 0f;
            isTransitioning = true;
        }
    }
}



