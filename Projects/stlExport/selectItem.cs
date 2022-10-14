﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stlExport
{
    public partial class selectItem : Form
    {
        public bool confirm = false;
        public selectItem()
        {
            InitializeComponent();
        }

        private void confirm_Click(object sender, System.EventArgs e)
        {
            confirm = true;
            this.Close();
        }

        private void cancel_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
}
