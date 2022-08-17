using Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsAimBot_1
{
    public partial class Form1 : Form
    {
        #region offsets

        string PlayerBase = "ac_client.exe+0x17E0A8";
        string EntityList = "ac_client.exe+0x18AC04";
        string Health = ",EC";
        string Name = ",205";
        string X = ",2C";
        string Y = ",30";
        string Z = ",28";
        string ViewY = ",38";
        string ViewX = ",34";


        #endregion
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys keys);
        Mem m = new Mem();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            int PID = m.GetProcIdFromName("ac_client");
            if (PID > 0)
            {
                m.OpenProcess(PID);
                Thread AB = new Thread(Aimbot) { IsBackground = true };
                AB.Start();
            }
        }
        void Aimbot()
        {
            while (true)
            {
                if (GetAsyncKeyState(Keys.RButton) < 0)
                {
                    var Localplayer = GetLocal();
                    var Players = GetPlayers(Localplayer);

                    Players = Players.OrderBy(o => o.Magnitube).ToList();
                    if (Players.Count != 0)
                    {
                        Aim(Localplayer , Players[0]);
                    }
                }

                Thread.Sleep(2);
            }
        }
        Player GetLocal()
        {
            var Player = new Player
            {
                X = m.ReadFloat(PlayerBase + X),
                Y = m.ReadFloat(PlayerBase + Y),
                Z = m.ReadFloat(PlayerBase + Z),
            };
            return Player;
        }
        float GetMag(Player player, Player entity)
        {
            float mag;
            mag = (float)Math.Sqrt(Math.Pow(entity.X - player.X, 2) +
                Math.Pow(entity.Y - player.Y, 2) + Math.Pow(entity.Z - player.Z, 2));// 3D

            return mag;
        }
        
        void Aim(Player Enemy, Player player)
        {
            float deltaX = Enemy.X - player.X;
            float deltaY = Enemy.Y - player.Y;

            float viewX = (float)(Math.Atan2(deltaX, deltaY) / Math.PI * 180 + 180); 

            float deltaZ = Enemy.Z - player.Z;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            float viewY = (float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);


            m.WriteMemory(PlayerBase + ViewX, "float", viewX.ToString());
            m.WriteMemory(PlayerBase + ViewY, "float", viewY.ToString());



        }
        List<Player> GetPlayers(Player local)// الحصول على اللاعبين
        {
            var players = new List<Player>();
            for (int i = 0; i < 20; i++)
            {
                var CurrentStr = EntityList + ",0x" + (i * 0x4).ToString("X");
                var player = new Player
                {
                    X = m.ReadFloat(CurrentStr + X),
                    Y = m.ReadFloat(CurrentStr + Y),
                    Z = m.ReadFloat(CurrentStr + Z),
                    Health = m.ReadInt(CurrentStr + Health),
                   
                    
                };
                player.Magnitube = GetMag(local, player);
                if (player.Health > 0 && player.Health < 102)
                {
                    players.Add(player);
                }
            }
            return players;
        }
    }
}
