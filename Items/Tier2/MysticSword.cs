using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using static MysticsItems.LegacyBalanceConfigManager;

namespace MysticsItems.Items
{
    public class MysticSword : BaseItem
    {
        public static ConfigurableValue<float> healthThreshold = new ConfigurableValue<float>(
            "Item: Mystic Sword",
            "HealthThreshold",
            1900f,
            "How many HP should the killed enemy have to trigger this item's effect",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_MYSTICSWORD_DESC"
            }
        );
        public static ConfigurableValue<float> damage = new ConfigurableValue<float>(
            "Item: Mystic Sword",
            "Damage",
            2f,
            "Damage bonus for each strong enemy killed (in %)",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_MYSTICSWORD_DESC"
            }
        );
        public static ConfigurableValue<float> maxDamage = new ConfigurableValue<float>(
            "Item: Mystic Sword",
            "MaxDamage",
            40f,
            "Maximum damage bonus from the first stack of this item (in %)",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_MYSTICSWORD_DESC"
            }
        );
        public static ConfigurableValue<float> maxDamagePerStack = new ConfigurableValue<float>(
            "Item: Mystic Sword",
            "MaxDamagePerStack",
            20f,
            "Maximum damage bonus for each additional stack of this item (in %)",
            new System.Collections.Generic.List<string>()
            {
                "ITEM_MYSTICSITEMS_MYSTICSWORD_DESC"
            }
        );

        private static string tooltipString = "<color=#BE2BE1></color><color=#EAEEDD></color><color=#AAAAA5></color>";

        public static DamageColorIndex damageColorIndex = DamageColorAPI.RegisterDamageColor(new Color32(247, 245, 197, 255));
        public static GameObject onKillOrbEffect;
        public static GameObject onKillVFX;
        public static NetworkSoundEventDef onKillSFX;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            NetworkingAPI.RegisterMessageType<MysticsItemsMysticSwordBehaviour.SyncDamageBonus>();
        }

        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "MysticsItems_MysticSword";
            SetItemTierWhenAvailable(ItemTier.Tier2);
            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.OnKillEffect,
                ItemTag.AIBlacklist
            };
            
            itemDef.pickupModelPrefab = PrepareModel(Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Mystic Sword/Model.prefab"));
            itemDef.pickupIconSprite = Main.AssetBundle.LoadAsset<Sprite>("Assets/Items/Mystic Sword/Icon.png");
            var mat = itemDef.pickupModelPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
            HopooShaderToMaterial.Standard.Apply(mat);
            HopooShaderToMaterial.Standard.Emission(mat, 1f, new Color32(0, 250, 255, 255));
            itemDef.pickupModelPrefab.transform.Find("GameObject").localScale *= 0.1f;

            var swordFollowerPrefab = PrefabAPI.InstantiateClone(PrepareItemDisplayModel(PrepareModel(Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Mystic Sword/DisplayModel.prefab"))), "MysticsItems_MysticSwordItemFollowerPrefab", false);
            swordFollowerPrefab.transform.Find("TranslatePivot").transform.localScale *= 0.02f;
            ObjectTransformCurve objectTransformCurve = swordFollowerPrefab.transform.Find("TranslatePivot").gameObject.AddComponent<ObjectTransformCurve>();
            objectTransformCurve.translationCurveX = AnimationCurve.Constant(0f, 1f, 0f);
            var floatY = 0.1f;
            objectTransformCurve.translationCurveY = new AnimationCurve
            {
                keys = new Keyframe[]
                {
                    new Keyframe(0.25f, floatY),
                    new Keyframe(0.75f, -floatY)
                },
                preWrapMode = WrapMode.PingPong,
                postWrapMode = WrapMode.PingPong
            };
            objectTransformCurve.translationCurveZ = AnimationCurve.Constant(0f, 1f, 0f);
            objectTransformCurve.useTranslationCurves = true;
            objectTransformCurve.timeMax = 10f;
            objectTransformCurve.rotationCurveX = AnimationCurve.Constant(0f, 1f, 0f);
            objectTransformCurve.rotationCurveY = AnimationCurve.Linear(0f, 0f, 1f, 360f);
            objectTransformCurve.rotationCurveY.preWrapMode = WrapMode.Loop;
            objectTransformCurve.rotationCurveY.postWrapMode = WrapMode.Loop;
            objectTransformCurve.rotationCurveZ = AnimationCurve.Constant(0f, 1f, 0f);
            objectTransformCurve.useRotationCurves = true;
            objectTransformCurve.gameObject.AddComponent<MysticsRisky2Utils.MonoBehaviours.MysticsRisky2UtilsObjectTransformCurveLoop>();

            itemDisplayPrefab = PrefabAPI.InstantiateClone(new GameObject("MysticsItems_MysticSwordFollower"), "MysticsItems_MysticSwordFollower", false);
            itemDisplayPrefab.AddComponent<ItemDisplay>();
            ItemFollower itemFollower = itemDisplayPrefab.AddComponent<ItemFollower>();
            itemFollower.followerPrefab = swordFollowerPrefab;
            itemFollower.distanceDampTime = 0.1f;
            itemFollower.distanceMaxSpeed = 20f;
            itemFollower.targetObject = itemDisplayPrefab;
            var itemDisplayHelper = itemDisplayPrefab.AddComponent<MysticsItemsMysticSwordItemDisplayHelper>();
            itemDisplayHelper.itemFollower = itemFollower;

            onSetupIDRS += () =>
            {
                AddDisplayRule("CommandoBody", "Base", new Vector3(0.17794F, -0.28733F, -0.73752F), new Vector3(3.15473F, 89.99998F, 270.0002F), Vector3.one);
                AddDisplayRule("HuntressBody", "Base", new Vector3(0.17816F, -0.23663F, -0.52846F), new Vector3(2.42504F, 269.9999F, 90.0001F), Vector3.one);
                AddDisplayRule("Bandit2Body", "Base", new Vector3(0.4537F, 0.29041F, -0.57258F), new Vector3(270F, 0F, 0F), Vector3.one);
                AddDisplayRule("ToolbotBody", "Base", new Vector3(-1.04879F, -4.19278F, 5.42458F), new Vector3(0F, 90F, 90F), Vector3.one);
                AddDisplayRule("EngiBody", "Base", new Vector3(0.0113F, -0.52335F, -0.69199F), new Vector3(270F, 0F, 0F), Vector3.one);
                AddDisplayRule("EngiTurretBody", "Base", new Vector3(1.03266F, 3.98892F, -2.18302F), new Vector3(0F, 90F, 0F), Vector3.one);
                AddDisplayRule("EngiWalkerTurretBody", "Base", new Vector3(1.53037F, 3.79942F, -2.10391F), new Vector3(0F, 90F, 0F), Vector3.one);
                AddDisplayRule("MageBody", "Base", new Vector3(0.38669F, -0.43447F, -0.48611F), new Vector3(270F, 0F, 0F), Vector3.one);
                AddDisplayRule("MercBody", "Base", new Vector3(0.38005F, -0.35752F, -0.53391F), new Vector3(270F, 0F, 0F), Vector3.one);
                AddDisplayRule("TreebotBody", "Base", new Vector3(0.69145F, -1.39195F, -1.94014F), new Vector3(270F, 0F, 0F), Vector3.one * 1f);
                AddDisplayRule("LoaderBody", "Base", new Vector3(0.26563F, -0.57799F, -0.60309F), new Vector3(270F, 0F, 0F), Vector3.one);
                AddDisplayRule("CrocoBody", "Base", new Vector3(2.43278F, 4.85691F, 4.92643F), new Vector3(90F, 0F, 0F), Vector3.one * 1f);
                AddDisplayRule("CaptainBody", "Base", new Vector3(0.52281F, -0.26508F, -0.8575F), new Vector3(270F, 0F, 0F), Vector3.one);
                AddDisplayRule("BrotherBody", "HandR", BrotherInfection.green, new Vector3(-0.00915F, 0.08592F, 0.02786F), new Vector3(77.05167F, 128.9087F, 289.6218F), new Vector3(0.06672F, 0.02927F, 0.06676F));
                AddDisplayRule("ScavBody", "Base", new Vector3(4.53188F, 14.35975F, 10.88982F), new Vector3(90F, 0F, 0F), Vector3.one * 2f);
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysSniper) AddDisplayRule("SniperClassicBody", "Base", new Vector3(-0.74382F, 1.77236F, -0.52436F), new Vector3(0F, 0F, 0F), new Vector3(1F, 1F, 1F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysDeputy) AddDisplayRule("DeputyBody", "BaseBone", new Vector3(-0.682F, -0.35925F, 0.60783F), new Vector3(90F, 0F, 0F), new Vector3(1F, 1F, 1F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysChirr) AddDisplayRule("ChirrBody", "Base", new Vector3(2.10156F, 3.67593F, -0.74223F), new Vector3(0F, 0F, 0F), new Vector3(1F, 1F, 1F));
                AddDisplayRule("RailgunnerBody", "Base", new Vector3(0.25904F, -0.39171F, -0.30991F), new Vector3(270F, 0F, 0F), new Vector3(1F, 1F, 1F));
                AddDisplayRule("VoidSurvivorBody", "Base", new Vector3(0.4739F, 0.74488F, 0.37712F), new Vector3(68.90421F, 0F, 0F), new Vector3(1F, 1F, 1F) * 0.8f);
            };

            {
                onKillVFX = Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Mystic Sword/SwordPowerUpKillEffect.prefab");
                EffectComponent effectComponent = onKillVFX.AddComponent<EffectComponent>();
                effectComponent.applyScale = true;
                VFXAttributes vfxAttributes = onKillVFX.AddComponent<VFXAttributes>();
                vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
                vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Medium;
                onKillVFX.AddComponent<DestroyOnTimer>().duration = 1f;
                MysticsItemsContent.Resources.effectPrefabs.Add(onKillVFX);
            }

            {
                onKillOrbEffect = Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Mystic Sword/SwordPowerUpOrbEffect.prefab");
                EffectComponent effectComponent = onKillOrbEffect.AddComponent<EffectComponent>();
                effectComponent.positionAtReferencedTransform = false;
                effectComponent.parentToReferencedTransform = false;
                effectComponent.applyScale = true;
                VFXAttributes vfxAttributes = onKillOrbEffect.AddComponent<VFXAttributes>();
                vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
                vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Medium;
                OrbEffect orbEffect = onKillOrbEffect.AddComponent<OrbEffect>();
                orbEffect.startVelocity1 = new Vector3(-25f, 5f, -25f);
                orbEffect.startVelocity2 = new Vector3(25f, 50f, 25f);
                orbEffect.endVelocity1 = new Vector3(0f, 0f, 0f);
                orbEffect.endVelocity2 = new Vector3(0f, 0f, 0f);
                var curveHolder = onKillVFX.transform.Find("Origin/Particle System").GetComponent<ParticleSystem>().sizeOverLifetime;
                orbEffect.movementCurve = curveHolder.size.curve;
                orbEffect.faceMovement = true;
                orbEffect.callArrivalIfTargetIsGone = false;
                DestroyOnTimer destroyOnTimer = onKillOrbEffect.transform.Find("Origin/Unparent").gameObject.AddComponent<DestroyOnTimer>();
                destroyOnTimer.duration = 0.5f;
                destroyOnTimer.enabled = false;
                MysticsRisky2Utils.MonoBehaviours.MysticsRisky2UtilsOrbEffectOnArrivalDefaults onArrivalDefaults = onKillOrbEffect.AddComponent<MysticsRisky2Utils.MonoBehaviours.MysticsRisky2UtilsOrbEffectOnArrivalDefaults>();
                onArrivalDefaults.orbEffect = orbEffect;
                onArrivalDefaults.transformsToUnparentChildren = new Transform[] { onKillOrbEffect.transform.Find("Origin/Unparent") };
                onArrivalDefaults.componentsToEnable = new MonoBehaviour[] { destroyOnTimer };
                MysticsItemsContent.Resources.effectPrefabs.Add(onKillOrbEffect);
            }

            onKillSFX = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            onKillSFX.eventName = "MysticsItems_Play_item_proc_MysticSword";
            MysticsItemsContent.Resources.networkSoundEventDefs.Add(onKillSFX);

            CharacterMaster.onStartGlobal += CharacterMaster_onStartGlobal;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            if (!SoftDependencies.SoftDependenciesCore.itemStatsEnabled) On.RoR2.UI.ItemIcon.SetItemIndex += ItemIcon_SetItemIndex;

            // GenericGameEvents.BeforeTakeDamage += GenericGameEvents_BeforeTakeDamage;

            MysticsItemsMysticSwordItemDisplayHelper.materialFlash = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashBright");
            MysticsItemsMysticSwordItemDisplayHelper.blinkEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressBlinkEffect.prefab").WaitForCompletion();
        }

        private void CharacterMaster_onStartGlobal(CharacterMaster obj)
        {
            if (obj.inventory) obj.inventory.gameObject.AddComponent<MysticsItemsMysticSwordBehaviour>();
        }

        private void GenericGameEvents_BeforeTakeDamage(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (attackerInfo.inventory && attackerInfo.inventory.GetItemCount(itemDef) > 0)
            {
                if (damageInfo.damageColorIndex == DamageColorIndex.Default)
                    damageInfo.damageColorIndex = damageColorIndex;
            }
        }

        private void ItemIcon_SetItemIndex(On.RoR2.UI.ItemIcon.orig_SetItemIndex orig, RoR2.UI.ItemIcon self, ItemIndex newItemIndex, int newItemCount)
        {
            orig(self, newItemIndex, newItemCount);

            if (newItemIndex == itemDef.itemIndex)
            {
                Transform parent = self.transform.parent;
                if (parent)
                {
                    RoR2.UI.ItemInventoryDisplay itemInventoryDisplay = parent.GetComponent<RoR2.UI.ItemInventoryDisplay>();
                    if (itemInventoryDisplay && itemInventoryDisplay.inventory)
                    {
                        MysticsItemsMysticSwordBehaviour swordBehaviour = itemInventoryDisplay.inventory.GetComponent<MysticsItemsMysticSwordBehaviour>();
                        if (swordBehaviour)
                        {
                            globalStringBuilder.Clear();
                            globalStringBuilder.Append(Language.GetString(self.tooltipProvider.bodyToken) + "\r\n");
                            globalStringBuilder.Append("\r\n");
                            globalStringBuilder.Append(Language.GetString("MYSTICSITEMS_STATCHANGE_LIST_HEADER"));
                            globalStringBuilder.Append("\r\n");
                            globalStringBuilder.Append(
                                Language.GetStringFormatted(
                                    "MYSTICSITEMS_STATCHANGE_LIST_DAMAGE",
                                    "+" + (Mathf.RoundToInt(swordBehaviour.damageBonus * 100f)).ToString(System.Globalization.CultureInfo.InvariantCulture)
                                )
                            );
                            globalStringBuilder.Append(tooltipString);
                            self.tooltipProvider.overrideBodyText = globalStringBuilder.ToString();
                            globalStringBuilder.Clear();
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(self.tooltipProvider.overrideBodyText) && self.tooltipProvider.overrideBodyText.Contains(tooltipString))
                    self.tooltipProvider.overrideBodyText = "";
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                var itemCount = sender.inventory.GetItemCount(itemDef);
                if (itemCount > 0)
                {
                    var component = sender.inventory.GetComponent<MysticsItemsMysticSwordBehaviour>();
                    if (component) args.damageMultAdd += component.damageBonus;
                }
            }
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (!NetworkServer.active) return;

            if (damageReport.victimBody)
            {
                var healthMultiplier = 1f;
                if (damageReport.victimBody.inventory)
                    healthMultiplier += damageReport.victimBody.inventory.GetItemCount(RoR2Content.Items.BoostHp) * 0.1f;
                var strongEnemy = (damageReport.victimBody.baseMaxHealth * healthMultiplier) >= healthThreshold;
                if (strongEnemy)
                {
                    var onKillOrbTargets = new List<GameObject>();

                    foreach (var teamMember in TeamComponent.GetTeamMembers(damageReport.attackerTeamIndex))
                    {
                        var teamMemberBody = teamMember.body;
                        if (teamMemberBody)
                        {
                            var inventory = teamMemberBody.inventory;
                            if (inventory)
                            {
                                int itemCount = inventory.GetItemCount(itemDef);
                                if (itemCount > 0)
                                {
                                    var component = inventory.GetComponent<MysticsItemsMysticSwordBehaviour>();
                                    var damageBonusCap = maxDamage / 100f + maxDamagePerStack / 100f * (float)(itemCount - 1);
                                    if (component && component.damageBonus < damageBonusCap)
                                    {
                                        component.damageBonus += Mathf.Clamp(damage / 100f, 0f, damageBonusCap - component.damageBonus);
                                        onKillOrbTargets.Add(teamMemberBody.gameObject);

                                        MysticsItemsMysticSwordItemDisplayHelper.TriggerBlinkForBody(teamMemberBody);
                                        //RoR2.Audio.EntitySoundManager.EmitSoundServer(onKillSFX.index, teamMember.body.gameObject);
                                    }
                                }
                            }
                        }
                    }

                    if (onKillOrbTargets.Count > 0)
                    {
                        for (var i = 0; i < 5; i++)
                        {
                            EffectData effectData = new EffectData
                            {
                                origin = damageReport.victimBody.corePosition,
                                genericFloat = UnityEngine.Random.Range(1.35f, 1.7f),
                                scale = UnityEngine.Random.Range(0.02f, 0.2f)
                            };
                            effectData.SetHurtBoxReference(RoR2Application.rng.NextElementUniform(onKillOrbTargets));
                            EffectManager.SpawnEffect(onKillOrbEffect, effectData, true);
                        }

                        EffectManager.SpawnEffect(onKillVFX, new EffectData
                        {
                            origin = damageReport.victimBody.corePosition,
                            scale = damageReport.victimBody.radius
                        }, true);
                        RoR2.Audio.PointSoundManager.EmitSoundServer(onKillSFX.index, damageReport.victimBody.corePosition);
                    }
                }
            }
        }

        public class MysticsItemsMysticSwordBehaviour : MonoBehaviour
        {
            private float _damageBonus;
            public float damageBonus
            {
                get { return _damageBonus; }
                set
                {
                    _damageBonus = value;
                    if (NetworkServer.active)
                        new SyncDamageBonus(gameObject.GetComponent<NetworkIdentity>().netId, value).Send(NetworkDestination.Clients);
                }
            }

            public class SyncDamageBonus : INetMessage
            {
                NetworkInstanceId objID;
                float damageBonus;

                public SyncDamageBonus()
                {
                }

                public SyncDamageBonus(NetworkInstanceId objID, float damageBonus)
                {
                    this.objID = objID;
                    this.damageBonus = damageBonus;
                }

                public void Deserialize(NetworkReader reader)
                {
                    objID = reader.ReadNetworkId();
                    damageBonus = reader.ReadSingle();
                }

                public void OnReceived()
                {
                    if (NetworkServer.active) return;
                    GameObject obj = Util.FindNetworkObject(objID);
                    if (obj)
                    {
                        MysticsItemsMysticSwordBehaviour component = obj.GetComponent<MysticsItemsMysticSwordBehaviour>();
                        if (component) component.damageBonus = damageBonus;
                    }
                }

                public void Serialize(NetworkWriter writer)
                {
                    writer.Write(objID);
                    writer.Write(damageBonus);
                }
            }
        }

        public class MysticsItemsMysticSwordItemDisplayHelper : MonoBehaviour
        {
            public CharacterBody body;
            public ItemFollower itemFollower;
            public Renderer renderer;
            public float blinkTimer = 0f;
            public float blinkDuration = 1f;
            public float flashTimer = 0f;
            public float flashDuration = 0.3f;
            public static Material materialFlash;
            public static GameObject blinkEffect;
            public Material materialInstance;

            public void Start()
            {
                var model = GetComponentInParent<CharacterModel>();
                body = model ? model.body : null;
            }

            public void TriggerBlink()
            {
                if (blinkTimer <= 0f && flashTimer <= 0f)
                {
                    blinkTimer = blinkDuration;
                    if (renderer)
                    {
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                        renderer.enabled = false;
                    }
                }
            }

            public void Update()
            {
                if (!renderer)
                {
                    if (itemFollower && itemFollower.followerInstance)
                    {
                        renderer = itemFollower.followerInstance.GetComponentInChildren<Renderer>();
                    }
                }

                if (blinkTimer > 0f)
                {
                    blinkTimer -= Time.deltaTime;
                    if (blinkTimer <= 0f)
                    {
                        if (renderer)
                        {
                            renderer.shadowCastingMode = ShadowCastingMode.On;
                            renderer.enabled = true;

                            EffectManager.SpawnEffect(blinkEffect, new EffectData
                            {
                                origin = renderer.transform.position
                            }, false);

                            TemporaryOverlay temporaryOverlay = renderer.gameObject.AddComponent<TemporaryOverlay>();
                            temporaryOverlay.duration = flashTimer = flashDuration;
                            temporaryOverlay.destroyObjectOnEnd = false;
                            temporaryOverlay.destroyComponentOnEnd = true;
                            temporaryOverlay.originalMaterial = materialFlash;
                            temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                            temporaryOverlay.animateShaderAlpha = true;
                            temporaryOverlay.SetupMaterial();

                            var materials = renderer.materials;
                            HG.ArrayUtils.ArrayAppend(ref materials, temporaryOverlay.materialInstance);
                            renderer.materials = materials;
                            materialInstance = renderer.materials.Last();
                        }
                    }
                }

                if (flashTimer > 0f)
                {
                    flashTimer -= Time.deltaTime;
                    if (flashTimer <= 0f)
                    {
                        if (renderer && materialInstance)
                        {
                            var materials = renderer.materials;
                            var index = System.Array.IndexOf(materials, materialInstance);
                            if (index != -1)
                            {
                                HG.ArrayUtils.ArrayRemoveAtAndResize(ref materials, index);
                            }
                            renderer.materials = materials;
                        }
                    }
                }
            }

            public void OnEnable()
            {
                InstanceTracker.Add(this);
            }

            public void OnDisable()
            {
                InstanceTracker.Remove(this);
            }

            public static void TriggerBlinkForBody(CharacterBody body)
            {
                foreach (var itemDisplayHelper in InstanceTracker.GetInstancesList<MysticsItemsMysticSwordItemDisplayHelper>().Where(x => x.body == body))
                    itemDisplayHelper.TriggerBlink();
            }
        }
    }
}
