#region Version Info Header
/*
 * $Id$
 * $HeadURL$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using NewsComponents.Utils;

namespace RssBandit.WinGui.ViewModel
{
    public class FolderViewModel : TreeNodeViewModelBase
    {

        private string _name;
        
        public FolderViewModel(string name)
        {
            name.ExceptionIfNullOrEmpty("name");
            _name = name;
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

       
      
    }
}