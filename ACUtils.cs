using RoR2;
using RoR2.Scripts.GameBehaviors;
using System;

namespace MysticsItems
{
    public static class ACUtils
    {
        public static bool ReplaceItem(this Inventory inv, ItemIndex from, ItemIndex to, int stack = 1)
        {
            var countTemp = inv.GetItemCountTemp(from);
            var countPerm = inv.GetItemCountPermanent(from);
            Main.logger.LogInfo("STACK:" + stack + ", TEMP:" + countTemp + ", PERM:" + countPerm);
            if (countTemp + countPerm < stack) return false;
            if (countTemp > 0)
            {
                inv.RemoveItemTemp(from, Math.Min(countTemp, stack));
                if (to != ItemIndex.None) inv.GiveItemTemp(to, Math.Min(countTemp, stack));
            }
            if (stack > countTemp)
            {
                inv.RemoveItemPermanent(from, stack - countTemp);
                if (to != ItemIndex.None) inv.GiveItemPermanent(to, stack - countTemp);
            }
            return true;
        }

        public static void SetItemCountPermanent(this Inventory inv, ItemIndex item, int stack)
        {
            var ic = inv.GetItemCountPermanent(item);
            if (ic == stack) return;
            if (ic > stack) inv.RemoveItemPermanent(item, ic - stack);
            else inv.GiveItemPermanent(item, stack - ic);
        }

        public static void SetItemCountTemp(this Inventory inv, ItemIndex item, int stack)
        {
            var ic = inv.GetItemCountTemp(item);
            if (ic == stack) return;
            if (ic > stack) inv.RemoveItemTemp(item, ic - stack);
            else inv.GiveItemTemp(item, stack - ic);
        }

        public static void SetItemCountChanneled(this Inventory inv, ItemIndex item, int stack)
        {
            var ic = inv.GetItemCountChanneled(item);
            if (ic == stack) return;
            if (ic > stack) inv.RemoveItemChanneled(item, ic - stack);
            else inv.GiveItemChanneled(item, stack - ic);
        }
    }
}
