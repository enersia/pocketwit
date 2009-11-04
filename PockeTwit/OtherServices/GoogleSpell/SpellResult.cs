using System;
using System.IO;
using System.Xml.Serialization;

namespace PockeTwit.OtherServices.GoogleSpell
{
	/// <summary>
	/// Summary description for SpellResult.
	/// </summary>
	[Serializable()]
	[XmlRoot("spellresult", Namespace="", IsNullable=false)]
	public class SpellResult
	{
		private static readonly XmlSerializer _resultSerializer = new XmlSerializer(typeof(SpellResult));

		[XmlAttributeAttribute("error", DataType="int")]
		public int Error = 0;
		
		[XmlAttributeAttribute("clipped", DataType="int")]
		public int Clipped = 0;
		
		[XmlAttributeAttribute("charschecked", DataType="int")]
		public int CharsChecked = 0;
		
		[XmlElementAttribute("c", IsNullable=true)]
		public SpellCorrection[] Corrections;

		public SpellResult()
		{

		}

		public static SpellResult Load(string resultXml)
		{
			StringReader reader = new StringReader(resultXml);
			return _resultSerializer.Deserialize(reader) as SpellResult;
		}
	}
}
