﻿using BepInEx;
using MysticsRisky2Utils;
using R2API;
using R2API.Networking;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MysticsItems
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(DamageAPI.PluginGUID)]
    [BepInDependency(DotAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(NetworkingAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(SoundAPI.PluginGUID)]
    [BepInDependency(MysticsRisky2UtilsPlugin.PluginGUID)]
    [BepInDependency("com.Moffein.ArchaicWisp", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("dev.ontrigger.itemstats", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("aaaa.bubbet.whatamilookingat", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.ThinkInvisible.TILER2", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class MysticsItemsPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.themysticsword.mysticsitems";
        public const string PluginName = "MysticsItems";
        public const string PluginVersion = "2.1.20";

        internal static BepInEx.Logging.ManualLogSource logger;
        internal static PluginInfo pluginInfo;
        
        public void Awake()
        {
            logger = Logger;
            pluginInfo = Info;
            Main.Init();
        }
    }

    public static partial class Main
    {
        private static AssetBundle _assetBundle;
        public static AssetBundle AssetBundle
        {
            get
            {
                if (_assetBundle == null)
                    _assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(MysticsItemsPlugin.pluginInfo.Location), "mysticsitemsunityassetbundle"));
                return _assetBundle;
            }
        }

        internal const BindingFlags bindingFlagAll = (BindingFlags)(-1);
        internal static BepInEx.Logging.ManualLogSource logger;

        internal static bool isDedicatedServer = Application.isBatchMode;

        public static Assembly executingAssembly;
        internal static System.Type declaringType;

        public static void Init()
        {
            logger = MysticsItemsPlugin.logger;

            ConfigManager.Init();

            // SoundAPI.SoundBanks.Add(System.IO.File.ReadAllBytes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(MysticsItemsPlugin.pluginInfo.Location), "MysticsItemsWwiseSoundbank.bnk")));

            executingAssembly = Assembly.GetExecutingAssembly();
            declaringType = MethodBase.GetCurrentMethod().DeclaringType;

            CustomStats.Init();
            DamageNumberTint.Init();
            GenericCostTypes.Init();
            NetworkPickupDiscovery.Init();
            SoftDependencies.SoftDependenciesCore.Init();
            TMProEffects.Init();

            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseItem>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseEquipment>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseBuff>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseInteractable>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterBody>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterMaster>(executingAssembly);

            // Load the content pack
            ContentManager.collectContentPackProviders += (addContentPackProvider) =>
            {
                addContentPackProvider(new MysticsItemsContent());
            };

            UpdateFirstLaunchManager.Init();
            FunEvents.Init();

            /*
            // Generate item preview table image
            RoR2Application.onLoad += () =>
            {
                ImageGeneration.Init();
                var itemDefs = typeof(MysticsItemsContent.Items).GetFields().Select(x => x.GetValue(null) as ItemDef)
                .Where(x => !x.hidden && x.inDroppableTier && x.DoesNotContainTag(ItemTag.WorldUnique)).ToList();
                var equipmentDefs = typeof(MysticsItemsContent.Equipment).GetFields().Select(x => x.GetValue(null) as EquipmentDef)
                    .Where(x => x.canDrop).ToList();

                var sections = new List<ImageGeneration.ItemTableSection>()
                {
                    new ImageGeneration.ItemTableSection()
                    {
                        itemDefs = itemDefs.Where(x => x.tier == ItemTier.Tier1).ToList(),
                        color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier1Item)
                    },
                    new ImageGeneration.ItemTableSection()
                    {
                        itemDefs = itemDefs.Where(x => x.tier == ItemTier.Tier2).ToList(),
                        color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier2Item)
                    },
                    new ImageGeneration.ItemTableSection()
                    {
                        itemDefs = itemDefs.Where(x => x.tier == ItemTier.Tier3).ToList(),
                        color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier3Item)
                    },
                    new ImageGeneration.ItemTableSection()
                    {
                        itemDefs = itemDefs.Where(x => x.tier == ItemTier.Lunar).ToList(),
                        color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarItem)
                    },
                    new ImageGeneration.ItemTableSection()
                    {
                        equipmentDefs = equipmentDefs.Where(x => !x.isLunar).ToList(),
                        color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Equipment)
                    },
                    new ImageGeneration.ItemTableSection()
                    {
                        equipmentDefs = equipmentDefs.Where(x => x.isLunar).ToList(),
                        color = ColorCatalog.GetColor(ColorCatalog.ColorIndex.LunarItem)
                    }
                };

                ImageGeneration.GenerateItemTable(100f, 1024f, 214f, 3, Language.english, sections);
            };
            */
        }
    }

    public class MysticsItemsContent : IContentPackProvider
    {
        public string identifier
        {
            get
            {
                return MysticsItemsPlugin.PluginName;
            }
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper contentLoadHelper = new MysticsRisky2Utils.ContentManagement.ContentLoadHelper();
            
            // Add content loading dispatchers to the content load helper
            System.Action[] loadDispatchers = new System.Action[]
            {
                () => contentLoadHelper.DispatchLoad<ItemDef>(Main.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseItem), x => contentPack.itemDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<EquipmentDef>(Main.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseEquipment), x => contentPack.equipmentDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<BuffDef>(Main.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseBuff), x => contentPack.buffDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<GameObject>(Main.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseInteractable), null),
                () => contentLoadHelper.DispatchLoad<GameObject>(Main.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseCharacterBody), x => contentPack.bodyPrefabs.Add(x)),
                () => contentLoadHelper.DispatchLoad<GameObject>(Main.executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseCharacterMaster), x => contentPack.masterPrefabs.Add(x))
            };
            int num = 0;
            for (int i = 0; i < loadDispatchers.Length; i = num)
            {
                loadDispatchers[i]();
                args.ReportProgress(Util.Remap((float)(i + 1), 0f, (float)loadDispatchers.Length, 0f, 0.05f));
                yield return null;
                num = i + 1;
            }

            // Start loading content. Longest part of the loading process, so we will dedicate most of the progress bar to it
            while (contentLoadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(Util.Remap(contentLoadHelper.progress.value, 0f, 1f, 0.05f, 0.9f));
                yield return contentLoadHelper.coroutine.Current;
            }

            // Populate static content pack fields and add various prefabs and scriptable objects generated during the content loading part to the content pack
            loadDispatchers = new System.Action[]
            {
                () => ContentLoadHelper.PopulateTypeFields<ItemDef>(typeof(Items), contentPack.itemDefs),
                () => ContentLoadHelper.PopulateTypeFields<EquipmentDef>(typeof(Equipment), contentPack.equipmentDefs),
                () => ContentLoadHelper.PopulateTypeFields<BuffDef>(typeof(Buffs), contentPack.buffDefs),
                () => contentPack.bodyPrefabs.Add(Resources.bodyPrefabs.ToArray()),
                () => contentPack.masterPrefabs.Add(Resources.masterPrefabs.ToArray()),
                () => contentPack.projectilePrefabs.Add(Resources.projectilePrefabs.ToArray()),
                () => contentPack.effectDefs.Add(Resources.effectPrefabs.ConvertAll(x => new EffectDef(x)).ToArray()),
                () => contentPack.networkedObjectPrefabs.Add(Resources.networkedObjectPrefabs.ToArray()),
                () => contentPack.networkSoundEventDefs.Add(Resources.networkSoundEventDefs.ToArray()),
                () => contentPack.unlockableDefs.Add(Resources.unlockableDefs.ToArray()),
                () => contentPack.entityStateTypes.Add(Resources.entityStateTypes.ToArray()),
                () => contentPack.skillDefs.Add(Resources.skillDefs.ToArray()),
                () => contentPack.skillFamilies.Add(Resources.skillFamilies.ToArray())
            };
            for (int i = 0; i < loadDispatchers.Length; i = num)
            {
                loadDispatchers[i]();
                args.ReportProgress(Util.Remap((float)(i + 1), 0f, (float)loadDispatchers.Length, 0.9f, 0.95f));
                yield return null;
                num = i + 1;
            }

            // Call "AfterContentPackLoaded" methods
            loadDispatchers = new System.Action[]
            {
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseItem>(Main.executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseEquipment>(Main.executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseBuff>(Main.executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseInteractable>(Main.executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterBody>(Main.executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterMaster>(Main.executingAssembly)
            };
            for (int i = 0; i < loadDispatchers.Length; i = num)
            {
                loadDispatchers[i]();
                args.ReportProgress(Util.Remap((float)(i + 1), 0f, (float)loadDispatchers.Length, 0.95f, 0.99f));
                yield return null;
                num = i + 1;
            }

            loadDispatchers = null;
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private ContentPack contentPack = new ContentPack();

        public static class Resources
        {
            public static List<GameObject> bodyPrefabs = new List<GameObject>();
            public static List<GameObject> masterPrefabs = new List<GameObject>();
            public static List<GameObject> projectilePrefabs = new List<GameObject>();
            public static List<GameObject> effectPrefabs = new List<GameObject>();
            public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();
            public static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();
            public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();
            public static List<System.Type> entityStateTypes = new List<System.Type>();
            public static List<RoR2.Skills.SkillDef> skillDefs = new List<RoR2.Skills.SkillDef>();
            public static List<RoR2.Skills.SkillFamily> skillFamilies = new List<RoR2.Skills.SkillFamily>();
        }

        public static class Items
        {
            public static ItemDef MysticsItems_AllyDeathRevenge;
            public static ItemDef MysticsItems_BackArmor;
            public static ItemDef MysticsItems_Backpack;
            public static ItemDef MysticsItems_CoffeeBoostOnItemPickup;
            public static ItemDef MysticsItems_Cookie;
            public static ItemDef MysticsItems_CrystalWorld;
            public static ItemDef MysticsItems_DasherDisc;
            public static ItemDef MysticsItems_DeathCeremony;
            public static ItemDef MysticsItems_DroneWires;
            public static ItemDef MysticsItems_ElitePotion;
            public static ItemDef MysticsItems_ExplosivePickups;
            public static ItemDef MysticsItems_ExtraShrineUse;
            public static ItemDef MysticsItems_HealOrbOnBarrel;
            public static ItemDef MysticsItems_Idol;
            public static ItemDef MysticsItems_JudgementCut;
            public static ItemDef MysticsItems_KeepShopTerminalOpen;
            public static ItemDef MysticsItems_KeepShopTerminalOpenConsumed;
            public static ItemDef MysticsItems_LimitedArmor;
            public static ItemDef MysticsItems_LimitedArmorBroken;
            public static ItemDef MysticsItems_Manuscript;
            public static ItemDef MysticsItems_MarwanAsh1;
            public static ItemDef MysticsItems_MarwanAsh2;
            public static ItemDef MysticsItems_MarwanAsh3;
            public static ItemDef MysticsItems_Moonglasses;
            public static ItemDef MysticsItems_MysticSword;
            public static ItemDef MysticsItems_RegenAndDifficultySpeed;
            public static ItemDef MysticsItems_Rhythm;
            public static ItemDef MysticsItems_RiftLens;
            public static ItemDef MysticsItems_RiftLensDebuff;
            public static ItemDef MysticsItems_ScratchTicket;
            public static ItemDef MysticsItems_SpeedGivesDamage;
            public static ItemDef MysticsItems_Spotter;
            public static ItemDef MysticsItems_ThoughtProcessor;
            public static ItemDef MysticsItems_TreasureMap;
            public static ItemDef MysticsItems_Voltmeter;
            public static ItemDef MysticsItems_VyraelCommandments;
            public static ItemDef MysticsItems_GachaponToken;
            public static ItemDef MysticsItems_Nanomachines;
            public static ItemDef MysticsItems_ShieldUpgrade;
            public static ItemDef MysticsItems_BuffInTPRange;
            public static ItemDef MysticsItems_StarBook;
            public static ItemDef MysticsItems_TimePiece;
            public static ItemDef MysticsItems_Flow;
            public static ItemDef MysticsItems_GhostApple;
            public static ItemDef MysticsItems_GhostAppleWeak;
            public static ItemDef MysticsItems_SnowRing;
        }

        public static class Equipment
        {
            public static EquipmentDef MysticsItems_ArchaicMask;
            public static EquipmentDef MysticsItems_FragileMask;
            public static EquipmentDef MysticsItems_GateChalice;
            public static EquipmentDef MysticsItems_Microphone;
            public static EquipmentDef MysticsItems_MechanicalArm;
            public static EquipmentDef MysticsItems_OmarHackTool;
            public static EquipmentDef MysticsItems_PrinterHacker;
            public static EquipmentDef MysticsItems_SirenPole;
            public static EquipmentDef MysticsItems_TuningFork;
            public static EquipmentDef MysticsItems_EquipmentEater;
        }

        public static class Buffs
        {
            public static BuffDef MysticsItems_AllyDeathRevenge;
            public static BuffDef MysticsItems_CoffeeBoost;
            public static BuffDef MysticsItems_Crystallized;
            public static BuffDef MysticsItems_DasherDiscActive;
            public static BuffDef MysticsItems_DasherDiscCooldown;
            public static BuffDef MysticsItems_Deafened;
            public static BuffDef MysticsItems_MarwanAshBurn;
            public static BuffDef MysticsItems_MarwanAshBurnStrong;
            public static BuffDef MysticsItems_MechanicalArmCharge;
            public static BuffDef MysticsItems_SpotterMarked;
            public static BuffDef MysticsItems_GachaponBonus;
            public static BuffDef MysticsItems_NanomachineArmor;
            public static BuffDef MysticsItems_BuffInTPRange;
            public static BuffDef MysticsItems_StarPickup;
            public static BuffDef MysticsItems_TimePieceSlow;
        }
    }
}
