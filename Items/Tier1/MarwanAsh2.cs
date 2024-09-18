﻿using RoR2;
using UnityEngine;
using Rewired.ComponentControls.Effects;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using static MysticsItems.LegacyBalanceConfigManager;

namespace MysticsItems.Items
{
    public class MarwanAsh2 : BaseItem
    {
        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "MysticsItems_MarwanAsh2";
            SetItemTierWhenAvailable(ItemTier.Tier1);
            itemDef.tags = new ItemTag[]
            {
                ItemTag.Damage,
                ItemTag.WorldUnique
            };
            itemDef.pickupModelPrefab = PrepareModel(Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Marwan's Ash/Level 2/Model.prefab"));
            itemDef.pickupIconSprite = Main.AssetBundle.LoadAsset<Sprite>("Assets/Items/Marwan's Ash/Level 2/IconWhite.png");

            itemDisplayPrefab = PrepareItemDisplayModel(PrepareModel(Main.AssetBundle.LoadAsset<GameObject>("Assets/Items/Marwan's Ash/Level 2/DisplayModel.prefab")));

            SetScalableChildEffect(itemDef.pickupModelPrefab, "Цилиндр/FireEffects/Point Light");
            SetScalableChildEffect(itemDef.pickupModelPrefab, "Цилиндр/FireEffectsBottom/Point Light");

            /*
            Material matMarwanAshFire = itemDef.pickupModelPrefab.transform.Find("Цилиндр/FireEffects/Fire").gameObject.GetComponent<Renderer>().sharedMaterial;
            HopooShaderToMaterial.CloudRemap.Apply(
                matMarwanAshFire,
                Main.AssetBundle.LoadAsset<Texture>("Assets/Items/Marwan's Ash/Level 2/texMarwanAshFireRamp.png")
            );
            HopooShaderToMaterial.CloudRemap.Boost(matMarwanAshFire, 1f);
            */

            onSetupIDRS += () =>
            {
                AddDisplayRule("CommandoBody", "Chest", new Vector3(-0.01315F, 0.31575F, -0.18382F), new Vector3(350.0374F, 186.9538F, 327.4666F), new Vector3(0.05072F, 0.05072F, 0.05072F));
                AddDisplayRule("HuntressBody", "Chest", new Vector3(0.15029F, 0.15519F, -0.09506F), new Vector3(30.45081F, 52.96541F, 358.0648F), new Vector3(0.05567F, 0.05567F, 0.05567F));
                AddDisplayRule("Bandit2Body", "Chest", new Vector3(0.01441F, 0.24763F, -0.19861F), new Vector3(323.4926F, 72.16805F, 6.39712F), new Vector3(0.06379F, 0.06379F, 0.06379F));
                AddDisplayRule("ToolbotBody", "Chest", new Vector3(1.73446F, 0.99271F, -2.04232F), new Vector3(11.32525F, 177.2767F, 25.48854F), new Vector3(0.59435F, 0.59435F, 0.59435F));
                AddDisplayRule("EngiBody", "Chest", new Vector3(-0.02402F, 0.25158F, -0.27853F), new Vector3(0F, 0F, 332.7774F), new Vector3(0.06896F, 0.06896F, 0.06896F));
                AddDisplayRule("EngiTurretBody", "Head", new Vector3(0.13397F, 0.83001F, -0.21654F), new Vector3(3.85755F, 21.84904F, 83.02315F), new Vector3(0.25393F, 0.25393F, 0.25393F));
                AddDisplayRule("EngiWalkerTurretBody", "Head", new Vector3(-0.19146F, 1.44408F, -0.19196F), new Vector3(82.73702F, 180.8069F, 248.3329F), new Vector3(0.17858F, 0.21723F, 0.17458F));
                AddDisplayRule("MageBody", "Chest", new Vector3(0.01261F, 0.11312F, -0.33656F), new Vector3(9.0439F, 166.8645F, 11.76951F), new Vector3(0.06848F, 0.06848F, 0.06848F));
                AddDisplayRule("MercBody", "Chest", new Vector3(0.16833F, 0.14363F, -0.19858F), new Vector3(352.4401F, 249.6687F, 359.9143F), new Vector3(0.07003F, 0.07003F, 0.07003F));
                AddDisplayRule("TreebotBody", "FlowerBase", new Vector3(0.0732F, 1.46304F, -0.0061F), new Vector3(0F, 0F, 0F), new Vector3(0.1865F, 0.1865F, 0.1865F));
                AddDisplayRule("LoaderBody", "MechBase", new Vector3(0.06656F, 0.17752F, -0.23503F), new Vector3(2.41504F, 210.6102F, 17.44111F), new Vector3(0.1077F, 0.1077F, 0.1077F));
                AddDisplayRule("CrocoBody", "SpineChest2", new Vector3(-0.20142F, 0.37072F, -0.02786F), new Vector3(8.80561F, 57.37401F, 259.0943F), new Vector3(0.69333F, 0.69333F, 0.69333F));
                AddDisplayRule("CaptainBody", "Chest", new Vector3(-0.16687F, 0.22055F, -0.20227F), new Vector3(16.87567F, 355.1706F, 29.03075F), new Vector3(0.10211F, 0.10211F, 0.10211F));
                AddDisplayRule("BrotherBody", "chest", BrotherInfection.green, new Vector3(-0.22101F, 0.42643F, -0.064F), new Vector3(0F, 0F, 281.5435F), new Vector3(0.04683F, 0.09274F, 0.10516F));
                AddDisplayRule("ScavBody", "Chest", new Vector3(7.79231F, -2.96603F, 2.57057F), new Vector3(349.2455F, 291.8784F, 352.6789F), new Vector3(2.1017F, 2.15959F, 2.1017F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysSniper) AddDisplayRule("SniperClassicBody", "Chest", new Vector3(0.0428F, 0.26372F, -0.34036F), new Vector3(338.6975F, 6.87321F, 337.6585F), new Vector3(0.053F, 0.053F, 0.053F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysDeputy) AddDisplayRule("DeputyBody", "Chest", new Vector3(0.09953F, 0.05801F, -0.09083F), new Vector3(0.12711F, 359.6725F, 314.7372F), new Vector3(0.05777F, 0.05777F, 0.05777F));
                if (SoftDependencies.SoftDependenciesCore.itemDisplaysChirr) AddDisplayRule("ChirrBody", "Chest", new Vector3(-0.08422F, 0.54551F, 0.59633F), new Vector3(317.497F, 66.63488F, 12.70776F), new Vector3(0.12415F, 0.12415F, 0.12415F));
                AddDisplayRule("RailgunnerBody", "Backpack", new Vector3(0.26934F, 0.05187F, 0.13162F), new Vector3(5.80651F, 358.8727F, 175.3925F), new Vector3(-0.05577F, -0.05577F, -0.05577F));
                AddDisplayRule("VoidSurvivorBody", "Chest", new Vector3(-0.194F, 0.02176F, -0.12993F), new Vector3(335.7027F, 347.8149F, 33.45188F), new Vector3(0.05121F, 0.05121F, 0.05121F));
            };
        }
    }
}
