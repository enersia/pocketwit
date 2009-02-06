﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace FingerUI
{
    public class SideMenu : System.Windows.Forms.Control
    {
        public SideMenu(FingerUI.KListControl.SideShown Side)
        {
            _Side = Side;
            _Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            _Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        }
        public delegate void delClearMe();

        private Bitmap _Rendered = null;
        public Bitmap Rendered
        {
            get
            {
                if (IsDirty)
                {
                    DrawMenu();
                }
                return _Rendered;
            }
        }

        public bool IsDirty = true;

        private FingerUI.KListControl.SideShown _Side;
        private List<SideMenuItem> Items = new List<SideMenuItem>();
        private SideMenuItem _SelectedItem = null;
        public SideMenuItem SelectedItem
        {
            get 
            {
                lock (Items)
                {
                    if (Items.Count == 0)
                    {
                        return null;
                    }
                    if(_SelectedItem==null)
                    {
                        return Items[0];
                    }
                    return _SelectedItem;
                }
            }
            set
            {
                lock (Items)
                {
                    if (Items.Contains(value))
                    {
                        _SelectedItem = value;
                    }
                    else
                    {
                        _SelectedItem = null;
                    }
                }
            }
        }
        public int Count
        {
            get { return Items.Count; }
        }
        private int _ItemHeight = 0;
        private int _TopOfMenu = 0;
        public int ItemHeight { get { return _ItemHeight; } }
        public int TopOfMenu { get { return _TopOfMenu; } }

        private string _UserName;
        public string UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                _UserName = value;
                IsDirty=true;
            }
        }

        private int _Height;
        public new int Height
        {
            get { return _Height; }
            set
            {
                if (value != _Height)
                {
                    _Height = value;
                    SetMenuHeight();
                    if (_Width > 0 && _Height > 0)
                    {
                        if (_Rendered != null)
                        {
                            _Rendered.Dispose();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        _Rendered = new Bitmap(_Width, _Height);
                    }
                }
            }
        }

        private int _Width;
        public new int Width
        {
            get{return _Width;}
            set
            {
                if (value != _Width)
                {
                    _Width = value;
                    if (_Width > 0 && _Height > 0)
                    {
                        if (_Rendered != null)
                        {
                            _Rendered.Dispose();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        _Rendered = new Bitmap(_Width, _Height);
                    }
                    IsDirty = true;
                }
            }
        }


        public void SetMenuHeight()
        {
            if (InvokeRequired)
            {
                delClearMe d = new delClearMe(SetMenuHeight);
                this.Invoke(d, null);
            }
            else
            {
                if (Items.Count > 0)
                {
                    try
                    {
                        int multiplyer = 4;
                        if (PockeTwit.DetectDevice.DeviceType == PockeTwit.DeviceType.Standard)
                        {
                            multiplyer = 3;
                        }
                        int Count = 0;
                        foreach (SideMenuItem item in Items)
                        {
                            if (item.Visible)
                            {
                                Count++;
                            }
                        }
                        _ItemHeight = (this.Height - (ClientSettings.TextSize * multiplyer)) / Count;
                        _TopOfMenu = ((this.Height / 2) - ((Count * ItemHeight) / 2));
                    }
                    catch (ObjectDisposedException) { }
                }
            }
        }

        public void InsertMenuItem(int index, SideMenuItem Item)
        {
            lock (Items)
            {
                Items.Insert(index, Item);
                SetMenuHeight();
            }
            IsDirty=true;
        }

        public void ResetMenu(IEnumerable<SideMenuItem> NewItems)
        {
            lock (Items)
            {
                Items.Clear();
                Items.AddRange(NewItems);
                SetMenuHeight();
            }
            IsDirty=true;
        }

        public void AddItems(IEnumerable<SideMenuItem> NewItems)
        {
            lock (Items)
            {
                foreach (SideMenuItem Item in NewItems)
                {
                    if (!Items.Contains(Item))
                    {
                        Items.Add(Item);
                    }
                }
                SetMenuHeight();
            }
            IsDirty=true;
        }
        public void RemoveItem(SideMenuItem OldItem)
        {
            lock (Items)
            {
                if (Items.Contains(OldItem))
                {
                    Items.Remove(OldItem);
                }
                SetMenuHeight();
            }
            IsDirty=true;
        }
        public SideMenuItem[] GetItems()
        {
            lock (Items)
            {
                return Items.ToArray();
            }
        }

        public bool Contains(SideMenuItem ItemToSeek)
        {
            lock (Items)
            {
                return Items.Contains(ItemToSeek);
            }
        }
        public bool Contains(string TextOfMenuItem)
        {
            bool bFound = false;
            lock (Items)
            {
                foreach (SideMenuItem Item in Items)
                {
                    if (Item.Text == TextOfMenuItem)
                    {
                        bFound = true;
                        break;
                    }
                }
            }
            return bFound;
        }

        public void SelectDown()
        {
            lock (Items)
            {
                int PrevSelected = Items.IndexOf(SelectedItem);
                if (PrevSelected < Items.Count - 1)
                {
                    _SelectedItem = Items[PrevSelected + 1];
                }
                else
                {
                    _SelectedItem = Items[0];
                }
            }
            IsDirty=true;
        }
        public void SelectUp()
        {
            lock (Items)
            {
                int PrevSelected = Items.IndexOf(SelectedItem);
                if (PrevSelected > 0)
                {
                    _SelectedItem = Items[PrevSelected - 1];
                }
                else
                {
                    _SelectedItem = Items[Items.Count- 1];
                }
            }
            IsDirty=true;
        }

        public void SelectByText(string ItemToSelect)
        {
            lock (Items)
            {
                foreach (SideMenuItem Item in Items)
                {
                    if (Item.Text == ItemToSelect)
                    {
                        _SelectedItem = Item;
                        IsDirty = true;

                        break;
                    }
                }
            }
        }

        public void InvokeSelected()
        {
            _SelectedItem.ClickedMethod();
        }

        public void InvokeByText(string ItemToInvoke)
        {
            lock (Items)
            {
                foreach (SideMenuItem Item in Items)
                {
                    if (Item.Text == ItemToInvoke)
                    {
                        Item.ClickedMethod();
                        break;
                    }
                }
            }
        }

        public void SelectByNumber(int Number)
        {
            lock (Items)
            {
                if (Items.Count >= Number)
                {
                    _SelectedItem = Items[Number];
                }
            }
            IsDirty = true;
        }
        public void ReplaceItem(SideMenuItem Original, SideMenuItem New)
        {
            lock (Items)
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == Original)
                    {
                        Items[i] = New;
                    }
                }
            }
            IsDirty=true;
        }

        private void DrawMenu()
        {
            lock (Items)
            {
                int i = 1;
                if (this.Items[0].CanHide && this.Items[0].Visible)
                {
                    i = 0;
                }
                if (_Rendered == null)
                {
                    _Rendered = new Bitmap(_Width, _Height);
                }
                using (Graphics m_backBuffer = Graphics.FromImage(_Rendered))
                {
                    m_backBuffer.Clear(ClientSettings.BackColor);
                    int LeftPos = 0;
                    int CurrentTop = TopOfMenu;
                    foreach (SideMenuItem Item in this.GetItems())
                    {
                        if (Item.Visible)
                        {
                            string DisplayItem = Item.Text;
                            if (PockeTwit.DetectDevice.DeviceType == PockeTwit.DeviceType.Standard)
                            {
                                DisplayItem = i.ToString() + ". " + DisplayItem;
                            }

                            if (_Side == FingerUI.KListControl.SideShown.Left)
                            {
                                int TextWidth = (int)m_backBuffer.MeasureString(DisplayItem, ClientSettings.MenuFont).Width + ClientSettings.Margin;
                                LeftPos = _Width - (TextWidth + 5);
                            }
                            else
                            {
                                LeftPos = 6;
                            }
                            using (Pen whitePen = new Pen(ClientSettings.ForeColor))
                            {

                                Rectangle menuRect = new Rectangle(0, CurrentTop, _Width, ItemHeight);
                                Color BackColor;
                                Color MenuTextColor;
                                Color GradColor;

                                if (Item == SelectedItem)
                                {
                                    BackColor = ClientSettings.SelectedBackColor;
                                    GradColor = ClientSettings.SelectedBackGradColor;
                                    MenuTextColor = ClientSettings.SelectedForeColor;
                                }
                                else
                                {
                                    BackColor = ClientSettings.BackColor;
                                    GradColor = ClientSettings.BackGradColor;
                                    MenuTextColor = ClientSettings.ForeColor;
                                }
                                try
                                {
                                    Gradient.GradientFill.Fill(m_backBuffer, menuRect, BackColor, GradColor, Gradient.GradientFill.FillDirection.TopToBottom);
                                }
                                catch
                                {
                                    using (Brush BackBrush = new SolidBrush(ClientSettings.SelectedBackColor))
                                    {
                                        m_backBuffer.FillRectangle(BackBrush, menuRect);
                                    }
                                }
                                m_backBuffer.DrawLine(whitePen, menuRect.Left, menuRect.Top, menuRect.Right, menuRect.Top);
                                using (Brush sBrush = new SolidBrush(MenuTextColor))
                                {
                                    StringFormat sFormat = new StringFormat();
                                    sFormat.LineAlignment = StringAlignment.Center;
                                    int TextTop = ((menuRect.Bottom - menuRect.Top) / 2) + menuRect.Top;
                                    DisplayItem = DisplayItem.Replace("@User", "@" + _UserName);
                                    m_backBuffer.DrawString(DisplayItem, ClientSettings.MenuFont, sBrush, LeftPos, TextTop, sFormat);
                                }
                                m_backBuffer.DrawLine(whitePen, menuRect.Left, menuRect.Bottom, menuRect.Right, menuRect.Bottom);
                                if (_Side == FingerUI.KListControl.SideShown.Right)
                                {
                                    m_backBuffer.DrawLine(whitePen, menuRect.Left, 0, menuRect.Left, this.Height);
                                }
                                else
                                {
                                    m_backBuffer.DrawLine(whitePen, menuRect.Right - 1, 0, menuRect.Right - 1, this.Height);
                                }
                                CurrentTop = CurrentTop + ItemHeight;
                                i++;
                            }
                        }
                    }
                }
                IsDirty = false;
            }
        }

        public FingerUI.SideMenuItem GetMenuItemForPoint(Point X, int LeftOfItem)
        {
            int TopOfItem = TopOfMenu;
            
            foreach (SideMenuItem MenuItem in Items)
            {
                if (MenuItem.Visible)
                {
                    Rectangle menuRect = new Rectangle(LeftOfItem, TopOfItem, Width, ItemHeight);
                    TopOfItem = TopOfItem + ItemHeight;
                    if (menuRect.Contains(X))
                    {
                        return MenuItem;
                    }
                }
            }
            return null;
        }
    }
}
