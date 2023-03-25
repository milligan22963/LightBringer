using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LightBringer.Visuals
{
    /// <summary>
    /// Interaction logic for StripSettings.xaml
    /// </summary>
    public partial class StripSettings : Window
    {
        public StripSettings()
        {
            InitializeComponent();
        }

        public int PixelCount
        {
            get;
            set;
        }

        public int StartingCount
        {
            set
            {
                PixelCount = value;
                NumberOfPixels.Text = value.ToString();
            }
        }
        public int StartStripId
        {
            set
            {
                StripId = value;
                StripNumber.Text = value.ToString();
            }
        }

        public int StripId
        {
            get;
            set;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            PixelCount = Convert.ToInt32(NumberOfPixels.Text);
            StripId = Convert.ToInt32(StripNumber.Text);

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
