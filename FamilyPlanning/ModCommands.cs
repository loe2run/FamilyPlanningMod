using System;
using StardewModdingAPI;
using StardewValley;

namespace FamilyPlanning
{
    class ModCommands
    {
        /* Given by the ModEntry class to access the console */
        public static IMonitor Monitor;
        /* Given by the ModEntry class when registering commands */
        public static IModHelper helper;

        /* The description and usage messages for the mod commands */
        private readonly string[] descripts =
        { "Returns the maximum number of children you can have.\n\n",
          "Sets the maximum number of children you can have.\n"
                + "Warning: If you set the value to more than 4, children will overlap in bed and Content Patcher mods may not work.\n\n",
          "Returns the percentage chance that your spouse will ask to have a child.\n\n",
          "Sets the probability that your spouse will ask to have a child. The default chance is 5%.\n\n"
        };

        private readonly string[] usage = {
            "Usage: get_max_children\n"
                + "- This command requires no input.\n"
                + "Examples: get_max_children returns a value of 2: "
                + "If you have fewer than 2 children currently, your spouse may ask for a child. "
                + "If you have 2 children currently, your spouse will not ask for any more children.\n"
                + "          get_max_children returns a value of 0: Your spouse will never ask for a child.",
            "Usage: set_max_children <value>\n"
                + "- value: the maximum number of children you want to have.\n"
                + "Example: Load the save file you want to change, type \"set_max_children 4\" (without quotation marks), then hit enter.\n"
                + "  Now the maximum number of children you can have is 4 children.\n"
                + "  If you have fewer than 4 children currently, your spouse may ask for a child, "
                + "and if you have 4 children currently, your spouse will not ask for any more children.\n"
                + "Example: If you don't want to be asked for children, type \"set_max_children 0\" (without quotation marks), then hit enter.",
            "Usage: get_question_chance\n"
                + "- This command requires no input.\n"
                + "Examples: get_question_chance returns a value of 5: "
                + "If you've been married to your spouse for at least 7 days, upgraded your house to have a nursery, "
                + "have at least 10 hearts with your spouse, ",
            "Usage: set_question_chance <value>\n"
                + "- value: the percentage chance that your spouse will ask you for a baby (when it's possible for them to do so). " 
                + "Enter this as a whole number between 1 and 100, representing the percentage chance.\n"
                + "Examples: set_question_chance 100 => 100% chance.\n"
                + "          set_question_chance 5 => 5% chance." };

        /* ModCommands Constructor
         * Used to get a copy of the IMonitor from ModEntry for printing to console.
         */ 
        public ModCommands(IMonitor monitorIn, IModHelper helperIn)
        {
            Monitor = monitorIn;
            helper = helperIn;
        }

        /* RegisterCommands - registers the mod commands
         * The description and usage strings are class variables for ease of formatting and editing.
         */
        public void RegisterCommands()
        {
            try
            {
                helper.ConsoleCommands.Add("get_max_children", descripts[0] + usage[0], GetMaxChildren);
                helper.ConsoleCommands.Add("set_max_children", descripts[1] + usage[1], SetMaxChildren);
                helper.ConsoleCommands.Add("get_question_chance", descripts[2] + usage[2], GetBabyQuestionChance);
                helper.ConsoleCommands.Add("set_question_chance", descripts[3] + usage[3], SetBabyQuestionChance);
            }
            catch (Exception ex)
            {
                Monitor.Log("Family Planning failed while setting up console commands. Commands may not work correctly.\n", LogLevel.Error);
                Monitor.Log("Exception: " + ex.Message, LogLevel.Trace);
            }
        }

        /* GetMaxChildren - performs the get_max_children console command
         * 
         * This command uses the format "get_max_children" with no inputs.
         * 
         * Returns the current MaxChildren value for this save file.
         * If the MaxChildren value cannot be loaded, prints error message and aborts.
         */
        private void GetMaxChildren(string command, string[] args)
        {
            if (!IsCommandSafe())
                return;

            FamilyData data = ModEntry.GetFamilyData();
            if (data == null)
            {
                Monitor.Log("An error occurred when loading this save file's settings.\n", LogLevel.Error);
                Monitor.Log("Exception: ModEntry.GetFamilyData() returned null value.", LogLevel.Trace);
                return;
            }
            
            Monitor.Log("The current maximum number of children you can have is: " + data.MaxChildren + ".", LogLevel.Info);
            
            /* TODO: Explicit advice about current state of game */
            /* Less than this num: "your spouse will keep asking" vs. at this num: "Your spouse will not be asking." */
        }

