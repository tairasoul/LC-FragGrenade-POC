using BepInEx;
using BepInEx.Logging;
using LC_API.ServerAPI;
using LC_API.BundleAPI;
using LethalLib.Modules;
using Unity.Netcode;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
            ModdedServer.SetServerModdedOnly();
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
                init = true;
                RegisterFragGrenade();
                BundleLoader.OnLoadedBundles += RealInit;
            }
        }


        private void RegisterFragGrenade()
        {
            try
            {
                Log.LogInfo("Getting Origin GameObject.");
                GameObject StunOrigin = Resources.InstanceIDToObject(13958) as GameObject;
                Log.LogInfo("Finding StunGrenadeItem");
                StunGrenadeItem Origin = StunOrigin.GetComponent<StunGrenadeItem>();
                Item stunGrenade = Origin.itemProperties;
                Item grenade = new Item
                {
                    canBeGrabbedBeforeGameStart = false,
                    itemId = Items.shopItems.Count + 1,
                    itemName = "Grenade",
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
                    weight = 0.875f
                };
                grenade.name = "GrenadeProps";
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
                grenadeI.TimeToExplode = Origin.TimeToExplode;
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
            //GrenadeMesh = BundleLoader.GetLoadedAsset<Mesh>("assets/GrenadeMesh");
        }
    }
}
