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

namespace Rotate
{
    /// <summary>
    /// Interaction logic for ConfigSettings.xaml
    /// </summary>
    public partial class ConfigSettings : Window
    {
        public ConfigSettings()
        {
            InitializeComponent();

            Direction = RotateDirection.Clockwise;
            Count = 1.0;
            RotationValue.DataContext = RotationCount;
        }

        public RotateDirection Direction
        {
            get
            {
                RotateDirection direction = RotateDirection.Clockwise;

                if (CounterClockWise.IsChecked == true)
                {
                    direction = RotateDirection.CounterClockwise;
                }

                return direction;
            }
            set
            {
                if (value == RotateDirection.Clockwise)
                {
                    Clockwise.IsChecked = true;
                    CounterClockWise.IsChecked = false;
                }
                else
                {
                    Clockwise.IsChecked = false;
                    CounterClockWise.IsChecked = true;
                }
            }
        }

        public double Count
        {
            get
            {
                return RotationCount.Value;
            }
            set
            {
                RotationCount.Value = value;
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
