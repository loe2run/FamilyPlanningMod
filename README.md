# FamilyPlanningMod
This is a Stardew Valley mod called Family Planning. Family Planning allows you to customize the maximum number of children you can have and their genders, adjust the chance of having children, and adopt children with a roommate by config.

## A Brief Overview
#### Customize number of children and their genders
By default in Stardew Valley, you are restricted to having exactly two children, a boy and a girl. With Family Planning, you can customize the maximum number of children you want to have for each save file and control their genders independently. So instead of having a boy and a girl in every save file, in one game you could have two girls, or exactly one boy, or two boys and two girls, or no children at all, etc.

Controlling the gender of a child is straightforward. When the child is born and you name them, the menu will also give you the option to choose their gender. Just click the male icon to have a boy, or click the female icon to have a girl. To customize the maximum number of children, you'll want to use either the built-in console command `set_max_children` or directly edit the Family Planning settings for that save file.

#### Adjust the chance of having children
In Stardew Valley, if you meet the requirements for having a child (having the nursery house upgrade, having 10 hearts with your spouse, etc.), your spouse has a 5% chance each night to ask you if you'd like to have a child. With Family Planning, you can change that probability. So you could make sure your spouse never asks for a child by setting it to 0%, or always asks for a child by setting it to 100%, or just make it slightly more likely by setting it to 10%, etc. Additionally, there's a config option that will print messages in the SMAPI console each night about whether your spouse can ask you for a child. This can help if you're unsure if you aren't being asked to have a child because you're missing a requirement or because you're just unlucky.

To change the probability that your spouse will ask if you want a baby, you can use either the built-in console command `set_question_chance` or directly edit the Family Planning settings for that save file. If you'd like to see the additional console messages, you can change the setting in the config file.

#### Adopt children with a roommate
After the Stardew Valley 1.4 update, you can now choose to have Krobus move in with you as a roomate. However, choosing to have Krobus as a roommate means that you can't have children, though Krobus will help raise your children from a previous marriage. Family Planning adds a config option that lets you adopt children with a roommate. This means that Krobus can ask you if you want to adopt a child with him. You can control this setting with the config file.

#### Sprite and Dialogue Customization
Stardew Valley only has four child sprites: male and light, female and light, male and dark, female and dark. This means that if you have two male children, the only way they will look different is if they have a different skin color. And if you have three children of the same gender, you guarantee that some of them will be identical.

If you'd like to customize the sprites for your children, there are two options: create a Content Patcher mod or create a Content Pack for Family Planning. If you want to make a Content Patcher mod, then you'll want to use the custom Content Patcher tokens that this mod provides. Check out the "Content Patcher Tokens" section of this readme. If you want to make a Family Planning Content Pack, the process is relatively simple! The explanation is after the Content Patcher section, titled "Creating a Content Pack".

## Config Options
There are two config options for Family Planning: `AdoptChildrenWithRoommate` and `BabyQuestionMessages`. These config options will affect all save files.

To change your config options, look for the `config.json` file in the Family Planning mod folder. You can edit this file by opening it in a text editor, changing what you want to change, then saving and closing it. The next time you open the game, your settings will have changed.

#### AdoptChildrenWithRoommate
This config option defaults to `false`. If you set this option to `true`, then your roommate can ask you if you want to adopt a child with them. Specifically, this allows you to adopt children with Krobus if he's your roommate.

#### BabyQuestionMessages
This config option defaults to `false`. If you set this option to `true`, this mod will print messages in the SMAPI console each night about whether your spouse can ask you for a child. This is mainly helpful when you're waiting for your spouse to ask you about having a child, and you're unsure whether they won't ask because you're just unlucky or because you're missing some requirement. Also, if you are missing something, like not having a house upgrade or not having enough hearts with your spouse, the mod will print a warning message only once and then stop, so you won't be spammed with messages until you fulfill the requirement.

## Save File Settings
Each save file has two config options for Family Planning: `MaxChildren` and `BabyQuestionChance`. These settings are different for each save file.

`MaxChildren` controls the maximum number of children you can have. By default this value is 2, so once you have 2 children, your spouse will not ask you about having more children. If you don't want to have children at all, you can set this value to 0. Or if you want two boys and two girls, you can set this value to 4. I would recommend that you choose a value between 0 and 4 because if you use a value above 4, there won't be enough space for toddlers in the beds and Content Patcher packs will not work for additional children. However, if you're fine with those problems, you can set `MaxChildren` as high as you want.

`BabyQuestionChance` controls the percentage chance that, if you meet all the requirements for having a child, your spouse will randomly ask you if you want to have a child. By default this value is 5, meaning there's a 5% chance that your spouse will ask to have a child each night. If you want your spouse to never ask, you can set this value to 0. If you want your spouse to always ask as soon as it's possible, you can set this value to 100. You can set this value to any number between 0 and 100.

