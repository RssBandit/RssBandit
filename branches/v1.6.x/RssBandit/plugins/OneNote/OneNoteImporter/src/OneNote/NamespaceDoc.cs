namespace Microsoft.Office.OneNote
{
#if DEBUG
	/// <summary>
	/// The Microsoft.Office.OneNote assembly provides a type-safe, managed
	/// interface to the new DataImport features available with the OneNote 
	/// SP1 release.  As an example, one can write:
	/// <code>
	/// Page p = new Page("General.one", "Import Test");
	/// OutlineObject outline = new OutlineObject();
	/// outline.AddContent(new HtmlContent("Hello World!");
	/// p.AddObject(outline);
	/// 
	/// p.Commit();
	/// p.NavigateTo();
	/// </code>
	/// to create a new OneNote page in the section "General" with the text
	/// "Hello World!".
	/// </summary>
	public class NamespaceDoc
	{
	}
#endif
}
