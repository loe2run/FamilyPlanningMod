using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Locations;
using StardewModdingAPI;
using Netcode;
using System.Globalization;

namespace FamilyPlanning.Patches
{
    /* Utility.pickPersonalFarmEvent():
     * This patch mostly reproduces the original method's code with different formatting,
     * with the addition of verbose messages to the player & customizing the random chance.
     * 
     * This method uses the same random seed as the original method, so the random numbers generated should match.
     * I.e. if BabyQuestionChance is 5 (default) and verbose is false, this method wouldn't create a different result.
     */

    class PickPersonalFarmEventPatch
    {
        public static void Postfix(ref FarmEvent __result)
        {
            // Print out messages to the console about question event probability
            bool verbose = ModEntry.MessagesConfig();

            // Get the custom question chance probability for this save file
            FamilyData data = ModEntry.GetFamilyData();
            if (data == null)
                return;

            int questionValue = data.BabyQuestionChance;
            double questionPercent = questionValue / 100.0;
            ModEntry.monitor.Log("QuestionChance is " + questionValue + ", as percent " + questionPercent + ".", LogLevel.Trace);
            int maxChildren = data.MaxChildren;

            // If I'm not changing anything (question chance or verbose), skip executing patch
            if (questionValue == 5 && !verbose)
                return;

            // Skip if there's a wedding
            if (Game1.weddingToday)
                return;

            // Skip if there's a birth
            if (__result is BirthingEvent || __result is PlayerCoupleBirthingEvent)
                return;

            // Skip if the player isn't married
            if (!Game1.player.isMarried())
                return;

            // The random seed for this method is the same as the game
            Random random = new Random(((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2)
                                       ^ 470124797 + (int)Game1.player.UniqueMultiplayerID);

            // Player may have an NPC spouse or a player spouse
            Farmer player = Game1.player;
            string npcSpouseName = player.spouse;
            long? playerSpouse = player.team.GetSpouse(Game1.player.UniqueMultiplayerID);

            // Check NPC Spouse for QuestionEvent
            if (npcSpouseName != null)
            {
                NPC npcSpouse = Game1.getCharacterFromName(npcSpouseName, true);

                // Check if farmer meets all invariants for baby question event
                if (CheckNPCSpouse(npcSpouse, maxChildren, verbose))
                {
                    // Generate a random value to determine whether the event happens
                    double randomDouble = random.NextDouble();
                    ModEntry.monitor.Log("Generate random value " + randomDouble.ToString("F2") + " < " + questionPercent, LogLevel.Trace);

                    if (randomDouble < questionPercent)
                    {
                        if (verbose)
                            ModEntry.monitor.Log("Luck is on your side, your spouse will ask for a child tonight!", LogLevel.Info);

                        __result = new QuestionEvent(1);
                        return;
                    }
                    else if (verbose)
                        ModEntry.monitor.Log("Luck wasn't on your side, your spouse will not ask for a child tonight.", LogLevel.Info);
                }
            }
            // Check player spouse for QuestionEvent
            else if (playerSpouse.HasValue && Game1.otherFarmers.ContainsKey(playerSpouse.Value))
            {
                Farmer otherFarmer = Game1.otherFarmers[playerSpouse.Value];

                // Check if farmer meets all invariants for baby question event
                if (CheckPlayerSpouse(otherFarmer, maxChildren, verbose))
                {
                    // Generate a random value to determine whether the event happens
                    double randomDouble = random.NextDouble();
                    ModEntry.monitor.Log("Generate random value " + randomDouble + " < " + questionPercent, LogLevel.Trace);

                    if (randomDouble < questionPercent)
                    {
                        if (verbose)
                            ModEntry.monitor.Log("Luck is on your side, your spouse will ask for a child tonight!", LogLevel.Info);

                        __result = new QuestionEvent(3);
                        return;
                    }
                    else if (verbose)
                        ModEntry.monitor.Log("Luck wasn't on your side, your spouse will not ask for a child tonight.", LogLevel.Info);
                }
            }

            // If no other event happened, then try to generate animal events
            __result = random.NextDouble() < 0.5 ? (FarmEvent)new QuestionEvent(2) : (FarmEvent)new SoundInTheNightEvent(2);
            return;
        }

        /* CheckNPCSpouse - checks whether an NPC spouse can get pregnant
         * Replaces the logic in Utility.pickPersonalFarmEvent and in NPC.canGetPregnant.
         * 
         * Instead of limiting children to 2 total, allows children up to a custom MaxChildren value,
         * and if set in config, allows a roommate to ask to adopt a child.
         */
        private static bool CheckNPCSpouse(NPC spouse, int maxChildren, bool verbose)
        {
            // Don't create question event if farmer is going to divorce tonight
            Farmer farmer = Game1.player;
            if (farmer == null || farmer.divorceTonight.Equals(new NetBool(true)))
                return false;

            // Normally Krobus can't get pregnant because he's a roommate
            if (spouse.isRoommate() && !ModEntry.RoommateConfig())
            {
                if (verbose)
                {
                    ModEntry.monitor.LogOnce("Your roommate cannot ask about adopting a child. If you want that to be possible, "
                        + "change the AdoptChildrenWithRoommate option in your config file to true.", LogLevel.Info);
                }
                return false;
            }

            /* This is from the original method */
            spouse.defaultMap.Value = farmer.homeLocation.Value;

            // Check if player is at home
            FarmHouse home = Utility.getHomeOfFarmer(farmer);
            if (!farmer.currentLocation.Equals(home))
            {
                if (verbose)
                    ModEntry.monitor.Log("Your spouse cannot ask for a child tonight because you aren't at home.", LogLevel.Info);
                return false;
            }
            // Check if the home has been upgraded
            if (home.upgradeLevel < 2)
            {
                if (verbose)
                    ModEntry.monitor.LogOnce("You must upgrade your house to have a nursery before you can have children.", LogLevel.Info);
                return false;
            }

            // Check if spouse is currently pregnant
            Friendship spouseFriendship = farmer.GetSpouseFriendship();
            if (spouseFriendship.DaysUntilBirthing >= 0 || spouseFriendship.NextBirthingDate != null)
            {
                if (verbose)
                    ModEntry.monitor.LogOnce("You need to wait until your previous child is born before you can have children.", LogLevel.Info);
                return false;
            }

            // Check heart level with spouse
            int heartLevelForNPC = farmer.getFriendshipHeartLevelForNPC(spouse.Name);
            if (heartLevelForNPC < 10)
            {
                if (verbose)
                    ModEntry.monitor.LogOnce("You need at least 10 hearts with your spouse before you can have children.", LogLevel.Info);
                return false;
            }

            // Check how long the farmer has been married
            if (farmer.GetDaysMarried() < 7)
            {
                if (verbose)
                    ModEntry.monitor.LogOnce("You must be married to your spouse for at least 7 days before you can have children.", LogLevel.Info);
                return false;
            }

            // Check how many children the farmer has so far
            List<Child> children = home.getChildren();
            if (children.Count >= maxChildren)
            {
                if (verbose)
                {
                    ModEntry.monitor.LogOnce("You currently have the maximum number of children allowed."
                        + " If you'd like to have more, use the set_max_children command.", LogLevel.Info);
                }
                return false;
            }

            // Check the ages of the children the farmer has so far
            /* If you have 0 children, skips straight to true */
            foreach (Child child in children)
            {
                if (child.Age < 3)
                {
                    if (verbose)
                        ModEntry.monitor.LogOnce("Your previous child needs to leave the crib before you can have more children.", LogLevel.Info);
                    return false;
                }
            }

            if (verbose)
                ModEntry.monitor.Log("Your spouse may ask for a child tonight...", LogLevel.Info);

            return true;
        }

        /* CheckPlayerSpouse - checks whether an player spouse can get pregnant
         * Replaces the logic in Utility.pickPersonalFarmEvent and in Utility.playersCanGetPregnantHere.
         * 
         * Instead of limiting children to 2 total, allows children up to a custom MaxChildren value.
         */
        private static bool CheckPlayerSpouse(Farmer otherFarmer, int maxChildren, bool verbose)
        {
            Farmer player = Game1.player;

            // Check that otherFarmer isn't current pregnant
            Friendship spouseFriendship = player.GetSpouseFriendship();
            if (spouseFriendship.DaysUntilBirthing >= 0 || spouseFriendship.NextBirthingDate != null)
            {
                if (verbose)
                    ModEntry.monitor.LogOnce("You need to wait until your previous child is born before you can have children.", LogLevel.Info);
                return false;
            }

            // Check that both players are in the same location
            GameLocation playerLocation = Game1.player.currentLocation;
            GameLocation otherLocation = otherFarmer.currentLocation;

            if (!otherLocation.Equals(playerLocation))
            {
                if (verbose)
                    ModEntry.monitor.LogOnce("Your spouse cannot ask for a child tonight because you aren't in the same place.", LogLevel.Info);
                return false;
            }

            // Check that both players are at one of their homes
            GameLocation home = Game1.getLocationFromName(Game1.player.homeLocation.Value);
            GameLocation otherHome = Game1.getLocationFromName(otherFarmer.homeLocation.Value);

            if (!otherLocation.Equals(otherHome) && !otherLocation.Equals(home))
            {
                if (verbose)
                    ModEntry.monitor.LogOnce("Your spouse cannot ask for a child tonight because you aren't at home together.", LogLevel.Info);
                return false;
            }

            // Check FarmHouse upgrade level
            FarmHouse farmHouse = otherLocation as FarmHouse;

            if (farmHouse.upgradeLevel < 2)
            {
                if (verbose)
                    ModEntry.monitor.LogOnce("You need to upgrade your house in order to have children.", LogLevel.Info);
                return false;
            }

            // Check how many children the farmer has so far
            List<Child> children = farmHouse.getChildren();
            if (children.Count >= maxChildren)
            {
                if (verbose)
                {
                    ModEntry.monitor.LogOnce("You currently have the maximum number of children allowed."
                        + " If you'd like to have more, use the set_max_children command.", LogLevel.Info);
                }
                return false;
            }

            // Check the ages of the children the farmer has so far
            /* If you have 0 children, skips straight to true */
            foreach (Child child in children)
            {
                if (child.Age < 3)
                {
                    if (verbose)
                        ModEntry.monitor.LogOnce("Your previous child needs to leave the crib before you can have more children.", LogLevel.Info);
                    return false;
                }
            }

            if (verbose)
                ModEntry.monitor.Log("Your spouse may ask for a child tonight...", LogLevel.Info);
            return true;
        }
    }
}