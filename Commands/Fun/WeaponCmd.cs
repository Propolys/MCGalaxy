﻿/*
    Copyright 2011 MCForge
    
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
    
    public abstract class WeaponCmd : Command {
        
        public override string type { get { return CommandTypes.Other; } }
        public override bool museumUsable { get { return false; } }
        public override LevelPermission defaultRank { get { return LevelPermission.AdvBuilder; } }
        protected abstract string Weapon { get; }
        
        public override void Use(Player p, string message) {
            if (!p.level.guns) {
                Player.Message(p, Weapon + "s cannot be used on this map!"); return;
            }
            if (p.Game.hasflag != null) {
                Player.Message(p, "You can't use a " + Weapon.ToLower() + " while you have the flag!"); return;
            }

            if (p.aiming && message == "") {
                p.aiming = false;
                p.ClearBlockchange();
                Player.Message(p, "Disabled " + Weapon.ToLower() );
                return;
            }

            CatchPos cpos = default(CatchPos);
            cpos.ending = GetEnd(p, message);
            if (cpos.ending == EndType.Invalid) return;
            p.blockchangeObject = cpos;
            p.ClearBlockchange();
            p.Blockchange += new Player.BlockchangeEventHandler(Blockchange1);

            p.SendMessage(Weapon + " mode engaged, fire at will");
            if (p.aiming) return;

            p.aiming = true;
            Thread aimThread = new Thread(() => DoAim(p));
            aimThread.Name = "MCG_WeaponAim";
            aimThread.Start();
        }
        
        EndType GetEnd(Player p, string mode) {
            if (mode == "") return EndType.Normal;
            if (mode.CaselessEq("destroy")) return EndType.Destroy;
            if (mode.CaselessEq("tp") || mode.CaselessEq("teleport")) return EndType.Teleport;
            
            if (mode.CaselessEq("explode")) {
                if (!p.allowTnt) Player.Message(p, "Tnt usage is currently disallowed, switching to normal gun.");
                return p.allowTnt ? EndType.Explode : EndType.Destroy;
            }
            if (mode.CaselessEq("laser")) {
                if (!p.allowTnt) Player.Message(p, "Tnt usage is currently disallowed, switching to normal gun.");
                return p.allowTnt ? EndType.Laser : EndType.Destroy;
            }
            Help(p);
            return EndType.Invalid;
        }
        
        void DoAim(Player p) {
            List<Vec3U16> lastSent = new List<Vec3U16>(), toSend = new List<Vec3U16>();
            while (p.aiming) {
                double a = Math.Sin(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
                double b = Math.Cos(((double)(128 - p.rot[0]) / 256) * 2 * Math.PI);
                double c = Math.Cos(((double)(p.rot[1] + 64) / 256) * 2 * Math.PI);

                try {
                    ushort y = (ushort)Math.Round((ushort)(p.pos[1] / 32) + c * 3);
                    ushort x = (ushort)Math.Round((ushort)(p.pos[0] / 32) + a * 3);
                    ushort z = (ushort)Math.Round((ushort)(p.pos[2] / 32) + b * 3);

                    int dirX = Math.Sign(a) >= 0 ? 1 : -1;
                    int dirZ = Math.Sign(b) >= 0 ? 1 : -1;
                    CheckTile(p.level, toSend, x, y, z);
                    CheckTile(p.level, toSend, x + dirX, y, z);
                    CheckTile(p.level, toSend, x, y, z + dirZ);
                    CheckTile(p.level, toSend, x + dirX, y, z + dirZ);

                    // Revert all glass blocks now not in the ray from the player's direction
                    for (int i = 0; i < lastSent.Count; i++) {
                        Vec3U16 cP = lastSent[i];
                        if (toSend.Contains(cP)) continue;
                        
                        if (p.level.IsValidPos(cP))
                            p.RevertBlock(cP.X, cP.Y, cP.Z);
                        lastSent.RemoveAt(i); i--;
                    }

                    // Place the new glass blocks that are in the ray from the player's direction
                    foreach (Vec3U16 cP in toSend) {
                        if (lastSent.Contains(cP)) continue;
                        lastSent.Add(cP);
                        p.SendBlockchange(cP.X, cP.Y, cP.Z, Block.glass);
                    }
                    toSend.Clear();
                }
                catch { }
                Thread.Sleep(20);
            }
            
            foreach (Vec3U16 cP in lastSent) {
                if (p.level.IsValidPos(cP))
                    p.RevertBlock(cP.X, cP.Y, cP.Z);
            }
        }
        
        void CheckTile(Level lvl, List<Vec3U16> toSend, int x, int y, int z) {
            Vec3U16 pos;
            if (lvl.GetTile((ushort)x, (ushort)(y - 1), (ushort)z) == Block.air) {
                pos.X = (ushort)x; pos.Y = (ushort)(y - 1); pos.Z = (ushort)z;
                toSend.Add(pos);
            }
            if (lvl.GetTile((ushort)x, (ushort)y, (ushort)z) == Block.air) {
                pos.X = (ushort)x; pos.Y = (ushort)y; pos.Z = (ushort)z;
                toSend.Add(pos);
            }
        }
        
        protected abstract void Blockchange1(Player p, ushort x, ushort y, ushort z, byte type, byte extType);
        
        protected Player GetPlayer(Player p, Vec3U16 pos, bool skipSelf) {
            Player[] players = PlayerInfo.Online.Items;
            foreach (Player pl in players) {
                if (pl.level != p.level) continue;
                if (p == pl && skipSelf) continue;
                
                if (Math.Abs((pl.pos[0] / 32) - pos.X) <= 1
                    && Math.Abs((pl.pos[1] / 32) - pos.Y) <= 1
                    && Math.Abs((pl.pos[2] / 32) - pos.Z) <= 1)
                {
                    return pl;
                }
            }
            return null;
        }
        
        protected static void DoTeleport(Player p, Vec3U16 pos) {
            try {
                p.SendPos(0xFF, (ushort)(pos.X * 32), (ushort)(pos.Y * 32 + 32), (ushort)(pos.Z * 32), p.rot[0], p.rot[1]);
            } catch {
            }
        }
        
        protected bool HandlesHitBlock(Player p, byte type, CatchPos bp, Vec3U16 pos, bool doExplode) {
            if (p.level.physics < 2 || bp.ending == EndType.Teleport
                || bp.ending == EndType.Normal) return true;
            
            if (bp.ending == EndType.Destroy) {
                if ((!Block.FireKill(type) && !Block.NeedRestart(type)) && type != Block.glass) {
                    return true;
                }
            } else if (p.level.physics >= 3) {
                if (type != Block.glass && p.allowTnt) {
                    if (doExplode)
                        p.level.MakeExplosion(pos.X, pos.Y, pos.Z, 1);
                    return true;
                }
            } else {
                return true;
            }
            return false;
        }

        protected struct CatchPos { public ushort x, y, z; public EndType ending; }
        protected enum EndType { Invalid, Normal, Destroy, Teleport, Explode, Laser };
    }
}
