using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace AmphetaRatePlugin
{
	/// <summary>
	/// Summary description for RatingForm.
	/// </summary>
	public class RatingForm : System.Windows.Forms.Form
	{
		int _rating = -1;
		private System.Windows.Forms.Label lblRating;
		private System.Windows.Forms.ComboBox cmbRatings;
		private System.Windows.Forms.Button btnRate;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public RatingForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lblRating = new System.Windows.Forms.Label();
			this.cmbRatings = new System.Windows.Forms.ComboBox();
			this.btnRate = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lblRating
			// 
			this.lblRating.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblRating.Location = new System.Drawing.Point(8, 8);
			this.lblRating.Name = "lblRating";
			this.lblRating.Size = new System.Drawing.Size(40, 23);
			this.lblRating.TabIndex = 1;
			this.lblRating.Text = "Rating:";
			// 
			// cmbRatings
			// 
			this.cmbRatings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbRatings.Items.AddRange(new object[] {
															"1",
															"2",
															"3",
															"4",
															"5",
															"6",
															"7",
															"8",
															"9",
															"10"});
			this.cmbRatings.Location = new System.Drawing.Point(64, 8);
			this.cmbRatings.MaxDropDownItems = 10;
			this.cmbRatings.Name = "cmbRatings";
			this.cmbRatings.Size = new System.Drawing.Size(48, 21);
			this.cmbRatings.TabIndex = 2;
			this.cmbRatings.SelectedIndexChanged += new System.EventHandler(this.cmbRatings_SelectedIndexChanged);
			// 
			// btnRate
			// 
			this.btnRate.Cursor = System.Windows.Forms.Cursors.Default;
			this.btnRate.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnRate.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnRate.Location = new System.Drawing.Point(120, 8);
			this.btnRate.Name = "btnRate";
			this.btnRate.Size = new System.Drawing.Size(48, 23);
			this.btnRate.TabIndex = 3;
			this.btnRate.Text = "Rate It!";
			this.btnRate.Click += new System.EventHandler(this.btnRate_Click);
			// 
			// RatingForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(176, 38);
			this.Controls.Add(this.btnRate);
			this.Controls.Add(this.cmbRatings);
			this.Controls.Add(this.lblRating);
			this.MaximizeBox = false;
			this.Name = "RatingForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.ResumeLayout(false);

		}
		#endregion

		private void btnRate_Click(object sender, System.EventArgs e)
		{
			try
			{
				_rating = int.Parse(cmbRatings.SelectedItem.ToString());
				this.DialogResult = DialogResult.OK;
			}
			catch(System.FormatException)
			{
				MessageBox.Show(this, "Please select a rating, or hit X to cancel.");
			}
		}

		private void cmbRatings_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(cmbRatings.SelectedItem.ToString().Length > 0)
				btnRate.Focus();
		}

		public int SelectedRating
		{
			get
			{
				return _rating;
			}
		}
	}
}
