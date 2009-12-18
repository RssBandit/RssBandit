using System;

using System.ComponentModel;
using System.Drawing;

using System.Windows.Forms;
using Infragistics.Win.UltraWinEditors;
using Infragistics.Win;
using Ribbon.WindowsApplication;
using Appearance=Infragistics.Win.Appearance;

namespace RssBandit.UI.Forms
{
    public partial class UrlComboBox : UltraComboEditor, IDragDropSource
    {
        public event EventHandler<BeforeNavigateCancelEventArgs> BeforeNavigate ;
        public event EventHandler<NavigatedEventArgs> Navigated;

        private readonly UrlCompletionExtender urlExtender;
        private Rectangle _dragOriginBox = Rectangle.Empty;
        
        public UrlComboBox()
        {
            urlExtender = new UrlCompletionExtender(this);

            AllowDrop = true;
            AutoCompleteMode = Infragistics.Win.AutoCompleteMode.SuggestAppend;

            DisplayStyle = EmbeddableElementDisplayStyle.Office2007;
            DropDownButtonDisplayStyle = ButtonDisplayStyle.OnMouseEnter;

            HasMRUList = true;
            MaxMRUItems = 3;

            Appearance appearance = new Infragistics.Win.Appearance();
            appearance.ForeColor = SystemColors.GrayText;
            NullText = "[empty]";   //TODO localize
            NullTextAppearance = appearance;

            ShowOverflowIndicator = true;
            SortStyle = ValueListSortStyle.Ascending;

            // this do the trick to auto-complete with the system urls/IE urls:
            this.AutoCompleteMode = Infragistics.Win.AutoCompleteMode.None;
            TextBox tb = ((EditorWithCombo)this.Editor).TextBox;
            tb.AutoCompleteSource = AutoCompleteSource.AllUrl;
            tb.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;

        }

        public virtual void OnBeforeNavigate(BeforeNavigateCancelEventArgs args)
        {
            if (BeforeNavigate != null)
                BeforeNavigate(this, args);
        }

        public virtual void OnNavigated(NavigatedEventArgs args)
        {
            if (Navigated != null)
                Navigated(this, args);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            
            if (!DesignMode)
            {
                Infragistics.Win.Appearance appearance = new Infragistics.Win.Appearance();
                appearance.Image = Properties.Resources.Html_16;
                EditorButton dragUrlButton = new EditorButton();
                dragUrlButton.Appearance = appearance;
                ButtonsLeft.Add(dragUrlButton);
            }
        }

        protected override void OnSelectionChangeCommitted(EventArgs args)
        {
            base.OnSelectionChangeCommitted(args);
            
            Uri uri;
            if (CanNavigate(Text, out uri))
            {
                OnNavigated(new NavigatedEventArgs(uri));
            }
        }

        protected override void OnBeforeExitEditMode(BeforeExitEditModeEventArgs args)
        {
            base.OnBeforeExitEditMode(args);

            Uri uri;
            if (!args.Cancel && !args.IsForcingExit)
                args.Cancel = !CanNavigate(Text, out uri);
        }

        protected override void OnAfterExitEditMode(EventArgs args)
        {
            base.OnAfterExitEditMode(args);
            
            Uri uri;
            if (CanNavigate(Text, out uri))
            {
                if (null == ValueList.FindByDataValue(uri))
                    Items.Add(uri, uri.AbsoluteUri);
                OnNavigated(new NavigatedEventArgs(uri));
            }
        }

        bool CanNavigate(string text, out Uri uri)
        {
            if (Uri.TryCreate(text, UriKind.Absolute, out uri))
            {
                BeforeNavigateCancelEventArgs e = new BeforeNavigateCancelEventArgs(uri);
                OnBeforeNavigate(e);
                return !e.Cancel;
            }
            return false;
        }

        protected override void OnEditorButtonClick(EditorButtonEventArgs e)
        {
            base.OnEditorButtonClick(e);
            // 
        }

        #region IDragDropSource

        private string _dragDropGroup = "";
        /// <summary>
        /// Drag-and-drop group to which the control belongs. Drag-and-drop is restricted to happen between controls having the same DragDropGroup.
        /// </summary>
        /// <example>Let's assume that we have a form with four DragDropListBoxes on it. Two of them contain cats and two of them contain dogs.
        /// We only want to be able to move cats between the cat lists and dogs between the dog lists (cats and dogs don't like each other).
        /// We can achieve this simply by setting the DragDropGroup property of the cat lists to "cats". In the dog lists we can leave the
        /// DragDropGroup empty or we can set it to "dogs" for instance. It just has to be different from the DragDropGroup in the cat lists.
        /// <code>catList1.DragDropGroup = "cats";   catList2.DragDropGroup = "cats";</code>
        /// </example>
        [Category("Behavior (drag-and-drop)"), DefaultValue(""), Description("Drag-and-drop group to which the control belongs. Drag-and-drop is restricted to happen between controls having the same DragDropGroup.")]
        public string DragDropGroup
        {
            get { return _dragDropGroup; }
            set { _dragDropGroup = value; }
        }

