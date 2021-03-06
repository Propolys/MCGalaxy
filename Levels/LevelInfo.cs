﻿/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
    Dual-licensed under the Educational Community License, Version 2.0 and
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

namespace MCGalaxy {

    public static class LevelInfo {
        
        /// <summary> Array of all current loaded levels. </summary>
        /// <remarks> Note this field is highly volatile, you should cache references to the items array. </remarks>
        public static VolatileArray<Level> Loaded = new VolatileArray<Level>(true);
        
        const StringComparison comp = StringComparison.OrdinalIgnoreCase;
        public static Level Find(string name) {
            Level match = null; int matches = 0;
            Level[] loaded = Loaded.Items;
            
            foreach (Level lvl in loaded) {
                if (lvl.name.Equals(name, comp)) return lvl;
                if (lvl.name.IndexOf(name, comp) >= 0) {
                    match = lvl; matches++;
                }
            }
            return matches == 1 ? match : null;
        }
        
        public static Level FindMatches(Player pl, string name) {
            int matches = 0; return FindMatches(pl, name, out matches);
        }
        
        public static Level FindMatches(Player pl, string name, out int matches) {
            return Extensions.FindMatches<Level>(pl, name, out matches, LevelInfo.Loaded.Items,
        	                                     l => true, l => l.name, "loaded levels");
        }
        
        public static string FindMapMatches(Player pl, string name) {
            int matches = 0; return FindMapMatches(pl, name, out matches);
        }
        
        public static string FindMapMatches(Player pl, string name, out int matches) {
            matches = 0;
            if (!Player.ValidName(name)) {
                Player.Message(pl, "\"{0}\" is not a valid level name.", name); return null;
            }
            
            var files = Directory.EnumerateFiles("levels", "*.lvl");
            string map = Extensions.FindMatches<string>(pl, name, out matches, files,
                                                        l => true, l => Path.GetFileNameWithoutExtension(l), "levels");
            if (map != null) 
                map = Path.GetFileNameWithoutExtension(map);
            return map;
        }

        public static Level FindExact(string name) {
            Level[] loaded = Loaded.Items;
            foreach (Level lvl in loaded) {
                if (lvl.name.Equals(name, comp)) return lvl;
            }
            return null;
        }
        
        public static bool ExistsOffline(string name) {
            return File.Exists(LevelPath(name));
        }
        
        public static bool ExistsBackup(string name, string backup) {
            return File.Exists(BackupPath(name, backup));
        }
        
        public static string LevelPath(string name) {
            return "levels/" + name.ToLower() + ".lvl";
        }
        
        public static string PrevPath(string name) {
            return "levels/prev/" + name.ToLower() + ".lvl.prev";
        }
        
        public static string BackupPath(string name, string backup) {
            return Server.backupLocation + "/" + name + "/" + backup + "/" + name + ".lvl";
        }
        
        public static string GetPropertiesPath(string name) {
            string file = "levels/level properties/" + name + ".properties";
            if (!File.Exists(file)) file = "levels/level properties/" + name;
            if (!File.Exists(file)) return null;
            return file;
        }
        
        public static string FindOfflineProperty(string name, string propKey) {
            string file = GetPropertiesPath(name);
            if (file == null) return null;

            string[] lines = null;
            try {
                lines = File.ReadAllLines(file);
            } catch {
                return null;
            }
            
            foreach (string line in lines) {
                try {
                    if (line == "" || line[0] == '#') continue;
                    int index = line.IndexOf(" = ");
                    if (index == -1) continue;
                    
                    string key = line.Substring(0, index).ToLower();
                    if (key == propKey) return line.Substring(index + 3);
                } catch (Exception e) {
                    Server.ErrorLog(e);
                }
            }
            return null;
        }
    }
}