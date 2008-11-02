#region Using directives

using System;
using System.Windows.Forms;

#endregion

namespace Genghis.Windows.Forms
{
//-----------------------------------------------------------------------------
//<filedescription file="CursorChanger.cs" company="Microsoft">
//  <copyright>
//     Copyright (c) 2004 Microsoft Corporation.  All rights reserved.
//  </copyright>
//  <purpose>
//  Contains Cursor Changer class which enables to change the cursor
//  of the current window.
//  </purpose>
//  <notes>
//  </notes>
//</filedescription>                                                                
//-----------------------------------------------------------------------------


    /// <summary>
    /// A class to change the cursor for the current window. 
    /// Upon Disposal, the class will return the original 
    /// cursor.
    /// </summary>
    /// <example>This example shows how the cursor will be changed during the scope 
    /// of the object (before disposal):
    /// <code>
    /// using (ChangeCursor cursorChanger = new CursorChanger(Cursors.WaitCursor))
    /// {
    ///   ...
    /// }
    /// </code>
    /// </example>
    public class CursorChanger : IDisposable
    {
        /// <summary>
        /// Constructs a new CursorChanger object specifying the new Cursor to show.
        /// </summary>
        /// <param name="newCursor">The new cursor to show during the lifetime 
        /// of the CursorChanger object</param>
        public CursorChanger(Cursor newCursor)
        {
            // Cache the original Cursor
            _originalCursor = Cursor.Current;

            // Change the cursor
            Cursor.Current = newCursor;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~CursorChanger()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose method.  Returns the original 
        /// </summary>
        public void Dispose()
        {
            Cursor.Current = _originalCursor;
        }

        Cursor _originalCursor;
    }
}
