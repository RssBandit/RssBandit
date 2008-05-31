#region CVS Version Header

/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */

#endregion

using System;
using System.Collections;
using RssBandit.AppServices;
using RssBandit.WinGui.Interfaces;
using RssBandit.WinGui.Menus;
using RssBandit.WinGui.Tools;

namespace RssBandit.WinGui.Utility
{
    /// <summary>
    /// Summary description for CommandMediator.
    /// </summary>
    public class CommandMediator : ICommandMediator
    {
        public event EventHandler BeforeCommandStateChanged;
        public event EventHandler AfterCommandStateChanged;

        private readonly Hashtable registeredCommands;

        public CommandMediator()
        {
            registeredCommands = new Hashtable(15);
        }

        /// <summary>
        /// Register a GUI component that implements ICommandComponent that
        /// visual state (Enabled, Checked, Visible) have to be controlled by 
        /// the Application.
        /// </summary>
        /// <param name="cmdId">A command identifier. Multiple commands that executes the same action should have the same identifier. 
        /// They will be switched all to the same state on every state change request.</param>
        /// <param name="cmd">The UI component.</param>
        public void RegisterCommand(string cmdId, ICommandComponent cmd)
        {
            ArrayList al;
            if (registeredCommands.ContainsKey(cmdId))
            {
                al = (ArrayList) registeredCommands[cmdId];
            }
            else
            {
                al = new ArrayList(1);
                registeredCommands.Add(cmdId, al);
            }
            al.Add(cmd);
        }

