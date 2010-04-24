#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NewsComponents;

namespace RssBandit.WinGui.ViewModel
{
    public class FeedSourceViewModel: ViewModelBase
    {
        private readonly FeedSourceEntry _feedSource;
        
        public FeedSourceViewModel(FeedSourceEntry feedSource)
        {
            _feedSource = feedSource;
        }

        public string Name {
            get { return _feedSource.Name; }
            set { _feedSource.Name = value; }
        }
    }
}
