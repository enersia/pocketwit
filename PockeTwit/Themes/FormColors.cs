using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Themes
{
    class FormColors
    {
        public static System.Drawing.Bitmap GetThemeIcon(string iconName, int size)
        {
            try
            {
                return new System.Drawing.Bitmap(GetThemeIconPath(iconName, size));
            }
            catch (System.IO.IOException)
            {
                return new System.Drawing.Bitmap(1,1);
            }
        }
        public static string GetThemeIconPath(string iconName, int size)
        {
            string format = "{0}{1}-{2}.png";
            string original = ClientSettings.AppPath + "\\Themes\\Original\\";
            int fixedSize = (size <= 32 ? 32 : 64);
            string[] searchPaths = {
                string.Format(format, ClientSettings.IconsFolder(), iconName, size ),
                string.Format(format, ClientSettings.IconsFolder(), iconName, fixedSize ),
                string.Format(format, ClientSettings.IconsFolder(), iconName, 32 ),
                string.Format(format, original, iconName, size ),
                string.Format(format, original, iconName, fixedSize ),
                string.Format(format, original, iconName, 32 )
            };
            try
            {
                string search = "";
                for(int i = 0; i < searchPaths.Length; i++)
                {
                    search = searchPaths[i];
                    if(System.IO.File.Exists(search))
                        break;
                }

                return search;
            }
            catch (System.IO.IOException)
            {
                return "";
            }
        }
        public static void SetColors(Control f)
        {
            foreach (Control c in f.Controls)
            {
                if (c is TextBox || c is ListBox || c is ListView || c is ComboBox /*|| c is Panel*/ || c is ProgressBar)
                {
                    c.ForeColor = ClientSettings.FieldForeColor;
                    c.BackColor = ClientSettings.FieldBackColor;
                }
                else if (c is LinkLabel)
                {
                    c.ForeColor = ClientSettings.LinkColor;
                }
                else if (c is TabControl)
                {
                    foreach (TabPage page in (c as TabControl).TabPages)
                    {
                        SetColors(page);
                    }
                }
                else if (c is Panel)
                {
                    SetColors(c);
                }
                else
                {
                    c.ForeColor = ClientSettings.ForeColor;
                    c.BackColor = ClientSettings.BackColor;
                }
            }
            f.ForeColor = ClientSettings.ForeColor;
            f.BackColor = ClientSettings.BackColor;
        }
        public static void SetColors(Form f)
        {
            foreach (Control c in f.Controls)
            {
                if (c is TextBox || c is ListBox || c is ListView || c is ComboBox /*|| c is Panel*/)
                {
                    c.ForeColor = ClientSettings.FieldForeColor;
                    c.BackColor = ClientSettings.FieldBackColor;
                }
                else if (c is LinkLabel)
                {
                    c.ForeColor = ClientSettings.LinkColor;
                }
                else if (c is TabControl)
                {
                    foreach (TabPage page in (c as TabControl).TabPages)
                    {
                        SetColors(page);
                    }
                }
                else if (c is Panel)
                {
                    SetColors(c);
                }
                else
                {
                    c.ForeColor = ClientSettings.ForeColor;
                    c.BackColor = ClientSettings.BackColor;
                }
            }
            f.ForeColor = ClientSettings.ForeColor;
            f.BackColor = ClientSettings.BackColor;
        }
    }
}
