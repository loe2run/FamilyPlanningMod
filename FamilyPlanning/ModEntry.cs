using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Events;
using StardewValley.Characters;
using StardewValley.Locations;
using Harmony;

/* TODO:
 * -> Check if multiplayer works
 * -> Implement multiplayer birthing event/naming menu stuff
 */

namespace FamilyPlanning
{
    /* Family Planning: 
     * This mod allows players to customize the number of children they have and their genders,
     * modify the chance of a spouse asking for a child, with or without console messages,
     * and allows them to adopt children with a roommate. (In Vanilla, the only roommate is Krobus.)
     * 
     * *** Global config options ***
     * -> Config Option: AdoptChildrenWithRoommate (default is false)
     *    If you set this value to true, then your roommate will prompt you to adopt a child.
     * -> Note: If you're using a mod that turns Krobus (or any other roommate) into a normal marriage candidate, like Krobus Marriage Mod,
     *    then you don't need this setting to be true, Krobus will be treated like a normal spouse automatically.
     * -> Note: Because this is a global config option, if you set it to true, then all save files will be affected.
     *    If you'd like to have some save files where Krobus will adopt children, and some where he will not,
     *    I would recommand using "set_max_children 0" on the save files where you don't want to adopt children.
     *    This will stop Krobus from asking to adopt a child, even if you have the config set to true.
     * 
     * -> Config Option: BabyQuestionMessages (default is false)
     *    If you set this value to true, then you will get messages in the SMAPI console each night
     *    about whether your spouse is able to ask you for a child.
     *    This can help if you're unsure whether your spouse hasn't asked because you don't meet a requirement,
     *    or if you're just getting unlucky repeatedly.
     * 
     * *** Save file config options ***
     * Customizing the number of children:
     * -> The player enters the number of children they want through a console command,
     *    or by editing the "MaxChildren" field in the file "FamilyPlanning/data/<save file name>.json".
     * -> This value can be customized per save file, so different save files can have different settings.
     * -> Using the console command:
     *    "set_max_children 0" -> they never get the question.
     *    "set_max_children 1" -> they only get the question until they have the first child, then don't get it again.
     *    "set_max_children 2" -> vanilla behavior.
     *    "set_max_children 3" or
     *    "set_max_children 4" -> they get the question event after having 2 children, until the limit (3 or 4).
     *    "set_max_children 5" or higher than 5 -> they will continue to get the question until they hit the limit,
     *      but mod features like where children sleep/custom sprites will not work properly.
     * 
     * Customizing the gender of children:
     * -> The player is given the option to customize the gender of the child at birth when you name them.
     *    Just click the male icon to have a male child, and the female icon to have a female child.
     * 
     * Changing the chance of your spouse asking for a child:
     * -> Every night, if it is possible for your spouse to do so, there is a chance they will ask if you want to have a child.
     *    Normally in Stardew Valley, the chance is 5%.
     * -> The player can change the chance of their spouse asking for a child through a console command,
     *    or by editing the "BabyQuestionChance" field in the file "FamilyPlanning/data/<save file name>.json".
     * -> This value can be customized per save file, so different save files can have different settings.
     * -> Using the console command:
     *    Enter a whole number from 1 to 100, representing the percentage chance of your spouse asking the question.
     *    "set_question_chance 0"   -> there is a 0% your spouse will ask, you will never get the question.
     *    "set_question_chance 5"   -> there is a 5% chance your spouse will ask, same chance as normal.
     *    "set_question_chance 25"  -> there is a 25% chance your spouse will ask, much more likely than normal.
     *    "set_question_chance 100" -> there is a 100% chance your spouse will ask, your spouse will always ask.
     * -> Note: this only changes the chance that your spouse will ask to have a child if it's possible for them to ask.
     *    (For more information on the requirements, check out https://stardewvalleywiki.com/Children)
     *    The requirements include:
     *    Having the house upgrade that gives you a nursery/upstairs (house upgrade 2),
     *    being married to your spouse for at least 7 days, having at least 10 hearts with your spouse,
     *    NOT currently waiting for a previous child to be born,
     *    NOT currently having a previous child in the crib (younger than toddler),
     *    NOT currently having the maximum number of children allowed. (See "Customizing the number of children" above.)
     */

