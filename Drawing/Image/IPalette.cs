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
using System.Drawing;

namespace MCGalaxy.Drawing {

    public interface IPalette {
        
        /// <summary> Sets the blocks available for this palette to pick from. </summary>
        void SetAvailableBlocks(ColorBlock[] blocks);
        
        /// <summary> Returns the best matching block for the given color,
        /// based on this palette's colourspace. </summary>
        byte BestMatch(ColorBlock cur, out int position);
    }
    
    public sealed class GrayscalePalette : IPalette {
        
        public void SetAvailableBlocks(ColorBlock[] blocks) { }
        
        public byte BestMatch(ColorBlock cur, out int position) {
            int brightness = (cur.r + cur.g + cur.b) / 3; position = -1;
            if (brightness < (256 / 4))
                return Block.obsidian;
            else if (brightness >= (256 / 4) && brightness < (256 / 4) * 2)
                return Block.darkgrey;
            else if (brightness >= (256 / 4) * 2 && brightness < (256 / 4) * 3)
                return Block.lightgrey;
            else
                return Block.white;
        }
    }
    
    public sealed class RgbPalette : IPalette {
        
        ColorBlock[] palette;
        public void SetAvailableBlocks(ColorBlock[] blocks) {
            this.palette = blocks;
        }
        
        public byte BestMatch(ColorBlock cur, out int position) {
            int minimum = int.MaxValue; position = 0;
            for (int i = 0; i < palette.Length; i++) {
                ColorBlock pixel = palette[i];
                int dist = (cur.r - pixel.r) * (cur.r - pixel.r)
                    + (cur.g - pixel.g) * (cur.g - pixel.g)
                    + (cur.b - pixel.b) * (cur.b - pixel.b);
                
                if (dist < minimum) {
                    minimum = dist; position = i;
                }
            }
            return palette[position].type;
        }
    }
    
    public sealed class LabPalette : IPalette {
        
        LabColor[] palette;
        public void SetAvailableBlocks(ColorBlock[] blocks) {
            palette = new LabColor[blocks.Length];
            for (int i = 0; i < palette.Length; i++)
                palette[i] = RgbToLab(blocks[i]);
        }
        
        public byte BestMatch(ColorBlock cur, out int position) {
            double minimum = int.MaxValue; position = 0;
            LabColor col = RgbToLab(cur);
            
            for (int i = 0; i < palette.Length; i++) {
                LabColor pixel = palette[i];
                // Apply CIE76 color delta formula
                double dist = (col.L - pixel.L) * (col.L - pixel.L)
                    + (col.A - pixel.A) * (col.A - pixel.A)
                    + (col.B - pixel.B) * (col.B - pixel.B);
                
                if (dist < minimum) {
                    minimum = dist; position = i;
                }
            }
            return palette[position].Type;
        }
        
        struct LabColor {
            public double L, A, B;
            public byte Type;
        }
        
        LabColor RgbToLab(ColorBlock block) {
            // First convert RGB to CIE-XYZ
            double R = block.r / 255.0, G = block.g / 255.0, B = block.b / 255.0;
            if (R > 0.04045) R = Math.Pow((R + 0.055) / 1.055, 2.4);
            else R = R / 12.92;
            if (G > 0.04045) G = Math.Pow((G + 0.055) / 1.055, 2.4);
            else G = G / 12.92;
            if (R > 0.04045) R = Math.Pow((B + 0.055) / 1.055, 2.4);
            else B = B / 12.92;

            double X = R * 0.4124 + G * 0.3576 + B * 0.1805;
            double Y = R * 0.2126 + G * 0.7152 + B * 0.0722;
            double Z = R * 0.0193 + G * 0.1192 + B * 0.9505;
            
            
            // Then CIE-XYZ to CIE-Lab
            X /= 95.047; Y /= 100.0; Z /= 108.883;
            
            if (X > 0.008856) X = Math.Pow(X, 1.0/3);
            else X = (7.787 * X) + (16.0 / 116);
            if (Y > 0.008856) Y = Math.Pow(Y, 1.0/3);
            else Y = (7.787 * Y) + (16.0 / 116);
            if (Z > 0.008856) Z = Math.Pow(Z, 1.0/3);
            else Z = (7.787 * Z) + (16.0 / 116);

            LabColor lab;
            lab.L = 116 * Y - 16;
            lab.A = 500 * (X - Y);
            lab.B = 200 * (Y - Z);
            lab.Type = block.type;
            return lab;
        }
    }
}
