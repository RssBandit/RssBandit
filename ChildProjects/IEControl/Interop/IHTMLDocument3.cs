#region Copyright
/*
Copyright (c) 2004-2006 by Torsten Rendelmann

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 2.1 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public
License along with this library; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */
#endregion

using System.Runtime.InteropServices;

namespace IEControl
{
	/// <summary>
	/// mshtml.IHTMLDocument3 interface
	/// </summary>
	[
	InterfaceType(ComInterfaceType.InterfaceIsDual), 
	ComVisible(true), 
	Guid(@"3050f485-98b5-11cf-bb82-00aa00bdce0b")
	]
	public interface IHTMLDocument3 {

		/// <summary>
		/// </summary>
		void releaseCapture(); 
		/// <summary>
		/// </summary>
		/// <param name="fForce"></param>
		void recalc(bool fForce);
		
		/// <summary>
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		[return: MarshalAs(UnmanagedType.Interface)] /* IHTMLDOMNode */
		object createTextNode(string text);

		/// <summary>
		/// </summary>
		/// <returns></returns>
		IHTMLElement documentElement();

		//... we need only documentElement(), more functions/properties see MSHTML.Idl/.h
	}
}
