using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleApp1
{
    public partial class MainForm : Form
    {
        public DataReception data;
        public MainForm()
        {
            InitializeComponent();

            AddItemToCb();

            data = new DataReception();
            Task.Run(() => {
                data.Start();
            });
        }
        private void AddItemToCb()
        {
            foreach (var item in Enum.GetValues(typeof(ClassCod)))
            {
                cbClass.Items.Add(item);
            }
            foreach (var item in Enum.GetValues(typeof(TimeFrame)))
            {
                cbTimeFrame.Items.Add(item);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            TimeFrame frame = Enum.GetValues(typeof(TimeFrame)).Cast<TimeFrame>().First(x => x.ToString() == cbTimeFrame.Text);
            string classCod = cbClass.Text;
            string security = cbSecurity.Text;
            Task.Run(() =>
            {
                data.SetQUIKCommandDataObject(DataReception.SW_Command, DataReception.SR_FlagCommand, DataReception.SW_FlagCommand, DataReception.GetCommandStringCb(classCod, security, frame));
             });
            listSecurity.Text += cbSecurity + "\n";
            cbClass.Text = String.Empty;
            cbSecurity.Text = String.Empty;
            cbTimeFrame.Text = String.Empty;
        }
        private void cbClass_Leave(object sender, EventArgs e)
        {
            if (cbClass.Text != String.Empty)
            {
                if (cbClass.Text == "SPBFUT")
                {
                    cbSecurity.Items.Clear();
                    foreach (var item in Enum.GetValues(typeof(Futures)))
                    {
                        cbSecurity.Items.Add(item);
                    }
                }
                if (cbClass.Text == "TQBR")
                {
                    cbSecurity.Items.Clear();
                    foreach (var item in Enum.GetValues(typeof(Security)))
                    {
                        cbSecurity.Items.Add(item);
                    }
                }
             //   cbSecurity.Enabled = true;
            }
            else
            {
                cbSecurity.Items.Clear();
            }
        }
        public enum ClassCod
        {
            SPBFUT = 1,
            TQBR = 2
        }
        public enum Futures
        {
            GZZ7,
            SRZ7,
            EuZ7,
            GDZ7,
            RIZ7,
            SiZ7,
            BRZ7
        }
        public enum Security
        {
            SBER,
            SBERP,
            GAZP,
            LKOH,
            MTSS,
            MGNT,
            MOEX,
            NVTK,
            NLMK,
            RASP,
            VTBR,
            RTKM,
            ROSN,
            AFLT,
            AKRN,
            AFKS,
            PHOR,
            GMKN,
            CHMF,
            SNGS,
            URKA,
            FEES,
            ALRS,
            APTK,
            YNDX,
            MTLRP,
            MAGN,
            BSPB,
            MTLR
        }
    }
}

