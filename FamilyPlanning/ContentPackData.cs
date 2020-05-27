using System.Collections.Generic;

namespace FamilyPlanning
{
    /* 
     * I'm expanding from a ChildSpriteData Content Pack to a generic ContentPackData.
     * This will support child sprites and will also support dialogue for your spouse.
     * Details on these are below.
     */ 

    class ContentPackData
    {
        /* SpriteNames:
         * -> The SpriteNames struct contains two strings.
         * -> The first string, BabySpriteName, is the file name for the baby sprites.
         * -> The second string, ToddlerSpriteName, is the file name for the toddler sprites.
         * -> A unique struct is created for each child.
         */

        public struct SpriteNames
        {
            public string BabySpriteName;
            public string ToddlerSpriteName;

            public SpriteNames(string babySpriteNameIn, string toddlerSpriteNameIn)
            {
                BabySpriteName = babySpriteNameIn;
                ToddlerSpriteName = toddlerSpriteNameIn;
            }
        }

        /* ChildSpriteID:
         * -> The ChildSpriteID data structure contains the data required to pair children with their sprites.
         * -> The first string, the key, is the name of the child.
         * -> The second string, the value, is the SpriteNames struct. It contains the file names for the baby and toddler sprites.
         */

        public Dictionary<string, SpriteNames> ChildSpriteID { get; set; }

        /* BirthDialogue:
         * -> The BirthDialogue struct contains an int and a string.
         * -> The int, BabyNumber, is which baby the dialogue belongs to. (Starts with 1 for the first child, 2 for second, etc.)
         * -> The string, Dialogue, is the dialogue line the spouse should say.
         */
        public struct BirthDialogue
        {
            public int BabyNumber;
            public string Dialogue;

            public BirthDialogue(int babyNumberIn, string dialogueIn)
            {
                BabyNumber = babyNumberIn;
                Dialogue = dialogueIn;
            }
        }

        /* BirthSpouseDialogue:
         * -> The BirthSpouseDialogue data structure contains dialogue information for the CustomBirthingEvent.
         * -> The first string, the key, is the name of the spouse. (This matches the spouse's displayName.)
         * -> The second string, the value, is the BirthDialogue struct. It contains the birth order number and the dialogue.
         *    The dialogue supports the use of {0} to represent the baby name and {1} to represent the player name.
         */

        public Dictionary<string, List<BirthDialogue>> SpouseDialogue { get; set; }

        public ContentPackData()
        {
            ChildSpriteID = new Dictionary<string, SpriteNames>();
            SpouseDialogue = new Dictionary<string, List<BirthDialogue>>();
        }

        /* The default .json file is organized as follows:
         * {
         *     "ChildSpriteID": {
         *         "<Child Name>": {
         *             "BabySpriteName": "<File location for the baby sprite>",
         *             "ToddlerSpriteName": "<File location for the toddler sprite>"
         *         },
         *         // You could insert more sprites for a different child here
         *     },
         *     "SpouseDialogue": {
         *         "<Name of spouse>": [
         *             {
         *                 "BabyNumber": <Number>,
         *                 "Dialogue": "<Dialogue text.>"
         *             },
         *             // You could insert more dialogue for this spouse here
         *         ],
         *         // You could insert dialogue for a different spouse here
         *     }
         * }
         */
    }
}