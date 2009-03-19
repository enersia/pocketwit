﻿using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace FingerUI
{
    public class SideMenu : System.Windows.Forms.Control
    {
        public delegate void delAnimateMe();
        public event delAnimateMe AnimateMe = delegate { };
        public bool IsAnimating { get { return _animationStep >= 0; }}

        public SideMenu(FingerUI.KListControl.SideShown Side)
        {
            _Side = Side;
            _Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            _Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        }
        public delegate void delClearMe();
        public event delClearMe ItemWasClicked = delegate { };
        public event delClearMe NeedRedraw = delegate { };
        private int TopOfSubMenu;

        private SideMenuItem _ExpandedItem = null;
        private SideMenuItem ExpandedItem
        {
            get
            {
                return _ExpandedItem;
            }
            set
            {
                if (value == null)
                {
                    if (_ExpandedItem != null)
                    {
                        ExpandedItem.Expanded = false;
                        IsDirty = true;
                        NeedRedraw();

                        _animationColor = ClientSettings.DimmedColor;
                        isFading = false;
                        _animationStep = 6;
                        AnimateMe();
                    }
                    SelectedItem = _ExpandedItem;
                }
                else
                {
                    SelectedItem = value.SubMenuItems[0];
                    _animationStep = 6;
                    isFading = true;
                    _animationColor = ClientSettings.ForeColor;
                    AnimateMe();
                }
                _ExpandedItem = value;
            }
        }
        public bool IsExpanded
        {
            get
            {
                return ExpandedItem != null;
            }
        }
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
                        return Items[1];
                    }
                    return _SelectedItem;
                }
            }
            set
            {
                _SelectedItem = value;
                IsDirty = true;
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
                    IsDirty = true;
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

        public void ForceRerender()
        {
            IsDirty = true;
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
                foreach (SideMenuItem item in Items)
                {
                    item.DoneWithClick -= new delMenuClicked(item_DoneWithClick);
                    item.MenuExpandedOrCollapsed -= new SideMenuItem.delItemExpanded(item_MenuExpandedOrCollapsed);
                    if (item.HasChildren)
                    {
                        foreach (SideMenuItem subItem in item.SubMenuItems)
                        {
                            subItem.DoneWithClick -= new delMenuClicked(item_DoneWithClick);
                            subItem.MenuExpandedOrCollapsed -= new SideMenuItem.delItemExpanded(item_MenuExpandedOrCollapsed);
                        }
                    }
                }
                Items.Clear();
                Items.AddRange(NewItems);
                SetMenuHeight();
                foreach (SideMenuItem item in Items)
                {
                    item.DoneWithClick += new delMenuClicked(item_DoneWithClick);
                    item.MenuExpandedOrCollapsed += new SideMenuItem.delItemExpanded(item_MenuExpandedOrCollapsed);
                    if (item.HasChildren)
                    {
                        foreach (SideMenuItem subItem in item.SubMenuItems)
                        {
                            subItem.DoneWithClick += new delMenuClicked(item_DoneWithClick);
                            subItem.MenuExpandedOrCollapsed += new SideMenuItem.delItemExpanded(item_MenuExpandedOrCollapsed);
                        }
                    }
                }
            }
            IsDirty=true;
        }

        void item_MenuExpandedOrCollapsed(SideMenuItem sender, bool Opened)
        {
            ExpandedItem = sender;
            this.IsDirty = true;
            NeedRedraw();
        }

        void item_DoneWithClick()
        {
            ExpandedItem = null;
            ItemWasClicked();
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
                    OldItem.DoneWithClick -= new delMenuClicked(item_DoneWithClick);
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
                    if (Item.Text == TextOfMenuItem && Item.Visible)
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
            List<SideMenuItem> ItemsToUse = Items;
            if (ExpandedItem != null)
            {
                ItemsToUse = ExpandedItem.SubMenuItems;
            }
            lock (ItemsToUse)
            {
                int PrevSelected = ItemsToUse.IndexOf(SelectedItem);
                if (PrevSelected < ItemsToUse.Count - 1)
                {
                    _SelectedItem = ItemsToUse[PrevSelected + 1];
                }
                else
                {
                    _SelectedItem = ItemsToUse[0];
                }
            }
            if (!_SelectedItem.Visible)
            {
                SelectDown();
            }
            IsDirty=true;
        }
        public void SelectUp()
        {
            List<SideMenuItem> ItemsToUse = Items;
            if (ExpandedItem != null)
            {
                ItemsToUse = ExpandedItem.SubMenuItems;
            }
            lock (ItemsToUse)
            {
                int PrevSelected = ItemsToUse.IndexOf(SelectedItem);
                if (PrevSelected > 0)
                {
                    _SelectedItem = ItemsToUse[PrevSelected - 1];
                }
                else
                {
                    _SelectedItem = ItemsToUse[ItemsToUse.Count - 1];
                }
            }
            if (!_SelectedItem.Visible)
            {
                SelectUp();
            }
            IsDirty=true;
        }

        public void SelectByText(string ItemToSelect)
        {
            List<SideMenuItem> ItemsToUse = Items;
            if (ExpandedItem != null)
            {
                ItemsToUse = ExpandedItem.SubMenuItems;
            }
            lock (ItemsToUse)
            {
                foreach (SideMenuItem Item in ItemsToUse)
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
            SelectedItem.ClickMe();
        }

        public void InvokeByText(string ItemToInvoke)
        {
            lock (Items)
            {
                foreach (SideMenuItem Item in Items)
                {
                    if (Item.Text == ItemToInvoke)
                    {
                        Item.ClickMe();
                        break;
                    }
                }
            }
        }

        public void SelectByNumber(int Number)
        {
            List<SideMenuItem> ItemsToUse = Items;
            if (ExpandedItem != null)
            {
                ItemsToUse = ExpandedItem.SubMenuItems;
            }
            lock (ItemsToUse)
            {
                if (ItemsToUse.Count >= Number)
                {
                    _SelectedItem = ItemsToUse[Number];
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

        public void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            SideMenuItem i = GetMenuItemForTransposedPoint(new Point(e.X, e.Y));
            if (i != null)
            {
                SelectedItem = i;
                IsDirty = true;
                NeedRedraw();
            }
        }

        public void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            SideMenuItem i = GetMenuItemForTransposedPoint(new Point(e.X, e.Y));
            if (i != null)
            {
                SelectedItem = i;
                CollapseExpandedMenu();
                i.ClickMe();
            }
            else
            {
                CollapseExpandedMenu();
            }
        }

        public void CollapseExpandedMenu()
        {
            if (ExpandedItem != null)
            {
                ExpandedItem.Expanded = false;
                ExpandedItem = null;
                IsDirty = true;
                NeedRedraw();
            }
        }

        private void DrawMenu()
        {
            Rectangle ExpandedRect = new Rectangle();
            lock (Items)
            {
                System.Drawing.Color MenuColor = animationColor;
                
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
                    int CurrentTop = TopOfMenu;
                    foreach (SideMenuItem Item in this.GetItems())
                    {
                        if (Item.Visible)
                        {
                            DrawSingleItem(i, m_backBuffer,0, CurrentTop, Item, MenuColor);

                            i++;
                            
                            if (Item.Expanded)
                            {
                                ExpandedRect = new Rectangle(0, CurrentTop, _Width, ItemHeight);
                            }
                            CurrentTop = CurrentTop + ItemHeight;
                        }
                    }

                    if (ExpandedItem != null)
                    {
                        DrawSubMenu(ExpandedItem, ExpandedRect);
                    }
                    using (Pen whitePen = new Pen(ClientSettings.ForeColor))
                    {
                        if (_Side == FingerUI.KListControl.SideShown.Right)
                        {
                            m_backBuffer.DrawLine(whitePen, 0, 0, 0, this.Height);
                        }
                        else
                        {
                            m_backBuffer.DrawLine(whitePen, _Width-1, 0, _Width- 1, this.Height);
                        }
                    }
                    if (_animationStep == 0) 
                    {
                        _animationStep = -1; 
                    }
                    if (_animationStep > 0)
                    {
                        _animationStep = _animationStep - 2;
                        //IsDirty = true;
                    }
                }
                IsDirty = IsAnimating;
            }

        }

        private void DrawSingleItem(int i, Graphics m_backBuffer, int Offset, int CurrentTop, SideMenuItem Item, System.Drawing.Color MenuColor)
        {
            int LeftPos = 0;
            string DisplayItem = Item.Text;
            if (PockeTwit.DetectDevice.DeviceType == PockeTwit.DeviceType.Standard)
            {
                DisplayItem = i.ToString() + ". " + DisplayItem;
            }

            if (_Side == FingerUI.KListControl.SideShown.Left)
            {
                int TextWidth = (int)m_backBuffer.MeasureString(DisplayItem, ClientSettings.MenuFont).Width + ClientSettings.Margin;
                LeftPos = _Width - (TextWidth + 5 + Offset);
            }
            else
            {
                LeftPos = LeftPos + ClientSettings.Margin + 5 + Offset;
            }
            using (Pen whitePen = new Pen(MenuColor))
            {
                Rectangle menuRect;
                if (_Side == KListControl.SideShown.Left)
                {
                    menuRect = new Rectangle(0, CurrentTop, _Width - Offset, ItemHeight);
                }
                else
                {
                    menuRect = new Rectangle(Offset, CurrentTop, _Width, ItemHeight);
                }

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
                    MenuTextColor = MenuColor;
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
            }
        }

        

        private int _animationStep = -1;
        private int _animationDelta
        {
            get
            {
                return ClientSettings.TextSize / 3;
            }
        }
        private int rColorDelta
        {
            get
            {
                return (ClientSettings.DimmedColor.R - ClientSettings.ForeColor.R) / 3;
            }
        }
        private int gColorDelta
        {
            get
            {
                return (ClientSettings.DimmedColor.G - ClientSettings.ForeColor.G) / 3;
            }
        }
        private int bColorDelta
        {
            get
            {
                return (ClientSettings.DimmedColor.B - ClientSettings.ForeColor.B) / 3;
            }
        }
        private bool isFading = true;
        private Color _animationColor = ClientSettings.ForeColor;
        private Color animationColor
        {
            get
            {
                if (_animationStep > 0)
                {
                    Color newColor;
                    if (isFading)
                    {
                        newColor = Color.FromArgb(_animationColor.R + rColorDelta, _animationColor.G + gColorDelta, _animationColor.B + bColorDelta);
                    }
                    else
                    {
                        newColor = Color.FromArgb(_animationColor.R - rColorDelta, _animationColor.G - gColorDelta, _animationColor.B - bColorDelta);
                    }
                    _animationColor = newColor;
                    return newColor;
                }
                else
                {
                    return _animationColor;
                }
            }
        }
        
        private void DrawSubMenu(SideMenuItem Item, Rectangle menuRect)
        {
            int i = 0;
            int OffSet = 0;
            if (_animationStep >= 0)
            {
                OffSet = ClientSettings.TextSize - (_animationStep * _animationDelta);
            }
            else
            {
                OffSet = ClientSettings.TextSize;
            }
            int ItemsCount = Item.SubMenuItems.Count;
            TopOfSubMenu = (((menuRect.Bottom - menuRect.Top) / 2) + menuRect.Top) - (ItemsCount * ItemHeight / 2);
            if (TopOfSubMenu < 0) { TopOfSubMenu = 0; }
            int CurrentTop = TopOfSubMenu;
            using (Graphics m_backBuffer = Graphics.FromImage(_Rendered))
            {
                foreach (SideMenuItem subItem in Item.SubMenuItems)
                {
                    
                    if (subItem.Visible)
                    {
                        DrawSingleItem(i, m_backBuffer, OffSet, CurrentTop, subItem, ClientSettings.ForeColor);
                        i++;
                        CurrentTop = CurrentTop + ItemHeight;
                    }
                }
                using (Pen whitePen = new Pen(ClientSettings.ForeColor))
                {
                    if (_Side == KListControl.SideShown.Left)
                    {
                        m_backBuffer.DrawLine(whitePen, _Width - OffSet, TopOfSubMenu, _Width - OffSet, (Item.SubMenuItems.Count * ItemHeight) + TopOfSubMenu);
                    }
                    else
                    {
                        m_backBuffer.DrawLine(whitePen, OffSet, TopOfSubMenu, OffSet, (Item.SubMenuItems.Count * ItemHeight) + TopOfSubMenu);
                    }
                }
            }
        }

        public FingerUI.SideMenuItem GetMenuItemForTransposedPoint(Point X)
        {
            List<SideMenuItem> ListOfItems = Items;
            int TopOfItem = TopOfMenu;
            int Offset = 0;
            if (ExpandedItem != null)
            {
                ListOfItems = ExpandedItem.SubMenuItems;
                TopOfItem = TopOfSubMenu;
                Offset = ClientSettings.TextSize; 
            }
            

            foreach (SideMenuItem MenuItem in ListOfItems)
            {
                if (MenuItem.Visible)
                {

                    Rectangle menuRect;
                    if (_Side == KListControl.SideShown.Left)
                    {
                        menuRect = new Rectangle(0, TopOfItem, Width-Offset, ItemHeight);
                    }
                    else
                    {
                        menuRect = new Rectangle(Offset, TopOfItem, Width, ItemHeight);
                    }
                    TopOfItem = TopOfItem + ItemHeight;
                    if (menuRect.Contains(X))
                    {
                        return MenuItem;
                    }
                }
            }
            return null;
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
