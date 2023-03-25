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

namespace Delay
{
    /// <summary>
    /// Interaction logic for ConfigSettings.xaml
    /// </summary>
    public partial class ConfigSettings : Window
    {
        double m_selectedValue = 0.0;

        public ConfigSettings()
        {
            InitializeComponent();

            DelayTimeValue.DataContext = DelaySlider;
        }

        private void DelayTimeValue_LostFocus(object sender, RoutedEventArgs e)
        {
            // See if they changed the text
            try
            {
                double delayTime = Convert.ToDouble(DelayTimeValue.Text);

                if (delayTime > DelaySlider.Maximum)
                {
                    DelaySlider.Maximum = delayTime + 1.0;
                }

                if (delayTime > 0.0)
                {
                    DelaySlider.Value = delayTime;
                }
                else
                {
                    DelayTimeValue.Text = DelaySlider.Value.ToString();
                }
            }

            catch
            {
                DelayTimeValue.Text = DelaySlider.Value.ToString();
            }
        }

        public double SelectedValue
        {
            get
            {
                return m_selectedValue;
            }
            set
            {
                m_selectedValue = value;
                DelaySlider.Value = value;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            SelectedValue = DelaySlider.Value;
            DialogResult = true;
            e.Handled = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            e.Handled = true;
        }
    }
}
