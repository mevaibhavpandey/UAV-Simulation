using UnityEngine;

namespace ASTRA.UAV.Simulation.Environment
{
    /// <summary>
    /// Procedural facility geometry builder and scene orchestrator.
    /// Creates and configures UAV testing facility objects (Runway, Helipad, GCS Building, Hangar,
    /// Fence perimeter, Road network, Wind sock, Lighting, Water body, and Vegetation).
    /// </summary>
    [ExecuteAlways]
    public class FacilityEnvironmentBuilder : MonoBehaviour
    {
        [Header("Facility Scale Settings")]
        [SerializeField] private float runwayLength = 200.0f;
        [SerializeField] private float runwayWidth = 20.0f;
        [SerializeField] private float helipadRadius = 12.0f;

        [Header("Materials")]
        [SerializeField] private Material asphaltMaterial;
        [SerializeField] private Material concreteMaterial;
        [SerializeField] private Material whiteMarkingMaterial;
        [SerializeField] private Material yellowMarkingMaterial;
        [SerializeField] private Material grassMaterial;
        [SerializeField] private Material waterMaterial;
        [SerializeField] private Material buildingMaterial;
        [SerializeField] private Material fenceMaterial;

        [Header("Auto Build On Start")]
        [SerializeField] private bool buildOnAwake = true;

        private void Awake()
        {
            if (buildOnAwake && transform.childCount == 0)
            {
                BuildFacility();
            }
        }

        [ContextMenu("Build UAV Testing Facility")]
        public void BuildFacility()
        {
            CreateDefaultMaterials();

            // Root Containers
            GameObject envRoot = GetOrCreateChild("[Environment]");
            GameObject bldgRoot = GetOrCreateChild("[Buildings]");
            GameObject roadRoot = GetOrCreateChild("[Roads & Infrastructure]");
            GameObject lightRoot = GetOrCreateChild("[Lighting & Atmosphere]");

            // 1. Terrain & Water
            BuildTerrainAndWater(envRoot);

            // 2. Runway & Helipad
            BuildRunwayAndMarkings(roadRoot);
            BuildHelipad(roadRoot);
            BuildRoadNetwork(roadRoot);

            // 3. Buildings (GCS, Hangar, Security Gate, Research Lab)
            BuildMissionControlBuilding(bldgRoot);
            BuildUAVHangar(bldgRoot);
            BuildBoundaryFence(bldgRoot);
            BuildSecurityGate(bldgRoot);

            // 4. Infrastructure Props
            BuildWindsock(roadRoot);
            BuildStreetlights(roadRoot);

            Debug.Log("[ASTRA FacilityBuilder] UAV Testing Facility generated successfully.");
        }

        private GameObject GetOrCreateChild(string name)
        {
            Transform t = transform.Find(name);
            if (t != null) return t.gameObject;

            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go;
        }

        private void CreateDefaultMaterials()
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Standard");

            if (asphaltMaterial == null)
            {
                asphaltMaterial = new Material(litShader) { name = "Mat_Asphalt" };
                asphaltMaterial.color = new Color(0.15f, 0.16f, 0.18f);
            }
            if (concreteMaterial == null)
            {
                concreteMaterial = new Material(litShader) { name = "Mat_Concrete" };
                concreteMaterial.color = new Color(0.65f, 0.67f, 0.70f);
            }
            if (whiteMarkingMaterial == null)
            {
                whiteMarkingMaterial = new Material(litShader) { name = "Mat_WhiteMarking" };
                whiteMarkingMaterial.color = Color.white;
            }
            if (yellowMarkingMaterial == null)
            {
                yellowMarkingMaterial = new Material(litShader) { name = "Mat_YellowMarking" };
                yellowMarkingMaterial.color = new Color(1.0f, 0.82f, 0.0f);
            }
            if (grassMaterial == null)
            {
                grassMaterial = new Material(litShader) { name = "Mat_Grass" };
                grassMaterial.color = new Color(0.22f, 0.42f, 0.18f);
            }
            if (waterMaterial == null)
            {
                waterMaterial = new Material(litShader) { name = "Mat_Water" };
                waterMaterial.color = new Color(0.12f, 0.35f, 0.55f, 0.8f);
            }
            if (buildingMaterial == null)
            {
                buildingMaterial = new Material(litShader) { name = "Mat_BuildingConcrete" };
                buildingMaterial.color = new Color(0.85f, 0.87f, 0.90f);
            }
            if (fenceMaterial == null)
            {
                fenceMaterial = new Material(litShader) { name = "Mat_FenceWire" };
                fenceMaterial.color = new Color(0.4f, 0.42f, 0.45f);
            }
        }

