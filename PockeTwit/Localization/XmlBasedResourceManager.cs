using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Xml;
using System.IO;
using System.Windows.Forms;

namespace PockeTwit.Localization
{
    public static class XmlBasedResourceManager
    {
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();
        private static string _directory;
        private static string _nameBase;

        static XmlBasedResourceManager()
        {
            _directory = String.Concat(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase), "\\Localization\\");
            _nameBase = "PockeTwit";
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
            _cache.Clear();

            string fileName = String.Concat(_directory, _nameBase, "_", _cultureInfo.Name, ".xml");
            if (!File.Exists(fileName))
            {
                // try fallback to neutral language
                fileName = String.Concat(_directory, _nameBase, "_", _cultureInfo.TwoLetterISOLanguageName, ".xml");
                if (!File.Exists(fileName))
                {
                    // give up
                    return;
                }
            }

            XmlTextReader xmlReader = new XmlTextReader(fileName);

            try
            {
                // skip root node
                xmlReader.MoveToContent();

                while (xmlReader.Read())
                {
                    if (xmlReader.MoveToContent() == XmlNodeType.Element)
                    {
                        string name = xmlReader.GetAttribute("name");
                        if (!String.IsNullOrEmpty(name))
                        {
                            string value = xmlReader.GetAttribute("localized");
                            if (!String.IsNullOrEmpty(value))
                                _cache.Add(name, value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Malformed resource: caught {0} in line {1}", ex.GetType().ToString(), xmlReader.LineNumber));
            }
            finally
            {
                xmlReader.Close();
            }
        }
        public static string GetString(string name, string defaultValue)
        {
            string result;
            if (!_cache.TryGetValue(name, out result))
            {
                return defaultValue;
            }
            return result;
        }

        private static void LocalizeMenuItem(MenuItem item)
        {
            item.Text = GetString(item.Text, item.Text);
            if (item.MenuItems != null)
            {
                foreach (MenuItem subItem in item.MenuItems)
                {
                    LocalizeMenuItem(subItem);
                }
            }
        }

        public static void LocalizeForm(Form form)
        {
            if (form.Menu != null)
            {
                foreach (MenuItem item in form.Menu.MenuItems)
                {
                    LocalizeMenuItem(item);
                }
            }

            form.Text = GetString(form.Text, form.Text);

            foreach (Control c in form.Controls)
            {
                if ((c is Label) || (c is CheckBox) || (c is TextBox))
                {                  
                    c.Text = GetString(c.Text, c.Text);
                    continue;
                }

            }
        }
    }
}
