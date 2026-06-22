#if UNITY_EDITOR
using System.IO;
using SciFiAnimation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SciFiAnimationEditor
{
    public static class DroneProjectBuilder
    {
        private const string GeneratedRoot = "Assets/Generated";
        private const string MaterialRoot = GeneratedRoot + "/Materials";
        private const string AnimationRoot = GeneratedRoot + "/Animations";
        private const string ScenePath = "Assets/Scenes/DroneDock.unity";

        [MenuItem("Tools/Sci-Fi Drone/Rebuild Playable Draft")]
        public static void Build()
        {
            EnsureFolders();

            Material darkMetal = CreateMaterial("DarkMetal", new Color(0.035f, 0.055f, 0.08f), 0.88f, 0.38f);
            Material panel = CreateMaterial("Panel", new Color(0.08f, 0.13f, 0.18f), 0.72f, 0.28f);
            Material silver = CreateMaterial("Silver", new Color(0.28f, 0.34f, 0.4f), 0.9f, 0.5f);
            Material cyan = CreateMaterial("CyanEmission", new Color(0.02f, 0.22f, 0.28f), 0.4f, 0.25f, new Color(0.05f, 2.8f, 4.2f));
            Material blue = CreateMaterial("BlueEmission", new Color(0.02f, 0.08f, 0.2f), 0.35f, 0.2f, new Color(0.05f, 0.6f, 4.5f));
            Material orange = CreateMaterial("OrangeEmission", new Color(0.3f, 0.08f, 0.01f), 0.3f, 0.2f, new Color(4.5f, 0.55f, 0.02f));
            Material red = CreateMaterial("RedEmission", new Color(0.3f, 0.01f, 0.01f), 0.3f, 0.2f, new Color(5f, 0.02f, 0.02f));
            Material scanBeam = CreateTransparentMaterial("ScanBeam", new Color(0.12f, 0.95f, 1f, 0.24f));

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ConfigureEnvironment();
            BuildHangar(darkMetal, panel, silver, cyan, orange);
            GameObject drone = BuildDrone(darkMetal, silver, cyan, blue, red, scanBeam);
            BuildLighting();
            BuildCameraAndHud(drone);

            AnimatorController controller = BuildAnimations();
            drone.GetComponent<Animator>().runtimeAnimatorController = controller;

            EditorSceneManager.SaveScene(scene, ScenePath);
            SetBuildScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = drone;
            Debug.Log("Playable autonomous drone draft created at " + ScenePath);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Scripts");
            EnsureFolder("Assets", "Editor");
            EnsureFolder("Assets", "Generated");
            EnsureFolder(GeneratedRoot, "Materials");
            EnsureFolder(GeneratedRoot, "Animations");
            EnsureFolder("Assets", "Scenes");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static Material CreateMaterial(string name, Color color, float metallic, float smoothness, Color? emission = null)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (emission.HasValue)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission.Value);
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            return material;
        }

        private static Material CreateTransparentMaterial(string name, Color color)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", color);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_ZWrite", 0f);
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        private static void ConfigureEnvironment()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.06f, 0.09f, 0.16f);
            RenderSettings.ambientEquatorColor = new Color(0.025f, 0.04f, 0.07f);
            RenderSettings.ambientGroundColor = new Color(0.008f, 0.012f, 0.02f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.012f, 0.025f, 0.045f);
            RenderSettings.fogDensity = 0.012f;
        }

        private static void BuildHangar(Material dark, Material panel, Material silver, Material cyan, Material orange)
        {
            GameObject environment = new GameObject("HANGAR ENVIRONMENT");
            CreatePrimitive("Floor", PrimitiveType.Cube, environment.transform, new Vector3(0f, -0.35f, 2f), new Vector3(24f, 0.6f, 22f), dark);
            CreatePrimitive("BackWall", PrimitiveType.Cube, environment.transform, new Vector3(0f, 5f, 12.5f), new Vector3(24f, 10f, 0.5f), panel);
            CreatePrimitive("LeftWall", PrimitiveType.Cube, environment.transform, new Vector3(-12f, 4f, 2f), new Vector3(0.5f, 8f, 22f), panel);
            CreatePrimitive("RightWall", PrimitiveType.Cube, environment.transform, new Vector3(12f, 4f, 2f), new Vector3(0.5f, 8f, 22f), panel);

            for (int z = -7; z <= 11; z += 3)
            {
                CreatePrimitive("FloorStrip", PrimitiveType.Cube, environment.transform, new Vector3(-5.5f, 0.01f, z), new Vector3(0.15f, 0.04f, 1.4f), cyan);
                CreatePrimitive("FloorStrip", PrimitiveType.Cube, environment.transform, new Vector3(5.5f, 0.01f, z), new Vector3(0.15f, 0.04f, 1.4f), cyan);
            }

            for (int x = -9; x <= 9; x += 3)
            {
                CreatePrimitive("WallRib", PrimitiveType.Cube, environment.transform, new Vector3(x, 5f, 12.15f), new Vector3(0.28f, 9f, 0.3f), silver);
            }

            GameObject pad = new GameObject("DOCKING PAD");
            pad.transform.SetParent(environment.transform);
            CreatePrimitive("PadBase", PrimitiveType.Cylinder, pad.transform, new Vector3(0f, 0.15f, 2f), new Vector3(4.4f, 0.25f, 4.4f), silver);
            CreatePrimitive("PadInset", PrimitiveType.Cylinder, pad.transform, new Vector3(0f, 0.33f, 2f), new Vector3(3.5f, 0.12f, 3.5f), dark);
            CreatePrimitive("PadCore", PrimitiveType.Cylinder, pad.transform, new Vector3(0f, 0.42f, 2f), new Vector3(1.2f, 0.08f, 1.2f), cyan);

            CreatePrimitive("ArchLeft", PrimitiveType.Cube, pad.transform, new Vector3(-4f, 2.8f, 3.4f), new Vector3(0.55f, 5.4f, 0.7f), silver);
            CreatePrimitive("ArchRight", PrimitiveType.Cube, pad.transform, new Vector3(4f, 2.8f, 3.4f), new Vector3(0.55f, 5.4f, 0.7f), silver);
            CreatePrimitive("ArchTop", PrimitiveType.Cube, pad.transform, new Vector3(0f, 5.35f, 3.4f), new Vector3(8.5f, 0.5f, 0.7f), silver);

            for (int i = -3; i <= 3; i++)
            {
                CreatePrimitive("WarningLamp", PrimitiveType.Cube, pad.transform, new Vector3(i * 1.1f, 5.02f, 3.02f), new Vector3(0.45f, 0.12f, 0.1f), i % 2 == 0 ? orange : cyan);
            }

            BuildObservationWindow(environment.transform, dark, cyan);
        }

        private static void BuildObservationWindow(Transform parent, Material dark, Material cyan)
        {
            GameObject window = new GameObject("ObservationWindow");
            window.transform.SetParent(parent);
            window.transform.localPosition = new Vector3(0f, 0f, 0f);
            CreatePrimitive("WindowVoid", PrimitiveType.Cube, window.transform, new Vector3(0f, 6.4f, 12.16f), new Vector3(19f, 6.2f, 0.12f), dark);
            CreatePrimitive("FrameTop", PrimitiveType.Cube, window.transform, new Vector3(0f, 9.6f, 12f), new Vector3(19.5f, 0.22f, 0.35f), cyan);
            CreatePrimitive("FrameBottom", PrimitiveType.Cube, window.transform, new Vector3(0f, 3.2f, 12f), new Vector3(19.5f, 0.22f, 0.35f), cyan);
            CreatePrimitive("FrameLeft", PrimitiveType.Cube, window.transform, new Vector3(-9.6f, 6.4f, 12f), new Vector3(0.22f, 6.6f, 0.35f), cyan);
            CreatePrimitive("FrameRight", PrimitiveType.Cube, window.transform, new Vector3(9.6f, 6.4f, 12f), new Vector3(0.22f, 6.6f, 0.35f), cyan);

            GameObject starField = new GameObject("StarField");
            starField.transform.SetParent(window.transform);
            starField.transform.position = new Vector3(0f, 6.4f, 11.8f);
            ParticleSystem stars = starField.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = stars.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.03f, 0.16f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.12f);
            main.maxParticles = 180;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.45f), new Color(1f, 0.62f, 0.08f));
            ParticleSystem.EmissionModule emission = stars.emission;
            emission.rateOverTime = 38f;
            ParticleSystem.ShapeModule shape = stars.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(18.5f, 5.8f, 0.2f);
            ParticleSystem.ColorOverLifetimeModule starColor = stars.colorOverLifetime;
            starColor.enabled = true;
            Gradient twinkle = new Gradient();
            twinkle.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.7f, 0.12f), 0f), new GradientColorKey(new Color(1f, 1f, 0.65f), 0.5f), new GradientColorKey(new Color(1f, 0.65f, 0.08f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.18f), new GradientAlphaKey(0.25f, 0.55f), new GradientAlphaKey(0f, 1f) });
            starColor.color = twinkle;
        }

        private static GameObject BuildDrone(Material dark, Material silver, Material cyan, Material blue, Material red, Material scanBeam)
        {
            GameObject drone = new GameObject("A-17 Autonomous Drone");
            drone.transform.position = new Vector3(0f, 0.9f, 2f);
            Animator animator = drone.AddComponent<Animator>();
            animator.applyRootMotion = false;

            GameObject rig = new GameObject("VisualRig");
            rig.transform.SetParent(drone.transform, false);
            rig.transform.localPosition = new Vector3(0f, -0.2f, 0f);

            CreatePrimitive("Body", PrimitiveType.Sphere, rig.transform, Vector3.zero, new Vector3(2.5f, 0.75f, 1.65f), dark);
            CreatePrimitive("BodySpine", PrimitiveType.Cube, rig.transform, new Vector3(0f, 0.12f, 0f), new Vector3(1.5f, 0.3f, 2.4f), silver);
            CreatePrimitive("SensorEye", PrimitiveType.Sphere, rig.transform, new Vector3(0f, 0.13f, -0.76f), new Vector3(0.9f, 0.38f, 0.28f), cyan);

            Transform leftWing = CreateWingPivot("WingLeft", rig.transform, -1.05f, silver, dark, cyan, true);
            Transform rightWing = CreateWingPivot("WingRight", rig.transform, 1.05f, silver, dark, cyan, false);

            GameObject mast = new GameObject("SensorMast");
            mast.transform.SetParent(rig.transform, false);
            mast.transform.localPosition = new Vector3(0f, 0.42f, 0.25f);
            CreatePrimitive("Mast", PrimitiveType.Cylinder, mast.transform, new Vector3(0f, 0.25f, 0f), new Vector3(0.12f, 0.3f, 0.12f), silver);
            GameObject dish = CreatePrimitive("Dish", PrimitiveType.Sphere, mast.transform, new Vector3(0f, 0.56f, 0f), new Vector3(0.65f, 0.12f, 0.65f), cyan);
            Spin dishSpin = dish.AddComponent<Spin>();
            dishSpin.Configure(new Vector3(0f, 90f, 0f));

            GameObject scanPivot = new GameObject("ScanPivot");
            scanPivot.transform.SetParent(rig.transform, false);
            scanPivot.transform.localPosition = new Vector3(0f, -0.05f, -0.7f);
            Light scanLight = scanPivot.AddComponent<Light>();
            scanLight.type = LightType.Spot;
            scanLight.color = new Color(0.1f, 0.85f, 1f);
            scanLight.range = 20f;
            scanLight.spotAngle = 46f;
            scanLight.intensity = 0f;
            scanLight.enabled = false;
            scanPivot.transform.localRotation = Quaternion.Euler(38f, 180f, 0f);
            GameObject beam = CreatePrimitive("ScanBeam", PrimitiveType.Cylinder, scanPivot.transform, new Vector3(0f, 0f, 3.8f), new Vector3(0.22f, 3.8f, 0.22f), scanBeam);
            beam.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            beam.GetComponent<MeshRenderer>().enabled = false;
            GameObject pulseOne = CreatePrimitive("ScanPulse1", PrimitiveType.Cylinder, rig.transform, new Vector3(0f, -1.32f, 0f), new Vector3(0.2f, 0.025f, 0.2f), scanBeam);
            GameObject pulseTwo = CreatePrimitive("ScanPulse2", PrimitiveType.Cylinder, rig.transform, new Vector3(0f, -1.3f, 0f), new Vector3(0.2f, 0.02f, 0.2f), scanBeam);
            pulseOne.GetComponent<MeshRenderer>().enabled = false;
            pulseTwo.GetComponent<MeshRenderer>().enabled = false;

            Light leftBeacon = CreateLight("LeftBeacon", rig.transform, new Vector3(-1.15f, 0.22f, 0f), Color.red, 0f, 4f);
            Light rightBeacon = CreateLight("RightBeacon", rig.transform, new Vector3(1.15f, 0.22f, 0f), Color.red, 0f, 4f);
            CreatePrimitive("LeftBeaconLens", PrimitiveType.Sphere, leftBeacon.transform, Vector3.zero, Vector3.one * 0.13f, red);
            CreatePrimitive("RightBeaconLens", PrimitiveType.Sphere, rightBeacon.transform, Vector3.zero, Vector3.one * 0.13f, red);

            ParticleSystem leftThruster = CreateThruster("LeftThruster", rig.transform, new Vector3(-0.75f, -0.32f, 0.45f), blue);
            ParticleSystem rightThruster = CreateThruster("RightThruster", rig.transform, new Vector3(0.75f, -0.32f, 0.45f), blue);

            DroneController controller = drone.AddComponent<DroneController>();
            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("visualRig").objectReferenceValue = rig.transform;
            serialized.FindProperty("leftThruster").objectReferenceValue = leftThruster;
            serialized.FindProperty("rightThruster").objectReferenceValue = rightThruster;
            serialized.FindProperty("scanLight").objectReferenceValue = scanLight;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            _ = leftWing;
            _ = rightWing;
            return drone;
        }

        private static Transform CreateWingPivot(string name, Transform parent, float x, Material silver, Material dark, Material cyan, bool left)
        {
            GameObject pivot = new GameObject(name);
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = new Vector3(x, 0f, 0f);
            float direction = left ? -1f : 1f;
            CreatePrimitive("WingPlate", PrimitiveType.Cube, pivot.transform, new Vector3(direction * 0.85f, -0.03f, 0.05f), new Vector3(1.65f, 0.12f, 1.1f), silver);
            CreatePrimitive("WingInset", PrimitiveType.Cube, pivot.transform, new Vector3(direction * 0.9f, 0.05f, 0.05f), new Vector3(1.25f, 0.05f, 0.65f), dark);
            CreatePrimitive("WingLight", PrimitiveType.Cube, pivot.transform, new Vector3(direction * 1.55f, 0.08f, 0.05f), new Vector3(0.14f, 0.09f, 0.75f), cyan);
            return pivot.transform;
        }

        private static ParticleSystem CreateThruster(string name, Transform parent, Vector3 position, Material glow)
        {
            GameObject nozzle = CreatePrimitive(name + "Nozzle", PrimitiveType.Cylinder, parent, position, new Vector3(0.28f, 0.25f, 0.28f), glow);
            nozzle.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            GameObject particlesObject = new GameObject(name);
            particlesObject.transform.SetParent(parent, false);
            particlesObject.transform.localPosition = position + Vector3.down * 0.22f;
            particlesObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ParticleSystem particles = particlesObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.38f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.88f, 0.98f, 1f, 0.95f), new Color(0.42f, 0.78f, 1f, 0.6f));
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 38f;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = 0.12f;
            ParticleSystem.ColorOverLifetimeModule vaporColor = particles.colorOverLifetime;
            vaporColor.enabled = true;
            Gradient vaporFade = new Gradient();
            vaporFade.SetKeys(
                new[] { new GradientColorKey(new Color(0.9f, 0.98f, 1f), 0f), new GradientColorKey(new Color(0.48f, 0.78f, 1f), 1f) },
                new[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) });
            vaporColor.color = vaporFade;
            return particles;
        }

        private static Light CreateLight(string name, Transform parent, Vector3 localPosition, Color color, float intensity, float range)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = localPosition;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            return light;
        }

        private static void BuildLighting()
        {
            GameObject keyObject = new GameObject("Hangar Key Light");
            keyObject.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            Light key = keyObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(0.45f, 0.62f, 1f);
            key.intensity = 0.7f;
            key.shadows = LightShadows.Soft;

            Light cyanFill = CreateLight("Cyan Fill", null, new Vector3(-5f, 4f, -1f), new Color(0.05f, 0.6f, 1f), 7f, 14f);
            cyanFill.transform.position = new Vector3(-5f, 4f, -1f);
            Light warmFill = CreateLight("Warm Fill", null, new Vector3(5f, 3f, 6f), new Color(1f, 0.25f, 0.05f), 4f, 11f);
            warmFill.transform.position = new Vector3(5f, 3f, 6f);
        }

        private static void BuildCameraAndHud(GameObject drone)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(10f, 7f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.008f, 0.015f, 0.03f);
            camera.fieldOfView = 55f;
            camera.allowHDR = true;
            cameraObject.AddComponent<AudioListener>();
            FollowCamera follow = cameraObject.AddComponent<FollowCamera>();
            follow.SetTarget(drone.transform);

            GameObject hudObject = new GameObject("HUD");
            DroneHUD hud = hudObject.AddComponent<DroneHUD>();
            hud.SetDrone(drone.GetComponent<DroneController>());
        }

        private static AnimatorController BuildAnimations()
        {
            AnimationClip docked = CreateDockedClip();
            AnimationClip launch = CreateDeployClip("Drone_HandKeyed_Launch", false);
            AnimationClip docking = CreateDeployClip("Drone_HandKeyed_Docking", true);
            AnimationClip flying = CreateFlyingClip();
            AnimationClip scanning = CreateScanningClip();
            AnimationClip emergency = CreateEmergencyClip();

            string controllerPath = AnimationRoot + "/DroneStateMachine.controller";
            AssetDatabase.DeleteAsset(controllerPath);
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Launch", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dock", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Scan", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Emergency", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Recover", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState dockedState = machine.AddState("Docked", new Vector3(170f, 80f));
            AnimatorState launchState = machine.AddState("Launching", new Vector3(390f, 30f));
            AnimatorState flyingState = machine.AddState("Flying", new Vector3(610f, 80f));
            AnimatorState scanState = machine.AddState("Scanning", new Vector3(610f, 230f));
            AnimatorState dockingState = machine.AddState("Docking", new Vector3(390f, 180f));
            AnimatorState emergencyState = machine.AddState("Emergency", new Vector3(390f, 330f));
            dockedState.motion = docked;
            launchState.motion = launch;
            flyingState.motion = flying;
            scanState.motion = scanning;
            dockingState.motion = docking;
            emergencyState.motion = emergency;
            machine.defaultState = dockedState;

            AddTriggerTransition(dockedState, launchState, "Launch", 0.08f);
            AddExitTransition(launchState, flyingState, 0.96f, 0.15f);
            AddTriggerTransition(flyingState, scanState, "Scan", 0.08f);
            AddExitTransition(scanState, flyingState, 0.96f, 0.12f);
            AddTriggerTransition(flyingState, dockingState, "Dock", 0.12f);
            AddExitTransition(dockingState, dockedState, 0.96f, 0.12f);
            AddTriggerTransition(emergencyState, flyingState, "Recover", 0.12f);

            AnimatorStateTransition emergencyTransition = machine.AddAnyStateTransition(emergencyState);
            emergencyTransition.hasExitTime = false;
            emergencyTransition.duration = 0.08f;
            emergencyTransition.canTransitionToSelf = false;
            emergencyTransition.AddCondition(AnimatorConditionMode.If, 0f, "Emergency");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void AddTriggerTransition(AnimatorState from, AnimatorState to, string parameter, float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.duration = duration;
            transition.AddCondition(AnimatorConditionMode.If, 0f, parameter);
        }

        private static void AddExitTransition(AnimatorState from, AnimatorState to, float exitTime, float duration)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = exitTime;
            transition.duration = duration;
        }

        private static AnimationClip CreateDockedClip()
        {
            AnimationClip clip = NewClip("Drone_Docked", true);
            SetCurve(clip, "VisualRig", "m_LocalPosition.y", Curve((0f, -0.2f), (1.2f, -0.12f), (2.4f, -0.2f)));
            SetCurve(clip, "VisualRig/WingLeft", "localEulerAnglesRaw.z", Curve((0f, -72f), (2.4f, -72f)));
            SetCurve(clip, "VisualRig/WingRight", "localEulerAnglesRaw.z", Curve((0f, 72f), (2.4f, 72f)));
            SetScaleCurves(clip, "VisualRig/SensorMast", 0.05f, 0.05f, 0f, 2.4f, 2.4f);
            SaveClip(clip);
            return clip;
        }

        private static AnimationClip CreateDeployClip(string name, bool reverse)
        {
            AnimationClip clip = NewClip(name, false);
            float y0 = reverse ? 1.5f : -0.2f;
            float y1 = reverse ? -0.2f : 1.5f;
            float wingStart = reverse ? 0f : 72f;
            float wingEnd = reverse ? 72f : 0f;
            float mastStart = reverse ? 1f : 0.05f;
            float mastEnd = reverse ? 0.05f : 1f;
            SetCurve(clip, "VisualRig", "m_LocalPosition.y", Curve((0f, y0), (0.35f, y0), (1.5f, y1), (2.2f, y1)));
            SetCurve(clip, "VisualRig/WingLeft", "localEulerAnglesRaw.z", Curve((0f, -wingStart), (0.45f, -wingStart), (1.35f, -wingEnd), (2.2f, -wingEnd)));
            SetCurve(clip, "VisualRig/WingRight", "localEulerAnglesRaw.z", Curve((0f, wingStart), (0.45f, wingStart), (1.35f, wingEnd), (2.2f, wingEnd)));
            SetScaleCurves(clip, "VisualRig/SensorMast", mastStart, mastEnd, 0.7f, 1.55f, 2.2f);
            SaveClip(clip);
            return clip;
        }

        private static AnimationClip CreateFlyingClip()
        {
            AnimationClip clip = NewClip("Drone_FlyingHover", true);
            SetCurve(clip, "VisualRig", "m_LocalPosition.y", Curve((0f, 1.5f), (0.8f, 1.62f), (1.6f, 1.5f)));
            SaveClip(clip);
            return clip;
        }

        private static AnimationClip CreateScanningClip()
        {
            AnimationClip clip = NewClip("Drone_EnergyScan", false);
            SetCurve(clip, "VisualRig", "m_LocalPosition.y", Curve((0f, 1.5f), (0.25f, 1.9f), (2.4f, 1.9f), (2.7f, 1.5f)));
            SetCurve(clip, "VisualRig/ScanPivot", "localEulerAnglesRaw.y", Curve((0f, 115f), (0.7f, 235f), (1.4f, 125f), (2.15f, 240f), (2.7f, 180f)));
            SetComponentCurve<Light>(clip, "VisualRig/ScanPivot", "m_Enabled", Curve((0f, 1f), (2.65f, 1f), (2.7f, 0f)));
            SetComponentCurve<Light>(clip, "VisualRig/ScanPivot", "m_Intensity", Curve((0f, 0f), (0.12f, 24f), (2.45f, 24f), (2.7f, 0f)));
            SetComponentCurve<MeshRenderer>(clip, "VisualRig/ScanPivot/ScanBeam", "m_Enabled", Curve((0f, 0f), (0.08f, 1f), (2.55f, 1f), (2.7f, 0f)));
            SetCurve(clip, "VisualRig/ScanPivot/ScanBeam", "m_LocalScale.x", Curve((0f, 0.03f), (0.2f, 0.22f), (2.5f, 0.22f), (2.7f, 0.03f)));
            SetCurve(clip, "VisualRig/ScanPivot/ScanBeam", "m_LocalScale.z", Curve((0f, 0.03f), (0.2f, 0.22f), (2.5f, 0.22f), (2.7f, 0.03f)));
            AddScanPulseCurves(clip, "VisualRig/ScanPulse1", 0.15f, 1.35f);
            AddScanPulseCurves(clip, "VisualRig/ScanPulse2", 1.15f, 2.35f);
            SaveClip(clip);
            return clip;
        }

        private static void AddScanPulseCurves(AnimationClip clip, string path, float start, float end)
        {
            SetComponentCurve<MeshRenderer>(clip, path, "m_Enabled", Curve((0f, 0f), (start, 1f), (end, 1f), (end + 0.05f, 0f), (2.7f, 0f)));
            SetCurve(clip, path, "m_LocalScale.x", Curve((0f, 0.2f), (start, 0.2f), (end, 5.6f), (2.7f, 0.2f)));
            SetCurve(clip, path, "m_LocalScale.z", Curve((0f, 0.2f), (start, 0.2f), (end, 5.6f), (2.7f, 0.2f)));
        }

        private static AnimationClip CreateEmergencyClip()
        {
            AnimationClip clip = NewClip("Drone_Emergency", true);
            SetCurve(clip, "VisualRig", "m_LocalPosition.x", Curve((0f, 0f), (0.08f, -0.08f), (0.16f, 0.08f), (0.24f, 0f)));
            SetCurve(clip, "VisualRig", "m_LocalPosition.y", Curve((0f, 1.5f), (0.12f, 1.56f), (0.24f, 1.5f)));
            AnimationCurve pulse = Curve((0f, 0f), (0.12f, 9f), (0.24f, 0f), (0.36f, 9f), (0.48f, 0f));
            SetComponentCurve<Light>(clip, "VisualRig/LeftBeacon", "m_Intensity", pulse);
            SetComponentCurve<Light>(clip, "VisualRig/RightBeacon", "m_Intensity", pulse);
            SaveClip(clip);
            return clip;
        }

        private static AnimationClip NewClip(string name, bool loop)
        {
            AnimationClip clip = new AnimationClip { name = name, frameRate = 60f };
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        private static void SaveClip(AnimationClip clip)
        {
            string path = AnimationRoot + "/" + clip.name + ".anim";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(clip, path);
        }

        private static AnimationCurve Curve(params (float time, float value)[] values)
        {
            Keyframe[] keys = new Keyframe[values.Length];
            for (int i = 0; i < values.Length; i++) keys[i] = new Keyframe(values[i].time, values[i].value);
            AnimationCurve curve = new AnimationCurve(keys);
            for (int i = 0; i < curve.length; i++) AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            for (int i = 0; i < curve.length; i++) AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            return curve;
        }

        private static void SetCurve(AnimationClip clip, string path, string property, AnimationCurve curve)
        {
            clip.SetCurve(path, typeof(Transform), property, curve);
        }

        private static void SetComponentCurve<T>(AnimationClip clip, string path, string property, AnimationCurve curve) where T : Component
        {
            clip.SetCurve(path, typeof(T), property, curve);
        }

        private static void SetScaleCurves(AnimationClip clip, string path, float start, float end, float delay, float finish, float duration)
        {
            AnimationCurve scale = Curve((0f, start), (delay, start), (finish, end), (duration, end));
            SetCurve(clip, path, "m_LocalScale.x", scale);
            SetCurve(clip, path, "m_LocalScale.y", scale);
            SetCurve(clip, path, "m_LocalScale.z", scale);
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject item = GameObject.CreatePrimitive(type);
            item.name = name;
            if (parent != null) item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = scale;
            Renderer renderer = item.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
            Collider collider = item.GetComponent<Collider>();
            if (collider != null) Object.DestroyImmediate(collider);
            return item;
        }

        private static void SetBuildScene()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
#endif
