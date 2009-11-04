using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace PockeTwit.OtherServices.GoogleSpell
{
	/// <summary>
	/// Summary description for SpellRequest.
	/// </summary>
	[Serializable()]
	[XmlRoot("spellrequest", Namespace="", IsNullable=false)]
	public class SpellRequest
	{
		private static readonly XmlSerializer _requestSerializer = new XmlSerializer(typeof(SpellRequest));

		[XmlAttribute("textalreadyclipped", DataType="int")]
		public int TextAlreadyClipped = 0;
		
		[XmlAttribute("ignoredups", DataType="int")]
		public int IgnoreDuplicates = 0;
		
		[XmlAttribute("ignoredigits", DataType="int")]
		public int IgnoreWordsWithDigits = 1;
		
		[XmlAttribute("ignoreallcaps", DataType="int")]
		public int IgnoreWordsWithAllCaps = 1;

		[XmlElement("text")]
		public string Text;

		public SpellRequest()
		{
			
		}

		public SpellRequest(string text)
		{
			this.Text = text;
		}

		public SpellRequest(string text, int ignoreDuplicates, int ignoreWordsWithDigits, int ignoreWordsWithAllCaps)
		{
			IgnoreDuplicates = ignoreDuplicates;
			IgnoreWordsWithDigits = ignoreWordsWithDigits;
			IgnoreWordsWithAllCaps = ignoreWordsWithAllCaps;
			Text = text;
		}

		public SpellRequest(string text, bool ignoreDuplicates, bool ignoreWordsWithDigits, bool ignoreWordsWithAllCaps)
		{
			IgnoreDuplicates = ignoreDuplicates ? 1 : 0;
			IgnoreWordsWithDigits = ignoreWordsWithDigits ? 1 : 0;
			IgnoreWordsWithAllCaps = ignoreWordsWithAllCaps ? 1 : 0;
			Text = text;
		}

		public override string ToString()
		{			
			StringWriter writer = new StringWriter();
			XmlTextWriter xmlWriter = new XmlTextWriter(writer);
			xmlWriter.Formatting = Formatting.None;
			
			XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
			xsn.Add("", null );

			_requestSerializer.Serialize(xmlWriter, this, xsn);
			
			return writer.ToString();
		}

	}
}
