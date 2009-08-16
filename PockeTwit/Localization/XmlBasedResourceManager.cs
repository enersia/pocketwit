using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace PockeTwit.Localization
{
    public static class XmlBasedResourceManager
    {
        private static readonly Dictionary<string, string> Cache = new Dictionary<string, string>();
        private static readonly string Directory;
        private static readonly string NameBase;

        static XmlBasedResourceManager()
        {
            Directory = String.Concat(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), "\\Localization\\");
            NameBase = "PockeTwit";
            CultureInfo = CultureInfo.CurrentUICulture;
        }

        private static CultureInfo _cultureInfo;
        public static CultureInfo CultureInfo
        {
            get
            {
                return _cultureInfo;
            }
            set
            {
                _cultureInfo = value;
                InitLanguageDictionary();
            }
        }

        private static void InitLanguageDictionary()
        {
            Cache.Clear();

            var fileName = String.Concat(Directory, NameBase, "_", _cultureInfo.Name, ".xml");
            if (!File.Exists(fileName))
            {
                // try fallback to neutral language
                fileName = String.Concat(Directory, NameBase, "_", _cultureInfo.TwoLetterISOLanguageName, ".xml");
                if (!File.Exists(fileName))
                {
                    // give up
                    return;
                }
            }

            var xmlReader = new XmlTextReader(fileName);

            try
            {
                // skip root node
                xmlReader.MoveToContent();

                while (xmlReader.Read())
                {
                    if (xmlReader.MoveToContent() != XmlNodeType.Element) continue;

                    var name = xmlReader.GetAttribute("name");
                    if (String.IsNullOrEmpty(name)) continue;

                    var value = xmlReader.GetAttribute("localized");
                    if (!String.IsNullOrEmpty(value))
                        Cache.Add(name, value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Malformed resource: caught {0} in line {1}", ex.GetType(), xmlReader.LineNumber));
            }
            finally
            {
                xmlReader.Close();
            }
        }
        public static string GetString(string name, string defaultValue)
        {
            string result;
            return !Cache.TryGetValue(name, out result) ? defaultValue : result;
        }

        private static void LocalizeMenuItem(MenuItem item)
        {
            item.Text = GetString(item.Text, item.Text);
            if (item.MenuItems == null) return;
            foreach (MenuItem subItem in item.MenuItems)
            {
                LocalizeMenuItem(subItem);
            }
        }

        public static void LocalizeForm(Control parent)
        {
            if (parent is Form)
            {
                var form = (Form)parent;
                LocalizeMenu(form);
            }
            LocalizeControlAndChildren(parent);
        }
        private static void LocalizeMenu(Form form)
        {
            if (form.Menu == null) return;
            foreach (MenuItem item in form.Menu.MenuItems)
            {
                LocalizeMenuItem(item);
            }
        }

        private static void LocalizeControlAndChildren(Control parentControl)
        {
            foreach (Control control in parentControl.Controls)
            {
                if ((control is Label) || (control is CheckBox) ||
                    (control is RadioButton) || (control is LinkLabel) ||
                    (control is Form))
                {                  
                    control.Text = GetString(control.Text, control.Text);
                    continue;
                }
                if(control.Controls.Count>0)
                {
                    LocalizeControlAndChildren(control);
                }
            }
        }
    }
}
