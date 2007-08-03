#region CVS Version Header
/*
 * $Id: ISearchEngine.cs,v 1.1 2005/09/08 13:42:45 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2005/09/08 13:42:45 $
 * $Revision: 1.1 $
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