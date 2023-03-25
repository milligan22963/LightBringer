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

namespace Fade
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class ConfigSettings : Window
    {
        public ConfigSettings()
        {
            InitializeComponent();

            Direction = FadeDirection.DarkToLight;
            Percentage = 100.0;
            PercentageValue.DataContext = FadePercentage;
        }

        public FadeDirection Direction
        {
            get
            {
                FadeDirection direction = FadeDirection.DarkToLight;

                if (FadeToDark.IsChecked == true)
                {
                    direction = FadeDirection.LightToDark;
                }

                return direction;
            }
            set
            {
                if (value == FadeDirection.DarkToLight)
                {
                    FadeToDark.IsChecked = false;
                    FadeToLight.IsChecked = true;
                }
                else
                {
                    FadeToDark.IsChecked = true;
                    FadeToLight.IsChecked = false;
                }
            }
        }

        public double Percentage
        {
            get
            {
                return FadePercentage.Value;
            }
            set
            {
                FadePercentage.Value = value;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
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
