#region CVS Version Header
/*
 * $Id$
 * Last modified by $Author$
 * Last modified at $Date$
 * $Revision$
 */
#endregion

using System;
using System.ComponentModel;
using IDesignerHost = System.ComponentModel.Design.IDesignerHost;
using System.Windows.Forms;
using System.Drawing;

using Genghis;

namespace RssBandit.WinGui.Controls
{
    /// <summary>
    /// A class to automatically serialize/deserialize the state of a form.
    /// This is a MODIFIED class from the original sources at Genghis!
    /// We added a small fix for multiple srceen support and some convinient
    /// public properties to control what properties of a form should get serialized.
    /// </summary>
    /// <example>
    /// The sample below will persist the state of the MainWindow form.
    /// <code>
    /// class MainWindow : Form {
    ///     WindowSerializer windowSerializer;
    ///     public MainWindow() {
    ///         windowSerializer = new WindowSerializer(this);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <remarks>
    /// We use a specific app.config setting to use our own preference (file) store:
	/// 	&lt;add key="CustomPreferencesStore" 
	/// 	           value="RssBandit.WinGui.Utility.Settings,RssBandit" /&gt;
    /// </remarks>
    [
        DesignTimeVisible(true),
        ToolboxItem(true),
        DefaultProperty("Form")
    ]
    public class WindowSerializer : Component {
        private Form form;
        private string formName;
		private bool saveOnlyLocation = false;
		private bool saveNoWindowState = false;

        private Rectangle dimensions;
        private FormWindowState windowState;

        // Event info that allows form to persist extra window state data.
        public delegate void WindowSerializerDelegate(object sender, Preferences preferences);
        public event WindowSerializerDelegate LoadStateEvent;
        public event WindowSerializerDelegate SaveStateEvent;

        /// <summary>
        /// Constructs an empty WindowSerializer.  If the Form property is not
        /// set, then this class will do nothing.
        /// </summary>
        public WindowSerializer() {
            form = null;
            formName = null;
            dimensions = Rectangle.Empty;
        }

        /// <summary>
        /// Constructs WindowSerializer with a form.
        /// </summary>
        /// <param name="form">
        /// The form whose state is to be persisted.
        /// </param>
        public WindowSerializer(Form form) : this() {
            Form = form;
        }

