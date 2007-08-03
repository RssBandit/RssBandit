using System;

namespace NewsComponents.Search.BooleanSearch
{
	/// <summary>
	/// IDocument is an interface class responsible for providing search facilities
	/// to the QueryTree class. You can subclass it in order to provide support for
	/// plain-text searching across a variety of media.
	/// </summary>
	public interface IDocument
	{
		bool Find(string str);
		string Name();
	}
}
