﻿#region license

// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Renderer;
using ClassicUO.Utility;

namespace ClassicUO.Game.UI.Controls
{
    internal abstract class GumpPicBase : Control
    {
        private ushort _graphic;

        protected GumpPicBase()
        {
            CanMove = true;
            AcceptMouseInput = true;
        }

        public ushort Graphic
        {
            get => _graphic;
            set
            {
                //if (_graphic != value)
                {
                    _graphic = value;

                    UOTexture texture = GumpsLoader.Instance.GetTexture(_graphic);

                    if (texture == null)
                    {
                        Dispose();

                        return;
                    }

                    Width = texture.Width;
                    Height = texture.Height;
                }
            }
        }

        public ushort Hue { get; set; }


        public override bool Contains(int x, int y)
        {
            UOTexture texture = GumpsLoader.Instance.GetTexture(Graphic);

            if (texture == null)
            {
                return false;
            }

            if (texture.Contains(x - Offset.X, y - Offset.Y))
            {
                return true;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                Control c = Children[i];

                // might be wrong x, y. They should be calculated by position
                if (c.Contains(x, y))
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal class GumpPic : GumpPicBase
    {
        public GumpPic(int x, int y, ushort graphic, ushort hue)
        {
            X = x;
            Y = y;
            Graphic = graphic;
            Hue = hue;
            IsFromServer = true;
        }

        public GumpPic(List<string> parts) : this
        (
            int.Parse(parts[1]), int.Parse(parts[2]), UInt16Converter.Parse(parts[3]),
            (ushort) (parts.Count > 4 ?
                TransformHue((ushort) (UInt16Converter.Parse(parts[4].Substring(parts[4].IndexOf('=') + 1)) + 1)) :
                0)
        )
        {
        }

        public bool IsPartialHue { get; set; }
        public bool ContainsByBounds { get; set; }
        public bool IsVirtue { get; set; }

        protected override bool OnMouseDoubleClick(int x, int y, MouseButtonType button)
        {
            if (IsVirtue && button == MouseButtonType.Left)
            {
                NetClient.Socket.Send(new PVirtueGumpReponse(World.Player, Graphic));

                return true;
            }

            return base.OnMouseDoubleClick(x, y, button);
        }

        public override bool Contains(int x, int y)
        {
            return ContainsByBounds || base.Contains(x, y);
        }

        private static ushort TransformHue(ushort hue)
        {
            if (hue <= 2)
            {
                hue = 0;
            }

            //if (hue < 2)
            //    hue = 1;
            return hue;
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (IsDisposed)
            {
                return false;
            }

            ResetHueVector();
            ShaderHueTranslator.GetHueVector(ref HueVector, Hue, IsPartialHue, Alpha, true);

            UOTexture texture = GumpsLoader.Instance.GetTexture(Graphic);

            if (texture != null)
            {
                batcher.Draw2D(texture, x, y, Width, Height, ref HueVector);
            }

            return base.Draw(batcher, x, y);
        }
    }
}