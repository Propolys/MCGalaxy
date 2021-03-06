//Copyright 2015 MCGalaxy
using System;
using System.Threading;

namespace MCGalaxy.Commands {
    
    public sealed class CmdTpA : Command {
        
        public override string name { get { return "tpa"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Other; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }

        public override void Use(Player p, string message) {
            if (message == "") {
                Help(p); return;
            }
            int number = message.Split(' ').Length;
            if (number > 1) { Help(p); return; }

            Player who = PlayerInfo.FindMatches(p, message);
            if (who == p) { Player.Message(p, "&cError:%S You cannot send yourself a request!"); return; }
            if (who == null) return;
            if (who.listignored.Contains(p.name))
            {
                //Lies
                Player.Message(p, "---------------------------------------------------------");
                Player.Message(p, "Your teleport request has been sent to " + who.ColoredName);
                Player.Message(p, "This request will timeout after " + Colors.aqua + "90%S seconds.");
                Player.Message(p, "---------------------------------------------------------");
                return;
            }
            if (who.name == p.currentTpa) { Player.Message(p, "&cError:%S You already have a pending request with this player."); return; }
            if (p.level != who.level && who.level.IsMuseum) {
                Player.Message(p, "Player \"" + who.ColoredName + "\" is in a museum!"); return;
            }

            if (who.Loading)
            {
                Player.Message(p, "Waiting for " + who.ColoredName + " %Sto spawn...");
                who.BlockUntilLoad(10);
            }

            Player.Message(p, "---------------------------------------------------------");
            Player.Message(p, "Your teleport request has been sent to " + who.ColoredName);
            Player.Message(p, "This request will timeout after " + Colors.aqua + "90 %Sseconds.");
            Player.Message(p, "---------------------------------------------------------");
            Player.Message(who, "---------------------------------------------------------");
            Player.Message(who, p.ColoredName + " %Swould like to teleport to you!");
            Player.Message(who, "Type " + Colors.green + "/tpaccept %Sor " + Colors.maroon + "/tpdeny%S!");
            Player.Message(who, "This request will timeout after " + Colors.aqua + "90 %Sseconds.");
            Player.Message(who, "---------------------------------------------------------");
            who.senderName = p.name;
            who.Request = true;
            p.currentTpa = who.name;
            
            Thread.Sleep(90000);
            if (who.Request) {
                Player.Message(p, "Your teleport request has timed out.");
                Player.Message(who, "Pending teleport request has timed out.");
                who.Request = false;
                who.senderName = "";
                p.currentTpa = "";
            }
        }

        public override void Help(Player p) {
            Player.Message(p, "/tpa <player> - Sends a teleport request to the given player");
            Player.Message(p, "/tpaccept - Accepts a teleport request");
            Player.Message(p, "/tpdeny - Denies a teleport request");
        }
    }

    public sealed class CmdTpAccept : Command {
        
        public override string name { get { return "tpaccept"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Other; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public CmdTpAccept() { }

        public override void Use(Player p, string message) {
            if (!p.Request) {
                Player.Message(p, "&cError: %SYou do not have any pending teleport requests!"); return;
            }
            
            Player who = PlayerInfo.FindExact(p.senderName);
            p.Request = false;
            p.senderName = "";
            if (who == null) {
                Player.Message(p, "The player who requested to teleport to you isn't online anymore."); return;
            }
            
            Player.Message(p, "You have accepted " + who.ColoredName + "%S's teleportation request.");
            Player.Message(who, p.ColoredName +  " %Shas accepted your request. Teleporting now...");            
            who.currentTpa = "";
            Thread.Sleep(1000);
            if (p.level != who.level)
            {
                Level where = p.level;
                PlayerActions.ChangeMap(who, where.name);
                Thread.Sleep(1000);
            }

            who.SendOwnHeadPos(p.pos[0], p.pos[1], p.pos[2], p.rot[0], 0);
        }
        
        public override void Help(Player p) {
            Player.Message(p, "/tpaccept - Accepts a teleport request");
            Player.Message(p, "For use with /tpa");
        }
    }

    public sealed class CmdTpDeny : Command {
        
        public override string name { get { return "tpdeny"; } }
        public override string shortcut { get { return ""; } }
        public override string type { get { return CommandTypes.Other; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Guest; } }
        public CmdTpDeny() { }

        public override void Use(Player p, string message) {
            if (!p.Request ) {
                Player.Message(p, "&cError: %SYou do not have any pending teleport requests!"); return;
            }
            
            Player who = PlayerInfo.FindExact(p.senderName);
            p.Request = false;
            p.senderName = "";
            if (who == null) {
                Player.Message(p, "The player who requested to teleport to you isn't online anymore."); return;
            }
            
            Player.Message(p, "You have denied " + who.ColoredName + "%S's teleportation request.");
            Player.Message(who, p.ColoredName + " %Shas denied your request.");
            who.currentTpa = "";
        }
        
        public override void Help(Player p) {
            Player.Message(p, "/tpdeny - Denies a teleport request");
            Player.Message(p, "For use with /tpa");
        }
    }
}
