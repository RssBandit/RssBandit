using System;
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
