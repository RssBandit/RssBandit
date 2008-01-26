/* ***** BEGIN LICENSE BLOCK ***** 
 * Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code was the NewsGator RSS Subscriber Extension,
 * modified to support feed subscriptions in the default aggregator (an 
 * feed aggregator that handles the "feed:" url protocol).
 *
 *The Initial Developer of the Original Code is Stuart Hamilton, modifications
 * to support the default aggregator by Torsten  Rendelmann.
 * Portions created by the Initial Developer are Copyright (C) 2004
 * the Initial Developer. All Rights Reserved.
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

// start of definition 
if (!window.feedSubscribeService) {

	var feedSubscribeService =
	{	
		initialized               : false,

		init : function(aIgnoreInitializedFlag)
		{
			try {
				this.updateGeneralFunctions();
				this.initialized = true;
			}
			catch(e) {
				alert(e);
			}
		},
		
		updateGeneralFunctions : function()
		{
			// Context Menu
			if ('nsContextMenu' in window) {
				nsContextMenu.prototype.__feedSubscribe__initItems = nsContextMenu.prototype.initItems;
				nsContextMenu.prototype.initItems = this.initItems;
			}
		},

		// updating nsContextMenu
		initItems : function()
		{
			this.__feedSubscribe__initItems();

			this.showItem( "context-sep-feedSubscribeRSS", this.onSaveableLink || ( this.inDirList && this.onLink ) );
			this.showItem( "context-feedSubscribeRSS", this.onSaveableLink || ( this.inDirList && this.onLink ) );
		},

		// Add RSS subscription to default aggregator.
		subscribeRSS : function( linkURL ) {
			// Determine linked-to URL.
			var path = ""+linkURL; 
	        
			// we use windows registry to access the feed: protocol handler:
			var isMac = (navigator.platform.toLowerCase().indexOf("mac") != -1);
			 if (navigator.platform.indexOf('Win32') == -1 || isMac) {
				alert( "This extension does not support your platform: "+ navigator.platform );
				return;
			}
	        
			if (path) {
				try {

					const nsIWindowsRegistry = Components.interfaces.nsIWindowsRegistry;
					const lfContractID1 = "@mozilla.org/winhooks;1"
					var reg = Components.classes[lfContractID1].createInstance(nsIWindowsRegistry);
					var cmdFeedHandler = reg.getRegistryEntry(0 , "feed\\shell\\open\\command", "" );
					if (cmdFeedHandler) {
						
						// cut out cmd.exe commandline placeholders:
						cmdFeedHandler = cmdFeedHandler.replace('"%1"','','g').replace('%1','','g');
						
						// Create a local file object pointing at the feed handler executable
						const nsILocalFile = Components.interfaces.nsILocalFile;
						const lfContractID = "@mozilla.org/file/local;1";
						var exe = Components.classes[lfContractID].createInstance(nsILocalFile);
						exe.initWithPath(cmdFeedHandler);
	
						// Create a new process for the executable
						const nsIProcess = Components.interfaces.nsIProcess;
						const lfContractID2 = "@mozilla.org/process/util;1";
						var proc = Components.classes[lfContractID2].createInstance(nsIProcess);
						proc.init(exe.nsIFile);
						
						// Run the executable, passing path as an argument
						proc.run(false, [path], 1);
					}

				} 
				catch (ex) {
					alert("Unable to add subscription to Default Aggregator: "+ex)
				}
			}
		}
	}
} //end of definition

// Listeners and Observers
window.addEventListener('load', function()
{
	if (feedSubscribeService.initialized) return;

	feedSubscribeService.init();
	
	//alert( 'Added event listener' );
	
},
false);