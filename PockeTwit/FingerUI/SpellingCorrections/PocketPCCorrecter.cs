using System;
using System.Drawing;
using System.Windows.Forms;

namespace PockeTwit.FingerUI.SpellingCorrections
{
    class PocketPCCorrecter : ContextMenu, ISpellingCorrector
    {
        private Control _parent = null;

        public Control AppliesTo 
        { 
            set 
            {
                _parent = value;
            }
        }


        public string[]  Suggestions
        {
	        set
	        {
	            ClearItems();
	            MenuItems.Clear();
	            foreach (var suggestion in value)
	            {
	                var item = new MenuItem {Text = suggestion};
                    item.Click += item_Click;
	                MenuItems.Add(item);
	            }
	        }
        }

        public string Original
        {
            set {  }
        }


        public void Display()
        {
            Show(_parent, new Point(50, 50));
        }
        private void ClearItems()
        {
            foreach (MenuItem item in MenuItems)
            {
                item.Click -= item_Click;
            }
        }

        void item_Click(object sender, EventArgs e)
        {
            var clickedItem = (MenuItem) sender;
            ItemSelected(clickedItem.Text);
        }

        public event delItemSelected  ItemSelected;
    }
}
