#region Using directives

using System.Collections;

#endregion

namespace NewsComponents.Xml.Serialization
{
	/// <summary>
	/// Helper class to simpify sorting
	/// strings (Not really necessary in Whidbey).
	/// </summary>
	internal class StringSorter
	{
		ArrayList list = new ArrayList();
		
		/// <summary>
		/// Helper class to sort strings alphabetically
		/// </summary>
		public StringSorter()
		{

		}

		/// <summary>
		/// Add a string to sort
		/// </summary>
		/// <param name="s"></param>
		public void AddString( string s )
		{
			list.Add(s);
		}

		/// <summary>
		/// Sort the strings that were added by calling
		/// <see cref="AddString"/>
		/// </summary>
		/// <returns>A sorted string array.</returns>
		public string[] GetOrderedArray()
		{
			list.Sort();
			return list.ToArray(typeof(string)) as string[];
		}
	}
}
