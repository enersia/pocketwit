using System;

using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace PockeTwit.FingerUI
{
    class MoreResultsItem : IDisplayItem
    {
        TweetList _list;
        string _searchString;
        bool _saveResults;

        public MoreResultsItem(TweetList list, string searchString, bool saveResults)
        {
            _list = list;
            _searchString = searchString;
            _saveResults = saveResults;
        }

        #region IDisplayItem Members

        Graphics _parentGraphics;
        public System.Drawing.Graphics ParentGraphics
        {
            set 
            {
                _parentGraphics = value;
            }
        }

        public KListControl Parent { get; set; }
        public int Index { get; set; }

        public void OnMouseClick(Point p)
        {
            _list.ShowSearchResults(_searchString, _saveResults, true);
        }


        public void Render(System.Drawing.Graphics g, System.Drawing.Rectangle bounds)
        {
            try
            {
                g.Clip = new Region(bounds);
                var foreBrush = new SolidBrush(ClientSettings.ForeColor);
                Rectangle textBounds;
                textBounds = new Rectangle(bounds.X + ClientSettings.Margin, bounds.Y, bounds.Width - (ClientSettings.Margin * 2), bounds.Height);
                var innerBounds = new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
                innerBounds.Offset(1, 1);
                innerBounds.Width--; innerBounds.Height--;
                if (Selected)
                {
                    foreBrush = new SolidBrush(ClientSettings.SelectedForeColor);
                    if (ClientSettings.SelectedBackColor != ClientSettings.SelectedBackGradColor)
                    {
                        try
                        {
                            Gradient.GradientFill.Fill(g, innerBounds, ClientSettings.SelectedBackColor, ClientSettings.SelectedBackGradColor, Gradient.GradientFill.FillDirection.TopToBottom);
                        }
                        catch
                        {
                            using (Brush backBrush = new SolidBrush(ClientSettings.SelectedBackColor))
                            {
                                g.FillRectangle(backBrush, innerBounds);
                            }
                        }
                    }
                    else
                    {
                        using (Brush backBrush = new SolidBrush(ClientSettings.SelectedBackColor))
                        {
                            g.FillRectangle(backBrush, innerBounds);
                        }
                    }
                }
                else
                {
                    if (ClientSettings.BackColor != ClientSettings.BackGradColor)
                    {
                        try
                        {
                            Gradient.GradientFill.Fill(g, innerBounds, ClientSettings.BackColor, ClientSettings.BackGradColor, Gradient.GradientFill.FillDirection.TopToBottom);
                        }
                        catch
                        {
                            using (Brush backBrush = new SolidBrush(ClientSettings.BackColor))
                            {
                                g.FillRectangle(backBrush, innerBounds);
                            }
                        }
                    }
                    else
                    {
                        using (Brush backBrush = new SolidBrush(ClientSettings.BackColor))
                        {
                            g.FillRectangle(backBrush, innerBounds);
                        }
                    }
                }

                SizeF textSize = g.MeasureString("More...", ClientSettings.MenuFont);
                Point startPoint = new Point((int)(bounds.Left + (bounds.Width - textSize.Width) / 2),(int)(bounds.Top + (bounds.Height - textSize.Height) / 2));
                
                Color drawColor = ClientSettings.MenuTextColor;
                using (Brush drawBrush = new SolidBrush(drawColor))
                {
                    g.DrawString("More...", ClientSettings.MenuFont, drawBrush, startPoint.X, startPoint.Y);
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public System.Drawing.Rectangle Bounds
        {
            get;
            set;
        }

        public bool Selected
        {
            get;
            set;
        }

        #endregion
    }
}
