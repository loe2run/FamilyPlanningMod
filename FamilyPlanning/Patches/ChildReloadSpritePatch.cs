using System;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;

namespace FamilyPlanning.Patches
{
    /* Child.reloadSprite():
     * This patch replaces the default child sprite with a custom sprite,
     * either from Family Planning content pack or from Content Patcher content pack.
     * 
     * The postfix tries to load the asset specified by an FP pack or CP pack,
     * and applies the sprite if the asset can be loaded successfully.
     * Otherwise, doesn't overwrite the default sprite.
     * 
     * If both an FP pack and a CP pack try to replace the same child, the FP pack will be applied.
     */

    class ChildReloadSpritePatch
    {
        public static void Postfix(Child __instance)
        {
            // This function is Postfix, so at this point, Child is initialized with default sprite.
            // If a sprite exists (content pack or patch) which should overwrite it, do so.
            string spriteName = null;

            // Try to load the child sprite from a content pack
            try
            {
                /* GetChildSpriteData returns null if the sprite asset doesn't exist */
                spriteName = ModEntry.GetChildSpriteData(__instance.Name, __instance.Age);
            }
            catch (Exception) { }

            //If that fails, try to load the child sprite patched by a Content Patcher content pack
            if (spriteName == null)
            {
                try
                {
                    // Verify the sprite "Characters\\Child_{Name}" exists in the GameContent (CP has patched)
                    ModEntry.helper.Content.Load<Texture2D>("Characters\\Child_" + __instance.Name, ContentSource.GameContent);
                    spriteName = "Characters\\Child_" + __instance.Name;
                }
                catch (Exception) { }
            }

            // If the custom sprite doesn't exist, don't change anything.
            if (spriteName == null)
                return;

            // Otherwise, initialize a new sprite based on the custom texture.
            __instance.HideShadow = true;
            __instance.Breather = false;

            switch (__instance.Age)
            {
                case 0:
                    __instance.Sprite = new AnimatedSprite(spriteName, 0, 22, 16);
                    break;
                case 1:
                    __instance.Sprite = new AnimatedSprite(spriteName, 4, 22, 32);
                    break;
                case 2:
                    __instance.Sprite = new AnimatedSprite(spriteName, 32, 22, 16);
                    break;
                case 3:
                    __instance.Sprite = new AnimatedSprite(spriteName, 0, 16, 32);
                    __instance.HideShadow = false;
                    break;
            }

            __instance.Sprite.UpdateSourceRect();
        }
    }
}