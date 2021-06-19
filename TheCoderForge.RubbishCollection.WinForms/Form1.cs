using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TheCoderForge.RubbishCollection;

namespace TheCoderForge.RubbishCollection.WinForms
{
    public partial class Form1 : Form
    {
        private IRubbishCollectionApp app;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            // initialise the app
            if (app == null) app = new RubbishCollectionApp();

           string result= app.DescribeNextPickup();
           MessageBox.Show(result);
        }
    }
}
