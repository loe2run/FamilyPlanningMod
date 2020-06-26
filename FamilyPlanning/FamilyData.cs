using StardewValley;

namespace FamilyPlanning
{
    /* *** Save file config options ***
     * 
     * MaxChildren:
     * -> The "MaxChildren" value is saved in the file "FamilyPlanning/data/<save file name>.json" for each save file.
     *    This value can be customized per save file, so different save files can have different settings.
     * -> The player can change this value by either using the console command "set_max_children"
     *    or by editing the "MaxChildren" field in the appropriate .json file.
     * -> The "MaxChildren" value controls the maximum number of children that a spouse will ask for.
     *    MaxChildren = 0 -> spouse will never ask to have a child.
     *    MaxChildren = 1 -> they get the question until they have the first child, then don't get it again.
     *    MaxChildren = 2 -> normal Stardew Valley behavior, this is the default value.
     *    MaxChildren = 3 -> they get the question until they have 3 children, then don't get it again.
     *    MaxChildren = 4 -> they get the question until they have 4 children, then don't get it again.
     *    MaxChildren = 5+ -> they continue to get the question until they reach the maximum number,
     *                        but mod features like children sharing beds/CP mod custom sprites will not work properly.
     *
     * BabyQuestionChance:
     * -> The "BabyQuestionChance" value is saved in the file "FamilyPlanning/data/<save file name>.json" for each save file.
     *    This value can be customized per save file, so different save files can have different settings.
     * -> The player can change this value by either using the console command "set_question_chance"
     *    or by editing the "BabyQuestionChance" field in the appropriate .json file.
     * -> Normally in Stardew Valley, if you meet all the requirements to be able to have a child,
     *    every night there's a chance that your spouse will ask if you want to have a child.
     *    Normally in Stardew Valley, that chance is 5%.
     * -> The "BabyQuestionChance" value allows you to control that percentage chance.
     *    You can set "BabyQuestionChance" to any value between 0 and 100, but here are a few examples:
     *    BabyQuestionChance = 0   -> 0% chance, you will never be asked by your spouse to have a child.
     *    BabyQuestionChance = 5   -> 5% chance, this is the default value.
     *    BabyQuestionChance = 25  -> 25% chance, much more likely than normal.
     *    BabyQuestionChance = 100 -> 100% chance, your spouse will always ask to have a child.
     * -> Note: this only changes the chance that your spouse will ask to have a child if you meet the requirements to have a child.
     *    (For more information on the requirements, check out https://stardewvalleywiki.com/Children)
     * -> The requirements include:
     *    Having the house upgrade that gives you a nursery/upstairs (house upgrade 2),
     *    being married to your spouse for at least 7 days, having at least 10 hearts with your spouse,
     *    NOT currently waiting for a previous child to be born/adopted,
     *    NOT currently having a previous child in the crib (younger than toddler),
     *    NOT currently having the maximum number of children allowed. (See "MaxChildren" above.)
     */

    class FamilyData
    {
        public int MaxChildren { get; set; }
        public int BabyQuestionChance { get; set; }

        public FamilyData()
        {
            if(Game1.player.getChildrenCount() > 2)
                MaxChildren = Game1.player.getChildrenCount();
            else
                MaxChildren = 2;

            BabyQuestionChance = 5;
        }
    }
}