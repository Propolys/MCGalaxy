﻿/*
    Copyright 2015 MCGalaxy
    
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
using System.Threading;

namespace MCGalaxy.Commands {
    
    public sealed class CmdMapAuthor : Command {
        public override string name { get { return "mapauthors"; } }
        public override string shortcut { get { return "authors"; } }
        public override string type { get { return CommandTypes.Games; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public override bool Enabled { get { return Server.ZombieModeOn || Server.lava.active; } }
        
        public override void Use(Player p, string message) {
            if (p == null) { MessageInGameOnly(p); return; }
            p.level.Authors = message;
            Player.SendMessage(p, "Sets the authors of the map to: " + message.Replace(" ", "%S, "));
        }
        
        public override void Help(Player p) {
            Player.SendMessage(p, "%T/mapauthors [name1] <name2>...");
            Player.SendMessage(p, "%HSets the authors of the current map. " +
                               "This is shown to players at the start of rounds in various games.");
        }
    }
}