        private void BuildTerrainAndWater(GameObject parent)
        {
            // Ground Base Plane
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "FacilityGround";
            ground.transform.SetParent(parent.transform, false);
            ground.transform.localScale = new Vector3(50f, 1f, 50f); // 500m x 500m
            ground.GetComponent<Renderer>().sharedMaterial = grassMaterial;

            // Water Lake Body
            GameObject lake = GameObject.CreatePrimitive(PrimitiveType.Plane);
            lake.name = "FacilityLake";
            lake.transform.SetParent(parent.transform, false);
            lake.transform.localPosition = new Vector3(-120f, 0.1f, -100f);
            lake.transform.localScale = new Vector3(10f, 1f, 8f); // 100m x 80m lake
            lake.GetComponent<Renderer>().sharedMaterial = waterMaterial;
        }

        private void BuildRunwayAndMarkings(GameObject parent)
        {
            GameObject runway = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runway.name = "UAVRunway";
            runway.transform.SetParent(parent.transform, false);
            runway.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            runway.transform.localScale = new Vector3(runwayWidth, 0.1f, runwayLength);
            runway.GetComponent<Renderer>().sharedMaterial = asphaltMaterial;

            // Center Dashed Line Markings
            GameObject markingsGroup = new GameObject("RunwayMarkings");
            markingsGroup.transform.SetParent(runway.transform, false);

            for (float z = -runwayLength * 0.4f; z <= runwayLength * 0.4f; z += 15f)
            {
                GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stripe.name = "CenterStripe";
                stripe.transform.SetParent(markingsGroup.transform, false);
                stripe.transform.localPosition = new Vector3(0f, 0.6f, z / runwayLength);
                stripe.transform.localScale = new Vector3(0.04f, 0.2f, 8f / runwayLength);
                stripe.GetComponent<Renderer>().sharedMaterial = whiteMarkingMaterial;
            }

            // Threshold Lines
            for (int i = -4; i <= 4; i++)
            {
                if (i == 0) continue;
                GameObject thresh1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                thresh1.transform.SetParent(markingsGroup.transform, false);
                thresh1.transform.localPosition = new Vector3(i * 0.08f, 0.6f, 0.45f);
                thresh1.transform.localScale = new Vector3(0.03f, 0.2f, 0.05f);
                thresh1.GetComponent<Renderer>().sharedMaterial = whiteMarkingMaterial;

                GameObject thresh2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                thresh2.transform.SetParent(markingsGroup.transform, false);
                thresh2.transform.localPosition = new Vector3(i * 0.08f, 0.6f, -0.45f);
                thresh2.transform.localScale = new Vector3(0.03f, 0.2f, 0.05f);
                thresh2.GetComponent<Renderer>().sharedMaterial = whiteMarkingMaterial;
            }
        }

        private void BuildHelipad(GameObject parent)
        {
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = "HelipadPad";
            pad.transform.SetParent(parent.transform, false);
            pad.transform.localPosition = new Vector3(35f, 0.08f, 20f);
            pad.transform.localScale = new Vector3(helipadRadius, 0.1f, helipadRadius);
            pad.GetComponent<Renderer>().sharedMaterial = concreteMaterial;

            // Yellow 'H' Marking
            GameObject hMarking = new GameObject("Helipad_H_Marking");
            hMarking.transform.SetParent(pad.transform, false);

            GameObject bar1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar1.transform.SetParent(hMarking.transform, false);
            bar1.transform.localPosition = new Vector3(-0.25f, 0.6f, 0f);
            bar1.transform.localScale = new Vector3(0.08f, 0.2f, 0.5f);
            bar1.GetComponent<Renderer>().sharedMaterial = yellowMarkingMaterial;

            GameObject bar2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar2.transform.SetParent(hMarking.transform, false);
            bar2.transform.localPosition = new Vector3(0.25f, 0.6f, 0f);
            bar2.transform.localScale = new Vector3(0.08f, 0.2f, 0.5f);
            bar2.GetComponent<Renderer>().sharedMaterial = yellowMarkingMaterial;

            GameObject cross = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cross.transform.SetParent(hMarking.transform, false);
            cross.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            cross.transform.localScale = new Vector3(0.5f, 0.2f, 0.08f);
            cross.GetComponent<Renderer>().sharedMaterial = yellowMarkingMaterial;
        }

        private void BuildRoadNetwork(GameObject parent)
        {
            // Taxiway connecting Runway to Helipad and Hangar
            GameObject taxiway = GameObject.CreatePrimitive(PrimitiveType.Cube);
            taxiway.name = "TaxiwayRoad";
            taxiway.transform.SetParent(parent.transform, false);
            taxiway.transform.localPosition = new Vector3(20f, 0.06f, 20f);
            taxiway.transform.localScale = new Vector3(25f, 0.08f, 10f);
            taxiway.GetComponent<Renderer>().sharedMaterial = asphaltMaterial;

            // Main Access Road to Gate
            GameObject accessRoad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            accessRoad.name = "AccessRoad";
            accessRoad.transform.SetParent(parent.transform, false);
            accessRoad.transform.localPosition = new Vector3(60f, 0.06f, 50f);
            accessRoad.transform.localScale = new Vector3(80f, 0.08f, 12f);
            accessRoad.GetComponent<Renderer>().sharedMaterial = asphaltMaterial;
        }

