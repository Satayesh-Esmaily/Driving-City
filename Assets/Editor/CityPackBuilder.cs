// City Pack - Stage 1 builder (2D top-down).
// Menu: City Pack > Build Stage 1 (Everything)
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CityPack.EditorTools
{
    // Every PNG inside Assets/CityPack/Sprites is imported automatically as a Sprite (64 pixels per unit).
    public class CityPackSpriteImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/CityPack/Sprites/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 64;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Bilinear;
            ti.wrapMode = TextureWrapMode.Clamp;
            ti.maxTextureSize = 4096;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            var s = new TextureImporterSettings();
            ti.ReadTextureSettings(s);
            s.spriteAlignment = (int)SpriteAlignment.Center;
            s.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(s);
        }
    }

    [System.Serializable] public class LayoutItem { public string name, sprite, group; public float x, y, rot; public int order; }
    [System.Serializable] public class LayoutData { public int cols, rows; public float tile; public LayoutItem[] items; }

    public static class CityPackBuilder
    {
        const string Root = "Assets/CityPack";
        const string LayoutPath = Root + "/Layout/Stage1_Layout.json";
        const string ScenePath = Root + "/Scenes/Stage1_City.unity";

        [MenuItem("City Pack/Build Stage 1 (Everything)")]
        public static void BuildAll()
        {
            ReimportSprites();
            CreatePrefabs();
            BuildScene();
        }

        [MenuItem("City Pack/1 - Reimport Sprites")]
        public static void ReimportSprites()
        {
            AssetDatabase.Refresh();
            foreach (var g in AssetDatabase.FindAssets("t:Texture2D", new[] { Root + "/Sprites" }))
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(g), ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("City Pack/2 - Create Prefabs")]
        public static void CreatePrefabs()
        {
            int count = 0;
            EnsureSortingLayers();
            foreach (var g in AssetDatabase.FindAssets("t:Sprite", new[] { Root + "/Sprites" }))
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                if (sprite == null) continue;
                string rel = p.Substring((Root + "/Sprites/").Length);
                rel = rel.Substring(0, rel.LastIndexOf('.'));
                string prefabPath = Root + "/Prefabs/" + rel + ".prefab";
                Directory.CreateDirectory(Path.GetDirectoryName(prefabPath));

                var go = new GameObject(Path.GetFileName(rel));
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = DefaultOrder(rel);
                sr.sortingLayerName = LayerFor(rel);
                if (rel.StartsWith("Buildings/")) go.AddComponent<BoxCollider2D>();
                PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                Object.DestroyImmediate(go);
                count++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CityPack] Prefabs created: " + count);
        }

        [MenuItem("City Pack/3 - Build Stage 1 Scene")]
        public static void BuildScene()
        {
            if (!File.Exists(LayoutPath)) { Debug.LogError("[CityPack] Layout file not found: " + LayoutPath); return; }
            var data = JsonUtility.FromJson<LayoutData>(File.ReadAllText(LayoutPath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            EnsureSortingLayers();
            var root = new GameObject("DrivingCity").transform;
            var decorations = new GameObject("Decorations").transform;
            decorations.SetParent(root);
            var groups = new Dictionary<string, Transform>();
            var counters = new Dictionary<string, int>();
            foreach (var it in data.items)
            {
                if (!groups.TryGetValue(it.group, out var parent))
                {
                    parent = new GameObject(it.group).transform;
                    bool isDecor = it.group == "Nature" || it.group == "Props" || it.group == "Entrance";
                    parent.SetParent(isDecor ? decorations : root);
                    groups[it.group] = parent;
                }
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/" + it.sprite + ".prefab");
                GameObject go;
                if (prefab != null) go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                else
                {
                    go = new GameObject();
                    go.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/" + it.sprite + ".png");
                }
                counters.TryGetValue(it.name, out int n);
                counters[it.name] = ++n;
                go.name = it.name + "_" + n.ToString("00");
                go.transform.SetParent(parent);
                go.transform.position = new Vector3(it.x, it.y, 0f);
                go.transform.rotation = Quaternion.Euler(0, 0, it.rot);
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null) { sr.sortingOrder = it.order; sr.sortingLayerName = LayerFor(it.sprite); }
            }

            // ---- Markers (visual only): start / pickup / destination
            var markers = new GameObject("Markers").transform;
            markers.SetParent(root);
            Vector3 startPos = new Vector3(38.625f, 10f, 0f);
            MakeMarker("StartPoint", startPos, new Color(0.3f, 1f, 0.4f), markers);
            MakeMarker("PickupMarker", new Vector3(46f, 14f, 0f), new Color(1f, 0.85f, 0.2f), markers);
            MakeMarker("DestinationMarker", new Vector3(66f, 22f, 0f), new Color(1f, 0.3f, 0.3f), markers);

            // ---- Player car
            var carPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/Vehicles/Cars/car_red_sedan.prefab");
            GameObject car = carPrefab != null ? (GameObject)PrefabUtility.InstantiatePrefab(carPrefab) : new GameObject();
            if (carPrefab == null) car.AddComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/Vehicles/Cars/car_red_sedan.png");
            car.name = "PlayerCar";
            car.transform.SetParent(root);
            car.transform.position = startPos;
            car.transform.rotation = Quaternion.identity;
            var csr = car.GetComponent<SpriteRenderer>();
            csr.sortingLayerName = "Vehicles";
            csr.sortingOrder = 30;
            car.AddComponent<DrivingCarController>();

            // Top-down orthographic camera
            float w = data.cols * data.tile, h = data.rows * data.tile;
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = h * 0.5f + 2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.40f, 0.65f, 0.36f);
            camGo.transform.position = new Vector3(w * 0.5f, h * 0.5f, -10f);

            Directory.CreateDirectory(Root + "/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[CityPack] Stage 1 scene saved: " + ScenePath);
        }


        static void MakeMarker(string name, Vector3 pos, Color c, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/FX/fx_light_glow.png");
            sr.color = new Color(c.r, c.g, c.b, 0.9f);
            sr.sortingLayerName = "Markers";
            sr.sortingOrder = 1;
            var ring = new GameObject("Ring");
            ring.transform.SetParent(go.transform, false);
            ring.transform.localScale = Vector3.one * 0.8f;
            var rsr = ring.AddComponent<SpriteRenderer>();
            rsr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Sprites/FX/fx_ripple_ring.png");
            rsr.color = c;
            rsr.sortingLayerName = "Markers";
            rsr.sortingOrder = 2;
        }

        static readonly string[] SortingLayerNames = { "Ground", "Roads", "Markers", "Buildings", "Props", "Vehicles", "Characters", "Overhead" };

        // Creates the Sorting Layers (in this draw order) if they do not exist yet.
        static void EnsureSortingLayers()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets == null || assets.Length == 0) return;
            var tm = new SerializedObject(assets[0]);
            var layers = tm.FindProperty("m_SortingLayers");
            foreach (var name in SortingLayerNames)
            {
                bool found = false;
                for (int i = 0; i < layers.arraySize; i++)
                    if (layers.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == name) { found = true; break; }
                if (found) continue;
                layers.InsertArrayElementAtIndex(layers.arraySize);
                var e = layers.GetArrayElementAtIndex(layers.arraySize - 1);
                e.FindPropertyRelative("name").stringValue = name;
                e.FindPropertyRelative("uniqueID").intValue = (name.GetHashCode() & 0x7fffffff) % 1000000000 + 1000;
                e.FindPropertyRelative("locked").intValue = 0;
            }
            tm.ApplyModifiedProperties();
        }

        static string LayerFor(string rel)
        {
            string top = rel.Split('/')[0];
            switch (top)
            {
                case "Ground": return "Ground";
                case "Roads": return "Roads";
                case "Buildings": return "Buildings";
                case "Props": return "Props";
                case "Vehicles": return "Vehicles";
                case "Characters": return "Characters";
                case "Entrance": return "Overhead";
                case "Nature": return rel.Contains("/Trees/") ? "Overhead" : "Props";
                case "FX": return "Overhead";
                default: return "Props";
            }
        }

        // Default sorting order for prefabs, based on the top-level sprite folder.
        static int DefaultOrder(string rel)
        {
            string top = rel.Split('/')[0];
            switch (top)
            {
                case "Ground": return 0;
                case "Roads": return 1;
                case "Buildings": return 10;
                case "Props": return 12;
                case "Vehicles": return 20;
                case "Characters": return 22;
                case "Nature": return rel.Contains("/Trees/") ? 30 : 8;
                case "Entrance": return 40;
                case "FX": return 50;
                default: return 10;
            }
        }
    }
}
#endif
