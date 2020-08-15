namespace FamilyPlanning.Integrations.ContentPatcher
{
    /// <summary>A snapshot of the token data for a child NPC.</summary>
    internal class ChildData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The child's name.</summary>
        public string Name { get; set; }

        /// <summary>Whether the child is a toddler.</summary>
        public string IsToddler { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get whether another instance has the same values as this one.</summary>
        /// <param name="other">The child data with which to compare.</param>
        public bool IsEquivalentTo(ChildData other)
        {
            return
                other != null
                && this.Name == other.Name
                && this.IsToddler == other.IsToddler;
        }
    }
}
