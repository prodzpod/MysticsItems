using RoR2;
using RoR2.Hologram;
using RoR2.Audio;
using R2API;
using R2API.Utils;
using UnityEngine;
using UnityEngine.Networking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Networking;
using R2API.Networking.Interfaces;
using System.Collections.ObjectModel;
using System.Text;
using TMPro;
using System.Collections.Generic;
using ThreeEyedGames;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using static MysticsItems.LegacyBalanceConfigManager;
using UnityEngine.AddressableAssets;

namespace MysticsItems.Items
{
    public class TreasureMap : BaseItem
    {
        public static GameObject zonePrefab;
        public static SpawnCard zoneSpawnCard;
        public static Material ghostMaterial;
        public static NetworkSoundEventDef soundEventDef;
        public static GameObject effectPrefab;

        public static ConfigurableValue<float> unearthTime = new ConfigurableValue<float>(
            "Item: Treasure Map",
            "UnearthTime",
            120f,
            "How long to stay in the treasure zone to unearth the legendary item (in seconds)",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_TREASUREMAP_DESC"
            }
        );
        public static ConfigurableValue<float> reductionPerStack = new ConfigurableValue<float>(
            "Item: Treasure Map",
            "ReductionPerStack",
            25f,
            "Unearth time reduction for each additional stack of this item (in %, hyperbolic)",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_TREASUREMAP_DESC"
            }
        );
        public static ConfigurableValue<float> radius = new ConfigurableValue<float>(
            "Item: Treasure Map",
            "Radius",
            7f,
            "Treasure zone radius (in meters)",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_TREASUREMAP_DESC"
            }
        );
        public static ConfigOptions.ConfigurableValue<bool> dropItemForEachPlayer = ConfigOptions.ConfigurableValue.CreateBool(
            ConfigManager.General.categoryGUID,
            ConfigManager.General.categoryName,
            ConfigManager.General.config,
            "Gameplay",
            "Treasure Map Per-Player Drops",
            true,
            "Should the Treasure Map treasure spot drop an item for each player in multiplayer? If false, only one item is dropped regardless of the player count."
        );
        public static ConfigOptions.ConfigurableValue<bool> seaBearCircleEnabled = ConfigOptions.ConfigurableValue.CreateBool(
            ConfigManager.General.categoryGUID,
            ConfigManager.General.categoryName,
            ConfigManager.General.config,
            "Gameplay",
            "Treasure Map Sea Bear Circle",
            false,
            "Should the Treasure Map spot appear as a 1m-wide circle when nobody has the item?",
            onChanged: MysticsItemsTreasureMapZone.ToggleSeaBearCircle
        );

        public override void OnPluginAwake()
        {
            zonePrefab = MysticsRisky2Utils.Utils.CreateBlankPrefab("MysticsItems_TreasureMapZone", true);

            NetworkingAPI.RegisterMessageType<MysticsItemsTreasureMapZone.SyncZoneShouldBeActive>();
            NetworkingAPI.RegisterMessageType<MysticsItemsTreasureMapZone.RequestZoneShouldBeActive>();
        }

        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "MysticsItems_TreasureMap";
            SetItemTierWhenAvailable(ItemTier.Tier3);
            itemDef.tags = new ItemTag[]
            {
                ItemTag.Utility,
                ItemTag.AIBlacklist,
                ItemTag.CannotCopy
            };
            itemDef.pickupModelPrefab = PrepareModel(Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Treasure Map/Model.prefab"));
            itemDef.pickupIconSprite = Main.AssetBundle.LoadAsset<Sprite>("Assets/Items/Treasure Map/Icon.png");
            MysticsItemsContent.Resources.unlockableDefs.Add(GetUnlockableDef());
            ModelPanelParameters modelPanelParams = itemDef.pickupModelPrefab.GetComponentInChildren<ModelPanelParameters>();
            modelPanelParams.minDistance = 3;
            modelPanelParams.maxDistance = 6;
            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(itemDef.pickupModelPrefab, itemDef.pickupModelPrefab.name + "Display", false));
            onSetupIDRS += () =>
            {
                AddDisplayRule("CommandoBody", "LowerArmR", new Vector3(-0.084F, 0.183F, -0.006F), new Vector3(83.186F, 36.557F, 131.348F), new Vector3(0.053F, 0.053F, 0.053F));
                AddDisplayRule("HuntressBody", "Muzzle", new Vector3(-0.527F, -0.032F, -0.396F), new Vector3(0.509F, 134.442F, 184.268F), new Vector3(0.042F, 0.042F, 0.042F));
                AddDisplayRule("Bandit2Body", "MuzzleShotgun", new Vector3(0.014F, -0.07F, -0.668F), new Vector3(0F, 180F, 180F), new Vector3(0.04F, 0.04F, 0.04F));
                AddDisplayRule("ToolbotBody", "Head", new Vector3(0.198F, 3.655F, -0.532F), new Vector3(304.724F, 180F, 180F), new Vector3(0.448F, 0.448F, 0.448F));
                AddDisplayRule("EngiBody", "WristDisplay", new Vector3(0.01F, -0.001F, 0.007F), new Vector3(86.234F, 155.949F, 155.218F), new Vector3(0.065F, 0.065F, 0.065F));
                AddDisplayRule("MageBody", "LowerArmR", new Vector3(0.116F, 0.188F, 0.008F), new Vector3(88.872F, 20.576F, 290.58F), new Vector3(0.074F, 0.074F, 0.074F));
                AddDisplayRule("MercBody", "LowerArmR", new Vector3(-0.01F, 0.144F, -0.116F), new Vector3(277.017F, 64.808F, 295.358F), new Vector3(0.072F, 0.072F, 0.072F));
                AddDisplayRule("TreebotBody", "HeadBase", new Vector3(-0.013F, 0.253F, -0.813F), new Vector3(1.857F, 5.075F, 0.053F), new Vector3(0.13F, 0.143F, 0.294F));
                AddDisplayRule("LoaderBody", "MechLowerArmR", new Vector3(-0.01F, 0.544F, -0.144F), new Vector3(275.35F, 95.995F, 266.284F), new Vector3(0.095F, 0.095F, 0.095F));
                AddDisplayRule("CrocoBody", "UpperArmR", new Vector3(1.735F, -0.575F, 0.196F), new Vector3(281.472F, 180.072F, 89.927F), new Vector3(0.868F, 0.868F, 0.868F));
                AddDisplayRule("CaptainBody", "HandR", new Vector3(-0.066F, 0.087F, 0.011F), new Vector3(76.759F, 135.292F, 224.52F), new Vector3(0.059F, 0.053F, 0.059F));
                AddDisplayRule("BrotherBody", "HandR", BrotherInfection.red, new Vector3(0.051F, -0.072F, 0.004F), new Vector3(44.814F, 122.901F, 267.545F), new Vector3(0.063F, 0.063F, 0.063F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysSniper) AddDisplayRule("SniperClassicBody", "Chest", new Vector3(-0.10307F, 0.44329F, -0.26101F), new Vector3(0F, 0F, 0F), new Vector3(0.05402F, 0.05402F, 0.05402F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysDeputy) AddDisplayRule("DeputyBody", "RevolverR", new Vector3(0F, 0.20444F, -0.04491F), new Vector3(90F, 180F, 0F), new Vector3(0.01997F, 0.01997F, 0.01997F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysChirr) AddDisplayRule("ChirrBody", "LowerArmR", new Vector3(-0.3581F, 0.03551F, -0.00592F), new Vector3(272.4088F, 276.7134F, 180F), new Vector3(0.16869F, 0.16869F, 0.16869F));
                AddDisplayRule("RailgunnerBody", "GunRoot", new Vector3(0.00726F, -0.15201F, 0.07672F), new Vector3(270F, 180F, 0F), new Vector3(0.05025F, 0.05025F, 0.05025F));
                AddDisplayRule("VoidSurvivorBody", "ForeArmL", new Vector3(0.06352F, 0.24419F, 0.00664F), new Vector3(69.26795F, 34.44471F, 302.5876F), new Vector3(0.05926F, 0.05926F, 0.05926F));
            };
            
            MysticsRisky2Utils.Utils.CopyChildren(Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Treasure Map/TreasureMapZone.prefab"), zonePrefab);
            HoldoutZoneController holdoutZone = zonePrefab.AddComponent<HoldoutZoneController>();
            holdoutZone.baseRadius = radius;
            holdoutZone.baseChargeDuration = unearthTime;
            holdoutZone.radiusSmoothTime = 1f;
            holdoutZone.radiusIndicator = zonePrefab.transform.Find("Visuals/Sphere").gameObject.GetComponent<Renderer>();
            holdoutZone.minimumRadius = 2.25f;
            holdoutZone.inBoundsObjectiveToken = "OBJECTIVE_MYSTICSITEMS_CHARGE_TREASUREMAPZONE";
            holdoutZone.outOfBoundsObjectiveToken = "OBJECTIVE_MYSTICSITEMS_CHARGE_TREASUREMAPZONE_OOB";
            holdoutZone.applyHealingNova = true;
            holdoutZone.applyFocusConvergence = true;
            holdoutZone.playerCountScaling = 0f; // Charge by 1 second regardless of how many players are charging the zone
            holdoutZone.dischargeRate = 0f;
            holdoutZone.enabled = false;
            MysticsItemsTreasureMapZone captureZone = zonePrefab.AddComponent<MysticsItemsTreasureMapZone>();
            captureZone.itemDef = itemDef;
            captureZone.dropTable = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/Base/GoldChest/dtGoldChest.asset").WaitForCompletion();
            captureZone.dropTransform = zonePrefab.transform.Find("DropPivot");
            HologramProjector hologramProjector = zonePrefab.AddComponent<HologramProjector>();
            hologramProjector.displayDistance = holdoutZone.baseRadius;
            hologramProjector.hologramPivot = zonePrefab.transform.Find("HologramPivot");
            hologramProjector.hologramPivot.transform.localScale *= 2f;
            hologramProjector.disableHologramRotation = false;
            captureZone.hologramProjector = hologramProjector;
            Decal decal = zonePrefab.transform.Find("Decal").gameObject.AddComponent<Decal>();
            decal.RenderMode = Decal.DecalRenderMode.Deferred;
            Material decalMaterial = new Material(LegacyShaderAPI.Find("Decalicious/Deferred Decal"));
            decal.Material = decalMaterial;
            decalMaterial.name = "MysticsItems_TreasureMapDecal";
            Texture decalTexture = Main.AssetBundle.LoadAsset<Texture>("Assets/Items/Treasure Map/texTreasureMapDecal.png");
            decalMaterial.SetTexture("_MainTex", decalTexture);
            decalMaterial.SetTexture("_MaskTex", decalTexture);
            decalMaterial.SetFloat("_AngleLimit", 0.6f);
            decalMaterial.SetFloat("_DecalLayer", 1f);
            decalMaterial.SetFloat("_DecalBlendMode", 0f);
            decalMaterial.SetColor("_Color", new Color32(70, 10, 10, 255));
            decalMaterial.SetColor("_EmissionColor", Color.black);
            decal.Fade = 1f;
            decal.DrawAlbedo = true;
            decal.UseLightProbes = true;
            decal.DrawNormalAndGloss = false;
            decal.HighQualityBlending = false;
            {
                decal.GetComponent<MeshFilter>().sharedMesh = LegacyResourcesAPI.Load<Mesh>("DecalCube");
                MeshRenderer component = decal.GetComponent<MeshRenderer>();
                component.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                component.receiveShadows = false;
                component.materials = new Material[0];
                component.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.BlendProbes;
                component.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            }
            decal.gameObject.transform.localScale = Vector3.one * 10f;
            HG.ArrayUtils.ArrayAppend(ref captureZone.toggleObjects, decal.gameObject);

            On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.ShouldBeFlashing += (orig, self) =>
            {
                if (self.sourceDescriptor.master)
                {
                    HoldoutZoneController holdoutZoneController = (HoldoutZoneController)self.sourceDescriptor.source;
                    if (holdoutZoneController && holdoutZoneController.gameObject.GetComponent<MysticsItemsTreasureMapZone>())
                    {
                        var teleporterInteraction = TeleporterInteraction.instance;
                        if (teleporterInteraction && teleporterInteraction.isCharged) return true;
                        return false;
                    }
                }
                return orig(self);
            };
            
            zoneSpawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            zoneSpawnCard.name = "iscMysticsItems_TreasureMapZone";
            zoneSpawnCard.prefab = zonePrefab;
            zoneSpawnCard.directorCreditCost = 0;
            zoneSpawnCard.sendOverNetwork = true;
            zoneSpawnCard.hullSize = HullClassification.Human;
            zoneSpawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            zoneSpawnCard.requiredFlags = RoR2.Navigation.NodeFlags.None;
            zoneSpawnCard.forbiddenFlags = RoR2.Navigation.NodeFlags.None;
            zoneSpawnCard.occupyPosition = false;

            GenericGameEvents.OnPopulateScene += (rng) =>
            {
                DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(zoneSpawnCard, new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                }, rng));
            };

            ghostMaterial = LegacyResourcesAPI.Load<Material>("Materials/matGhostEffect");

            soundEventDef = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            soundEventDef.eventName = "MysticsItems_Play_env_treasuremap";
            MysticsItemsContent.Resources.networkSoundEventDefs.Add(soundEventDef);

            effectPrefab = Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Treasure Map/UnearthEffect.prefab");
            EffectComponent effectComponent = effectPrefab.AddComponent<EffectComponent>();
            VFXAttributes vfxAttributes = effectPrefab.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Medium;
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
            effectPrefab.AddComponent<DestroyOnTimer>().duration = 1f;
            ShakeEmitter shakeEmitter = effectPrefab.AddComponent<ShakeEmitter>();
            shakeEmitter.shakeOnStart = true;
            shakeEmitter.shakeOnEnable = false;
            shakeEmitter.duration = 0.3f;
            shakeEmitter.radius = 25f;
            shakeEmitter.scaleShakeRadiusWithLocalScale = false;
            shakeEmitter.wave = new Wave
            {
                amplitude = 3f,
                frequency = 200f
            };
            shakeEmitter.amplitudeTimeDecay = true;
            MysticsItemsContent.Resources.effectPrefabs.Add(effectPrefab);

            TeleporterInteraction.onTeleporterChargedGlobal += TeleporterInteraction_onTeleporterChargedGlobal;
        }

        private void TeleporterInteraction_onTeleporterChargedGlobal(TeleporterInteraction teleporterInteraction)
        {
            if (teleporterInteraction && teleporterInteraction.enabled && MysticsItemsTreasureMapZone.instance && MysticsItemsTreasureMapZone.instance.ShouldBeActive && NetworkServer.active)
            {
                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "MYSTICSITEMS_TREASUREMAP_WARNING"
                });
            }
        }

        public static float CalculateChargeTime(int itemCount)
        {
            return unearthTime * (1f - Util.ConvertAmplificationPercentageIntoReductionPercentage(reductionPerStack * (itemCount - 1)) / 100f);
        }

        public class MysticsItemsTreasureMapZone : MonoBehaviour, IHologramContentProvider
        {
            public TeamIndex teamIndex = TeamIndex.Player;
            public ItemDef itemDef;
            public HologramProjector hologramProjector;
            public HoldoutZoneController holdoutZoneController;
            public GameObject[] toggleObjects = new GameObject[] { };

            public Xoroshiro128Plus rng;
            public PickupDropTable dropTable;
            public PickupIndex dropPickup = PickupIndex.none;
            public Transform dropTransform;
            public float dropUpVelocityStrength = 20f;
            public float dropForwardVelocityStrength = 2f;

            public static MysticsItemsTreasureMapZone instance;

            public void Start()
            {
                holdoutZoneController = GetComponent<HoldoutZoneController>();

                if (NetworkServer.active)
                {
                    ShouldBeActive = false;
                    rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
                    if (dropTable) dropPickup = dropTable.GenerateDrop(rng);
                }
                else
                {
                    new RequestZoneShouldBeActive(gameObject.GetComponent<NetworkIdentity>().netId).Send(NetworkDestination.Server);
                }

                holdoutZoneController.onCharged = new HoldoutZoneController.HoldoutZoneControllerChargedUnityEvent();
                holdoutZoneController.onCharged.AddListener(zone => Unearth());

                instance = this;

                ToggleSeaBearCircle(seaBearCircleEnabled);
            }

            public void Unearth()
            {
                if (NetworkServer.active)
                {
                    EffectManager.SimpleEffect(effectPrefab, transform.position, Quaternion.identity, true);
                    PointSoundManager.EmitSoundServer(soundEventDef.index, transform.position);

                    var dropCount = 1;
                    if (dropItemForEachPlayer) dropCount = Mathf.Max(Run.instance.participatingPlayerCount, 1);
                    float angle = 360f / (float)dropCount;
                    Vector3 vector = Vector3.up * dropUpVelocityStrength + dropTransform.forward * dropForwardVelocityStrength;
                    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                    for (int i = 0; i < dropCount; i++)
                    {
                        PickupDropletController.CreatePickupDroplet(dropPickup, dropTransform.position + Vector3.up * 1.5f, vector);
                        vector = rotation * vector;
                    }
                }

                if (holdoutZoneController && holdoutZoneController.radiusIndicator) holdoutZoneController.radiusIndicator.transform.localScale = Vector3.zero;

                Object.Destroy(gameObject);
            }

            public static void ToggleSeaBearCircle(bool setEnabled)
            {
                if (instance)
                {
                    if (instance.holdoutZoneController && instance.holdoutZoneController.radiusIndicator)
                    {
                        if (!instance.shouldBeActive)
                            instance.holdoutZoneController.radiusIndicator.transform.localScale = setEnabled ? Vector3.one : Vector3.zero;
                    }
                }
            }

            public void FixedUpdate()
            {
                int itemCount = Util.GetItemCountForTeam(teamIndex, MysticsItemsContent.Items.MysticsItems_TreasureMap.itemIndex, true);
                holdoutZoneController.baseChargeDuration = CalculateChargeTime(itemCount);
                bool anyoneHasItem = itemCount > 0;

                hologramProjector.displayDistance = holdoutZoneController.currentRadius + 15f;
                if (!holdoutZoneController.enabled) hologramProjector.displayDistance = 0f;
                hologramProjector.hologramPivot.position = transform.position + Vector3.up * holdoutZoneController.currentRadius * 0.5f;

                if (!holdoutZoneController.wasCharged)
                {
                    if (NetworkServer.active)
                    {
                        if (!anyoneHasItem && ShouldBeActive) ShouldBeActive = false;
                        if (anyoneHasItem && !ShouldBeActive) ShouldBeActive = true;
                    }
                }
            }

            public bool ShouldDisplayHologram(GameObject viewer)
            {
                return !holdoutZoneController.wasCharged && ShouldBeActive;
            }

            public GameObject GetHologramContentPrefab()
            {
                return MysticsRisky2Utils.PlainHologram.hologramContentPrefab;
            }

            public void UpdateHologramContent(GameObject hologramContentObject, Transform viewerBody)
            {
                var component = hologramContentObject.GetComponent<PlainHologram.MysticsRisky2UtilsPlainHologramContent>();
                if (component)
                {
                    component.text = string.Format(
                        "<color=#{0}>{1}%</color>",
                        ColorUtility.ToHtmlStringRGB(new Color32(248, 235, 39, 255)),
                        Mathf.FloorToInt(holdoutZoneController.charge * 100f).ToString()
                    );
                    component.color = Color.white;
                }
            }

            public bool shouldBeActive = false;
            public bool ShouldBeActive
            {
                get
                {
                    return shouldBeActive;
                }
                set
                {
                    shouldBeActive = value;
                    if (holdoutZoneController)
                    {
                        holdoutZoneController.enabled = value;
                        if (value == false && holdoutZoneController.radiusIndicator) holdoutZoneController.radiusIndicator.transform.localScale = Vector3.zero;
                    }
                    foreach (GameObject toggleObject in toggleObjects)
                    {
                        toggleObject.SetActive(value);
                    }
                    if (NetworkServer.active) new SyncZoneShouldBeActive(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                }
            }

            public class SyncZoneShouldBeActive : INetMessage
            {
                NetworkInstanceId objID;
                bool value;

                public SyncZoneShouldBeActive()
                {
                }

                public SyncZoneShouldBeActive(NetworkInstanceId objID, bool value)
                {
                    this.objID = objID;
                    this.value = value;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objID = reader.ReadNetworkId();
                    value = reader.ReadBoolean();
                }

                public void OnReceived()
                {
                    if (NetworkServer.active) return;
                    GameObject obj = Util.FindNetworkObject(objID);
                    if (obj)
                    {
                        MysticsItemsTreasureMapZone component = obj.GetComponent<MysticsItemsTreasureMapZone>();
                        if (component)
                        {
                            component.ShouldBeActive = value;
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objID);
                    writer.Write(value);
                }
            }

            public class RequestZoneShouldBeActive : INetMessage
            {
                NetworkInstanceId objID;

                public RequestZoneShouldBeActive()
                {
                }

                public RequestZoneShouldBeActive(NetworkInstanceId objID)
                {
                    this.objID = objID;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objID = reader.ReadNetworkId();
                }

                public void OnReceived()
                {
                    if (!NetworkServer.active) return;
                    GameObject obj = Util.FindNetworkObject(objID);
                    if (obj)
                    {
                        MysticsItemsTreasureMapZone component = obj.GetComponent<MysticsItemsTreasureMapZone>();
                        if (component)
                        {
                            new SyncZoneShouldBeActive(obj.GetComponent<NetworkIdentity>().netId, component.ShouldBeActive).Send(NetworkDestination.Clients);
                        }
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objID);
                }
            }
        }
    }
}
