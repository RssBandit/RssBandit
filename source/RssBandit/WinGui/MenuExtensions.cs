using RssBandit.WinGui.Menus;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace RssBandit.WinGui
{
    internal static class MenuExtensions
    {
        public static ToolStripMenuItem CloneMenu(this ToolStripMenuItem item)
        {
            ToolStripMenuItem retVal;
            if(item is AppContextMenuCommand command)
            {
                retVal = new AppContextMenuCommand(command.CommandID, command.Mediator, command.Executor, command.Text, command.Description);
            }
            else
            {
                retVal = new ToolStripMenuItem
                {
                    Tag = item.Tag,
                    Text = item.Text,
                };
            }


            retVal.Checked = item.Checked;
            retVal.Enabled = item.Enabled;
            retVal.ShortcutKeys = item.ShortcutKeys;
            retVal.ShowShortcutKeys = item.ShowShortcutKeys;
            retVal.Image = item.Image?.Clone() as Image;

            foreach (ToolStripMenuItem child in item.DropDownItems)
            {
                retVal.DropDownItems.Add(child.CloneMenu());
            }

            return retVal;
        }
    }
}