    /* Harmony patches:
     * -> StardewValley.Utility.pickPersonalFarmEvent() -> control baby question events & writes console messages by config
     * -> StardewValley.NPC.isGaySpouse() -> ensures that roommates are given adoption dialogue
     * -> StardewValley.Characters.Child.reloadSprite() -> allows custom sprites for children
     * -> StardewValley.Characters.Child.dayUpdate() -> changes the bed position for toddlers
     * -> StardewValley.Characters.Child.tenMinuteUpdate() -> changes the bed position for toddlers 
     */

    /* Content Packs:
     * Instructions for how to make a Content Pack are in the README.md on GitHub.
     */

    class ModEntry : Mod
    {
        /* The list of the content packs for this mod */
        private static List<IContentPack> contentPacks;

        /* An individual save file's settings for this mod */
        private static FamilyData data;

        /* The global configuration for this mod */
        private ModConfig config;
        /* The configured values */
        private static bool AdoptChildrenWithRoommate;
        private static bool BabyQuestionMessages;

        /* SMAPI objects */
        public static IMonitor monitor;
        public static IModHelper helper;
        
        /* Used to reload the child sprites */
        private bool firstTick = true;

        /* Used to create the CP custom child tokens */
        private readonly int MaxTokens = 4;
        private readonly string[] nameTokens = { "FirstChildName", "SecondChildName", "ThirdChildName", "FourthChildName" };
        private readonly string[] ageTokens = { "FirstChildIsToddler", "SecondChildIsToddler", "ThirdChildIsToddler", "FourthChildIsToddler" };
        private ChildToken[] tokens;

