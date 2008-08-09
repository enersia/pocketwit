using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class SearchForm : Form
    {

		#region Constructors (1) 

        public SearchForm()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.Cancel;
        }

		#endregion Constructors 

		#region Properties (1) 

        public string SearchText{get;set;}

		#endregion Properties 

		#region Methods (2) 


		// Private Methods (2) 

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void menuSearch_Click(object sender, EventArgs e)
        {
            this.SearchText = txtSearch.Text;
            this.DialogResult = DialogResult.OK;
        }


		#endregion Methods 

    }
}