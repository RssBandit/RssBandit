#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

namespace RssBandit.WebSearch
{
	public interface ISearchEngine
	{
		/// <remarks/>
		string Title { get;  }

		/// <remarks/>
		string SearchLink { get;  }

		/// <remarks/>
		string Description { get;  }

		/// <remarks/>
		string ImageName { get;  }

		/// <remarks/>
		bool IsActive { get;  }

		/// <remarks/>
		bool ReturnRssResult { get;  }

		/// <remarks/>
		bool MergeRssResult { get;  }
	}
}