using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;

namespace FamilyPlanning
{
    public interface IContentPatcherAPI
    {
        /*********
        ** Methods
        *********/
        /// <summary>Register a token.</summary>
        /// <param name="mod">The manifest of the mod defining the token (see <see cref="Mod.ModManifest"/> on your entry class).</param>
        /// <param name="name">The token name. This only needs to be unique for your mod; Content Patcher will prefix it with your mod ID automatically, like <c>Pathoschild.ExampleMod/SomeTokenName</c>.</param>
        /// <param name="getValue">A function which returns the current token value. If this returns a null or empty list, the token is considered unavailable in the current context and any patches or dynamic tokens using it are disabled.</param>
        void RegisterToken(IManifest mod, string name, Func<IEnumerable<string>> getValue);
    }

    /* The ChildToken class caches a child's information for Content Patcher tokens.
     * 
     * Available tokens: ChildName, ChildIsToddler
     * A copy of these tokens exists for the first four children born.
     * Example: FirstChildName, SecondChildName, ThirdChildName, FourthChildName.
     * 
     * ChildName: returns the child's name.
     * ChildIsToddler: returns the string "true" if the child is toddler age or older, "false" otherwise.
     */
    internal class ChildToken
    {
        private readonly int ChildNumber;
        private bool Initialized;
        private string ChildName;
        private string ChildIsToddler;

        /* ChildToken constructor - initializes class variables
         * 
         * Tokens are registered with the Content Patcher API before child information is available,
         * so this initializes class fields to null when created.
         */ 
        public ChildToken(int childNumberIn)
        {
            ChildNumber = childNumberIn;
            ChildName = null;
            ChildIsToddler = null;
            Initialized = false;
        }

        /* InitializeToken - initializes class fields
         * 
         * Loads the ChildName and ChildIsToddler fields from game data.
         * If the fields are successfully initialized, the bool Initialized is set to true.
         * If the function fails because information isn't available, the token remains uninitialized.
         */ 
        public void InitializeToken()
        {
            if (Context.IsWorldReady)
            {
                List<Child> children = Game1.player.getChildren();
                if (children != null && children.Count >= ChildNumber)
                {
                    Child child = children[ChildNumber - 1];
                    ChildName = child.Name;
                    ChildIsToddler = (child.Age >= 3) ? "true" : "false";
                    Initialized = true;
                }
            }
        }

        /* UpdateToken - updates the appropriate class field(s)
         * 
         * The name of a child doesn't change during play, but the age of the child does,
         * so this method updates the ChildIsToddler value from game data.
         */
        public void UpdateToken()
        {
            if (Context.IsWorldReady)
            {
                List<Child> children = Game1.player.getChildren();
                if (children != null && children.Count >= ChildNumber)
                {
                    Child child = children[ChildNumber - 1];
                    ChildIsToddler = (child.Age >= 3) ? "true" : "false";
                }
            }
        }

        /* ClearToken - unintializes the class fields
         * 
         * Clears the cached data in the token and sets Initialized to false
         * in preparation for loading a different save file.
         */ 
        public void ClearToken()
        {
            ChildName = null;
            ChildIsToddler = null;
            Initialized = false;
        }

        /* Returns the Initialize value */
        public bool IsInitialized()
        {
            return Initialized;
        }

        /* GetChildName - the Content Patcher API registered function for the token ChildName
         * 
         * Returns the ChildName string in a form that the Content Patcher API can use.
         */ 
        public IEnumerable<string> GetChildName()
        {
            if (ChildName != null)
                return new[] { ChildName };
            return null;
        }

        /* GetChildIsToddler - the Content Patcher API registered function for the token ChildIsToddler
         * 
         * Returns the ChildIsToddler string in a form that the Content Patcher API can use.
         */
        public IEnumerable<string> GetChildIsToddler()
        {
            if(ChildIsToddler != null)
                return new[] { ChildIsToddler };
            return null;
        }
    }
}