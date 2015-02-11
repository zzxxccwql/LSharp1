#region

using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Marksman
{
    internal class Twitch : Champion
    {
        public static Spell W;
        public static Spell E;

        public Twitch()
        {
            Utils.PrintMessage("Twitch loaded.");

            W = new Spell(SpellSlot.W, 950);
            W.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);
            E = new Spell(SpellSlot.E, 1200);

            Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
            Utility.HpBarDamageIndicator.Enabled = true;
        }

        public override void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var t = target as Obj_AI_Hero;
            if (t == null || (!ComboActive && !HarassActive) || !unit.IsMe)
                return;

            var useW = GetValue<bool>("UseW" + (ComboActive ? "C" : "H"));

            if (useW && W.IsReady())
                W.Cast(t, false, true);
        }

        public override void Drawing_OnDraw(EventArgs args)
        {
            Spell[] spellList = { W };
            foreach (var spell in spellList)
            {
                var menuItem = GetValue<Circle>("Draw" + spell.Slot);
                if (menuItem.Active)
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        public override void Game_OnGameUpdate(EventArgs args)
        {
            if (Orbwalking.CanMove(100) && (ComboActive || HarassActive))
            {
                var useW = GetValue<bool>("UseW" + (ComboActive ? "C" : "H"));
                var useE = GetValue<bool>("UseE" + (ComboActive ? "C" : "H"));

                if (useW)
                {
                    var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                    if (W.IsReady() && wTarget.IsValidTarget())
                        W.Cast(wTarget, false, true);
                }

                if (useE && E.IsReady())
                {
                    var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                    if (eTarget.IsValidTarget(E.Range))
                    {
                        foreach (
                            var buff in
                                eTarget.Buffs.Where(buff => buff.DisplayName.ToLower() == "twitchdeadlyvenom")
                                    .Where(buff => buff.Count == 6)) 
                        {
                            E.Cast();
                        }
                    }
                }
            }

            if (GetValue<bool>("UseEM") && E.IsReady())
            {
                foreach (
                    var hero in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(
                                hero =>
                                    hero.IsValidTarget(E.Range) &&
                                    (ObjectManager.Player.GetSpellDamage(hero, SpellSlot.E) - 10 > hero.Health)))
                {
                    E.Cast();
                }
            }
        }

        private static float GetComboDamage(Obj_AI_Hero t)
        {
            var fComboDamage = 0f;

            if (E.IsReady())
                fComboDamage += (float) ObjectManager.Player.GetSpellDamage(t, SpellSlot.E);

            if (ObjectManager.Player.GetSpellSlot("summonerdot") != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(ObjectManager.Player.GetSpellSlot("summonerdot")) ==
                SpellState.Ready && ObjectManager.Player.Distance(t) < 550) 
                fComboDamage += (float) ObjectManager.Player.GetSummonerSpellDamage(t, Damage.SummonerSpell.Ignite);

            if (Items.CanUseItem(3144) && ObjectManager.Player.Distance(t) < 550)
                fComboDamage += (float) ObjectManager.Player.GetItemDamage(t, Damage.DamageItems.Bilgewater);

            if (Items.CanUseItem(3153) && ObjectManager.Player.Distance(t) < 550)
                fComboDamage += (float) ObjectManager.Player.GetItemDamage(t, Damage.DamageItems.Botrk);

            return fComboDamage;
        }

        public override bool ComboMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseWC" + Id, "使用W").SetValue(true));
            config.AddItem(new MenuItem("UseEC" + Id, "E层数").SetValue(true));
            return true;
        }

        public override bool HarassMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseWH" + Id, "使用W").SetValue(false));
            config.AddItem(new MenuItem("UseEH" + Id, "E层数").SetValue(false));
            return true;
        }

        public override bool DrawingMenu(Menu config)
        {
            config.AddItem(
                new MenuItem("DrawW" + Id, "W 范围").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "连招损伤").SetValue(true);
            Config.AddItem(dmgAfterComboItem);

            return true;
        }

        public override bool MiscMenu(Menu config)
        {
            config.AddItem(new MenuItem("UseEM" + Id, "可杀用E").SetValue(true));
            return true;
        }

        public override bool ExtrasMenu(Menu config)
        {

            return true;
        }

        public override bool LaneClearMenu(Menu config)
        {
            return true;
        }

    }
}
