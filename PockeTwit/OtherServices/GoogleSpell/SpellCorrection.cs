using System;
using System.Xml.Serialization;

namespace PockeTwit.OtherServices.GoogleSpell
{
	/// <summary>
	/// Summary description for SpellCorrection.
	/// </summary>
	[Serializable()]
	[XmlRoot("c", DataType="string", Namespace="", IsNullable=false)]
	public class SpellCorrection
	{
		[XmlAttributeAttribute("o", DataType="int")]
		public int Offset = 0;
		
		[XmlAttributeAttribute("l", DataType="int")]
		public int Length = 0;
		
		[XmlAttributeAttribute("s", DataType="int")]
		public int Confidence = 0;
		
		[XmlTextAttribute()]
		public string Value = "";

		public SpellCorrection()
		{

		}

		public string[] Suggestions
		{
			get
			{
				if (Value != null && Value.Length > 0)
					return Value.Split('\t');
				else
					return new string[] {};
			}
		}
	}
}
