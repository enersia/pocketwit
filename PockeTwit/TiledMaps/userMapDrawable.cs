﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PockeTwit
{
    class userMapDrawable : TiledMaps.IGraphicsDrawable
    {
        public Library.User userToDraw;
        public char charToUse;
        public bool IsOpened = false;
        public Bitmap markerImage = null;

        private Brush B = new SolidBrush(Color.Black);
        #region IGraphicsDrawable Members
        public Rectangle Location = new Rectangle();

        public void Draw(System.Drawing.Graphics graphics, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
        {
            if (IsOpened)
            {
                using (Brush b= new SolidBrush(Color.Red))
                {

                    graphics.FillPolygon(b, new Point[]{
                        new Point(destRect.Left, destRect.Top),
                        new Point(destRect.Right, destRect.Top),
                        new Point(destRect.Right, destRect.Bottom-ClientSettings.Margin),
                        new Point(destRect.Left + (destRect.Width/2)+ClientSettings.Margin, destRect.Bottom-ClientSettings.Margin),
                        new Point(destRect.Left + (destRect.Width/2), destRect.Bottom),
                        new Point(destRect.Left + (destRect.Width/2)-ClientSettings.Margin, destRect.Bottom-ClientSettings.Margin),
                        new Point(destRect.Left, destRect.Bottom - ClientSettings.Margin),
                        new Point(destRect.Left, destRect.Top)});
                    graphics.DrawImage(ThrottledArtGrabber.GetArt(this.userToDraw.screen_name, this.userToDraw.high_profile_image_url), destRect.X + ClientSettings.Margin, destRect.Y + ClientSettings.Margin);
                }
            }
            else
            {
                TiledMaps.IGraphicsDrawable graphicsDrawable = ThrottledArtGrabber.mapMarkerImage as TiledMaps.IGraphicsDrawable;
                graphicsDrawable.Draw(graphics, destRect, sourceRect);
                //graphics.DrawString(charToUse.ToString(), ClientSettings.SmallFont, B, destRect);
            }
            Location = destRect;
        }

        #endregion

        #region IMapDrawable Members

        public int Width
        {
            get 
            {
                if (IsOpened)
                {
                    return ClientSettings.SmallArtSize + (ClientSettings.Margin * 2);
                }
                else
                {
                    return ThrottledArtGrabber.mapMarkerImage.Width;
                }
            }
        }

        public int Height
        {
            get 
            {
                if (IsOpened)
                {
                    return ClientSettings.SmallArtSize + (ClientSettings.Margin * 2);
                }
                else
                {
                    return ThrottledArtGrabber.mapMarkerImage.Height;
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
