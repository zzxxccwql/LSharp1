using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace KurisuBlitz
{
    //   _____       _    _____           _ 
    //  |   __|___ _| |  |  |  |___ ___ _| |
    //  |  |  | . | . |  |     | .'|   | . |
    //  |_____|___|___|  |__|__|__,|_|_|___|
    //  Copyright © Kurisu Solutions 2015
      
    internal class Program
    {
        private static Spell q;
        private static Spell e;
        private static Spell r;

        private static Menu _menu;
        private static Obj_AI_Hero _target;
        private static Orbwalking.Orbwalker _orbwalker;
        private static  Obj_AI_Hero _player = ObjectManager.Player;

        static void Main(string[] args)
        {
            Console.WriteLine("Blitzcrank injected...");
            CustomEvents.Game.OnGameLoad += BlitzOnLoad;
        }

        private static void BlitzOnLoad(EventArgs args)
        {
            if (_player.ChampionName != "Blitzcrank")
            {
                return;
            }

            // Set spells      
            q = new Spell(SpellSlot.Q, 1050f);
            q.SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);

            e = new Spell(SpellSlot.E, 150f);
            r = new Spell(SpellSlot.R, 550f);

            // Load Menu
            BlitzMenu();

            // Load Drawings
            Drawing.OnDraw += BlitzOnDraw;

            // OnUpdate
            Game.OnGameUpdate += BlitzOnUpdate;

            // Interrupter
            Interrupter.OnPossibleToInterrupt += BlitzOnInterrupt;

        }

        private static void BlitzOnInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (_menu.Item("interrupt").GetValue<bool>())
            {
                var prediction = q.GetPrediction(unit);
                if (prediction.Hitchance >= HitChance.Low)
                {
                    q.Cast(prediction.CastPosition);
                }

                else if (unit.Distance(_player.Position) < r.Range)
                {
                    r.Cast();
                }
            }
        }

        private static void BlitzOnDraw(EventArgs args)
        {
            if (!_player.IsDead)
            {
                var rcircle = _menu.SubMenu("drawings").Item("drawR").GetValue<Circle>();
                var qcircle = _menu.SubMenu("drawings").Item("drawQ").GetValue<Circle>();

                if (qcircle.Active)
                    Render.Circle.DrawCircle(_player.Position, q.Range, qcircle.Color);

                if (rcircle.Active)
                    Render.Circle.DrawCircle(_player.Position, r.Range, qcircle.Color);
            }

            if (_target.IsValidTarget(q.Range * 2))
                Render.Circle.DrawCircle(_target.Position, _target.BoundingRadius, Color.Red, 10);
        }


        private static void BlitzOnUpdate(EventArgs args)
        {
            _target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Physical);

            // do KS
            GodKS(q);
            GodKS(r);
            GodKS(e);

            var actualHealthSetting = _menu.Item("hneeded").GetValue<Slider>().Value;
            var actualHealthPercent = (int)(_player.Health / _player.MaxHealth * 100);

            if (actualHealthPercent < actualHealthSetting)
            {
                return;
            }

            // use the god hand
            GodHand(_target);

            var powerfistTarget = 
                ObjectManager.Get<Obj_AI_Hero>().First(hero => hero.IsValidTarget(_player.AttackRange));
            if (powerfistTarget.Distance(_player.ServerPosition) <= _player.AttackRange)
            {
                if (_menu.Item("useE").GetValue<bool>() && !q.IsReady())
                    e.CastOnUnit(_player);
            }
        }

        private static void GodHand(Obj_AI_Base target)
        {
            if (!target.IsValidTarget() || !q.IsReady())
            {
                return;
            }

            if (!_menu.Item("combokey").GetValue<KeyBind>().Active)
            {
                return;
            }
         
            if ((target.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value) &&
                (target.Distance(_player.Position) < _menu.Item("dneeded2").GetValue<Slider>().Value))
            {
                var prediction = q.GetPrediction(target);
                if (_menu.Item("dograb" + target.SkinName).GetValue<StringList>().SelectedIndex != 0)
                {
                    if (prediction.Hitchance >= HitChance.High &&
                        _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 2)
                    {
                        q.Cast(prediction.CastPosition);
                    }

                    else if (prediction.Hitchance >= HitChance.Medium &&
                             _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 1)
                    {
                        q.Cast(prediction.CastPosition);
                    }

                    else if (prediction.Hitchance >= HitChance.Low &&
                             _menu.Item("hitchance").GetValue<StringList>().SelectedIndex == 0)
                    {
                        q.Cast(prediction.CastPosition);
                    }
                }
            }



            foreach (
                var unit in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            hero =>
                                hero.IsValidTarget(q.Range) &&
                                _menu.Item("dograb" + hero.SkinName).GetValue<StringList>().SelectedIndex == 2))
                
            {
                if (unit.Distance(_player.Position) > _menu.Item("dneeded").GetValue<Slider>().Value)
                {
                    var prediction = q.GetPrediction(unit);
                    if (prediction.Hitchance == HitChance.Immobile &&
                        _menu.Item("immobile").GetValue<bool>())
                    {
                        q.Cast(prediction.CastPosition);
                    }

                    if (prediction.Hitchance == HitChance.Dashing &&
                        _menu.Item("dashing").GetValue<bool>())
                    {
                        q.Cast(prediction.CastPosition);
                    }
                }
            }
        }

        private static void GodKS(Spell spell)
        {
            if (_menu.Item("killsteal" + spell.Slot).GetValue<bool>() && spell.IsReady())
            {
                foreach (
                    var enemy in
                        ObjectManager.Get<Obj_AI_Hero>()
                            .Where(e => e.IsValidTarget(spell.Range)))
                {
                    var ksDmg = _player.GetSpellDamage(enemy, spell.Slot);
                    if (ksDmg > enemy.Health)
                    {
                        if (spell.Slot.ToString() == "Q")
                        {
                            var po = spell.GetPrediction(enemy);
                            if (po.Hitchance >= HitChance.Medium)
                                spell.Cast(po.CastPosition);
                        }

                        else
                        {
                            spell.CastOnUnit(_player);
                        }
                    }
                }
            }
        }

        private static void BlitzMenu()
        {
            _menu = new Menu("【超神汉化】Kurisu机器人", "blitz", true);

            var blitzOrb = new Menu("走砍", "orbwalker");
            _orbwalker = new Orbwalking.Orbwalker(blitzOrb);
            _menu.AddSubMenu(blitzOrb);

            var blitzTS = new Menu("目标选择", "tselect");
            TargetSelector.AddToMenu(blitzTS);
            _menu.AddSubMenu(blitzTS);

            var menuD = new Menu("显示", "drawings");
            menuD.AddItem(new MenuItem("drawQ", "Q范围")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            menuD.AddItem(new MenuItem("drawR", "R范围")).SetValue(new Circle(true, Color.FromArgb(150, Color.White)));
            _menu.AddSubMenu(menuD);

            var menuG = new Menu("上帝之手", "autograb");
            menuG.AddItem(new MenuItem("hitchance", "命中几率"))
                .SetValue(new StringList(new[] { "低", "中", "高" }, 2));
            menuG.AddItem(new MenuItem("dneeded", "Q最小距离")).SetValue(new Slider(255, 0, (int)q.Range));
            menuG.AddItem(new MenuItem("dneeded2", "Q最大距离")).SetValue(new Slider((int)q.Range, 0, (int)q.Range));
            menuG.AddItem(new MenuItem("dashing", "自动Q突进敌人")).SetValue(true);
            menuG.AddItem(new MenuItem("immobile", "自动Q不动敌人")).SetValue(true);
            menuG.AddItem(new MenuItem("hneeded", "血量控制（不Q）")).SetValue(new Slider(0));
            menuG.AddItem(new MenuItem("sep", ""));

            foreach (
                var e in
                    ObjectManager.Get<Obj_AI_Hero>()
                        .Where(
                            e =>
                                e.Team != _player.Team))
            {
                menuG.AddItem(new MenuItem("Q" + e.SkinName, e.SkinName))
                    .SetValue(new StringList(new[] { "不Q", "正常Q", "自动Q" }, 1));
            }

            _menu.AddSubMenu(menuG);

            var menuK = new Menu("抢人头", "blitzks");
            menuK.AddItem(new MenuItem("killstealQ", "使用Q")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealE", "使用E")).SetValue(false);
            menuK.AddItem(new MenuItem("killstealR", "使用R")).SetValue(false);
            _menu.AddSubMenu(menuK);

            _menu.AddItem(new MenuItem("interrupt", "打断技能")).SetValue(true);
            _menu.AddItem(new MenuItem("useE", "Q中自动E")).SetValue(true);
            _menu.AddItem(new MenuItem("combokey", "连招热键")).SetValue(new KeyBind(32, KeyBindType.Press));
            _menu.AddToMainMenu();
        }
    }
}
