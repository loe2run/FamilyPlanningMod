using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;

namespace FamilyPlanning.Patches
{
    /* Child.dayUpdate():
     * This patch overrides the call to the getChildBed method, to handle having more than 2 children.
     * 
     * For the first two children, this method behaves the same as vanilla,
     * with the exception that beds are chosen by birth order instead of by gender.
     * After two children, child 3 will try to share a bed with a like-gender sibling,
     * and child 4 will use whatever the last open spot is.
     * If there are more than 4 children, child 5+ will end up in the same spot as child 4.
     */

    class ChildDayUpdatePatch
    {
        public static void Postfix(ref Child __instance)
        {
            // This function only changes the position of toddlers
            if (__instance.Age < 3)
                return;

            // If child isn't at home, abort
            if (!(__instance.currentLocation is FarmHouse))
                return;

            // Get FarmHouse location of child
            FarmHouse farmHouse = __instance.currentLocation as FarmHouse;

            // Get random seed value
            int uniqueMultiplayerId;
            if (farmHouse.owner != null)
                uniqueMultiplayerId = (int)farmHouse.owner.UniqueMultiplayerID;
            else
                uniqueMultiplayerId = (int)Game1.MasterPlayer.UniqueMultiplayerID;

            // This is the same random value as the original
            Random random = new Random(Game1.Date.TotalDays + (int)Game1.uniqueIDForThisGame / 2 + uniqueMultiplayerId * 2);

            // Get the new position for the child at the beginning of day
            Point openPoint = farmHouse.getRandomOpenPointInHouse(random, 1, 60);
            
            // If random point fails, use custom GetChildBed method instead of farmHouse.getChildBed
            if (openPoint.Equals(Point.Zero))
                openPoint = ModEntry.GetChildBed(__instance, farmHouse);

            // Change the position of child in the house
            __instance.setTilePosition(openPoint);
        }
    }
}