#### Sidenote: Editing your save file
You can control these settings by using console commands, but if you aren't comfortable with that, you can directly edit these values by going to the file where they're stored and manually changing them with a text editor.

The first time you load the save in Stardew Valley, the Family Planning mod will create a new file for that save. Go to the FamilyPlanning mod folder, then to the "data" folder inside it, and you will see a file that looks like "JonSnow_123456789", with the name of your character and some numbers. This is where the settings are for that save.

If you open this file in a text editor, the initial version of the file will look like this:
```cs
{
  "MaxChildren": 2,
  "BabyQuestionChance": 5
}
```
All you have to do is change the number and save the file. The next time you play Stardew Valley and load that save file, your settings will be changed.

## Console Commands
The Family Planning provides four console commands, two for controlling the maximum number of children you can have and two for changing the probability that your spouse asks you to have a child. These will change the `MaxChildren` and `BabyQuestionChance` values for that save file.

#### Max Number of Children
`get_max_children` will tell you what the `MaxChildren` value for you save file currently is.

`set_max_children <value>` allows you to change the `MaxChildren` value.

Example: You want to be able to have four children. Load the save file, go to the SMAPI console, type in `set_max_children 4`, and hit enter. Your settings will now be changed!

#### Question Chance
`get_question_chance` will tell you what the current `BabyQuestionChance` value for your save file is.

`set_question_chance <value>` allows you to change the `BabyQuestionChance` value.

Example: You want to have a 10% change each night of your spouse asking for a child. Load the save file, go to the SMAPI console, type in `set_question_chance 10`, and hit enter. Your settings will now be changed!

## Content Patcher Tokens
If you aren't familiar with using mod-provided Content Patcher tokens, you should check out the Content Patcher readme [here](https://github.com/Pathoschild/StardewMods/blob/stable/ContentPatcher/docs/author-tokens-guide.md#mod-provided-tokens).

To use the CP tokens that Family Planning provides, make sure to set Family Planning ("Loe2run.FamilyPlanning") as a dependency.

Family Planning will attempt to load the child sprite from the file `Characters\\Child_<Child Name>`. In order for your CP mod to target that file, you'll need to use the ChildName token. There's a version of this token for each child (by birth order).
  
So for example, when you want to patch over the appearance of the oldest child, your Target field would look like this.

```cs
"Target": "Characters\\Child_{{Loe2run.FamilyPlanning/FirstChildName}}"
```
The second token that you can use is the IsToddler token, which returns `"true"` or `"false"`. This helps you distinguish whether to use a toddler sprite or a baby sprite.

So to continue the example from above, this is what your full entry would look like when trying to load a new toddler sprite for the oldest child.
```cs
{
  "LogName": "First Child Sprites (Toddler)",
  "Action": "Load",
  "Target": "Characters/Child_{{Loe2run.FamilyPlanning/FirstChildName}}",
  "FromFile": "assets/first_child_toddler.png",
  "When":
  {
    "Loe2run.FamilyPlanning/FirstChildIsToddler": "true"
  }
},
```
Note: If you run into an issue where you'd like Family Planning to provide more tokens for your CP pack to work, let me know! I'd be happy to add more as necessary.

## Creating a Content Pack
To create a Content Pack, you should first download the Example Content Pack from the Nexus page for Family Planning to use as a template. There are three steps to creating the Content Pack: you will need to edit the manifest.json, add your image files to the assets folder, and edit the data.json in the assets folder.

#### Manifest.json
Inside the manifest.json, you should replace the text `"<Your Name Here>"` in the Author and UniqueID fields with your name. For example, my name is Loe2run, so I would replace `"<Your Name Here>"` with `"Loe2run"` in the Author field. Also, you don't have to use exactly `"<Your Name Here>.MyCustomChildren"` as the UniqueID. So long as it starts with `<Your Name Here>` and then a `.`, you can put in a different suffix if you'd like.

#### Image files
Decide on which image files you'd like to use for your children and place those files in the assets folder.

For example, let's say that your Farmer is married to Leah and you have two daughters, Amber and Beverly, who need new sprites. For Amber, you want to use `leahbaby.png` as a baby and `leahhairbuns.png` as a toddler. For Beverly, you want `leahbaby.png` as a baby and `leahpigtails.png` as a toddler. Copy the image files you want, "leahbaby.png", "leahhairbuns.png", and "leahpigtails.png" to the assets folder.

#### Data.json
Now that you've added the image files, you'll see a `data.json` file in the assets folder with your image files.

