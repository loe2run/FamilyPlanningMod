using System;
using StardewModdingAPI;
using StardewValley;

namespace FamilyPlanning
{
    /* The ModCommands class holds the code responsible for console commands.
     * 
     * Current console commands: 
     * get_max_children, set_max_children, get_question_chance, set_question_chance.
     */
    
    class ModCommands
    {
        /* Given by the ModEntry class to access the console */
        public static IMonitor Monitor;
        /* Given by the ModEntry class when registering commands */
        public static IModHelper helper;

        /* The description and usage messages for the mod commands */
        private readonly string[] descripts =
        {
            // get_max_children
            "Returns the maximum number of children you can have.\n",
            // set_max_children
            "Sets the maximum number of children you can have.\n" +
            "Warning: If you set the value to more than 4, children will overlap in bed and Content Patcher mods may not work.\n",
            // get_question_chance
            "Returns the percentage chance that your spouse will ask to have a child.\n",
            // set_question_chance
            "Sets the probability that your spouse will ask to have a child. The default chance is 5%.\n"
        };
        private readonly string[] usage = 
        {
            // get_max_children
            "Usage: get_max_children\n" +
            "- This command requires no input.\n" +
            "Example: get_max_children returns a value of 2: " +
            "If you have fewer than 2 children currently, your spouse may ask for a child. " +
            "If you have 2 children currently, your spouse will not ask for any more children.\n" +
            "Example: get_max_children returns a value of 0: Your spouse will never ask for a child.\n",
            // set_max_children
            "Usage: set_max_children <value>\n" +
            "- value: the maximum number of children you want to have.\n" +
            "Example: Load the save file you want to change, type \"set_max_children 4\" (without quotation marks), then hit enter.\n" +
            "  Now the maximum number of children you can have is 4 children.\n" +
            "  If you have fewer than 4 children currently, your spouse may ask for a child, " +
            "and if you have 4 children currently, your spouse will not ask for any more children.\n" +
            "Example: If you don't want to be asked for children, type \"set_max_children 0\" (without quotation marks), then hit enter.\n",
            // get_question_chance
            "Usage: get_question_chance\n" +
            "- This command requires no input.\n" +
            "Examples: get_question_chance returns a value of 5: " +
            "If you meet the requirements to have a child, your spouse has a 5% chance to ask you to have/adopt a child.\n",
            // set_question_chance
            "Usage: set_question_chance <value>\n" +
            "- value: the percentage chance that your spouse will ask you for a baby (when it's possible for them to do so). " +
            "Enter this as a whole number between 1 and 100, representing the percentage chance.\n" +
            "Example: set_question_chance 100 => 100% chance, your spouse will always ask to have/adopt a child (if you meet the requirements).\n" +
            "Example: set_question_chance 5 => 5% chance your spouse will ask to have/adopt a child (if you meet the requirements).\n" +
            "Example: set_question_chance 0 => 0% chance, your spouse will never ask to have a child.\n"
        };

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
                Monitor.Log("Family Planning failed while setting up console commands. Commands may not work correctly.", LogLevel.Error);
                Monitor.Log("Exception: " + ex.Message, LogLevel.Trace);
            }
        }

        /* IsCommandSafe - checks if it's safe to use the given command right now
         * 
         * These commands have similar requirements, so this method will check the basic world state
         * and print error messages for the player if commands shouldn't be run right now.
         */
        private bool IsCommandSafe()
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("A save must be fully loaded to use this command, please wait.", LogLevel.Info);
                return false;
            }
            if (Game1.farmEvent != null)
            {
                Monitor.Log("An event is in progress, please wait until it is finished to use this command.", LogLevel.Info);
                return false;
            }

            return true;
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

            // Load the current MaxChildren value for this save file
            FamilyData data = ModEntry.GetFamilyData();
            if (data == null)
            {
                Monitor.Log("An error occurred when loading this save file's settings.\n", LogLevel.Error);
                Monitor.Log("Exception: ModEntry.GetFamilyData() returned null value.", LogLevel.Trace);
                return;
            }

            // Print out verbose messages based on the new value
            Monitor.Log("The current maximum number of children you can have is: " + data.MaxChildren + ".", LogLevel.Info);
            MaxChildrenMessage(data.MaxChildren);
            Monitor.Log("If you'd like to change your MaxChildren value, use the set_max_children command.", LogLevel.Info);
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

            // Load this save file's MaxChildren value
            FamilyData data = ModEntry.GetFamilyData();
            if (data == null)
            {
                Monitor.Log("An error occurred when loading this save file's settings.\n", LogLevel.Error);
                Monitor.Log("Exception: ModEntry.GetFamilyData() returned null value.", LogLevel.Trace);
                return;
            }

            // Update the .json for this save file
            try
            {
                data.MaxChildren = input;
                helper.Data.WriteJsonFile<FamilyData>("data/" + Constants.SaveFolderName + ".json", data);
            }
            catch (Exception ex)
            {
                Monitor.Log("An error occurred when editing this save file's settings.\n" +
                            "The new MaxChildren value will work while the game remains open, " +
                            "but if you close and re-open this save file, your new settings will be lost.", LogLevel.Error);
                Monitor.Log("Exception: " + ex.Message, LogLevel.Trace);
                return;
            }

            // Print out verbose messages based on the new value
            Monitor.Log("This save file's maximum number of children allowed is now set to " + input + ".", LogLevel.Info);
            MaxChildrenMessage(data.MaxChildren);
        }

        /* MaxChildrenMessage - used by GetMaxChildren and SetMaxChildren to provide verbose messages
         * 
         * When the MaxChildren value is checked/changed by either of these commands,
         * look at the current save file's state and print verbose messages about what this value means.
         */ 
        private void MaxChildrenMessage(int MaxChildren)
        {
            // MaxChildren of 0 can be intentional, check this first before checking for mistakes
            if (MaxChildren == 0)
            {
                Monitor.Log("A MaxChildren value of 0 means that you will never be asked about children.", LogLevel.Info);
                return;
            }

            // Try to load current save file information
            int count = -1;
            string spouse = null;
            try
            {
                count = Game1.player.getNumberOfChildren();
                spouse = Game1.player.spouse;
            }
            catch (Exception) { }

            // If either of this is out of bounds, don't worry about verbose messaging
            if (count < 0 || spouse == null)
                return;

            // Check if spouse is roommate & roommate config is set
            if (Game1.player.isRoommate(spouse) && !ModEntry.RoommateConfig())
            {
                Monitor.Log(spouse + " is your roommate, but your AdoptChildrenWithRoommate config is false.", LogLevel.Warn);
                Monitor.Log("If this is intentional, you can freely ignore this warning.", LogLevel.Info);
                Monitor.Log("If this was a mistake and you'd like to adopt children with " + spouse + ", " +
                            "go to the Family Planning config and set AdoptChildrenWithRoommate to true.", LogLevel.Info);
                return;
            }

            // Change the message based on whether or not they are going to "have" or "adopt" children
            bool adopt = Game1.player.getSpouse().isGaySpouse();

            // Handle the special case message first (don't have any children, MaxChildren > 0)
            if (count == 0)
            {
                if (adopt)
                    Monitor.Log("You don't currently have any children, so " + spouse + " may ask to adopt a child.");
                else
                    Monitor.Log("You don't currently have any children, so " + spouse + " may ask to have a child.");
            }
            // Check if they've already hit the MaxChildren limit
            else if (count >= MaxChildren)
            {
                // Current child count message
                string statusMessage = "You already have a child and your MaxChildren value is 1, ";
                if (count != 1)
                    statusMessage = "You already have " + count + " children and your MaxChildren value is " + MaxChildren + ", ";

                // Won't ask for child message
                string wontAskMessage = "so " + spouse + " will never ask to have a child.";
                if (adopt)
                    wontAskMessage = "so " + spouse + " will never ask to adopt a child.";

                // Print the warning
                Monitor.Log(statusMessage + wontAskMessage, LogLevel.Warn);
                Monitor.Log("If this is intentional, you can freely ignore this warning.", LogLevel.Info);
            }
            else
            {
                // Current child count message
                string statusMessage = "You currently have 1 child, ";
                if (count != 1)
                    statusMessage = "You currently have " + count + " children, ";

                // Will ask for child message
                string willAskMessage = "so " + spouse + " may ask to have more children.";
                if (adopt)
                    willAskMessage = "so " + spouse + " may ask to adopt more children.";

                // Print the message
                Monitor.Log(statusMessage + willAskMessage, LogLevel.Info);

                // Warn the player if they want to have more than 4 children
                if (MaxChildren > 4)
                {
                    Monitor.Log("Warning: Having more than 4 children will lead to issues with this mod, " +
                                "like children overlapping in bed and Content Patcher packs not working.", LogLevel.Warn);
                    Monitor.Log("If you are fine with those problems occuring, you can ignore this warning.", LogLevel.Info);
                }
            }
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

            Monitor.Log("The current probability that your spouse will ask for a baby is: " + data.BabyQuestionChance + "%.", LogLevel.Info);
            Monitor.Log("If you'd like to change this value, use the set_question_chance command.", LogLevel.Info);
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

            // Load this save file's MaxChildren value
            FamilyData data = ModEntry.GetFamilyData();
            if (data == null)
            {
                Monitor.Log("An error occurred when loading this save file's settings.", LogLevel.Error);
                Monitor.Log("Exception: ModEntry.GetFamilyData() returned null value.", LogLevel.Trace);
                return;
            }

            // Edit the MaxChildren value
            data.BabyQuestionChance = input;
            Monitor.Log("For this save file, the probability that your spouse will ask for a baby is now set to " + input + "%.", LogLevel.Info);
            Monitor.Log("BabyQuestionDouble is now " + (input / 100.0), LogLevel.Trace);

            // Update the .json for this save file
            try
            {
                helper.Data.WriteJsonFile<FamilyData>("data/" + Constants.SaveFolderName + ".json", data);
            }
            catch (Exception ex)
            {
                Monitor.Log("An error occurred when editing this save file's settings.", LogLevel.Error);
                Monitor.Log("The new MaxChildren value will work while the game remains open, " +
                            "but if you close and re-open this save file, your new settings will be lost.", LogLevel.Error);
                Monitor.Log("Exception: " + ex.Message, LogLevel.Trace);
                return;
            }

            // Try to provide explicit advice about the current state of the game
            if (data.BabyQuestionChance == 0)
            {
                Monitor.Log("By setting your BabyQuestionChance value to 0, your spouse will never ask to have children.", LogLevel.Info);
                return;
            }

            // Check if they can't currently have children for a warning
            int numChildren = -1;
            string spouseName = null;
            try
            {
                numChildren = Game1.player.getNumberOfChildren();
                spouseName = Game1.player.spouse;
            }
            catch (Exception) { }

            if (numChildren < 0 || spouseName == null)
                return;

            // Check if spouse is roommate & roommate config is set
            if (Game1.player.isRoommate(spouseName) && !ModEntry.RoommateConfig())
            {
                Monitor.Log(spouseName + " is your roommate, but your AdoptChildrenWithRoommate config is false.", LogLevel.Warn);
                Monitor.Log("If this is intentional, you can freely ignore this warning.", LogLevel.Info);
                Monitor.Log("If this was a mistake and you'd like to adopt children with " + spouseName + ", " +
                            "go to the Family Planning config and set AdoptChildrenWithRoommate to true.", LogLevel.Info);
                return;
            }

            // Check if they've already hit the limit for children
            bool adoptMessage = Game1.player.getSpouse().isGaySpouse();
            if (numChildren >= data.MaxChildren)
            {
                string statusMessage = "You already have " + numChildren + " children and your MaxChildren value is " + data.MaxChildren + ", ";
                if (data.MaxChildren == 0)
                    statusMessage = "Currently, your MaxChildren value is 0, ";
                else if (numChildren == 1)
                    statusMessage = "You already have a child and your MaxChildren value is 1, ";

                string wontAskMessage = "so " + spouseName + " will never ask to have a child.";
                if (adoptMessage)
                    wontAskMessage = "so " + spouseName + " will never ask to adopt a child.";

                Monitor.Log("Warning: Did you mean to change your question chance even though your spouse will never ask about a child?", LogLevel.Warn);
                Monitor.Log(statusMessage + wontAskMessage, LogLevel.Warn);
                Monitor.Log("If this is intentional, you can freely ignore this warning.", LogLevel.Info);
                Monitor.Log("If this was a mistake, use the set_max_children command.", LogLevel.Info);
                return;
            }
        }
    }
}