using System;

namespace Microsoft.Office.OneNote
{
	/// <summary>
	/// Represents errors that occur during import into OneNote.
	/// </summary>
	[Serializable]
	public class ImportException : Exception
	{
		/// <summary>
		/// Constructs a new ImportException.
		/// </summary>
		public ImportException()
		{
		}

		/// <summary>
		/// Constructs a new ImportException based off of the
		/// specified HResult.
		/// </summary>
		/// <param name="errorCode">
		/// The HRESULT returned by the ISimpleImporter COM interface.
		/// </param>
		public ImportException(int errorCode)
		{
			HResult = errorCode;

			// Map the hResult to an error string:
			switch ((uint) errorCode)
			{
				case 0x80041000:
					// MALFORMED_XML_ERROR
					// Seeing this error indicates that something major
					// has gone really really wrong, particularly given
					// that we actually validate all XML before importing!
					message = "Error: Attempting to import malformed XML!";
					break;

				case 0x80041001:
					// INVALID_XML_ERROR
					// Ditto -- this should never be happening, but may
					// be occurring if our schema doesn't match the schema
					// being used by OneNote.
					message = "Error: Attempting to import invalid XML!";
					break;

				case 0x80041002:
					// ERROR_CREATING_SECTION
					// OneNote tried to create the section specified but 
					// failed -- the filepath may have been invalid, or
					// permission was denied, etc.
					message = "Unable to create the specified section.";
					break;

				case 0x80041003:
					// ERROR_OPENING_SECTION
					// The specified section could not be opened -- most
					// likely due to lack of file permissions.
					message = "Unable to open the specified section.";
					break;

				case 0x80041004:
					// SECTION_DOES_NOT_EXIST_ERROR
					// The section specified no longer exists... perhaps
					// the user deleted it after it was initially imported?
					// This is a fairly normal error when calling 
					// Page.NavigateTo(), but shouldn't be thrown via Page.Commit().
					message = "The specified section does not exist.";
					break;

				case 0x80041005:
					// PAGE_DOES_NOT_EXIST_ERROR
					// The section page no longer exists... perhaps
					// the user deleted it after it was initially imported?
					// This is a fairly normal error when calling 
					// Page.NavigateTo(), but shouldn't be thrown via Page.Commit().
					message = "The specified page does not exist.";
					break;
				
				case 0x80041006:
					// FILE_DOES_NOT_EXIST_ERROR
					// One of the FileData objects is referencing a path
					// that does not exist, or cannot be accessed due
					// to permission issues.
					message = "Unable to access the referenced file.";
					break;
				
				case 0x80041007:
					// ERROR_INSERTING_IMAGE
					// The specified image file or data is not a supported
					// image filetype.
					message = "Unrecognized image data.";
					break;
				
				case 0x80041008:
					// ERROR_INSERTING_INK
					// The specified ink file or data is not valid ISF.
					message = "Unrecognized ink data.";
					break;
				
				case 0x80041009:
					// ERROR_INSERTING_HTML
					// The specified html file could not be imported
					message = "Error import html content.";
					break;
				
				case 0x8004100a:
					// ERROR_NAVIGATING_TO_PAGE
					// An unknown error occurred while OneNote attempting to 
					// navigate to the specified page.
					message = "Unable to navigate to the specified page.";
					break;
			}
		}

		/// <summary>
		/// Constructs a new ImportException with the specified message.
		/// </summary>
		/// <param name="message">The error message to be displayed to the user.</param>
		public ImportException(string message)
		{
			this.message = message;
		}

		/// <summary>
		/// Gets a message that describes the current exception.   
		/// </summary>
		public override string Message
		{
			get
			{
				return message;
			}
		}

		/// <summary>
		/// A message describing the current exception.
		/// </summary>
		private string message;
	}
}
