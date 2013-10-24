#region License / Copyright
/* 
Copyright (c) 2003-2013, Dare Obasanjo & Torsten Rendelmann
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:
    * Redistributions of source code must retain the above copyright
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright
      notice, this list of conditions and the following disclaimer in the
      documentation and/or other materials provided with the distribution.
    * Neither the name RSS Bandit nor the
      names of its contributors may be used to endorse or promote products
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

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
using System.Runtime.Remoting.Messaging;

namespace RssBandit.ViewModel
{
	/// <summary>
	/// When publishing events in C#, you need to test that the delegate has targets. 
	/// You also must handle exceptions the subscribers throw, otherwise, the publishing 
	/// sequence is aborted. You can iterate over the delegate’s internal invocation list 
	/// and handle individual exceptions that way. 
	/// This generic helper class called EventsHelper that does just that. 
	/// EventsHelper can publish to any delegate, accepting any collection of parameters. 
	/// EventsHelper can also publish asynchronously and concurrently to the subscribers 
	/// using the thread pool, turning any subscriber’s target method into a fire-and-forget 
	/// method.
	/// </summary>
	/// <remarks>Thanks to http://www.idesign.net/ </remarks>
	public class EventsHelper 
	{
		private static readonly log4net.ILog _log = Common.Logging.Log.GetLogger(typeof(EventsHelper));
		delegate void AsyncFire(Delegate eventDelegate, object[] args);
		
		/// <summary>
		/// Fires the specified event delegate.
		/// </summary>
		/// <param name="eventDelegate">The delegate.</param>
		/// <param name="args">The optional arguments.</param>
		public static void Fire(Delegate eventDelegate,params object[] args)
		{
			Delegate temp = eventDelegate;
			if (temp == null)
			{
				return;
			}
			Delegate[] delegates = temp.GetInvocationList();
			foreach (Delegate sink in delegates)
			{
				try
				{
					sink.DynamicInvoke(args);
				}
				catch (Exception sinkEx)
				{
					if (temp.Method.DeclaringType != null)
						_log.Error(String.Format("Calling '{0}.{1}' caused an exception.", temp.Method.DeclaringType.FullName, temp.Method.Name), sinkEx);
				}
			}
		}

		/// <summary>
		/// Fires the event delegate asynchronous.
		/// </summary>
		/// <param name="eventDelegate">The event delegate.</param>
		/// <param name="args">The arguments.</param>
		public static void FireAsync(Delegate eventDelegate, params object[] args)
		{
			Delegate temp = eventDelegate;
			if (temp == null)
			{
				return;
			}
			Delegate[] delegates = eventDelegate.GetInvocationList();
			foreach (Delegate sink in delegates)
			{
				AsyncFire asyncFire = InvokeDelegate;
				asyncFire.BeginInvoke(sink, args, null, null);
			}
		}

		
		/// <summary>
		/// Invokes the event delegate.
		/// </summary>
		/// <param name="eventDelegate">The event delegate.</param>
		/// <param name="args">The arguments.</param>
		[OneWay]
		static void InvokeDelegate(Delegate eventDelegate, object[] args)
		{
			eventDelegate.DynamicInvoke(args);
		}

	}
}