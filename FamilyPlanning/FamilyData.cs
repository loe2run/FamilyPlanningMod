using StardewValley;

namespace FamilyPlanning
{
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