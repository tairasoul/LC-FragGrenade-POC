using BepInEx;
using BepInEx.Logging;
using LC_API.ServerAPI;
using LC_API.BundleAPI;
using LethalLib.Modules;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace FragGrenade.LCMod
{
    [BepInPlugin("tairasoul.frag.grenade", "FragGrenade", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log;
        //public static Mesh GrenadeMesh;
        public static bool init = false;
        internal void Awake()
        {
            Log = Logger;
            Log.LogInfo("Initializing FragGrenade.");
            Log.LogInfo("Waiting for Init.");
        }

        internal void Start()
        {
            Init();
        }

        internal void OnDestroy()
        {
            Init();
        }

        internal void Init()
        {
            if (!init)
            {
                Log.LogInfo("Starting init.");
                SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
                init = true;
                BundleLoader.OnLoadedBundles += RealInit;
            }
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            if (arg1.name == "MainMenu")
            {
                Log.LogInfo("MainMenu loaded.");
                RegisterFragGrenade();
                SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
            }
        }

        private void RegisterFragGrenade()
        {
            try
            {
                Log.LogInfo("Getting Origin GameObject.");
                List<GameObject> ResList = new List<GameObject>();
                foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    GameObject go = obj as GameObject;
                    if (go.transform.parent == null && !go.scene.IsValid())
                    {
                        ResList.Add(go);
                    }
                }
                GameObject[] Res = ResList.ToArray();
                GameObject StunOrigin = null;
                foreach (GameObject obj in Res)
                {
                    if (obj.name == "StunGrenade") StunOrigin = obj;
                }
                Log.LogInfo("Finding StunGrenadeItem");
                StunGrenadeItem Origin = StunOrigin.GetComponent<StunGrenadeItem>();
                Item stunGrenade = Origin.itemProperties;
                Item grenade = new Item
                {
                    canBeGrabbedBeforeGameStart = false,
                    itemId = 4,
                    itemName = "Frag grenade",
                    itemSpawnsOnGround = false,
                    requiresBattery = false,
                    dropSFX = stunGrenade.dropSFX,
                    grabSFX = stunGrenade.grabSFX,
                    pocketSFX = stunGrenade.pocketSFX,
                    throwSFX = stunGrenade.throwSFX,
                    syncUseFunction = stunGrenade.syncUseFunction,
                    syncGrabFunction = stunGrenade.syncGrabFunction,
                    syncDiscardFunction = stunGrenade.syncDiscardFunction,
                    syncInteractLRFunction = stunGrenade.syncInteractLRFunction,
                    holdButtonUse = true,
                    allowDroppingAheadOfPlayer = true,
                    highestSalePercentage = 90,
                    weight = 0.875f,
                    name = "GrenadeProps"
                };
                Log.LogInfo("Creating prefab.");
                grenade.spawnPrefab = Instantiate(StunOrigin);
                grenade.spawnPrefab.name = "FragGrenade";
                grenade.spawnPrefab.transform.position = new Vector3(2000, 2000, 2000);
                DontDestroyOnLoad(grenade.spawnPrefab);
                Log.LogInfo($"SpawnPrefab {grenade.spawnPrefab}");
                Log.LogInfo("Add component");
                GrenadeItem grenadeI = grenade.spawnPrefab.AddComponent<GrenadeItem>();
                Log.LogInfo("Set fields");
                grenadeI.itemProperties = grenade;
                grenadeI.grabbable = true;
                grenadeI.grabbableToEnemies = true;
                grenadeI.explodeSFX = Origin.explodeSFX;
                grenadeI.explodeTimer = Origin.explodeTimer;
                grenadeI.grenadeFallCurve = Origin.grenadeFallCurve;
                grenadeI.grenadeVerticalFallCurve = Origin.grenadeVerticalFallCurve;
                grenadeI.grenadeVerticalFallCurveNoBounce = Origin.grenadeVerticalFallCurveNoBounce;
                grenadeI.stunGrenadeExplosion = Origin.stunGrenadeExplosion;
                grenadeI.grenadeHit = Origin.grenadeHit;
                grenadeI.grenadeThrowRay = Origin.grenadeThrowRay;
                grenadeI.itemAnimator = Origin.itemAnimator;
                grenadeI.itemAudio = Origin.itemAudio;
                grenadeI.pullPinSFX = Origin.pullPinSFX;
                grenadeI.customGrabTooltip = "";
                grenadeI.hasHitGround = false;
                grenadeI.insertedBattery = Origin.insertedBattery;
                grenadeI.reachedFloorTarget = false;
                grenadeI.targetFloorPosition = Vector3.zero;
                Destroy(grenade.spawnPrefab.GetComponent<StunGrenadeItem>());
                NetworkPrefabs.RegisterNetworkPrefab(grenade.spawnPrefab);
                Items.RegisterShopItem(grenade, 30);
                Log.LogInfo("Registered and initialized FragGrenade.");
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
        }

        internal void RealInit()
        {
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                if (scene.name == "MainMenu")
                {
                    Log.LogInfo("Registering frag grenade.");
                    RegisterFragGrenade();
                }
            };
        }
    }
}
