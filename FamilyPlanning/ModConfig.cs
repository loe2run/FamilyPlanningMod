namespace FamilyPlanning
{
    /* *** Global config options ***
     * 
     * Config Option: AdoptChildrenWithRoommate (default is false)
     * -> If you set this value to true, then your roommate will prompt you to adopt a child.
     * -> The default value is false, by default your roommate will not try to adopt children. (Vanilla behavior)
     * -> Note: If you're using a mod that turns Krobus (or any other roommate) into a normal marriage candidate, 
     *    like Krobus Marriage Mod, then you don't need this setting to be true.
     *    Krobus will be treated like a normal spouse automatically (because he won't be a roommate).
     * -> Note: config options affect all save files.
     *    If you'd like to have some save files where Krobus will adopt children, and some where he will not,
     *    I would recommand using "set_max_children 0" on the save files where you don't want to adopt children.
     *    This will stop Krobus from asking to adopt a child, even if you have the config set to true.
     *    
     * Config Option: BabyQuestionMessages (default is false)
     * -> If you set this value to true, then you will get messages in the SMAPI console each night
     *    about whether your spouse is able to ask you for a child.
     *    This can help if you're unsure whether your spouse hasn't asked because you don't meet a requirement,
     *    or if you're just getting unlucky repeatedly.
     * -> Messages which tell you that you are missing a requirement should only be printed in the console once,
     *    so you won't be spammed by a missing requirement every night.
     */

    class ModConfig
    {
        public bool AdoptChildrenWithRoommate { get; set; }
        public bool BabyQuestionMessages { get; set; }

        public ModConfig()
        {
            AdoptChildrenWithRoommate = false;
            BabyQuestionMessages = false;
        }
    }
}