        /* entry - the entry method for a SMAPI mod 
         *
         * Loads the config and adds console commands/event handlers for SMAPI,
         * patches the original Stardew Valley methods using Harmony.
         */
        public override void Entry(IModHelper helper)
        {
            // Set the global variables
            monitor = Monitor;
            ModEntry.helper = helper;
            
            // Load the config info from file
            config = helper.ReadConfig<ModConfig>();
            AdoptChildrenWithRoommate = config.AdoptChildrenWithRoommate;
            BabyQuestionMessages = config.BabyQuestionMessages;

            // Initialize save file data to null for now
            data = null;

            // Initialize the CP tokens list
            tokens = new ChildToken[MaxTokens];

            // Add the console commands
            ModCommands commander = new ModCommands(monitor, helper);
            commander.RegisterCommands();

            // Add the event handlers
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            
            // Load content packs
            contentPacks = new List<IContentPack>();
            foreach (IContentPack contentPack in helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                contentPacks.Add(contentPack);
            }

            // Use Harmony to patch the original methods
            HarmonyInstance harmony = HarmonyInstance.Create("Loe2run.FamilyPlanning");
            
            // Child.reloadSprite (postfix)
            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.reloadSprite)),
               postfix: new HarmonyMethod(typeof(Patches.ChildReloadSpritePatch), nameof(Patches.ChildReloadSpritePatch.Postfix))
            );
            // Child.tenMinuteUpdate (postfix)
            harmony.Patch(
               original: AccessTools.Method(typeof(Child), nameof(Child.tenMinuteUpdate)),
               postfix: new HarmonyMethod(typeof(Patches.ChildTenMinuteUpdatePatch), nameof(Patches.ChildTenMinuteUpdatePatch.Postfix))
            );
            // Child.dayUpdate (postfix)
            harmony.Patch(
                original: AccessTools.Method(typeof(Child), nameof(Child.dayUpdate)),
                postfix: new HarmonyMethod(typeof(Patches.ChildDayUpdatePatch), nameof(Patches.ChildDayUpdatePatch.Postfix))
            );
            // NPC.isGaySpouse (postfix)
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.isGaySpouse)),
                postfix: new HarmonyMethod(typeof(Patches.IsGaySpousePatch), nameof(Patches.IsGaySpousePatch.Postfix))
            );
            // Utility.pickPersonalFarmEvent (postfix)
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
                postfix: new HarmonyMethod(typeof(Patches.PickPersonalFarmEventPatch), nameof(Patches.PickPersonalFarmEventPatch.Postfix))
            );
        }

        /* OnGameLaunched - register the custom content patcher tokens
         * 
         * Uses the Content Patcher API to register new tokens for this mod.
         * These tokens are used to represent the child's name and age for use in CP packs.
         */
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Try to load the api
            IContentPatcherAPI api = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (api == null)
            {
                monitor.Log("Family Planning failed to load the Content Patcher API. CP packs which use Family Planning will not work.", LogLevel.Warn);
                return;
            }

            // Register Content Patcher custom tokens for children
            ChildToken token;
            for (int i = 0; i < MaxTokens; i++)
            {
                token = new ChildToken(i + 1);
                api.RegisterToken(ModManifest, nameTokens[i], token.GetChildName);
                api.RegisterToken(ModManifest, ageTokens[i], token.GetChildIsToddler);
                tokens[i] = token;
            }
        }

        /* OnUpdateTicked - replace the default BirthingEvent
         * 
         * If a BirthingEvent is occuring, replaces it with this mod's CustomBirthingEvent.
         */
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.farmEvent != null && Game1.farmEvent is BirthingEvent)
            {
                Game1.farmEvent = new CustomBirthingEvent();
                Game1.farmEvent.setUp();
            }
        }

        /* OnOneSecondUpdateTicked - initialize the custom child sprites
         * 
         * Triggers the child sprites to reload when the world is first loaded
         * to update the sprites from the game's default to the custom sprites.
         */
        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!firstTick || !Context.IsWorldReady)
                return;

            try
            {
                List<Child> children = Game1.player.getChildren();
                foreach (Child child in children)
                    child.reloadSprite();
                firstTick = false;
            }
            catch (Exception) { }
        }

        /* OnSaveLoaded - loads mod data from file when save is loaded
         * 
         * Loads the configuration data for this save file from "data/{SaveFolderName}.json" in the mod folder.
         * If the .json isn't there, creates a new default file and saves it there.
         */
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // Load the data/<save name>.json file
            try
            {
                data = Helper.Data.ReadJsonFile<FamilyData>("data/" + Constants.SaveFolderName + ".json");
            } 
            catch (Exception) { }

            if (data == null)
            {
                // Create a new data/<save name>.json file
                try
                {
                    data = new FamilyData();
                    Helper.Data.WriteJsonFile("data/" + Constants.SaveFolderName + ".json", data);
                }
                catch (Exception ex)
                {
                    Monitor.Log("Family Planning failed to create a new config file at data/" + Constants.SaveFolderName + ".json", LogLevel.Error);
                    Monitor.Log("Exception: " + ex.Message, LogLevel.Trace);
                }
            }
        }

        /* OnReturnedToTitle - clears save-specific data for this mod
         * 
         * Resets global variables to initial values to prepare for loading a new save file.
         */
        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            // Reset to the first tick the new save file is loaded
            firstTick = true;

            // Clears the FamilyData settings from the other save file
            data = null;

            // Clears the CP token data from the other save file
            foreach (ChildToken token in tokens)
                token.ClearToken();
        }

        /* OnDayStarted - initializes and updates cached CP token values daily
         *
         * Prior to this method being run, CP tokens are registered but not initialized.
         * Once the day starts, child data is available for tokens to cache.
         */
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (ChildToken token in tokens)
            {
                if (!token.IsInitialized())
                    token.InitializeToken();
                else
                    token.UpdateToken();
            }
        }

        /* GetChildSpriteData - finds the content pack child sprite asset key
         * 
         * Looks through the content packs and tries to find a patch for this child's sprite,
         * then loads the asset key for use in the game.
         */
        public static string GetChildSpriteData(string childName, int childAge)
        {
            ContentPackData cpdata;
            foreach (IContentPack contentPack in contentPacks)
            {
                try
                {
                    cpdata = contentPack.ReadJsonFile<ContentPackData>("assets/data.json");
                    if (cpdata.ChildSpriteID.TryGetValue(childName, out ContentPackData.SpriteNames spriteNames))
                    {
                        if (childAge >= 3)
                            return contentPack.GetActualAssetKey(spriteNames.ToddlerSpriteName);
                        else
                            return contentPack.GetActualAssetKey(spriteNames.BabySpriteName);
                    }
                }
                catch (Exception e)
                {
                    monitor.Log("An exception occurred in Loe2run.FamilyPlanning while loading child sprites.", LogLevel.Debug);
                    monitor.Log(e.Message, LogLevel.Debug);
                }
            }
            return null;
        }

        /* GetSpouseDialogueData - finds the content pack spouse dialogue
         * 
         * Looks through the content packs and tries to find a patch for this spouse's dialogue,
         * then loads that dialogue for use in the game.
         */
        public static List<ContentPackData.BirthDialogue> GetSpouseDialogueData(string spouseName)
        {
            ContentPackData cpdata;
            foreach (IContentPack contentPack in contentPacks)
            {
                try
                {
                    cpdata = contentPack.ReadJsonFile<ContentPackData>("assets/data.json");
                    if (cpdata.SpouseDialogue.TryGetValue(spouseName, out List<ContentPackData.BirthDialogue> spouseDialogue))
                        return spouseDialogue;
                }
                catch (Exception e)
                {
                    monitor.Log("An exception occurred in Loe2run.FamilyPlanning while loading spouse dialogue.", LogLevel.Debug);
                    monitor.Log(e.Message, LogLevel.Debug);
                }
            }
            return null;
        }

        /* GetFamilyData - returns the private FamilyData data variable */
        public static FamilyData GetFamilyData()
        {
            return data;
        }

        /* RoommateConfig - returns the private ModConfig config.AdoptChildrenWithRoommate value */
        public static bool RoommateConfig()
        {
            return AdoptChildrenWithRoommate;
        }

        /* MessagesConfig - returns the private ModConfig config.BabyQuestionMessages value */
        public static bool MessagesConfig()
        {
            return BabyQuestionMessages;
        }

        /* GetChildBed - override for farmHouse.getChildBed used by my Harmony patch methods
         * 
         * This method is used by two different patches, so they access the method through the ModEntry class.
         * 
         * For the first two children, this method behaves nearly the same as vanilla,
         * with the exception that positions are determined by birth order instead of gender.
         * 
         * After two children, child 3 will try to share a bed with a like-gender sibling,
         * and child 4 will use whatever the last open spot is.
         * 
         * If there are more than 4 children, child 5+ will end up overlapping with child 4.
         */
        public static Point GetChildBed(Child child, FarmHouse farmHouse)
        {
            // To make this method more readable
            Point bed1 = new Point(23, 5); // Right side of bed 1 (left bed)
            Point share1 = new Point(22, 5); // Left side of bed 1 (left bed)
            Point bed2 = new Point(27, 5); // Right side of bed 2 (right bed)
            Point share2 = new Point(26, 5); // Left side of bed 2 (right bed)

            // If loading child list fails, default to first bed position.
            List<Child> children = farmHouse.getChildren();
            if (children == null)
            {
                monitor.Log("Failure in GetChildBed, cannot get child list.", LogLevel.Trace);
                return bed1;
            }

            // Get birth order for this child & number of children using beds
            int childCount = children.Count;
            int toddler = 0;
            int childNum = 1;

            Child c;
            for (int i = 0; i < childCount; i++)
            {
                c = children[childCount];
                if (c.Age >= 3)
                    toddler++;
                if (c.Equals(child))
                    childNum = i + 1;
            }

            // Child1 always gets right side of bed 1
            if (childNum == 1)
                return bed1;

            // If only two toddlers, Child1 gets bed 1 and Child2 gets bed 2
            if (toddler == 2)
                return bed2;

            // More than 2 kids and first two share gender
            if (children[0].Gender == children[1].Gender)
            {
                // Child1 and Child2 share bed 1
                if (childNum == 2)
                    return share1;
                // Child3 and Child4 share bed 2
                else if (childNum == 3)
                    return bed2;
                return share2;
            }

            // More than 2 kids and first two don't share gender, Child1 gets bed 1 and Child2 gets bed 2
            if (childNum == 2)
                return bed2;

            // More than 2 kids and Child2 and Child3 share gender
            if (children[1].Gender == children[2].Gender)
            {
                // Child2 and Child3 share bed 2
                if (childNum == 3)
                    return share2;
                // Child1 and Child4 share bed 1
                return share1;
            }

            // More than 2 kids, Child1 and Child2 can't share, Child2 and Child3 can't share
            // Child1 and Child3 share bed 1
            if (childNum == 3)
                return share1;

            // Child2 and Child4 share bed 2
            return share2;
        }
    }
}