        /// <summary>
        /// Gets or sets the form which is to have it's state persisted.
        /// </summary>
        /// <value>
        /// The form which is to have it's state persisted.
        /// </value>
        /// <remarks>
        /// If the WindowSerializer is used inside Visual Studio in a Windows
        /// Forms designer, Form is automatically set to the control that
        /// contains the WindowSerializer. For example, if you place a
        /// WindowSerializer on a designer for Form1 (which inherits from
        /// Form), the Form property of WindowSerializer is set to the
        /// instance of Form1.
        /// </remarks>
        [
            Category("Misc"),
            Localizable(false),
            Description("The form which is to have it's state persisted.")
        ]
        public Form Form {
            get {
                // The purpose of the following rather magical code is to get
                // the designer to automatically initialize this property
                // to the form that is being designed (ie. the form that this
                // component is dropped into).
                if (form == null && this.DesignMode) {
                    IDesignerHost designerHost =
                        (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (designerHost != null) {
                        IComponent rootComponent = designerHost.RootComponent;
                        if (rootComponent != null && rootComponent as Form != null)
                                this.form = (Form) rootComponent;
                    }
                }

                return form;
            }

            set {
                if (form != null && this.DesignMode == false) {
                    // Unsubscribe from previous form's events.
                    form.Closing -= new CancelEventHandler(OnClosing);
                    form.Resize -= new EventHandler(OnResize);
                    form.Move -= new EventHandler(OnMove);
                    form.Load -= new EventHandler(OnLoad);
                }

                form = value;

                if (form != null && this.DesignMode == false) {
                    // Subscribe to the new form's events.
                    form.Closing += new CancelEventHandler(OnClosing);
                    form.Resize += new EventHandler(OnResize);
                    form.Move += new EventHandler(OnMove);
                    form.Load += new EventHandler(OnLoad);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the form.
        /// </summary>
        /// <value>
        /// The name of the form.
        /// </value>
        /// <remarks>
        /// <para>The name of the form should be unique and unchanging.</para>
        /// <para>The default name is the full class name of the Form.</para>
        /// </remarks>
        [
        Category("Misc"),
        Localizable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        Description("The name of the form (automatically set and used).")
        ]
        public string FormName {
            get {
                // Default is null if Form is also null.
                if (Form == null)
                    return null;
                // Otherwise, default is full class name of Form.
                if (formName == null)
                    return form.GetType().FullName;
                return formName;
            }

        }


		/// <summary>
		/// Get or set the boolean to save only the location of a form, but not the size.
		/// Default is False.
		/// </summary>
		[
		Category("Behavior"),
		DefaultValue(false),
		Description("Get or set the boolean to save only the location of a form, but not the size.")
		]
		public bool SaveOnlyLocation { 
			get { return saveOnlyLocation; } 
			set { saveOnlyLocation = value; }
		}

		/// <summary>
		/// Get or set the boolean to prevent saving of the WindowState.
		/// Default is False.
		/// </summary>
		[
		Category("Behavior"),
		DefaultValue(false),
		Description("Get or set the boolean to prevent saving of the WindowState.")
		]
		public bool SaveNoWindowState { 
			get { return saveNoWindowState; } 
			set { saveNoWindowState = value; }
		}
		
		/// <summary>
        /// Handle Load event.  Loads position settings and moves/sizes the
        /// form.
        /// </summary>
        private void OnLoad(object sender, System.EventArgs e) {
			dimensions.X = form.Left;
			dimensions.Y = form.Top;
			dimensions.Width = form.Width;
			dimensions.Height = form.Height;
			FormWindowState windowState = form.WindowState;
 
			// Attempt to read state from preferences.
            Preferences prefReader = Preferences.GetUserNode(GetType());
            prefReader = prefReader.GetSubnode(FormName);
            dimensions.X = prefReader.GetInt32("Left", form.Left);
            dimensions.Y = prefReader.GetInt32("Top", form.Top);
			
			if (Screen.AllScreens.Length < 2) {	
				// if only one sreen, correct initial location to fit the screen
				if (dimensions.X < 0) dimensions.X = 0;
				if (dimensions.Y < 0) dimensions.Y = 0;
				if (dimensions.X >= Screen.PrimaryScreen.WorkingArea.Width) dimensions.X -= Screen.PrimaryScreen.WorkingArea.Width;
				if (dimensions.Y >= Screen.PrimaryScreen.WorkingArea.Height) dimensions.Y -= Screen.PrimaryScreen.WorkingArea.Height;
			}

			if (!this.saveOnlyLocation) {
				dimensions.Width = prefReader.GetInt32("Width", form.Width);
				dimensions.Height = prefReader.GetInt32("Height", form.Height);
			}
			if (!this.saveNoWindowState) {
				windowState = (FormWindowState) prefReader.GetInt32("WindowState", (int) form.WindowState);
			}


            // Fire LoadState event.
            if (LoadStateEvent != null)
                LoadStateEvent(this, prefReader);

            prefReader.Close();

            // Alter window state.
            form.Bounds = dimensions;
            form.WindowState = windowState;
        }


        /// <summary>
        /// Handle Move event.  Records form position.
        /// </summary>
        private void OnMove(object sender, System.EventArgs e) {
            // Save position.
            if (form.WindowState == FormWindowState.Normal)
                dimensions.Location = form.Location;

            // Save window state.
            windowState = form.WindowState;
        }

        /// <summary>
        /// Handle Resize event.  Records form size.
        /// </summary>
        private void OnResize(object sender, System.EventArgs e) {
            // Save width and height.
            if (form.WindowState == FormWindowState.Normal)
                dimensions.Size = form.Size;
        }

        /// <summary>
        /// Handle Close event.  Saves window state.
        /// </summary>
        private void OnClosing(object sender, CancelEventArgs e) {
            // If the window state is minimized, save as normal.
            if (windowState == FormWindowState.Minimized)
                windowState = FormWindowState.Normal;

            // Save position, size and state to preferences.
            Preferences prefWriter = Preferences.GetUserNode(GetType());
            prefWriter = prefWriter.GetSubnode(FormName);
            prefWriter.SetProperty("Left", dimensions.Left);
            prefWriter.SetProperty("Top", dimensions.Top);
			if (this.saveOnlyLocation) {
				prefWriter.SetProperty("Width", null);	// removes the entry
				prefWriter.SetProperty("Height", null);
			} else {
				prefWriter.SetProperty("Width", dimensions.Width);
				prefWriter.SetProperty("Height", dimensions.Height);
			}
			if (this.saveNoWindowState) {
				prefWriter.SetProperty("WindowState", null);
			} else {
				prefWriter.SetProperty("WindowState", (int) windowState);
			}

            // Fire SaveState event.
            if (SaveStateEvent != null)
                SaveStateEvent(this, prefWriter);

            prefWriter.Close();
        }
    }
}