        /// <summary>
        /// Re-registers the command.
        /// </summary>
        public void ReRegisterCommand(ICommand cmd)
        {
            if (cmd != null && registeredCommands.ContainsKey(cmd.CommandID))
            {
                var al = (ArrayList) registeredCommands[cmd.CommandID];
                Type toRegisterType = cmd.GetType();
                for (int i = 0; al != null && i < al.Count; i++)
                {
                    object registeredCommand = al[i];
                    if (registeredCommand.GetType().Equals(toRegisterType))
                    {
                        if (registeredCommand is AppPopupMenuCommand)
                        {
                            var appCmd = (AppPopupMenuCommand) cmd;
                            appCmd.ReJoinMediatorFrom((AppPopupMenuCommand) registeredCommand);
                            ((AppPopupMenuCommand) al[i]).Dispose();
                            al[i] = appCmd;
                        }
                        else if (registeredCommand is AppButtonToolCommand)
                        {
                            var appCmd = (AppButtonToolCommand) cmd;
                            appCmd.ReJoinMediatorFrom((AppButtonToolCommand) registeredCommand);
                            ((AppButtonToolCommand) al[i]).Dispose();
                            al[i] = appCmd;
                        }
                        else if (registeredCommand is AppStateButtonToolCommand)
                        {
                            var appCmd = (AppStateButtonToolCommand) cmd;
                            appCmd.ReJoinMediatorFrom((AppStateButtonToolCommand) registeredCommand);
                            ((AppStateButtonToolCommand) al[i]).Dispose();
                            al[i] = appCmd;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Unregister a command.
        /// </summary>
        /// <param name="cmdId">Command identifier</param>
        /// <param name="cmd">The UI component</param>
        public void UnregisterCommand(string cmdId, ICommandComponent cmd)
        {
            if (registeredCommands.ContainsKey(cmdId) && cmd != null)
            {
                var al = (ArrayList) registeredCommands[cmdId];
                al.Remove(cmd);
            }
        }

        private bool IsCommandComponentEnabled(string cmdId)
        {
            if (registeredCommands.ContainsKey(cmdId))
                return ((ICommandComponent) ((ArrayList) registeredCommands[cmdId])[0]).Enabled;
            return false;
        }

        private void SetCommandComponentEnabled(string cmdId, bool newValue)
        {
            if (registeredCommands.ContainsKey(cmdId))
            {
                var al = (ArrayList) registeredCommands[cmdId];
                foreach (ICommandComponent cmd in al)
                    cmd.Enabled = newValue;
            }
        }

        private bool IsCommandComponentChecked(string cmdId)
        {
            if (registeredCommands.ContainsKey(cmdId))
                return ((ICommandComponent) ((ArrayList) registeredCommands[cmdId])[0]).Checked;
            return false;
        }

        private void SetCommandComponentChecked(string cmdId, bool newValue)
        {
            if (registeredCommands.ContainsKey(cmdId))
            {
                var al = (ArrayList) registeredCommands[cmdId];
                foreach (ICommandComponent cmd in al)
                    cmd.Checked = newValue;
            }
        }

        private bool IsCommandComponentVisible(string cmdId)
        {
            if (registeredCommands.ContainsKey(cmdId))
                return ((ICommandComponent) ((ArrayList) registeredCommands[cmdId])[0]).Visible;
            return false;
        }

        private void SetCommandComponentVisible(string cmdId, bool newValue)
        {
            if (registeredCommands.ContainsKey(cmdId))
            {
                var al = (ArrayList) registeredCommands[cmdId];
                foreach (ICommandComponent cmd in al)
                    cmd.Visible = newValue;
            }
        }

        /// <summary>
        /// Switches the visual state of all registered ICommandComponents
        /// to enabled/disabled on base of the provided arguments.
        /// </summary>
        /// <param name="args">Provide an array of strings build up like this:
        /// <c>SetEnable("+cmdCloseExit", "-cmdMoveNext", "-cmdMoveBack")</c>.
        /// The first character controls the enable ("+") and disable ("-") state,
        /// the following characters specify a registered command ID.
        /// </param>
        public void SetEnabled(params string[] args)
        {
            RaiseBeforeCommandStateChanged();
            foreach (var cmdParam in args)
            {
                bool b = String.Compare(cmdParam.Substring(0, 1), "+") == 0;
                string cmdId = cmdParam.Substring(1);
                SetCommandComponentEnabled(cmdId, b);
            }
            RaiseAfterCommandStateChanged();
        }

        public void SetEnabled(bool newState, params string[] args)
        {
            RaiseBeforeCommandStateChanged();
            foreach (var cmdParam in args)
            {
                SetCommandComponentEnabled(cmdParam, newState);
            }
            RaiseAfterCommandStateChanged();
        }

        public void SetChecked(params string[] args)
        {
            RaiseBeforeCommandStateChanged();
            foreach (var cmdParam in args)
            {
                bool b = String.Compare(cmdParam.Substring(0, 1), "+") == 0;
                string cmdId = cmdParam.Substring(1);
                SetCommandComponentChecked(cmdId, b);
            }
            RaiseAfterCommandStateChanged();
        }

        public void SetChecked(bool newState, params string[] args)
        {
            RaiseBeforeCommandStateChanged();
            foreach (var cmdParam in args)
            {
                SetCommandComponentChecked(cmdParam, newState);
            }
            RaiseAfterCommandStateChanged();
        }

        public void SetVisible(params string[] args)
        {
            RaiseBeforeCommandStateChanged();
            foreach (var cmdParam in args)
            {
                bool b = String.Compare(cmdParam.Substring(0, 1), "+") == 0;
                string cmdId = cmdParam.Substring(1);
                SetCommandComponentVisible(cmdId, b);
            }
            RaiseAfterCommandStateChanged();
        }

        public void SetVisible(bool newState, params string[] args)
        {
            RaiseBeforeCommandStateChanged();
            foreach (var cmdParam in args)
            {
                SetCommandComponentVisible(cmdParam, newState);
            }
            RaiseAfterCommandStateChanged();
        }

        public void Execute(string identifier)
        {
            if (registeredCommands.ContainsKey(identifier))
            {
                var al = (ArrayList) registeredCommands[identifier];
                foreach (ICommand cmd in al)
                {
                    cmd.Execute();
                    break; // not every instance, just one command to execute
                }
            }
        }

        /// <summary>
        /// Gets the unified checked state of the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <remarks>Currently the checked state returned here depends on
        /// the kind of used component. For IG menu/state buttons the
        /// check state is already changed after user clicked the menu, while
        /// the (build in) context menu item has still the old
        /// checked state</remarks>
        /// <returns></returns>
        public bool IsChecked(ICommand command)
        {
            bool _checked = false;

            var cmd = command as ICommandComponent;
            if (cmd != null)
            {
                // visual state not yet changed:
                if (command is AppContextMenuCommand)
                    _checked = !cmd.Checked;

                // visual state IS already changed:
                if (command is AppStateButtonToolCommand)
                    _checked = cmd.Checked;
            }
            return _checked;
        }

        private void RaiseBeforeCommandStateChanged()
        {
            if (BeforeCommandStateChanged != null)
                BeforeCommandStateChanged(this, EventArgs.Empty);
        }

        private void RaiseAfterCommandStateChanged()
        {
            if (AfterCommandStateChanged != null)
                AfterCommandStateChanged(this, EventArgs.Empty);
        }

        #region ICommandMediator Members

        public bool IsVisible(string identifier)
        {
            return IsCommandComponentVisible(identifier);
        }

        public bool IsChecked(string identifier)
        {
            return IsCommandComponentChecked(identifier);
        }

        public bool IsEnabled(string identifier)
        {
            return IsCommandComponentEnabled(identifier);
        }

        void ICommandMediator.SetChecked(params string[] identifierArgs)
        {
            SetChecked(true, identifierArgs);
        }

        public void SetUncheck(params string[] identifierArgs)
        {
            SetChecked(false, identifierArgs);
        }

        void ICommandMediator.SetEnabled(params string[] identifierArgs)
        {
            SetEnabled(true, identifierArgs);
        }

        public void SetDisabled(params string[] identifierArgs)
        {
            SetEnabled(false, identifierArgs);
        }

        public void SetInvisible(params string[] identifierArgs)
        {
            SetVisible(false, identifierArgs);
        }

        void ICommandMediator.SetVisible(params string[] identifierArgs)
        {
            SetVisible(true, identifierArgs);
        }

        #endregion
    }
}

#region CVS Version Log

/*
 * $Log: CommandMediator.cs,v $
 * Revision 1.8  2006/12/15 13:31:01  t_rendelmann
 * reworked to make dynamic menus work after toolbar gets loaded from .settings.xml
 *
 * Revision 1.6  2006/12/10 11:54:17  t_rendelmann
 * fixed: column layout context menu not working correctly (regression caused by toolbar migration)
 *
 */

#endregion