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
using System.Windows.Forms;

namespace DarkViperOhko
{
    public class OhkoConstants
    {
        public static bool enabled = true;
        public static IniFile Configuration = new IniFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ohko_config.ini"));

        public static Keys IncreaseDeathCount = (Keys) Enum.Parse(typeof(Keys), Configuration.IniReadValue("Keybinds", "IncreaseDeathCount"));
        public static Keys DecreaseDeathCount = (Keys)Enum.Parse(typeof(Keys), Configuration.IniReadValue("Keybinds", "DecreaseDeathCount"));
        public static Keys ToggleMod = (Keys)Enum.Parse(typeof(Keys), Configuration.IniReadValue("Keybinds", "ToggleMod"));
        public static bool ShowDefaultDeathCounter = bool.Parse(Configuration.IniReadValue("Display", "ShowDefaultDeathCounter"));
    }
    public class DeathTracker: Script
    {
        public static int deaths;
        public static bool isDead;
        public static string deathSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ohko_stats.txt");

        public DeathTracker()
        {
            Tick += DeathCountScript_Tick;
            KeyUp += DeathTracker_KeyUp;
            try
            {
                if (File.Exists(deathSavePath))
                {
                    deaths = int.Parse(File.ReadAllText(deathSavePath));
                }
            }
            catch (Exception)
            {
                Notification.Show("Failed to load deaths. Is ohko_stats.txt corrupt?");
            }
        }

        private void DeathTracker_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == OhkoConstants.IncreaseDeathCount)
            {
                SetDeathCount(deaths + 1);
            } else if (e.KeyCode == OhkoConstants.DecreaseDeathCount)
            {
                SetDeathCount(deaths - 1);
            }
        }

        private void SetDeathCount(int newd)
        {
            deaths = newd;
            try
            {
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ohko_stats.txt"), deaths.ToString());
            }
            catch (Exception ex)
            {
                Notification.Show(ex.Message);
            }
        }
        
        private readonly TextElement deathCounter = new TextElement("0", new PointF(1100, 450), 1f, Color.White, GTA.UI.Font.ChaletComprimeCologne, Alignment.Left, true, true);

        private readonly TextElement debug = new TextElement("0", new PointF(100, 450), 1f, Color.White, GTA.UI.Font.ChaletComprimeCologne, Alignment.Left, true, true);

        private long tick = 0;
        private readonly TextElement ticke = new TextElement("0", new PointF(100, 350), 1f, Color.White, GTA.UI.Font.ChaletComprimeCologne, Alignment.Left, true, true);
    
        private void DeathCountScript_Tick(object sender, EventArgs e)
        {
            if (isDead && !Game.Player.IsDead)
            {
                isDead = false;
            }
            else if (!isDead && Game.Player.IsDead)
            {
                isDead = true;
                SetDeathCount(deaths + 1);
            }
            
            if (OhkoConstants.ShowDefaultDeathCounter && OhkoConstants.enabled)
            {
                deathCounter.Caption = deaths + " death" + (DeathTracker.deaths != 1 ? "s" : "");
                deathCounter.ScaledDraw();
            }

            //debug.Caption = $"isDead: {isDead} GameDead: {Game.Player.IsDead}";
            //debug.ScaledDraw();
            //tick++;
            //ticke.Caption = tick.ToString();
            //ticke.ScaledDraw();
        }
    }

    public class OhkoScript: Script
    {
        private Model trevor = new Model(PedHash.Trevor);
        public OhkoScript()
        {
            Tick += OhkoScript_Tick;
            KeyUp += OhkoScript_KeyUp;
            Interval = 1000;
            Notification.Show("Loaded One-Hit Knock-Out.");
        }

        private void OhkoScript_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == OhkoConstants.ToggleMod)
            {
                OhkoConstants.enabled = !OhkoConstants.enabled;
                if (!OhkoConstants.enabled)
                {
                    Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.Player.Character.Handle, 200);
                    Function.Call(Hash.SET_ENTITY_HEALTH, Game.Player.Character.Handle, 200);

                    Game.Player.IsSpecialAbilityEnabled = true;
                    Game.Player.RefillSpecialAbility();
                }

                Notification.Show(OhkoConstants.enabled ? "OHKO Enabled" : "OHKO Disabled");
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
                if (!OhkoConstants.enabled)
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
            catch (Exception e2)
            {
                try
                {
                    Notification.Show("Exception during tick: " + e2.Message);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
