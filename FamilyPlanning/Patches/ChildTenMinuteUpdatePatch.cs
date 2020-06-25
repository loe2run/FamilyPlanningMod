using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System.Collections.Generic;
using System.Linq;

namespace FamilyPlanning.Patches
{
    /* Child.tenMinuteUpdate():
     * This patch overrides the call to the getChildBed method, to handle having more than 2 children.
     * 
     * For the first two children, this method behaves exactly the same as vanilla.
     * After two children, child 3 will try to share a bed with a like-gender sibling,
     * and child 4 will use whatever the last open spot is.
     * If there are more than 4 children, child 5+ will end up in the same spot as child 4.
     */

    class ChildTenMinuteUpdatePatch
    {
        public static void Postfix(ref Child __instance)
        {
            // We only want to patch bedtime code
            if (!Game1.IsMasterGame || __instance.Age != 3 || Game1.timeOfDay != 1900)
                return;

            // Child should be at home, but abort if not
            if (!(__instance.currentLocation is FarmHouse))
            {
                ModEntry.monitor.Log("TenMinuteUpdate found child not at home: " + __instance.Name, LogLevel.Trace);
                return;
            }

            FarmHouse farmHouse = __instance.currentLocation as FarmHouse;
            if (!farmHouse.characters.Contains(__instance))
            {
                ModEntry.monitor.Log("TenMinuteUpdate found home doesn't contain child: " + __instance.Name, LogLevel.Trace);
                return;
            }

            // Change where the child is pathfinding to using my custom GetChildBed method
            Point childBed = ModEntry.GetChildBed(__instance, farmHouse);
            __instance.controller = new PathFindController(__instance, farmHouse, childBed, -1,
                                                           new PathFindController.endBehavior(__instance.toddlerReachedDestination));

            // Abort if the controller failed to find a path
            Stack<Point> path = __instance.controller.pathToEndPoint;
            if (path == null || !farmHouse.isTileOnMap(path.Last().X, path.Last().Y))
                __instance.controller = null;
        }
    }
}