        /* SetMaxChildren - performs the set_max_children console command
         * 
         * This command uses the format "set_max_children <value>" where <value> is the new MaxChildren value.
         * 
         * Sets the current MaxChildren value for this save file.
         * If the MaxChildren value cannot be set, prints error message and aborts.
         */
        private void SetMaxChildren(string command, string[] args)
        {
            if (!IsCommandSafe())
                return;

            // Verify that the player input "set_max_children <value>"
            if (args == null || args.Length < 1 || !int.TryParse(args[0], out int input) || input < 0)
            {
                // Print usage information and return
                Monitor.Log(usage[2], LogLevel.Info);
                return;
            }

            // Edit this save file's MaxChildren value
            FamilyData data = ModEntry.GetFamilyData();
            if (data == null)
            {
                Monitor.Log("An error occurred when loading this save file's settings.\n", LogLevel.Error);
                Monitor.Log("Exception: ModEntry.GetFamilyData() returned null value.", LogLevel.Trace);
                return;
            }

            try
            {
                data.MaxChildren = input;
                helper.Data.WriteJsonFile<FamilyData>("data/" + Constants.SaveFolderName + ".json", data);
            }
            catch (Exception ex)
            {
                Monitor.Log("An error occurred when editing this save file's settings.\n", LogLevel.Error);
                Monitor.Log("Exception: " + ex.Message, LogLevel.Trace);
                return;
                /* TODO: Reset the MaxChildren value if we failed at the writing stage. */
            }

            Monitor.Log("This save file's maximum number of children allowed is now set to " + input + ".\n", LogLevel.Info);
            /* TODO: Explicit advice about current state of game */
            /* Less than this num: "your spouse will keep asking" vs. at this num: "Your spouse will not be asking." */
        }

        /* GetBabyQuestionChance - performs the get_question_chance console command
         * 
         * This command uses the format "get_question_chance" with no inputs.
         * 
         * Returns the current BabyQuestionChance value for this save file.
         * If the BabyQuestionChance value cannot be loaded, prints error message instead.
         */
        public void GetBabyQuestionChance(string command, string[] args)
        {
            if (!IsCommandSafe())
                return;

            FamilyData data = ModEntry.GetFamilyData();
            if (data == null)
            {
                Monitor.Log("An error occurred when loading this save file's settings.\n", LogLevel.Error);
                Monitor.Log("Exception: ModEntry.GetFamilyData() returned null value.", LogLevel.Trace);
                return;
            }

            Monitor.Log("The current probability that your spouse will ask for a baby is: " + data.BabyQuestionChance + "%.\n", LogLevel.Info);
            /* TODO: Explicit advice about current state of game */
            /* Less than this num: "your spouse will keep asking" vs. at this num: "Your spouse will not be asking." */
        }

        /* SetBabyQuestionChance - performs the set_question_chance console command
         * 
         * This command uses the format "set_question_chance <value>" where <value> is the new BabyQuestionChance value.
         * 
         * Sets the current BabyQuestionChance value for this save file.
         * If the BabyQuestionChance value cannot be set, prints error message instead.
         */
        public void SetBabyQuestionChance(string command, string[] args)
        {
            if (!IsCommandSafe())
                return;

            // Verify that the player input "set_question_chance <value>"
            if (args == null || args.Length < 1 || !int.TryParse(args[0], out int input) || input < 0 || input > 100)
            {
                // Print usage information and return
                Monitor.Log(usage[4], LogLevel.Info);
                return;
            }

            // Edit this save file's MaxChildren value
            FamilyData data = ModEntry.GetFamilyData();
            if (data == null)
            {
                Monitor.Log("An error occurred when loading this save file's settings.\n", LogLevel.Error);
                Monitor.Log("Exception: ModEntry.GetFamilyData() returned null value.", LogLevel.Trace);
                return;
            }

            try
            {
                data.BabyQuestionChance = input;
                helper.Data.WriteJsonFile<FamilyData>("data/" + Constants.SaveFolderName + ".json", data);
            }
            catch (Exception ex)
            {
                Monitor.Log("An error occurred when editing this save file's settings.\n", LogLevel.Error);
                Monitor.Log("Exception: " + ex.Message, LogLevel.Trace);
                return;
                /* TODO: Reset the BabyQuestionChance value if we failed at the writing stage. */
            }

            Monitor.Log("For this save file, the probability that your spouse will ask for a baby is now set to " + input + "%.\n", LogLevel.Info);
            /* TODO: Explicit advice about current state of game */
            /* Less than this num: "your spouse will keep asking" vs. at this num: "Your spouse will not be asking." */
            Monitor.Log("BabyQuestionDouble is now " + (input / 100.0), LogLevel.Trace);
        }

        /* IsCommandSafe - checks if it's safe to use the given command right now
         * 
         * Many of these commands have similar requirements, so this method will check for basic requirements
         * and print error messages for the player if the command shouldn't be run right now.
         */
        private bool IsCommandSafe()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("A save must be fully loaded to use this command.", LogLevel.Info);
                return false;
            }
            if (Game1.farmEvent != null)
            {
                Monitor.Log("An event is in progress, please wait until it is finished to use this command.", LogLevel.Info);
                return false;
            }

            return true;
        }
    }
}
