using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;

namespace LightBringer
{
    public static class CommandSet
    {
        private static readonly RoutedUICommand m_modifySwatchCmd = new RoutedUICommand("Modify Swatch", "ModifySwatchCommand", typeof(CommandSet));

        public static RoutedUICommand ModifySwatchCommand
        {
            get
            {
                return m_modifySwatchCmd;
            }
        }
    }
}
