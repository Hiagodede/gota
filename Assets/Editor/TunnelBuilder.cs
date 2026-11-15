using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Gota.EditorTools
{
    /// <summary>
    /// Constrói a versão 2.5D do jogo (Marco 5) a partir de código, em passos
    /// reproduzíveis: renderer 3D, texturas e materiais, prefabs 3D e a cena Tunnel.
    /// A cena 2D (SampleScene) continua intacta como válvula de escape.
    ///
    /// Convenções de espaço: o mundo continua rolando em +Y (código de jogo igual).
    /// A câmera fica em Y positivo olhando para -Y, com "cima" = +Z. Assim, X é
    /// esquerda/direita (gameplay), Z é chão/teto (só visual) e Y é profundidade.
    /// </summary>
    public static class TunnelBuilder
    {
        // Geometria do túnel (metros)
        public const float HalfWidth = 3.5f;      // paredes em x = ±3,5
        public const float CenterZ = 1.2f;        // centro do túnel em z (a gota fica abaixo do centro da tela)
        public const float SegmentLength = 10f;
        public const int SegmentCount = 6;
        public const float PlayerY = 2.5f;        // mesma altura da cena 2D
        public const float CameraDistance = 9.5f; // câmera em y = PlayerY + CameraDistance

        private const string SettingsFolder = "Assets/Settings";
        private const string MaterialsFolder = "Assets/Art/Materials";
        private const string MeshFolder = "Assets/Art/Meshes";
        private const string PrefabFolder = "Assets/Prefabs/Tunnel";
        private const string ScenePath = "Assets/Scenes/Tunnel.unity";

        // ------------------------------------------------------------------ 1. renderer

        [MenuItem("Gota/Túnel/1. Criar renderer 3D")]
        public static void CreateRenderer()
        {
            string path = SettingsFolder + "/Renderer3D.asset";
            var existing = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            if (existing == null)
            {
                // O método interno da URP preenche os shaders e o PostProcessData certos.
                var method = typeof(UniversalRenderPipelineAsset).GetMethod("CreateRendererAsset",
                    BindingFlags.NonPublic | BindingFlags.Static);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    object[] args = parameters.Length == 4
                        ? new object[] { path, RendererType.UniversalRenderer, false, "" }
                        : new object[] { path, RendererType.UniversalRenderer, false };
                    method.Invoke(null, args);
                }
                else
                {
                    var data = ScriptableObject.CreateInstance<UniversalRendererData>();
                    AssetDatabase.CreateAsset(data, path);
                }
                AssetDatabase.SaveAssets();
                existing = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
            }

            // Sem sombras: o túnel é iluminado por emissão; sombra custa caro no celular.
            existing.shadowTransparentReceive = false;
            EditorUtility.SetDirty(existing);

            // Adiciona à lista de renderers do pipeline (índice 1; o 2D continua sendo o padrão).
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(SettingsFolder + "/UniversalRP.asset");
            var so = new SerializedObject(pipeline);
            var list = so.FindProperty("m_RendererDataList");
            bool found = false;
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == existing) found = true;
            }
            if (!found)
            {
                list.arraySize++;
                list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = existing;
            }
            so.FindProperty("m_SupportsHDR").boolValue = true;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            Debug.Log("Renderer 3D pronto: " + path + " (índice " + RendererIndex() + ")");
        }

        public static int RendererIndex()
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(SettingsFolder + "/UniversalRP.asset");
            var so = new SerializedObject(pipeline);
            var list = so.FindProperty("m_RendererDataList");
            for (int i = 0; i < list.arraySize; i++)
            {
                if (list.GetArrayElementAtIndex(i).objectReferenceValue is UniversalRendererData) return i;
            }
            return 0;
        }

        // ------------------------------------------------------------------ 2. materiais

        [MenuItem("Gota/Túnel/2. Gerar texturas e materiais")]
        public static void CreateMaterials()
        {
            EnsureFolder(MaterialsFolder);
            EnsureFolder(MeshFolder);

            string concreteTex = "Assets/Art/Concrete.png";
            if (!File.Exists(concreteTex)) WriteConcreteTexture(concreteTex, 512);
            AssetDatabase.ImportAsset(concreteTex);
            var importer = (TextureImporter)AssetImporter.GetAtPath(concreteTex);
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.mipmapEnabled = true;
            importer.SaveAndReimport();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(concreteTex);

            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var unlit = Shader.Find("Universal Render Pipeline/Unlit");

            // Concreto escuro: cor base vem da paleta (slot Wall), textura dá a sujeira.
            var concrete = GetOrCreateMaterial(MaterialsFolder + "/Concrete.mat", lit);
            concrete.SetTexture("_BaseMap", tex);
            concrete.SetTextureScale("_BaseMap", new Vector2(2f, 3f));
            concrete.SetColor("_BaseColor", new Color(0.42f, 0.62f, 0.68f));
            concrete.SetFloat("_Smoothness", 0.25f);
            concrete.SetFloat("_Metallic", 0f);

            // Neon: sem luz, só cor HDR (o bloom faz o brilho).
            var neon = GetOrCreateMaterial(MaterialsFolder + "/Neon.mat", unlit);
            neon.SetColor("_BaseColor", new Color(0.37f, 0.89f, 0.94f) * 1.8f);

            // Perigo: branco frio, quase sem textura, levemente emissivo para ler no escuro.
            var danger = GetOrCreateMaterial(MaterialsFolder + "/Danger.mat", lit);
            danger.SetColor("_BaseColor", new Color(0.78f, 0.86f, 0.9f));
            danger.SetFloat("_Smoothness", 0.5f);
            danger.EnableKeyword("_EMISSION");
            danger.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            danger.SetColor("_EmissionColor", new Color(0.9f, 0.98f, 1f) * 0.12f);

            // Gotinha: neon mais fraco, mas emissivo.
            var collectible = GetOrCreateMaterial(MaterialsFolder + "/Collectible.mat", lit);
            collectible.SetColor("_BaseColor", new Color(0.37f, 0.89f, 0.94f));
            collectible.SetFloat("_Smoothness", 0.9f);
            collectible.EnableKeyword("_EMISSION");
            collectible.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            collectible.SetColor("_EmissionColor", new Color(0.37f, 0.89f, 0.94f) * 1.2f);

            // Vidro da gota: transparente, muito liso, com um leve brilho interno.
            var glass = GetOrCreateMaterial(MaterialsFolder + "/Glass.mat", lit);
            MakeTransparent(glass);
            glass.SetColor("_BaseColor", new Color(0.75f, 0.95f, 1f, 0.45f));
            glass.SetFloat("_Smoothness", 0.98f);
            glass.SetFloat("_Metallic", 0.1f);
            glass.EnableKeyword("_EMISSION");
            glass.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            glass.SetColor("_EmissionColor", new Color(0.37f, 0.89f, 0.94f) * 0.35f);

            // Volume de pós-processamento: bloom é o que faz o neon "vazar" luz.
            string volumePath = SettingsFolder + "/TunnelVolume.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(volumePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, volumePath);
            }
            var bloom = GetOrAddOverride<Bloom>(profile);
            bloom.threshold.Override(0.8f);
            bloom.intensity.Override(2.2f);
            bloom.scatter.Override(0.7f);
            bloom.highQualityFiltering.Override(false);
            bloom.maxIterations.Override(4); // menos passes = mais barato no celular
            bloom.downscale.Override(BloomDownscaleMode.Half);
            var tone = GetOrAddOverride<Tonemapping>(profile);
            tone.mode.Override(TonemappingMode.ACES);
            var vignette = GetOrAddOverride<Vignette>(profile);
            vignette.intensity.Override(0.3f);
            vignette.smoothness.Override(0.5f);
            EditorUtility.SetDirty(profile);

            // Malha da ponta (espinho): pirâmide apontando para +Y, base em y = -0,5.
            string spikePath = MeshFolder + "/Spike.asset";
            if (AssetDatabase.LoadAssetAtPath<Mesh>(spikePath) == null)
            {
                AssetDatabase.CreateAsset(BuildPyramid(), spikePath);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("Texturas, materiais, volume e malhas prontos em " + MaterialsFolder);
        }

        // ------------------------------------------------------------------ 3. prefabs

        [MenuItem("Gota/Túnel/3. Criar prefabs 3D")]
        public static void CreatePrefabs()
        {
            EnsureFolder(PrefabFolder);

            // Segmento do túnel: 4 placas de concreto + 2 aros de neon + frisos nos cantos.
            var segment = new GameObject("TunnelSegment");
            float w = HalfWidth, len = SegmentLength, t = 0.1f;
            Slab(segment, "Floor", new Vector3(0f, 0f, CenterZ - w - t / 2f), new Vector3(w * 2f + t * 2f, len, t));
            Slab(segment, "Ceiling", new Vector3(0f, 0f, CenterZ + w + t / 2f), new Vector3(w * 2f + t * 2f, len, t));
            Slab(segment, "WallLeft", new Vector3(-w - t / 2f, 0f, CenterZ), new Vector3(t, len, w * 2f));
            Slab(segment, "WallRight", new Vector3(w + t / 2f, 0f, CenterZ), new Vector3(t, len, w * 2f));
            Ring(segment, "RingA", -len / 4f);
            Ring(segment, "RingB", len / 4f);
            var scroll = segment.AddComponent<ScrollingSegment>();
            var so = new SerializedObject(scroll);
            so.FindProperty("segmentHeight").floatValue = SegmentLength;
            so.FindProperty("wrapY").floatValue = PlayerY + CameraDistance + SegmentLength / 2f;
            so.FindProperty("loopLength").floatValue = SegmentLength * SegmentCount;
            so.ApplyModifiedPropertiesWithoutUndo();
            SavePrefab(segment, PrefabFolder + "/TunnelSegment.prefab");

            // Obstáculos: copia o prefab 2D (colisores, Scrollable, Obstacle) e troca o visual.
            var block = Convert3D("Assets/Prefabs/Obstacle_Block.prefab", PrimitiveType.Cube, null, new Vector3(2.56f, 2.56f, 1.6f), MaterialSlot.Danger);
            SavePrefab(block, PrefabFolder + "/Obstacle_Block3D.prefab");

            var spikeMesh = AssetDatabase.LoadAssetAtPath<Mesh>(MeshFolder + "/Spike.asset");
            var spike = Convert3D("Assets/Prefabs/Obstacle_Spike.prefab", PrimitiveType.Cube, spikeMesh, new Vector3(2.56f, 2.56f, 1.6f), MaterialSlot.Danger);
            SavePrefab(spike, PrefabFolder + "/Obstacle_Spike3D.prefab");

            var mover = Convert3D("Assets/Prefabs/Obstacle_Mover.prefab", PrimitiveType.Cube, null, new Vector3(2.56f, 2.56f, 1.6f), MaterialSlot.Danger);
            SavePrefab(mover, PrefabFolder + "/Obstacle_Mover3D.prefab");

            var drop = Convert3D("Assets/Prefabs/Collectible_Drop.prefab", PrimitiveType.Sphere, null, new Vector3(1.1f, 1.1f, 1.1f), MaterialSlot.Collectible);
            SavePrefab(drop, PrefabFolder + "/Collectible_Drop3D.prefab");

            AssetDatabase.SaveAssets();
            Debug.Log("Prefabs 3D prontos em " + PrefabFolder);
        }

        // ------------------------------------------------------------------ 4. cena

        [MenuItem("Gota/Túnel/4. Montar cena Tunnel")]
        public static void BuildScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // Parte da cena 2D para herdar UI, estados, áudio e efeitos já ligados.
            AssetDatabase.CopyAsset("Assets/Scenes/SampleScene.unity", ScenePath);
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            Object.DestroyImmediate(GameObject.Find("Walls"));
            Object.DestroyImmediate(GameObject.Find("Background"));

            // Câmera em perspectiva, olhando para -Y com o teto (+Z) para cima.
            var cam = Camera.main;
            cam.orthographic = false;
            cam.fieldOfView = 75f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 70f;
            cam.transform.position = new Vector3(0f, PlayerY + CameraDistance, CenterZ);
            cam.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            cam.clearFlags = CameraClearFlags.SolidColor;
            var camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.SetRenderer(RendererIndex());
            camData.antialiasing = AntialiasingMode.None;

            // Luz fraca: o neon é a luz principal. Sem sombras.
            var lightGo = new GameObject("Key Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.5f, 0.9f, 1f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.None;
            lightGo.transform.rotation = Quaternion.LookRotation(new Vector3(0.3f, -1f, -0.6f));

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.78f, 0.85f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 15f;
            RenderSettings.fogEndDistance = 55f;
            RenderSettings.fogColor = cam.backgroundColor;

            // Pós-processamento
            var volumeGo = new GameObject("Post Volume");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(SettingsFolder + "/TunnelVolume.asset");

            // Materiais de execução
            var matsGo = new GameObject("TunnelMaterials");
            var mats = matsGo.AddComponent<TunnelMaterials>();
            var mso = new SerializedObject(mats);
            mso.FindProperty("concrete").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Concrete.mat");
            mso.FindProperty("neon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Neon.mat");
            mso.FindProperty("danger").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Danger.mat");
            mso.FindProperty("collectible").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Collectible.mat");
            mso.FindProperty("glass").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/Glass.mat");
            mso.ApplyModifiedPropertiesWithoutUndo();

            // Túnel: 6 segmentos em fila, do mais perto da câmera ao mais longe.
            var tunnel = new GameObject("Tunnel");
            var segmentPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/TunnelSegment.prefab");
            float firstCenter = PlayerY + CameraDistance - SegmentLength / 2f;
            for (int i = 0; i < SegmentCount; i++)
            {
                var seg = (GameObject)PrefabUtility.InstantiatePrefab(segmentPrefab);
                seg.name = "TunnelSegment_" + i;
                seg.transform.SetParent(tunnel.transform);
                seg.transform.position = new Vector3(0f, firstCenter - i * SegmentLength, 0f);
            }

            // Gota: esfera de vidro no lugar do sprite.
            var player = GameObject.Find("Player");
            Object.DestroyImmediate(player.GetComponent<PaletteColor>());
            Object.DestroyImmediate(player.GetComponent<SpriteRenderer>());
            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.name = "Visual";
            visual.transform.SetParent(player.transform, false);
            visual.transform.localScale = new Vector3(0.8f, 0.95f, 0.8f); // levemente alongada no sentido da queda
            visual.AddComponent<MaterialSlotRenderer>();
            SetSlot(visual, MaterialSlot.Glass);
            // Núcleo de luz dentro do vidro: é o que faz a gota "ler" no túnel escuro.
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(core.GetComponent<Collider>());
            core.name = "Core";
            core.transform.SetParent(visual.transform, false);
            core.transform.localScale = Vector3.one * 0.45f;
            core.AddComponent<MaterialSlotRenderer>();
            SetSlot(core, MaterialSlot.Collectible);

            var effects = GameObject.Find("Game").GetComponent<EffectsManager>();
            var eso = new SerializedObject(effects);
            eso.FindProperty("playerSprite").objectReferenceValue = visual.GetComponent<MeshRenderer>();
            eso.ApplyModifiedPropertiesWithoutUndo();

            // Spawner aponta para os prefabs 3D.
            var spawner = GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>();
            var sso = new SerializedObject(spawner);
            sso.FindProperty("blockPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Obstacle_Block3D.prefab").GetComponent<Obstacle>();
            sso.FindProperty("spikePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Obstacle_Spike3D.prefab").GetComponent<Obstacle>();
            sso.FindProperty("moverPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Obstacle_Mover3D.prefab").GetComponent<Obstacle>();
            sso.FindProperty("collectiblePrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Collectible_Drop3D.prefab").GetComponent<Collectible>();
            sso.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Cena Tunnel montada e salva em " + ScenePath);
        }

        // ------------------------------------------------------------------ 5. biomas

        private const string PaletteFolder = "Assets/Data/Palettes";

        [MenuItem("Gota/Túnel/5. Biomas e portal")]
        public static void BuildBiomes()
        {
            EnsureFolder(PaletteFolder);
            var ferrugem = GetOrCreatePalette("Ferrugem",
                new Color(0.09f, 0.05f, 0.03f), new Color(0.30f, 0.17f, 0.10f),
                new Color(1.00f, 0.55f, 0.22f), new Color(1.00f, 0.92f, 0.84f));
            var neon = GetOrCreatePalette("Neon",
                new Color(0.02f, 0.07f, 0.11f), new Color(0.08f, 0.24f, 0.31f),
                new Color(0.37f, 0.89f, 0.94f), new Color(0.90f, 0.98f, 1.00f));

            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                Debug.LogWarning("Abra a cena Tunnel antes (ou rode o passo 4).");
                return;
            }

            // Portal: aro grosso, nasce escondido.
            var old = GameObject.Find("Portal");
            if (old != null) Object.DestroyImmediate(old);
            var portalGo = new GameObject("Portal");
            Ring(portalGo, "Ring", 0f);
            foreach (Transform bar in portalGo.transform.GetChild(0))
            {
                Vector3 sc = bar.localScale;
                sc.y = 0.5f; // bem mais grosso que um aro comum
                bar.localScale = sc;
            }
            portalGo.transform.position = new Vector3(0f, -14f, 0f);
            var portal = portalGo.AddComponent<PortalRing>();
            portalGo.SetActive(false);

            // Nome do bioma no centro da tela (some sozinho).
            var canvas = GameObject.Find("Canvas");
            var oldLabel = canvas.transform.Find("BiomeLabel");
            if (oldLabel != null) Object.DestroyImmediate(oldLabel.gameObject);
            var reference = canvas.GetComponentInChildren<UnityEngine.UI.Text>(true);
            var labelGo = new GameObject("BiomeLabel", typeof(RectTransform));
            labelGo.transform.SetParent(canvas.transform, false);
            var rect = labelGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.6f);
            rect.anchorMax = new Vector2(0.9f, 0.7f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var group = labelGo.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            var text = labelGo.AddComponent<UnityEngine.UI.Text>();
            text.font = reference != null ? reference.font : null;
            text.fontSize = 44;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.9f, 0.98f, 1f);
            text.text = "NEON";
            text.raycastTarget = false;

            // Gerente de biomas no objeto Game.
            var game = GameObject.Find("Game");
            var manager = game.GetComponent<BiomeManager>();
            if (manager == null) manager = game.AddComponent<BiomeManager>();
            var so = new SerializedObject(manager);
            var list = so.FindProperty("biomes");
            list.arraySize = 2;
            list.GetArrayElementAtIndex(0).objectReferenceValue = ferrugem;
            list.GetArrayElementAtIndex(1).objectReferenceValue = neon;
            so.FindProperty("portal").objectReferenceValue = portal;
            so.FindProperty("biomeLabel").objectReferenceValue = group;
            so.FindProperty("biomeLabelText").objectReferenceValue = text;
            so.FindProperty("score").objectReferenceValue = game.GetComponent<ScoreManager>();
            so.FindProperty("scroller").objectReferenceValue = game.GetComponent<WorldScroller>();
            so.FindProperty("player").objectReferenceValue = GameObject.Find("Player").GetComponent<PlayerController>();
            so.FindProperty("spawner").objectReferenceValue = GameObject.Find("ObstacleSpawner").GetComponent<ObstacleSpawner>();
            so.ApplyModifiedPropertiesWithoutUndo();

            var mats = GameObject.Find("TunnelMaterials").GetComponent<TunnelMaterials>();
            var mso = new SerializedObject(mats);
            mso.FindProperty("targetCamera").objectReferenceValue = Camera.main;
            mso.FindProperty("neonIntensity").floatValue = 1.4f;
            mso.FindProperty("concreteLift").floatValue = 0.3f;
            mso.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Biomas Ferrugem e Neon + portal prontos.");
        }

        private static GamePalette GetOrCreatePalette(string name, Color background, Color wall, Color accent, Color light)
        {
            string path = PaletteFolder + "/" + name + ".asset";
            var p = AssetDatabase.LoadAssetAtPath<GamePalette>(path);
            if (p == null)
            {
                p = ScriptableObject.CreateInstance<GamePalette>();
                AssetDatabase.CreateAsset(p, path);
            }
            p.background = background;
            p.wall = wall;
            p.accent = accent;
            p.light = light;
            EditorUtility.SetDirty(p);
            return p;
        }

        // ------------------------------------------------------------------ 6. abertura e idle

        [MenuItem("Gota/Túnel/6. Abertura e idle do menu")]
        public static void BuildIntro()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                Debug.LogWarning("Abra a cena Tunnel antes (ou rode o passo 4).");
                return;
            }

            var player = GameObject.Find("Player");
            var visual = player.transform.Find("Visual").gameObject;
            if (visual.GetComponent<MenuIdle>() == null) visual.AddComponent<MenuIdle>();

            // Cano enferrujado saindo do teto, em cima da posição inicial da gota.
            var old = GameObject.Find("IntroPipe");
            if (old != null) Object.DestroyImmediate(old);
            var pipe = new GameObject("IntroPipe");
            float ceilingZ = CenterZ + HalfWidth;
            float pipeLength = 1.4f;
            var tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(tube.GetComponent<Collider>());
            tube.name = "Tube";
            tube.transform.SetParent(pipe.transform, false);
            tube.transform.localRotation = Quaternion.Euler(90f, 0f, 0f); // cilindro ao longo de Z
            tube.transform.localScale = new Vector3(0.9f, pipeLength / 2f, 0.9f);
            tube.AddComponent<MaterialSlotRenderer>();
            SetSlot(tube, MaterialSlot.Concrete);
            // Anel de neon na boca do cano, para a abertura ter luz.
            var lip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(lip.GetComponent<Collider>());
            lip.name = "Lip";
            lip.transform.SetParent(pipe.transform, false);
            lip.transform.localPosition = new Vector3(0f, 0f, -pipeLength / 2f + 0.05f);
            lip.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            lip.transform.localScale = new Vector3(1.0f, 0.04f, 1.0f);
            lip.AddComponent<MaterialSlotRenderer>();
            SetSlot(lip, MaterialSlot.Neon);
            pipe.transform.position = new Vector3(-3f, PlayerY + 1.5f, ceilingZ - pipeLength / 2f);

            var game = GameObject.Find("Game");
            var intro = game.GetComponent<IntroSequence>();
            if (intro == null) intro = game.AddComponent<IntroSequence>();
            var so = new SerializedObject(intro);
            so.FindProperty("dropVisual").objectReferenceValue = visual.transform;
            so.FindProperty("canvas").objectReferenceValue = GameObject.Find("Canvas").GetComponent<Canvas>();
            so.FindProperty("splash").objectReferenceValue = GameObject.Find("Effects/SwitchBurst").GetComponent<ParticleSystem>();
            so.FindProperty("pipe").objectReferenceValue = pipe;
            float mouthZ = ceilingZ - pipeLength - 0.35f; // boca do cano menos o raio da gota
            so.FindProperty("spoutLocalPosition").vector3Value = new Vector3(0f, 1.5f, mouthZ);
            so.ApplyModifiedPropertiesWithoutUndo();

            // Túnel desliza devagar fora da partida: o conceito vira o menu.
            var scroller = game.GetComponent<WorldScroller>();
            var wso = new SerializedObject(scroller);
            wso.FindProperty("idleSpeed").floatValue = 1.2f;
            wso.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Abertura e idle do menu prontos.");
        }

        [MenuItem("Gota/Túnel/Tudo (1 a 6)")]
        public static void BuildAll()
        {
            CreateRenderer();
            CreateMaterials();
            CreatePrefabs();
            BuildScene();
            BuildBiomes();
            BuildIntro();
        }

        // ------------------------------------------------------------------ helpers

        private static void Slab(GameObject parent, string name, Vector3 localPos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.AddComponent<MaterialSlotRenderer>();
            SetSlot(go, MaterialSlot.Concrete);
        }

        private static void Ring(GameObject parent, string name, float localY)
        {
            var ring = new GameObject(name);
            ring.transform.SetParent(parent.transform, false);
            ring.transform.localPosition = new Vector3(0f, localY, 0f);
            float w = HalfWidth, depth = 0.12f, thick = 0.07f;
            Bar(ring, "Top", new Vector3(0f, 0f, CenterZ + w - depth / 2f), new Vector3(w * 2f, thick, depth));
            Bar(ring, "Bottom", new Vector3(0f, 0f, CenterZ - w + depth / 2f), new Vector3(w * 2f, thick, depth));
            Bar(ring, "Left", new Vector3(-w + depth / 2f, 0f, CenterZ), new Vector3(depth, thick, w * 2f));
            Bar(ring, "Right", new Vector3(w - depth / 2f, 0f, CenterZ), new Vector3(depth, thick, w * 2f));
        }

        private static void Bar(GameObject parent, string name, Vector3 localPos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.AddComponent<MaterialSlotRenderer>();
            SetSlot(go, MaterialSlot.Neon);
        }

        private static void SetSlot(GameObject go, MaterialSlot slot)
        {
            var msr = go.GetComponent<MaterialSlotRenderer>();
            var so = new SerializedObject(msr);
            so.FindProperty("slot").enumValueIndex = (int)slot;
            so.ApplyModifiedPropertiesWithoutUndo();
            // No editor, mostra o material do asset para a cena não ficar rosa.
            var mats = AssetDatabase.LoadAssetAtPath<Material>(MaterialsFolder + "/" + slot + ".mat");
            if (mats != null) go.GetComponent<Renderer>().sharedMaterial = mats;
        }

        private static GameObject Convert3D(string sourcePrefab, PrimitiveType primitive, Mesh meshOverride, Vector3 visualScale, MaterialSlot slot)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefab);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            go.name = source.name + "3D";
            Object.DestroyImmediate(go.GetComponent<PaletteColor>());
            Object.DestroyImmediate(go.GetComponent<SpriteRenderer>());

            var visual = GameObject.CreatePrimitive(primitive);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.name = "Visual";
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = visualScale;
            if (meshOverride != null) visual.GetComponent<MeshFilter>().sharedMesh = meshOverride;
            visual.AddComponent<MaterialSlotRenderer>();
            SetSlot(visual, slot);

            // Sai de cena só depois de passar pela câmera.
            var scrollable = go.GetComponent<Scrollable>();
            var so = new SerializedObject(scrollable);
            so.FindProperty("despawnY").floatValue = PlayerY + CameraDistance + 1f;
            so.ApplyModifiedPropertiesWithoutUndo();
            return go;
        }

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static Material GetOrCreateMaterial(string path, Shader shader)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            else
            {
                m.shader = shader;
            }
            return m;
        }

        private static void MakeTransparent(Material m)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.DisableKeyword("_ALPHATEST_ON");
            m.renderQueue = (int)RenderQueue.Transparent;
        }

        private static T GetOrAddOverride<T>(VolumeProfile profile) where T : VolumeComponent
        {
            T c;
            if (!profile.TryGet(out c)) c = profile.Add<T>(true);
            return c;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>Concreto: ruído fractal + manchas verticais (escorrido), tons de cinza.</summary>
        private static void WriteConcreteTexture(string path, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGB24, false);
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size, v = (float)y / size;
                    float n = 0f, amp = 0.5f, freq = 4f;
                    for (int o = 0; o < 5; o++)
                    {
                        n += amp * Mathf.PerlinNoise(u * freq + 7.1f, v * freq + 3.3f);
                        amp *= 0.5f;
                        freq *= 2f;
                    }
                    float streak = Mathf.PerlinNoise(u * 40f, v * 1.5f) * 0.12f; // escorrido vertical
                    float grain = (Mathf.PerlinNoise(u * 200f, v * 200f) - 0.5f) * 0.08f;
                    float g = Mathf.Clamp01(0.55f + (n - 0.5f) * 0.6f - streak + grain);
                    pixels[y * size + x] = new Color(g, g, g);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
        }

        /// <summary>Pirâmide de base quadrada (1x1 em XZ, y de -0,5 a +0,5), ponta em +Y.</summary>
        private static Mesh BuildPyramid()
        {
            var mesh = new Mesh { name = "Spike" };
            Vector3 apex = new Vector3(0f, 0.5f, 0f);
            Vector3 a = new Vector3(-0.5f, -0.5f, -0.5f);
            Vector3 b = new Vector3(0.5f, -0.5f, -0.5f);
            Vector3 c = new Vector3(0.5f, -0.5f, 0.5f);
            Vector3 d = new Vector3(-0.5f, -0.5f, 0.5f);
            var verts = new System.Collections.Generic.List<Vector3>();
            var tris = new System.Collections.Generic.List<int>();
            void Tri(Vector3 p0, Vector3 p1, Vector3 p2)
            {
                int i = verts.Count;
                verts.Add(p0); verts.Add(p1); verts.Add(p2);
                tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
            }
            Tri(a, apex, b);
            Tri(b, apex, c);
            Tri(c, apex, d);
            Tri(d, apex, a);
            Tri(a, b, c);
            Tri(a, c, d);
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
