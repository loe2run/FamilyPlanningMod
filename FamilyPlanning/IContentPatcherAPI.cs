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

    /* ChildToken keeps track of the child's information for Content Patcher tokens.
     */
    internal class ChildToken
    {
        private readonly int ChildNumber;
        private bool Initialized;
        private string ChildName;
        private string ChildIsToddler;

        public ChildToken(int childNumberIn)
        {
            ChildNumber = childNumberIn;
            ChildName = null;
            ChildIsToddler = null;
            Initialized = false;
        }

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

        public void ClearToken()
        {
            ChildName = null;
            ChildIsToddler = null;
            Initialized = false;
        }

        public bool IsInitialized()
        {
            return Initialized;
        }

        public IEnumerable<string> GetChildName()
        {
            if (ChildName != null)
                return new[] { ChildName };
            return null;
        }

        public IEnumerable<string> GetChildIsToddler()
        {
            if(ChildIsToddler != null)
                return new[] { ChildIsToddler };
            return null;
        }
    }
}