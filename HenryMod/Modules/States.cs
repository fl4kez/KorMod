using KorMod.SkillStates;
using KorMod.SkillStates.BaseStates;
using System.Collections.Generic;
using System;
using KorMod.SkillStates.Kor;

namespace KorMod.Modules
{
    public static class States
    {
        internal static void RegisterStates()
        {
            Modules.Content.AddEntityState(typeof(BaseMeleeAttack));
            Modules.Content.AddEntityState(typeof(SlashCombo));

            Modules.Content.AddEntityState(typeof(Shoot));

            Modules.Content.AddEntityState(typeof(Roll));

            Modules.Content.AddEntityState(typeof(ThrowBomb));

            //KOR
            Content.AddEntityState(typeof(Dodge));
            Content.AddEntityState(typeof(HammerThrow));
        }
    }
}