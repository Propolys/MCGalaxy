/*
    Copyright 2010 MCSharp team (Modified for use with MCZall/MCLawl/MCGalaxy)
    
    Dual-licensed under the    Educational Community License, Version 2.0 and
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
namespace MCGalaxy.Commands
{
    public sealed class CmdBlockSet : Command
    {
        public override string name { get { return "blockset"; } }
        public override string shortcut { get { return ""; } }
       public override string type { get { return CommandTypes.Moderation; } }
        public override bool museumUsable { get { return true; } }
        public override LevelPermission defaultRank { get { return LevelPermission.Operator; } }
        public CmdBlockSet() { }

        public override void Use(Player p, string message)
        {
            if (message == "" || message.IndexOf(' ') == -1) { Help(p); return; }

            byte foundBlock = Block.Byte(message.Split(' ')[0]);
            if (foundBlock == Block.Zero) { Player.Message(p, "Could not find block entered"); return; }
            LevelPermission newPerm = Level.PermissionFromName(message.Split(' ')[1]);
            if (newPerm == LevelPermission.Null) { Player.Message(p, "Could not find rank specified"); return; }
            if (p != null && newPerm > p.group.Permission) { Player.Message(p, "Cannot set to a rank higher than yourself."); return; }

            if (p != null && !Block.canPlace(p, foundBlock)) { Player.Message(p, "Cannot modify a block set for a higher rank"); return; }

            Block.BlockList[foundBlock].lowestRank = newPerm;
            Block.SaveBlocks(Block.BlockList);
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players) {
                if (!pl.HasCpeExt(CpeExt.BlockPermissions)) continue;
                
                int count = pl.hasCustomBlocks ? Block.CpeCount : Block.OriginalCount;
                if (foundBlock < count) {
                    bool canAffect = Block.canPlace(pl, foundBlock);
                    pl.SendSetBlockPermission(foundBlock, canAffect, canAffect);
                }
            }

            Player.GlobalMessage("&d" + Block.Name(foundBlock) + "%S's permission was changed to " + Level.PermissionToName(newPerm));
            if (p == null)
                Player.Message(p, Block.Name(foundBlock) + "'s permission was changed to " + Level.PermissionToName(newPerm));
        }
        
        public override void Help(Player p)
        {
            Player.Message(p, "/blockset [block] [rank] - Changes [block] rank to [rank]");
            Player.Message(p, "Only blocks you can use can be modified");
            Player.Message(p, "Available ranks: " + Group.concatList());
        }
    }
}
