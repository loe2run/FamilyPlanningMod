using System;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace FamilyPlanning.Integrations.ContentPatcher
{
    /// <summary>Handles integrating with the Content Patcher API.</summary>
    internal class ContentPatcherIntegration
    {
        /*********
        ** Fields
        *********/
        /// <summary>The current mod's manifest.</summary>
        private readonly IManifest Manifest;

        /// <summary>The Content Patcher API, or <c>null</c> if Content Patcher isn't installed.</summary>
        private readonly IContentPatcherAPI ContentPatcher;

        /// <summary>The ordinal prefixes for child token names.</summary>
        private readonly string[] Ordinals = { "First", "Second", "Third", "Fourth" };

        /// <summary>The game tick when the child data was last updated.</summary>
        private int CacheTick = -1;

        /// <summary>A snapshot of the child data as of the last context update.</summary>
        private ChildData[] Cache;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="manifest">The current mod's manifest.</param>
        /// <param name="modRegistry">The SMAPI mod registry.</param>
        public ContentPatcherIntegration(IManifest manifest, IModRegistry modRegistry)
        {
            this.Manifest = manifest;
            this.ContentPatcher = modRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
        }

        /// <summary>Register custom tokens with Content Patcher.</summary>
        public void RegisterTokens()
        {
            if (this.ContentPatcher == null)
                return;

            // per-child tokens
            for (int i = 0; i < this.Ordinals.Length; i++)
            {
                string ordinal = this.Ordinals[i];
                int index = i;

                this
                    .AddToken($"{ordinal}ChildName", index, child => child.Name)
                    .AddToken($"{ordinal}ChildIsToddler", index, child => child.IsToddler);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Register a token with Content Patcher.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="isReady">Get whether the token is ready as of the last context update.</param>
        /// <param name="getValue">Get the token value as of the last context update.</param>
        private ContentPatcherIntegration AddToken(string name, Func<bool> isReady, Func<string> getValue)
        {
            this.ContentPatcher.RegisterToken(
                mod: this.Manifest,
                name: name,
                token: new ChildToken(
                    updateContext: this.UpdateContextIfNeeded,
                    isReady: isReady,
                    getValue: getValue
                )
            );

            return this;
        }

        /// <summary>Register a token with Content Patcher.</summary>
        /// <param name="name">The token name.</param>
        /// <param name="childIndex">The index of the child for which to add a token.</param>
        /// <param name="getValue">Get the token value.</param>
        private ContentPatcherIntegration AddToken(string name, int childIndex, Func<ChildData, string> getValue)
        {
            return this.AddToken(
                name: name,
                isReady: () => this.IsReady(childIndex),
                getValue: () =>
                {
                    ChildData child = this.GetChild(childIndex);
                    return child != null
                        ? getValue(child)
                        : null;
                }
            );
        }

        /// <summary>Get the cached data for a child.</summary>
        /// <param name="index">The child index.</param>
        private ChildData GetChild(int index)
        {
            if (this.Cache == null || index >= this.Cache.Length)
                return null;

            return this.Cache[index];
        }

        /// <summary>Get whether tokens for a given child should be marked ready.</summary>
        /// <param name="index">The child index.</param>
        private bool IsReady(int index)
        {
            return this.GetChild(index)?.Name != null;
        }

        /// <summary>Update all tokens for the current context.</summary>
        private bool UpdateContextIfNeeded()
        {
            // already updated this tick
            if (Game1.ticks == this.CacheTick)
                return false;
            this.CacheTick = Game1.ticks;

            // update context
            ChildData[] oldData = this.Cache;
            this.Cache = this.FetchNewData();
            return this.IsChanged(oldData, this.Cache);
        }

        /// <summary>Fetch the latest child data.</summary>
        private ChildData[] FetchNewData()
        {
            if (!Context.IsWorldReady)
                return new ChildData[0];

            return Game1.player.getChildren()
                .Select(child => new ChildData
                {
                    Name = child.Name,
                    IsToddler = (child.Age == 3).ToString().ToLower()
                })
                .ToArray();
        }

        /// <summary>Get whether the cached data changed.</summary>
        /// <param name="oldData">The previous child data.</param>
        /// <param name="newData">The current child data.</param>
        private bool IsChanged(ChildData[] oldData, ChildData[] newData)
        {
            if (oldData == null || newData == null)
                return oldData != newData;

            if (oldData.Length != newData.Length)
                return true;

            for (int i = 0; i < oldData.Length; i++)
            {
                if (!oldData[i].IsEquivalentTo(newData[i]))
                    return true;
            }

            return false;
        }
    }
}
