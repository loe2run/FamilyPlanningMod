using StardewValley;

namespace FamilyPlanning.Patches
{
    /* NPC.isGaySpouse():
     * This patch causes a roommate spouse to be recognized as a gay spouse.
     * IsGaySpouse triggers in a few places to determine parenting dialogue, and roommates will always adopt children,
     * so patching them as a gay spouse gives them adoption dialogue instead.
     * 
     * While the only vanilla roommate option is Krobus, this patch is based on roommate status instead of name,
     * so any additional roommate options should also be covered by this patch.
     */
    
    class IsGaySpousePatch
    {
        public static void Postfix(NPC __instance, ref bool __result)
        {
            if(ModEntry.RoommateConfig() && __instance.isRoommate())
            {
                __result = true;
                return;
            }
        }
    }
}