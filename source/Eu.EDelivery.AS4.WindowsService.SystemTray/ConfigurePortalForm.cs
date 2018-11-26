using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eu.EDelivery.AS4.WindowsService.SystemTray
{
    public partial class ConfigurePortalForm : Form
    {
        public ConfigurePortalForm()
        {
            InitializeComponent();
        }

        public string PortalUrl => txtPortal.Text;
    }
}
