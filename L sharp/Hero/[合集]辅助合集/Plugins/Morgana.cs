#region LICENSE

// Copyright 2014-2015 Support
// Morgana.cs is part of Support.
// 
// Support is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Support is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Support. If not, see <http://www.gnu.org/licenses/>.
// 
// Filename: Support/Support/Morgana.cs
// Created:  01/10/2014
// Date:     24/01/2015/13:14
// Author:   h3h3

#endregion

namespace Support.Plugins
{
    #region

    using System;
    using System.Linq;
    using LeagueSharp;
    using LeagueSharp.Common;
    using Support.Util;
    using ActiveGapcloser = Support.Util.ActiveGapcloser;

    #endregion

    #region

    #endregion

    public class Morgana : PluginBase
    {
        public Morgana()
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 550);

            Q.SetSkillshot(0.25f, 80f, 1200f, true, SkillshotType.SkillshotLine);
            W.SetSkillshot(0.28f, 175f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        public override void OnUpdate(EventArgs args)
        {
            try
            {
                if (ComboMode)
                {
                    if (Q.CastCheck(Target, "ComboQ") && Q.CastWithHitChance(Target, "ComboQHC"))
                    {
                        return;
                    }

                    if (W.CastCheck(Target, "ComboW"))
                    {
                        if (
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(hero => (hero.IsValidTarget(W.Range) && hero.IsMovementImpaired()))
                                .Any(enemy => W.Cast(enemy.Position)))
                        {
                            return;
                        }

                        if (
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(hero => hero.IsValidTarget(W.Range))
                                .Any(enemy => W.CastIfWillHit(enemy, 1)))
                        {
                            return;
                        }
                    }

                    if (R.CastCheck(Target, "ComboR") &&
                        Helpers.EnemyInRange(ConfigValue<Slider>("ComboCountR").Value, R.Range))
                    {
                        R.Cast();
                    }
                    return;
                }

                if (HarassMode)
                {
                    if (Q.CastCheck(Target, "HarassQ") && Q.CastWithHitChance(Target, "HarassQHC"))
                    {
                        return;
                    }

                    if (W.CastCheck(Target, "HarassW"))
                    {
                        if (
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(hero => (hero.IsValidTarget(W.Range) && hero.IsMovementImpaired()))
                                .Any(enemy => W.Cast(enemy.Position)))
                        {
                            return;
                        }

                        if (
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(hero => hero.IsValidTarget(W.Range))
                                .Any(enemy => W.CastIfWillHit(enemy, 1))) {}
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (gapcloser.Sender.IsAlly)
            {
                return;
            }

            if (Q.CastCheck(gapcloser.Sender, "GapcloserQ"))
            {
                Q.Cast(gapcloser.Sender);
            }
        }

        public override void ComboMenu(Menu config)
        {
            var comboQ = config.AddSubMenu(new Menu("Q 设置", "Q"));
            comboQ.AddBool("ComboQ", "使用 Q", true);
            comboQ.AddHitChance("ComboQHC", "最小命中率", HitChance.Medium);

            var comboW = config.AddSubMenu(new Menu("W 设置", "W"));
            comboW.AddBool("ComboW", "使用 W", true);

            var comboE = config.AddSubMenu(new Menu("E 设置", "E"));
            comboE.AddBool("ComboE", "使用 E", true);

            var comboR = config.AddSubMenu(new Menu("R 设置", "R"));
            comboR.AddBool("ComboR", "使用 R", true);
            comboR.AddSlider("ComboCountR", "几个敌人使用大招", 2, 1, 5);
        }

        public override void HarassMenu(Menu config)
        {
            var harassQ = config.AddSubMenu(new Menu("Q 设置", "Q"));
            harassQ.AddBool("HarassQ", "使用 Q", true);
            harassQ.AddHitChance("HarassQHC", "最小击中几率", HitChance.High);

            var harassW = config.AddSubMenu(new Menu("W 设置", "W"));
            harassW.AddBool("HarassW", "使用 W", true);
        }

        public override void InterruptMenu(Menu config)
        {
            config.AddBool("GapcloserQ", "使用 Q 防突进", true);
        }
    }
}