When you open the default version of this file, you'll see a section called "ChildSpriteID".
```cs
{
  "ChildSpriteID": {
    "<Child Name Here>": {
      "BabySpriteName": "assets/<name of baby sprites>.png",
      "ToddlerSpriteName": "assets/<name of toddler sprites>.png"
    }
  }
}
```
This default version only has a single entry, but you can copy and paste this as many times as you need for all of your children. Just be sure to put commas in between the sections. In our example with two children, so we edit the `data.json` to read as:
```cs
{
  "ChildSpriteID": {
    "Amber": {
      "BabySpriteName": "assets/leahbaby.png",
      "ToddlerSpriteName": "assets/leahhairbuns.png"
    },
    "Beverly": {
      "BabySpriteName": "assets/leahbaby.png",
      "ToddlerSpriteName": "assets/leahpigtails.png"
    }
  }
}
```
And that's it! Save the changes to the file. You should now be able to run the game with the custom sprites.

#### Spouse Dialogue
In addition to Content Packs allowing for customization of child sprites, they can also allow for customization of spouse dialogue. Your spouse can have a custom dialogue line on the day of a child's birth. This dialogue is also added to the `data.json` file in the assets folder.

In the default version of this file, you'll see a section called "SpouseDialogue".
```cs
{
  "SpouseDialogue": {
    "<Spouse Name>": [
      {
        "BabyNumber": <birth number>,
        "Dialogue": "<insert text here>"
      }
    ],
  }
}
```

For an example of how to fill this out, let's say you want your spouse, Maru, to say "(child's name)'s birth reminded me of MARILDA... I wonder what she's doing right now." when your first child is born and "Having a family with you really means a lot to me (player's name), thank you." when your second child is born. You would edit your `data.json` file to look like this:

```cs
{
  "SpouseDialogue": {
    "Maru": [
      {
        "BabyNumber": 1,
        "Dialogue": "{0}'s birth reminded me of MARILDA... I wonder what she's doing right now."
      },
      {
        "BabyNumber": 2,
        "Dialogue": "Having a family with you really means a lot to me {1}, thank you."
      }
    ],
  }
}
```
`BabyNumber` represents which birth should trigger the dialogue, with 1 as the first child, 2 as the second, etc. `Dialogue` is the dialogue line itself. As you see in this example, the characters `{0}` can be used to represent your child's name and `{1}` can be used to represent the player's name.

### Using both at once

Here's an example of formatting your data.json to contain both child sprite information and spouse dialogue information:

```cs
{
  "ChildSpriteID": {
    "Amber": {
      "BabySpriteName": "assets/leahbaby.png",
      "ToddlerSpriteName": "assets/leahhairbuns.png"
    },
    "Beverly": {
      "BabySpriteName": "assets/leahbaby.png",
      "ToddlerSpriteName": "assets/leahpigtails.png"
    }
  },
  "SpouseDialogue": {
    "Maru": [
      {
        "BabyNumber": 1,
        "Dialogue": "{0}'s birth reminded me of MARILDA... I wonder what she's doing right now."
      },
      {
        "BabyNumber": 2,
        "Dialogue": "Having a family with you really means a lot to me {1}, thank you."
      }
    ],
  }
}
```

#### Additional Notes
Currently, Content Packs only support .png image files. If you have an .xnb file you'd like to use, your best bet is to try and convert the .xnb file you like to a .png image. More information on that at the [Stardew Valley Wiki.](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)

If you don't know the name of your child because they haven't been born yet, that's fine! Just wait until the child is born, name them, wait for the game to save, and then exit to desktop and follow the steps above. The next time your open the game, your newly named child should have a custom sprite.

If you have two children with the same name between save files, the current method means they will end up with the same sprite. I'm hoping to fix this issue in the future.

If you're using this mod for multiple save files and want to have multiple save files worth of children to change, you can put all of the child information into the same Content Pack, or you could create multiple Content Packs. you have two options. Both methods work perfectly fine, the number of Content Packs you can have isn't limited.

Using a Content Pack, the gender or skin color of the child has no effect on their sprite appearance. You could decide to use a male toddler sprite for a female child and Family Planning will go ahead and apply the sprite regardless.

## Multiplayer

I haven't checked compatibility with multiplayer yet, but it's likely not compatible. If you're attempting to use this mod with multiplayer, proceed with caution. Historically, there have been issues when one player has a Family Planning content pack and the other players don't.

## Compatibility:

Works with Stardew Valley 1.4 beta on Linux/Mac/Windows.
This mod uses Harmony, so there may be interference with other mods using Harmony to patch the same methods. If you notice any issues, please let me know.

## How to Uninstall
To uninstall Family Planning, remove the Family Planning mod folder and any content packs you have for Family Planning. The children you had will using the mod with not be removed. The only way to remove children is to remove all of them by turning them into doves at the witch's hut.
