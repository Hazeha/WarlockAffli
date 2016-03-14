using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;
using CustomClassTemplate.Data;
using CustomClassTemplate.Settings;

namespace CustomClassTemplate.Objects
{
    internal class Spellbook
    {
        private static Spell lastSpell = new Spell(string.Empty, -1, false, false);

        private List<Spell> spells;
        
        //--Dmg Spells--// ((MAYBE RE-WRITE WHOLE CC))
        //-------------------------------------------------------------------------------------------------------Immolate---//
        public static readonly Spell Immolate = new Spell("Immolate", 100, false, false, isWanted:
                () =>
                //--What Parametters to take care of before casting--//
                
                    Target.HealthPercent >= 10 && 
                    !Target.GotDebuff("Immolate") && 
                    lastSpell.Priority != 100 &&
                    Me.ManaPercent >= 5 &&
                    Helpers.CanCast("Immolate"));
        //-------------------------------------------------------------------------------------------------------Corruption--//
        public static readonly Spell Corruption = new Spell("Corruption", 200, false, false, false, isWanted:
                () =>
                //--What Parametters to take care of before casting--//
                    Target.HealthPercent >= 15 && 
                    !Target.GotDebuff("Corruption") && 
                    lastSpell.Priority != 200
                    && Me.ManaPercent >= 5 &&
                    Helpers.CanCast("Corruption"));
        //----------------------------------------------------------------------------------------------------Curse of Agony--//
        public static readonly Spell CoA = new Spell("Curse of Agony", 300, false, true, true, isWanted:
                () =>
                //--What Parametters to take care of before casting--//
                    Target.HealthPercent >= 10 &&
                    !Target.GotDebuff("Curse of Agony") &&
                    lastSpell.Priority != 300 &&
                    Me.ManaPercent >= 5 && 
                    Helpers.CanCast("Curse of Agony"));                    
        //--------------------------------------------------------------------------------------------------------Drain Life--//
        public static readonly Spell DrainLife = new Spell("Drain Life", 600, false, true, true, isChanneled: true, isWanted:
                () =>
                //--What Parametters to take care of before casting--//
                    Me.HealthPercent <= 60 && 
                    !Target.GotDebuff("Drain Life") && 
                    Target.GotDebuff("Corruption") &&
                    Target.GotDebuff("Curse of Agony") && 
                    Target.GotDebuff("Immolate") && 
                    Helpers.CanCast("Drain Life"));
        //-------------------------------------------------------------------------------------------------------Shadow Bolt--//
        public static readonly Spell ShadowBolt = new Spell("Shadow Bolt", 700, false, true, false, isWanted:
               () =>
               //--What Parametters to take care of before casting--//             
                    Target.GotDebuff("Immolate") || !Helpers.CanCast("Immolate") &&
                    Target.GotDebuff("Corruption") || !Helpers.CanCast("Corruption") &&
                    Target.GotDebuff("Curse of Agony") || !Helpers.CanCast("Curse of Agony") &&
                    Helpers.CanCast("Shadow Bolt"));
        //--------------------------------------------------------------------------------------------------------Drain Soul--//
        public static readonly Spell DrainSoul = new Spell("Drain Soul", 500, false, true, true, isChanneled: true, isWanted:
                () =>
                //--What Parametters to take care of before casting--//                               Need Soul Shard Identifier.
                    Target.HealthPercent <= 30 &&
                    Target.GotDebuff("Immolate") || !Helpers.CanCast("Immolate") &&
                    Target.GotDebuff("Corruption") || !Helpers.CanCast("Corruption") &&
                    Target.GotDebuff("Curse of Agony") || !Helpers.CanCast("Curse of Agony") &&
                    Helpers.CanCast("Drain Soul"));

        //--Buff Spells--//
        //-------------------------------------------------------------------------------------------------------Demon Armor--//
        public static readonly Spell DemonArmor = new Spell("Demon Armor", 10, true, false, isWanted:
                () =>
                    !Helpers.CanCast("Demon Skin") &&
                    Helpers.ShouldBuffSelf("Demon Armor"));
        //--------------------------------------------------------------------------------------------------------Demon Skin--//
        public static readonly Spell DeamonSkin = new Spell("Demon Skin", 7, true, false, isWanted:
                () =>
                    !Helpers.CanCast("Demon Armor") &&
                    Helpers.CanCast("Demon Skin"));
        //-------------------------------------------------------------------------------------------------Summon Voidwalker--//
        public static readonly Spell SummonVoidwalker = new Spell("Summon Voidwalker", 9, true, false, false, isWanted:
                () =>
                                                                                                        //Need Pet Identifier.
                    Helpers.CanCast("Summon Voidwalker"));
        
        //-----------------------------------------------------------------------------------------------------Summon Imp-----//
        public static readonly Spell SummonImp = new Spell("Summon Imp", 12, true, false, false, isWanted:
                () =>
                    ///Neeeeed Code here---------------------                                           //Need Pet Identifier.
                    !Helpers.CanCast("Summon Voidwalker") && 
                    Helpers.CanCast("Summon Imp"));

       
    
        //--If Low Mana--//
        //--------------------------------------------------------------------------------------------------------Shoot Wand--//
        public static readonly Spell Wand = new Spell("Shoot", 650, false, false, true, true, isWanted:
            () =>
        {
            return Helpers.CanWand() && Me.ManaPercent <= 30 && 
            Target.GotDebuff("Immolate") && Target.GotDebuff("Corruption") &&
            Target.GotDebuff("Curse of Agony") || !Helpers.CanCast("Corruption") &&
            !Helpers.CanCast("Corruption") && !Helpers.CanCast("Curse of Agony") && 
            !Helpers.CanCast("Shadow Bolt") &&
            /**/ (!Me.GotAura("Power Word: Shield") || !Me.GotDebuff("Weakened Soul")/**/); //I dont know why, but i may not remove this without errer
        }, customAction: () => ZzukBot.Game.Statics.Spell.Instance.StartWand());
        
        //-------------------------------------------------------------------------------------------------------------------//
        public Spellbook()
        {
            this.spells = new List<Spell>();
            this.InitializeSpellbook();
        }
        //-------------------------------------------------------------------------------------------------------------------//
        public IEnumerable<Spell> GetDamageSpells()
        {
            return Cache.Instance.GetOrStore("damageSpells", () => this.spells.Where(s => !s.IsBuff));
        }
        //-------------------------------------------------------------------------------------------------------------------//
        public IEnumerable<Spell> GetBuffSpells()
        {
            return Cache.Instance.GetOrStore("buffSpells", () => this.spells.Where(s => s.IsBuff && !s.DoesDamage));
        }
        
        //-------------------------------------------------------------------------------------------------------------------//
        public void UpdateLastSpell(Spell spell)
        {
            lastSpell = spell;
        }
        //-------------------------------------------------------------------------------------------------------------------//
        private void InitializeSpellbook()
        {
            foreach (var property in this.GetType().GetFields())
            {
                spells.Add(property.GetValue(property) as Spell);
            }

            spells = spells.OrderBy(s => s.Priority).ToList();
        }
        //-------------------------------------------------------------------------------------------------------------------//
        private static WoWUnit Me
        {
            get { return ObjectManager.Instance.Player; }
        }
        //-------------------------------------------------------------------------------------------------------------------//
        private static WoWUnit Target
        {
            get { return ObjectManager.Instance.Target; }
        }
        //-------------------------------------------------------------------------------------------------------------------//
        private static WoWUnit Pet
        {
            get { return ObjectManager.Instance.Pet; }
        }
       
        //-------------------------------------------------------------------------------------------------------------------//
    }
}
