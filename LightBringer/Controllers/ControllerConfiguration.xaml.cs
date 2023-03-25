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

namespace LightBringer.Controllers
{
    /// <summary>
    /// Interaction logic for ControllerConfiguration.xaml
    /// </summary>
    public partial class ControllerConfiguration : Window
    {
        public ControllerConfiguration()
        {
            InitializeComponent();

            ControllerTypeSelection.Items.Add(new Arduino());

            ControllerTypeSelection.SelectedIndex = 0; // select the first one
        }

        public ControllerType SelectedType
        {
            get
            {
                return ControllerTypeSelection.SelectedItem as ControllerType;
            }
            set
            {
                ControllerTypeSelection.Items.Add(value);
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ControllerTypeSelection_Selected(object sender, RoutedEventArgs e)
        {
            // See which items was selected in the combo box
            ControllerType controller = ControllerTypeSelection.SelectedItem as ControllerType;

            StripCount.Text = controller.Strips.Count.ToString();

            StripGrid.DataContext = controller.Strips;

            e.Handled = true;
        }

        private void StripCount_TextInput(object sender, TextCompositionEventArgs e)
        {
            // Number of strips have changed
            ControllerType controller = ControllerTypeSelection.SelectedItem as ControllerType;
            int stripCount = Convert.ToInt32(StripCount.Text);

            if (controller != null)
            {
                if ((stripCount != 0) && (stripCount != controller.NumberOfStrips))
                {
                    // Update to the right amount of strips
                    controller.NumberOfStrips = stripCount;
                }
            }

            e.Handled = true;
        }

        private void AddStrip_Click(object sender, RoutedEventArgs e)
        {
            ControllerType controller = ControllerTypeSelection.SelectedItem as ControllerType;

            if (controller != null)
            {
                int stripCount = controller.NumberOfStrips + 1;
                if (stripCount != 0)
                {
                    // Update to the right amount of strips
                    controller.NumberOfStrips = stripCount;
                }
                StripCount.Text = controller.Strips.Count.ToString();

                StripGrid.DataContext = controller.Strips;

                e.Handled = true;
            }
        }

        private void DeleteStrip_Click(object sender, RoutedEventArgs e)
        {
            ControllerType controller = ControllerTypeSelection.SelectedItem as ControllerType;

            if (controller != null)
            {
                int stripCount = controller.NumberOfStrips - 1;
                if (stripCount >= 0)
                {
                    // Update to the right amount of strips
                    controller.NumberOfStrips = stripCount;
                }
                StripCount.Text = controller.Strips.Count.ToString();

                StripGrid.DataContext = controller.Strips;

                e.Handled = true;
            }
        }
    }
}
