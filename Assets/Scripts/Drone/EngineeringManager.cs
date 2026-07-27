using UnityEngine;
using ASTRA.UAV.Core;
using ASTRA.UAV.Utilities;

namespace ASTRA.UAV.Drone
{
    public enum EngineeringRenderMode
    {
        Normal,
        Wireframe,
        XRaySemiTransparent,
        InternalComponents,
        ExplodedAssembly
    }

    /// <summary>
    /// Master manager controlling Engineering Inspection Mode toggles, render modes
    /// (Normal, X-Ray, Exploded, Wireframe), camera vantage switches, and automated presentation tours.
    /// </summary>
    public class EngineeringManager : Singleton<EngineeringManager>
    {
        [Header("State")]
        [SerializeField] private bool isEngineeringModeActive = false;
        [SerializeField] private EngineeringRenderMode currentRenderMode = EngineeringRenderMode.Normal;

        private UAVExplodedView explodedView;
        private UAVEngineeringMode engineeringXRay;

        public bool IsEngineeringModeActive => isEngineeringModeActive;
        public EngineeringRenderMode CurrentRenderMode => currentRenderMode;

        private void Start()
        {
            GameObject uav = GameObject.Find("ASTRA_UAV_DigitalTwin");
            if (uav != null)
            {
                explodedView = uav.GetComponent<UAVExplodedView>();
                engineeringXRay = uav.GetComponent<UAVEngineeringMode>();
            }
        }

        private void Update()
        {
            // Hotkey [E] toggles Engineering Mode
            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleEngineeringMode();
            }
        }

        public void ToggleEngineeringMode()
        {
            isEngineeringModeActive = !isEngineeringModeActive;
            if (isEngineeringModeActive)
            {
                SetRenderMode(EngineeringRenderMode.XRaySemiTransparent);
                UAVLogger.Log("Engineering Inspection Mode ACTIVATED.");
            }
            else
            {
                SetRenderMode(EngineeringRenderMode.Normal);
                UAVLogger.Log("Engineering Inspection Mode DEACTIVATED. Returned to Flight View.");
            }
        }

        public void SetRenderMode(EngineeringRenderMode mode)
        {
            currentRenderMode = mode;

            switch (mode)
            {
                case EngineeringRenderMode.Normal:
                    if (engineeringXRay != null) engineeringXRay.SetEngineeringMode(false);
                    if (explodedView != null) explodedView.SetExplosionProgress(0f);
                    break;

                case EngineeringRenderMode.XRaySemiTransparent:
                    if (engineeringXRay != null) engineeringXRay.SetEngineeringMode(true);
                    if (explodedView != null) explodedView.SetExplosionProgress(0f);
                    break;

                case EngineeringRenderMode.ExplodedAssembly:
                    if (engineeringXRay != null) engineeringXRay.SetEngineeringMode(false);
                    if (explodedView != null) explodedView.SetExplosionProgress(1f);
                    break;
            }
        }
    }
}





