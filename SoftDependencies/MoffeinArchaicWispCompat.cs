using RoR2;

namespace MysticsItems.SoftDependencies
{
    internal static class MoffeinArchaicWispCompat
    {
        internal static void Init()
        {
            RoR2Application.onLoad += ReplaceArchaicMaskSpawnCard;
        }

        private static void ReplaceArchaicMaskSpawnCard()
        {
            Equipment.ArchaicMask.ArchWispSpawnCard = (CharacterSpawnCard)ArchaicWisp.ArchaicWispContent.ArchaicWispCard.Card.spawnCard;
        }
    }
}