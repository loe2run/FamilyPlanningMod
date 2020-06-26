using System;
using System.Collections.Generic;
using Netcode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Events;
using StardewValley.Characters;
using StardewValley.BellsAndWhistles;

namespace FamilyPlanning
{
    /*
     * The vast majority of this code is the same as the original BirthingEvent.
     * That's the cause of all the unused variables, etc.
     * 
     * The main difference is that this event loads the CustomNamingMenu, not the normal NamingMenu,
     * and that it adds new logic for handling dialogue past 2 children,
     * including the option to load custom dialogue from a Family Planning content pack.
     */

    class CustomBirthingEvent : FarmEvent, INetObject<NetFields>
    {
        // private int behavior;
        private int timer;
        // private string soundName;
        private string message;
        private string babyName;
        // private bool playedSound;
        // private bool showedMessage;
        private bool isMale;
        private bool getBabyName;
        private bool naming;
        // private Vector2 targetLocation;
        // private TextBox babyNameBox;
        // private ClickableTextureComponent okButton;
        public NetFields NetFields { get; } = new NetFields();

        public bool setUp()
        {
            //Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
            NPC spouse = Game1.getCharacterFromName(Game1.player.spouse, true);
            Game1.player.CanMove = false;
            
            // CustomNamingMenu allows the player to choose gender, and male is the default.
            isMale = true;

            // I'm removing the gender references in the message text, otherwise message would always imply male.
            string genderTerm = Lexicon.getGenderedChildTerm(isMale);
            message = !spouse.isGaySpouse() ? (spouse.Gender != 0 ? Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(isMale), spouse.displayName) : Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(isMale))) : Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(isMale));
            // starting from i = 1 is a guess, but I'm pretty confident.
            for (int i = 1; i < message.Length - genderTerm.Length; i++)
            {
                if(message.Substring(i, genderTerm.Length).Equals(genderTerm))
                {
                    message = message.Substring(0, i - 1) + message.Substring(i + genderTerm.Length, message.Length - i - genderTerm.Length);
                    i = message.Length;
                }
            }

            return false;
        }

        public void returnBabyName(string name, string gender)
        {
            if (gender.Equals("Male"))
                isMale = true;
            else if (gender.Equals("Female"))
                isMale = false;
            babyName = name;
            Game1.exitActiveMenu();
        }

        public void afterMessage()
        {
            getBabyName = true;
        }

        public bool tickUpdate(GameTime time)
        {
            Game1.player.CanMove = false;
            timer += time.ElapsedGameTime.Milliseconds;
            Game1.fadeToBlackAlpha = 1f;

            if (timer > 1500 /* && !playedSound */ && !getBabyName)
            {
                /*
                if (soundName != null && !soundName.Equals(""))
                {
                    Game1.playSound(soundName);
                    playedSound = true;
                }
                */
                if (/* !playedSound && */ message != null && (!Game1.dialogueUp && Game1.activeClickableMenu == null))
                {
                    Game1.drawObjectDialogue(message);
                    Game1.afterDialogues = new Game1.afterFadeFunction(afterMessage);
                }
            }
            else if (getBabyName)
            {
                if (!naming)
                {
                    //I replaced the old NamingMenu with my CustomNamingMenu (to allow for gender control)
                    //This title dialogue isn't so easily edited to allow for all languages, so CustomNamingMenu fixes it.
                    Game1.activeClickableMenu = new CustomNamingMenu(new CustomNamingMenu.doneNamingBehavior(returnBabyName), Game1.content.LoadString("Strings\\Events:BabyNamingTitle_Male"), Game1.content.LoadString("Strings\\Events:BabyNamingTitle_Female"), "");
                    naming = true;
                }
                if (babyName != null && babyName != "" && babyName.Length > 0)
                {
                    double num = (Game1.player.spouse.Equals("Maru") ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
                    bool isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < num;

                    DisposableList<NPC> allCharacters = Utility.getAllCharacters();
                    foreach (Character character in allCharacters)
                    {
                        if (character.Name.Equals(babyName))
                        {
                            babyName += " ";
                            break;
                        }
                    }

                    /*
                     * Generates the new child.
                     */
                    Child child = new Child(babyName, isMale, isDarkSkinned, Game1.player);
                    child.Age = 0;
                    child.Position = new Vector2(16f, 4f) * 64f + new Vector2(0.0f, -24f);
                    Utility.getHomeOfFarmer(Game1.player).characters.Add(child);
                    Game1.playSound("smallSelect");
                    Game1.player.getSpouse().daysAfterLastBirth = 5;
                    Game1.player.GetSpouseFriendship().NextBirthingDate = null;

                    /*
                     * Gives new lines to spouse based on number of children.
                     * My lines are relatively simple (and worth revisiting).
                     */
                    NPC spouse = Game1.player.getSpouse();
                    string dialogue = "";

                    // Attempt to load content pack dialogue
                    List<ContentPackData.BirthDialogue> spouseDialogue = ModEntry.GetSpouseDialogueData(spouse.displayName);
                    
                    if (spouseDialogue != null && spouseDialogue.Count > 0)
                    {
                        foreach(ContentPackData.BirthDialogue birthDialogue in spouseDialogue)
                        {
                            if (Game1.player.getChildrenCount() == birthDialogue.BabyNumber)
                            {
                                dialogue = birthDialogue.Dialogue;

                                //{0} to represent the baby name and {1} to represent the player name.
                                spouse.shouldSayMarriageDialogue.Value = true;
                                spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + Game1.random.Next(1, 3), true, new string[2] { babyName, Game1.player.Name }));
                            }
                        }

                        if (Game1.player.getChildrenCount() == 2)
                            Game1.getSteamAchievement("Achievement_FullHouse");
                    }

                    // If content pack dialogue isn't available, use vanilla
                    if(dialogue.Equals(""))
                    {
                        if (Game1.player.getChildrenCount() == 2)
                        {
                            spouse.shouldSayMarriageDialogue.Value = true;
                            spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + Game1.random.Next(1, 3), true, new string[0]));
                            Game1.getSteamAchievement("Achievement_FullHouse");
                        }
                        else if(Game1.player.getChildrenCount() == 1)
                        {
                            if (spouse.isGaySpouse())
                                spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, new string[1] { babyName }));
                            else
                                spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_FirstChild", true, new string[1] { babyName }));
                        }
                        else if (Game1.player.getChildrenCount() == 3)
                        {
                            dialogue = "Three beautiful children... This is so wonderful.";
                            
                        }
                        else
                        {
                            dialogue = "What a big, happy family... I couldn't have imagined I would be so happy before I met you.";
                            
                        }
                    }

                    //Game1.morningQueue.Enqueue((DelayedAction.delayedBehavior)(() => Game1.multiplayer.globalChatInfoMessage("Baby", Lexicon.capitalize(Game1.player.Name), Game1.player.spouse, Lexicon.getGenderedChildTerm(this.isMale), Lexicon.getPronoun(this.isMale), baby.displayName)));
                    if (Game1.keyboardDispatcher != null)
                        Game1.keyboardDispatcher.Subscriber = null;
                    Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).getBedSpot()) * 64f;
                    Game1.globalFadeToClear(null, 0.02f);
                    return true;
                }
            }
            return false;
        }

        public void draw(SpriteBatch b)
        {
        }

        public void makeChangesToLocation()
        {
        }

        public void drawAboveEverything(SpriteBatch b)
        {
        }
    }
}