        private bool _isDragDropCopySource = true;
        /// <summary>
        /// Indicates whether the user can copy items from this control by draging them to another control.
        /// </summary>
        [Category("Behavior (drag-and-drop)"), DefaultValue(true), Description("Indicates whether the user can copy items from this control by draging them to another control.")]
        public bool IsDragDropCopySource
        {
            get { return _isDragDropCopySource; }
            set { _isDragDropCopySource = value; }
        }

        private bool _isDragDropMoveSource = true;
        /// <summary>
        /// Indicates whether the user can remove items from this control by draging them to another control.
        /// </summary>
        [Category("Behavior (drag-and-drop)"), DefaultValue(true), Description("Indicates whether the user can remove items from this control by draging them to another control.")]
        public bool IsDragDropMoveSource
        {
            get { return _isDragDropMoveSource; }
            set { _isDragDropMoveSource = value; }
        }

        /// <summary>
        /// Returns the selected list items in a array.
        /// </summary>
        /// <returns>Array with the selected items.</returns>
        object[] IDragDropSource.GetSelectedItems()
        {
            if (this.SelectedItem != null)
            {
                return new object[] { this.SelectedItem.DisplayText };
            }
            return new object[] { };
        }

        /// <summary>
        /// Removes the selected items from the list and adjusts the item-index passed to this method,
        /// so that this index points to the same item afterwards.
        /// </summary>
        /// <param name="rowIndexToAjust">The row index to ajust.</param>
        void IDragDropSource.RemoveSelectedItems(ref int rowIndexToAjust)
        {
            //for (int i = SelectedIndices.Count - 1; i >= 0; i--)
            //{
            //    int at = SelectedIndices[i];
            //    Items.RemoveAt(at);
            //    if (at < itemIndexToAjust)
            //    {
            //        itemIndexToAjust--;  // Adjust index pointing to stuff behind the delete position.
            //    }
            //}
        }

        /// <summary>
        /// Is called when a drag-and-drop operation is completed in order to raise the Dropped event.
        /// </summary>
        /// <param name="e">Event arguments which hold information on the completed operation.</param>
        /// <remarks>Is called for the target as well as for the source.
        /// The role a control plays (source or target) can be determined from e.Operation.</remarks>
        public virtual void OnDropped(DroppedEventArgs e)
        {
            var dropEvent = Dropped;
            if (dropEvent != null)
            {
                dropEvent(this, e);
            }
        }

        #endregion

        private bool _isDragDropTarget = true;
        /// <summary>
        /// Indicates whether the user can drop items from another control.
        /// </summary>
        [Category("Behavior (drag-and-drop)"), DefaultValue(true), Description("Indicates whether the user can drop items from another control.")]
        public bool IsDragDropTarget
        {
            get { return _isDragDropTarget; }
            set
            {
                _isDragDropTarget = value;
                base.AllowDrop = _isDragDropTarget;
            }
        }

        /// <summary>
        /// Occurs when a extended DragDropListBox drag-and-drop operation is completed.
        /// </summary>
        /// <remarks>This event is raised for the target as well as for the source.
        /// The role a control plays (source or target) can be determined from the Operation property of the DroppedEventArgs.</remarks>
        [Category("Drag Drop"), Description("Occurs when a extended DragDropListBox drag-and-drop operation is completed.")]
        public event EventHandler<DroppedEventArgs> Dropped;


        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            IDragDropSource src = drgevent.Data.GetData("IDragDropSource") as IDragDropSource;
            if (src != null)
            {
                object[] srcItems = src.GetSelectedItems();
                if (srcItems.Length > 0)
                    this.SelectedItem.DisplayText = srcItems[0] as string;

                DropOperation operation = DropOperation.CopyToHere; // Remembers the operation for the event we'll raise.

                // Notify the target (this control).
                DroppedEventArgs de = new DroppedEventArgs()
                {
                    Operation = operation,
                    Source = src,
                    Target = this,
                    DroppedItems = srcItems
                };

                OnDropped(de);

                // Notify the source (the other control).
                if (operation != DropOperation.Reorder)
                {
                    de = new DroppedEventArgs()
                    {
                        Operation = operation == DropOperation.MoveToHere ? DropOperation.MoveFromHere : DropOperation.CopyFromHere,
                        Source = src,
                        Target = this,
                        DroppedItems = srcItems
                    };
                    src.OnDropped(de);
                }
            }
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            drgevent.Effect = GetDragDropEffect(drgevent);
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            base.OnDragOver(drgevent);
            drgevent.Effect = GetDragDropEffect(drgevent);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (SelectedItem != null)
            {
                Size dragSize = SystemInformation.DragSize;
                _dragOriginBox = new Rectangle(new Point(e.X -
                  (dragSize.Width / 2), e.Y - (dragSize.Height / 2)), dragSize);

            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_dragOriginBox != Rectangle.Empty &&
             !_dragOriginBox.Contains(e.X, e.Y))
            {
                DoDragDrop(
                    new DataObject("IDragDropSource", this),
                    DragDropEffects.All);
                _dragOriginBox = Rectangle.Empty;
            }

        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragOriginBox = Rectangle.Empty;
        }

