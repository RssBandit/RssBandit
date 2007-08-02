#region CVS Version Header
/*
 * $Id: CommandMediator.cs,v 1.3 2003/12/17 14:03:24 t_rendelmann Exp $
 * Last modified by $Author: t_rendelmann $
 * Last modified at $Date: 2003/12/17 14:03:24 $
 * $Revision: 1.3 $
 */
#endregion

using System;
using System.Collections;
using RssBandit.WinGui.Interfaces;

namespace RssBandit.WinGui.Utility
{
	/// <summary>
	/// Summary description for CommandMediator.
	/// </summary>
	public class CommandMediator
	{
		public CommandMediator()
		{
			this.registeredCommands = new Hashtable(15);
		}

		/// <summary>
		/// Register a GUI component that implements ICommandComponent that
		/// visual state (Enabled, Checked, Visible) have to be controlled by 
		/// the Application.
		/// </summary>
		/// <param name="cmdId">A command identifier. Multiple commands that executes the same action should have the same identifier. 
		/// They will be switched all to the same state on every state change request.</param>
		/// <param name="cmd">The GUI component.</param>
		public void RegisterCommand(string cmdId, ICommandComponent cmd) {
			ArrayList al;
			if (registeredCommands.ContainsKey(cmdId)) {
				al = (ArrayList)registeredCommands[cmdId];
			} 
			else {
				al = new ArrayList();
				registeredCommands.Add (cmdId,al);
			}
			al.Add(cmd);
		}

		public bool IsCommandComponentEnabled(string cmdId) {
			if (registeredCommands.ContainsKey(cmdId)) 
				return ((ICommandComponent)((ArrayList)registeredCommands[cmdId])[0]).Enabled;
			return false;
		}
		public void SetCommandComponentEnabled(string cmdId, bool newValue) {
			if (registeredCommands.ContainsKey(cmdId)) {
				ArrayList al = (ArrayList)registeredCommands[cmdId];
				foreach (ICommandComponent cmd in al)
					cmd.Enabled = newValue;
			}
		}
		public bool IsCommandComponentChecked(string cmdId) {
			if (registeredCommands.ContainsKey(cmdId)) 
				return ((ICommandComponent)((ArrayList)registeredCommands[cmdId])[0]).Checked;
			return false;
		}
		public void SetCommandComponentChecked(string cmdId, bool newValue) {
			if (registeredCommands.ContainsKey(cmdId)) {
				ArrayList al = (ArrayList)registeredCommands[cmdId];
				foreach (ICommandComponent cmd in al)
					cmd.Checked = newValue;
			}
		}
		public bool IsCommandComponentVisible(string cmdId) {
			if (registeredCommands.ContainsKey(cmdId)) 
				return ((ICommandComponent)((ArrayList)registeredCommands[cmdId])[0]).Visible;
			return false;
		}
		public void SetCommandComponentVisible(string cmdId, bool newValue) {
			if (registeredCommands.ContainsKey(cmdId)) {
				ArrayList al = (ArrayList)registeredCommands[cmdId];
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
		public void SetEnable(params string[] args) {
			bool b; string cmdId;
			foreach (string cmdParam in args) {
				b = String.Compare(cmdParam.Substring(0,1),"+") == 0;
				cmdId = cmdParam.Substring(1);
				SetCommandComponentEnabled (cmdId, b);
			}
		}
		public void SetEnable(bool newState, params string[] args) {
			foreach (string cmdParam in args) {
				SetCommandComponentEnabled (cmdParam, newState);
			}
		}

		public void SetChecked(params string[] args) {
			bool b; string cmdId;
			foreach (string cmdParam in args) {
				b = String.Compare(cmdParam.Substring(0,1),"+") == 0;
				cmdId = cmdParam.Substring(1);
				SetCommandComponentChecked (cmdId, b);
			}
		}
		public void SetChecked(bool newState, params string[] args) {
			foreach (string cmdParam in args) {
				SetCommandComponentChecked (cmdParam, newState);
			}
		}

		public void SetVisible(params string[] args) {
			bool b; string cmdId;
			foreach (string cmdParam in args) {
				b = String.Compare(cmdParam.Substring(0,1),"+") == 0;
				cmdId = cmdParam.Substring(1);
				SetCommandComponentVisible (cmdId, b);
			}
		}
		public void SetVisible(bool newState, params string[] args) {
			foreach (string cmdParam in args) {
				SetCommandComponentVisible (cmdParam, newState);
			}
		}

		private Hashtable registeredCommands;
	}
}
