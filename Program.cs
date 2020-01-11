using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA.Native;
using GTA;
using System.Drawing;
using GTA.UI;
using Microsoft.Win32;
using System.IO;

namespace DarkViperOhko
{
    public class DeathCountCounterScript : Script
    {
        private TextElement deathCounter = new TextElement("0", new PointF(1100, 600), 1f, Color.White, GTA.UI.Font.ChaletComprimeCologne, Alignment.Left, true, true);
        public static string noDrawPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "okho_no_draw_counter.txt");

        public DeathCountCounterScript()
        {
            this.Tick += DeathCountCounterScript_Tick;
        }

        private void DeathCountCounterScript_Tick(object sender, EventArgs e)
        {
            if (!File.Exists(noDrawPath))
            {
                deathCounter.Caption = DeathCountScript.deaths.ToString() + " death" + (DeathCountScript.deaths != 1 ? "s" : "");
                deathCounter.Draw();
            }
        }
    }
    public class DeathCountScript: Script
    {
        public static int deaths = 0;
        public static long lastDeathTime = 0;
        public static string deathSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "okho_stats.txt");

        public DeathCountScript()
        {
            this.Tick += DeathCountScript_Tick;
            try
            {
                if (File.Exists(deathSavePath))
                {
                    deaths = int.Parse(File.ReadAllText(deathSavePath));
                }
            }
            catch (Exception)
            {

            }
        }

        private void DeathCountScript_Tick(object sender, EventArgs e)
        {
            if (Game.Player.IsDead && (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastDeathTime) > 10000)
            {
                deaths++;
                lastDeathTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                try
                {
                    System.IO.File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "okho_stats.txt"), deaths.ToString());
                } catch (Exception ex)
                {
                    Notification.Show(ex.Message);
                }
            }
        }
    }

    public class OhkoScript: Script
    {
        private bool enabled = true;
        private Model trevor;
        public OhkoScript()
        {
            var mod = new Model(PedHash.Trevor);
            mod.Request(500);
            trevor = mod;
            this.Tick += OhkoScript_Tick;
            KeyUp += OhkoScript_KeyUp;
            this.Interval = 1000;
            Notification.Show("Loaded Abyssal's One-Hit Knock-Out.");
        }

        private void OhkoScript_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.F8)
            {
                enabled = !enabled;
                if (!enabled)
                {
                    Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle, 200);
                    Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character.Handle, 200);
                    Game.Player.IsSpecialAbilityEnabled = true;
                    Game.Player.RefillSpecialAbility();
                }
                new TextElement(enabled ? "OHKO Enabled" : "OHKO Disabled", new PointF(Screen.Width / 2, Screen.Height / 2), 1.0f).ScaledDraw();
                OhkoScript_Tick(null, null);
            }
        }

        private bool IsTrevor()
        {
            return Game.Player.Character.Model == trevor;
        }

        private void OhkoScript_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!enabled)
                {
                    if (Function.Call<int>(Hash.GET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle) != 200)
                    {
                        Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle, 200);
                    }
                    Game.Player.IsSpecialAbilityEnabled = true;
                } 
                else
                {
                    if (Function.Call<int>(Hash.GET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle) > 101)
                    {
                        Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle, 101);
                        Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character.Handle, 101);
                    }
                    if (Function.Call<int>(Hash.GET_ENTITY_HEALTH, Game.Player.Character.Handle) > 101)
                    {
                        Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character.Handle, 101);
                    }

                    Game.Player.Character.Armor = 0;
                    Game.Player.MaxArmor = 100;


                    if (IsTrevor())
                    {
                        Game.Player.IsSpecialAbilityEnabled = false;
                        Game.Player.DepleteSpecialAbility();
                    } else
                    {
                        Game.Player.IsSpecialAbilityEnabled = true;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