        private void BuildMissionControlBuilding(GameObject parent)
        {
            GameObject gcs = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gcs.name = "MissionControlBuilding";
            gcs.transform.SetParent(parent.transform, false);
            gcs.transform.localPosition = new Vector3(50f, 6f, 0f);
            gcs.transform.localScale = new Vector3(24f, 12f, 30f);
            gcs.GetComponent<Renderer>().sharedMaterial = buildingMaterial;

            // Roof Radar / Comm Tower
            GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.name = "CommTower";
            tower.transform.SetParent(gcs.transform, false);
            tower.transform.localPosition = new Vector3(0.3f, 0.8f, 0.3f);
            tower.transform.localScale = new Vector3(0.1f, 0.6f, 0.1f);
            tower.GetComponent<Renderer>().sharedMaterial = concreteMaterial;
        }

        private void BuildUAVHangar(GameObject parent)
        {
            GameObject hangar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hangar.name = "UAVHangar";
            hangar.transform.SetParent(parent.transform, false);
            hangar.transform.localPosition = new Vector3(-35f, 7f, 40f);
            hangar.transform.localScale = new Vector3(30f, 14f, 25f);
            hangar.GetComponent<Renderer>().sharedMaterial = concreteMaterial;
        }

        private void BuildBoundaryFence(GameObject parent)
        {
            GameObject fence = new GameObject("BoundaryFencePerimeter");
            fence.transform.SetParent(parent.transform, false);

            float halfSize = 220f;
            // 4 perimeter walls
            CreateFenceSegment(fence, new Vector3(0f, 1.5f, halfSize), new Vector3(halfSize * 2f, 3f, 0.2f));
            CreateFenceSegment(fence, new Vector3(0f, 1.5f, -halfSize), new Vector3(halfSize * 2f, 3f, 0.2f));
            CreateFenceSegment(fence, new Vector3(halfSize, 1.5f, 0f), new Vector3(0.2f, 3f, halfSize * 2f));
            CreateFenceSegment(fence, new Vector3(-halfSize, 1.5f, 0f), new Vector3(0.2f, 3f, halfSize * 2f));
        }

        private void CreateFenceSegment(GameObject parent, Vector3 pos, Vector3 scale)
        {
            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "FenceSegment";
            seg.transform.SetParent(parent.transform, false);
            seg.transform.localPosition = pos;
            seg.transform.localScale = scale;
            seg.GetComponent<Renderer>().sharedMaterial = fenceMaterial;
        }

        private void BuildSecurityGate(GameObject parent)
        {
            GameObject gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gate.name = "SecurityGateHouse";
            gate.transform.SetParent(parent.transform, false);
            gate.transform.localPosition = new Vector3(100f, 2.5f, 50f);
            gate.transform.localScale = new Vector3(6f, 5f, 6f);
            gate.GetComponent<Renderer>().sharedMaterial = buildingMaterial;
        }

        private void BuildWindsock(GameObject parent)
        {
            GameObject sockPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sockPole.name = "WindsockPole";
            sockPole.transform.SetParent(parent.transform, false);
            sockPole.transform.localPosition = new Vector3(-15f, 3f, 15f);
            sockPole.transform.localScale = new Vector3(0.2f, 3f, 0.2f);
            sockPole.GetComponent<Renderer>().sharedMaterial = concreteMaterial;

            GameObject sockCloth = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sockCloth.name = "WindsockCloth";
            sockCloth.transform.SetParent(sockPole.transform, false);
            sockCloth.transform.localPosition = new Vector3(0.8f, 0.8f, 0f);
            sockCloth.transform.localRotation = Quaternion.Euler(0f, 0f, -80f);
            sockCloth.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);
            sockCloth.GetComponent<Renderer>().sharedMaterial = yellowMarkingMaterial;

            sockPole.AddComponent<WindSock>();
        }

        private void BuildStreetlights(GameObject parent)
        {
            GameObject group = new GameObject("Streetlights");
            group.transform.SetParent(parent.transform, false);

            for (float z = -runwayLength * 0.4f; z <= runwayLength * 0.4f; z += 40f)
            {
                CreatePole(group, new Vector3(-runwayWidth * 0.7f, 4f, z));
                CreatePole(group, new Vector3(runwayWidth * 0.7f, 4f, z));
            }
        }

        private void CreatePole(GameObject parent, Vector3 pos)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "StreetLightPole";
            pole.transform.SetParent(parent.transform, false);
            pole.transform.localPosition = pos;
            pole.transform.localScale = new Vector3(0.2f, 4f, 0.2f);
            pole.GetComponent<Renderer>().sharedMaterial = concreteMaterial;
        }
    }
}