        /// <summary>
        /// Determines the drag-and-drop operation which is beeing performed, which can be either None, Move or Copy. 
        /// </summary>
        /// <param name="drgevent">DragEventArgs.</param>
        /// <returns>The current drag-and-drop operation.</returns>
        private DragDropEffects GetDragDropEffect(DragEventArgs drgevent)
        {
            const int CtrlKeyPlusLeftMouseButton = 9; // KeyState.

            DragDropEffects effect = DragDropEffects.None;

            // Retrieve the source control of the drag-and-drop operation.
            IDragDropSource src = drgevent.Data.GetData("IDragDropSource") as IDragDropSource;

            if (src != null && _dragDropGroup == src.DragDropGroup)
            { // The stuff being draged is compatible.
                if (src == this)
                {
                    // Drag-and-drop happens within this control.

                    //if (_allowReorder && !this.Sorted)
                    //{
                    //    effect = DragDropEffects.Move;
                    //}
                }
                else if (_isDragDropTarget)
                {
                    // If only Copy is allowed then copy. If Copy and Move are allowed, then Move, unless the Ctrl-key is pressed.
                    if (src.IsDragDropCopySource && (!src.IsDragDropMoveSource || drgevent.KeyState == CtrlKeyPlusLeftMouseButton))
                    {
                        effect = DragDropEffects.Copy;
                    }
                    else if (src.IsDragDropMoveSource)
                    {
                        effect = DragDropEffects.Move;
                    }
                }
            }
            return effect;
        }

        #region UrlCompletionExtender

        /// <summary>
        /// Used for Ctrl-Enter completion, similar to IE url combobox
        /// </summary>
        class UrlCompletionExtender
        {

            private readonly string[] urlTemplates = new[] {
												"http://www.{0}.com/",
												"http://www.{0}.net/",
												"http://www.{0}.org/",
												"http://www.{0}.info/",
		        };
            
            private int lastExpIndex = -1;
            private string toExpand;

            public UrlCompletionExtender(Control monitorControl)
            {
                if (monitorControl != null)
                {
                    monitorControl.KeyDown += OnMonitorControlKeyDown;
                    
                }
            }

            private void ResetExpansion()
            {
                lastExpIndex = -1;
                toExpand = null;
            }

            private void RaiseExpansionIndex()
            {
                lastExpIndex = (++lastExpIndex % urlTemplates.Length);
            }

            private void OnMonitorControlKeyDown(object sender, KeyEventArgs e)
            {
                Control ctrl = sender as Control;
                if (ctrl == null) return;

                UltraComboEditor ce = sender as UltraComboEditor;

                bool ctrlKeyPressed = (ModifierKeys & Keys.Control) == Keys.Control;
                if (e.KeyCode == Keys.Return && ctrlKeyPressed)
                {
                    if (lastExpIndex < 0 || toExpand == null)
                    {
                        string txt = ctrl.Text;
                        if (txt.Length > 0 && txt.IndexOfAny(new char[] { ':', '.', '/' }) < 0)
                        {
                            toExpand = txt;
                            RaiseExpansionIndex();
                        }
                    }
                    if (lastExpIndex >= 0 && toExpand != null)
                    {
                        ctrl.Text = String.Format(urlTemplates[lastExpIndex], toExpand);
                        
                        if (ce != null && ce.DropDownStyle != DropDownStyle.DropDownList)
                            ce.SelectionStart = ctrl.Text.Length;
                        RaiseExpansionIndex();
                    }
                }
                else
                {
                    ResetExpansion();
                }
            }

            
        }
        #endregion

    }

    public class BeforeNavigateCancelEventArgs: CancelEventArgs
    {
        public readonly Uri Uri;
        
        internal BeforeNavigateCancelEventArgs(Uri uri):base(false)
        {
            Uri = uri;
        }
    }

    public class NavigatedEventArgs: EventArgs
    {
        public readonly Uri Uri;
        
        internal NavigatedEventArgs(Uri uri)
        {
            Uri = uri;
        }
    }
}
