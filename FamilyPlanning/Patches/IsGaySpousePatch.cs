using StardewValley;

namespace FamilyPlanning.Patches
{
    /* IsGaySpouse triggers in a few places to determine parenting dialogue.
     * I'm manually setting Krobus as always a gay spouse for the sake of adoption dialogue.
     * (If any mods come out that add new roommates, I'd like to come back and add compatibility here.)
     */
     
    class IsGaySpousePatch
    {
        public static void Postfix(NPC __instance, ref bool __result)
        {
            if(__instance.Name.Equals("Krobus"))
            {
                __result = true;
                return;
            }
        }
    }
}
