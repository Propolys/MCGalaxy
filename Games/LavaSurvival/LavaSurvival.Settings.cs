﻿/*
	Copyright 2011 MCForge
		
	Dual-licensed under the	Educational Community License, Version 2.0 and
	the GNU General Public License, Version 3 (the "Licenses"); you may
	not use this file except in compliance with the Licenses. You may
	obtain a copy of the Licenses at
	
	http://www.opensource.org/licenses/ecl2.php
	http://www.gnu.org/licenses/gpl-3.0.html
	
	Unless required by applicable law or agreed to in writing,
	software distributed under the Licenses are distributed on an "AS IS"
	BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
	or implied. See the Licenses for the specific language governing
	permissions and limitations under the Licenses.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;

namespace MCGalaxy.Games
{
    public sealed partial class LavaSurvival
    {

        public MapData GenerateMapData(MapSettings settings)
        {
            MapData data = new MapData(settings);
            data.killer = rand.Next(1, 101) <= settings.killer;
            data.destroy = rand.Next(1, 101) <= settings.destroy;
            data.water = rand.Next(1, 101) <= settings.water;
            data.layer = rand.Next(1, 101) <= settings.layer;
            data.fast = rand.Next(1, 101) <= settings.fast && !data.water;
            data.block = data.water ? (data.killer ? Block.activedeathwater : Block.water) : (data.fast ? (data.killer ? Block.fastdeathlava : Block.lava_fast) : (data.killer ? Block.activedeathlava : Block.lava));
            return data;
        }

        public void LoadSettings()
        {
            if (!File.Exists("properties/lavasurvival.properties"))
            {
                SaveSettings();
                return;
            }

            foreach (string line in File.ReadAllLines("properties/lavasurvival.properties"))
            {
                try
                {
                    if (line[0] != '#')
                    {
                        string value = line.Substring(line.IndexOf(" = ") + 3);
                        switch (line.Substring(0, line.IndexOf(" = ")).ToLower())
                        {
                            case "start-on-startup":
                                startOnStartup = bool.Parse(value);
                                break;
                            case "send-afk-to-main":
                                sendAfkMain = bool.Parse(value);
                                break;
                            case "vote-count":
                                voteCount = (byte)MathHelper.Clamp(decimal.Parse(value), 2, 10);
                                break;
                            case "vote-time":
                                voteTime = double.Parse(value);
                                break;
                            case "lives":
                                lifeNum = int.Parse(value);
                                break;
                            case "setup-rank":
                                if (Group.Find(value.ToLower()) != null)
                                    setupRank = Group.Find(value.ToLower()).Permission;
                                break;
                            case "control-rank":
                                if (Group.Find(value.ToLower()) != null)
                                    controlRank = Group.Find(value.ToLower()).Permission;
                                break;
                            case "maps":
                                foreach (string mapname in value.Split(','))
                                    if(!String.IsNullOrEmpty(mapname) && !maps.Contains(mapname)) maps.Add(mapname);
                                break;
                        }
                    }
                }
                catch (Exception e) { Server.ErrorLog(e); }
            }
        }
        public void SaveSettings()
        {
            using (StreamWriter w = new StreamWriter("properties/lavasurvival.properties")) {
                w.WriteLine("#Lava Survival main properties");
                w.WriteLine("start-on-startup = " + startOnStartup.ToString().ToLower());
                w.WriteLine("send-afk-to-main = " + sendAfkMain.ToString().ToLower());
                w.WriteLine("vote-count = " + voteCount.ToString());
                w.WriteLine("vote-time = " + voteTime.ToString());
                w.WriteLine("lives = " + lifeNum.ToString());
                w.WriteLine("setup-rank = " + Level.PermissionToName(setupRank).ToLower());
                w.WriteLine("control-rank = " + Level.PermissionToName(controlRank).ToLower());
                w.WriteLine("maps = " + maps.Concatenate(","));
            }
        }

        public MapSettings LoadMapSettings(string name)
        {
            MapSettings settings = new MapSettings(name);
            if (!Directory.Exists(propsPath)) Directory.CreateDirectory(propsPath);
            if (!File.Exists(propsPath + name + ".properties"))
            {
                SaveMapSettings(settings);
                return settings;
            }

            foreach (string line in File.ReadAllLines(propsPath + name + ".properties"))
            {
                try
                {
                    if (line[0] != '#')
                    {
                        string[] sp;
                        string value = line.Substring(line.IndexOf(" = ") + 3);
                        switch (line.Substring(0, line.IndexOf(" = ")).ToLower())
                        {
                            case "fast-chance":
                                settings.fast = (byte)MathHelper.Clamp(decimal.Parse(value), 0, 100);
                                break;
                            case "killer-chance":
                                settings.killer = (byte)MathHelper.Clamp(decimal.Parse(value), 0, 100);
                                break;
                            case "destroy-chance":
                                settings.destroy = (byte)MathHelper.Clamp(decimal.Parse(value), 0, 100);
                                break;
                            case "water-chance":
                                settings.water = (byte)MathHelper.Clamp(decimal.Parse(value), 0, 100);
                                break;
                            case "layer-chance":
                                settings.layer = (byte)MathHelper.Clamp(decimal.Parse(value), 0, 100);
                                break;
                            case "layer-height":
                                settings.layerHeight = int.Parse(value);
                                break;
                            case "layer-count":
                                settings.layerCount = int.Parse(value);
                                break;
                            case "layer-interval":
                                settings.layerInterval = double.Parse(value);
                                break;
                            case "round-time":
                                settings.roundTime = double.Parse(value);
                                break;
                            case "flood-time":
                                settings.floodTime = double.Parse(value);
                                break;
                            case "block-flood":
                                sp = value.Split(',');
                                settings.blockFlood = new Vec3U16(ushort.Parse(sp[0]), ushort.Parse(sp[1]), ushort.Parse(sp[2]));
                                break;
                            case "block-layer":
                                sp = value.Split(',');
                                settings.blockLayer = new Vec3U16(ushort.Parse(sp[0]), ushort.Parse(sp[1]), ushort.Parse(sp[2]));
                                break;
                            case "safe-zone":
                                sp = value.Split('-');
                                string[] p1 = sp[0].Split(','), p2 = sp[1].Split(',');
                                settings.safeZone = new Vec3U16[] { new Vec3U16(ushort.Parse(p1[0]), ushort.Parse(p1[1]), ushort.Parse(p1[2])), new Vec3U16(ushort.Parse(p2[0]), ushort.Parse(p2[1]), ushort.Parse(p2[2])) };
                                break;
                        }
                    }
                }
                catch (Exception e) { Server.ErrorLog(e); }
            }
            return settings;
        }
        public void SaveMapSettings(MapSettings settings)
        {
            if (!Directory.Exists(propsPath)) Directory.CreateDirectory(propsPath);

            using (StreamWriter w = new StreamWriter(propsPath + settings.name + ".properties")) {
                w.WriteLine("#Lava Survival properties for " + settings.name);
                w.WriteLine("fast-chance = " + settings.fast);
                w.WriteLine("killer-chance = " + settings.killer);
                w.WriteLine("destroy-chance = " + settings.destroy);
                w.WriteLine("water-chance = " + settings.water);
                w.WriteLine("layer-chance = " + settings.layer);
                w.WriteLine("layer-height = " + settings.layerHeight);
                w.WriteLine("layer-count = " + settings.layerCount);
                w.WriteLine("layer-interval = " + settings.layerInterval);
                w.WriteLine("round-time = " + settings.roundTime);
                w.WriteLine("flood-time = " + settings.floodTime);
                w.WriteLine("block-flood = " + settings.blockFlood.ToString());
                w.WriteLine("block-layer = " + settings.blockLayer.ToString());
                w.WriteLine(String.Format("safe-zone = {0}-{1}", settings.safeZone[0].ToString(), settings.safeZone[1].ToString()));
            }
        }

        // Internal classes
        public class MapSettings
        {
            public string name;
            public byte fast, killer, destroy, water, layer;
            public int layerHeight, layerCount;
            public double layerInterval, roundTime, floodTime;
            public Vec3U16 blockFlood, blockLayer;
            public Vec3U16[] safeZone;

            public MapSettings(string name)
            {
                this.name = name;
                fast = 0;
                killer = 100;
                destroy = 0;
                water = 0;
                layer = 0;
                layerHeight = 3;
                layerCount = 10;
                layerInterval = 2;
                roundTime = 15;
                floodTime = 5;
                blockFlood = new Vec3U16();
                blockLayer = new Vec3U16();
                safeZone = new Vec3U16[2];
            }
        }

        public class MapData : IDisposable
        {
            public bool fast, killer, destroy, water, layer;
            public byte block;
            public int currentLayer;
            public Timer roundTimer, floodTimer, layerTimer;

            public MapData(MapSettings settings)
            {
                fast = false;
                killer = false;
                destroy = false;
                water = false;
                layer = false;
                block = Block.lava;
                currentLayer = 1;
                roundTimer = new Timer(TimeSpan.FromMinutes(settings.roundTime).TotalMilliseconds); roundTimer.AutoReset = false;
                floodTimer = new Timer(TimeSpan.FromMinutes(settings.floodTime).TotalMilliseconds); floodTimer.AutoReset = false;
                layerTimer = new Timer(TimeSpan.FromMinutes(settings.layerInterval).TotalMilliseconds); layerTimer.AutoReset = true;
            }

            public void Dispose()
            {
                roundTimer.Dispose();
                floodTimer.Dispose();
                layerTimer.Dispose();
            }
        }
    }
}
