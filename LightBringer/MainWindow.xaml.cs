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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using MostRecentFiles;
using System.Windows.Media.Media3D;

namespace LightBringer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // We need to store application data such as swatches etc.
        // For now these are constant as I don't see a need to allow changes to them
        const string m_swatchFile = "colorswatches.xml";
        const string m_appDirectory = "Light Bringer";
        const string m_appTitle = "Light Bringer";
        const string m_untitledDocument = "Untitled";

        // Clipboard management
        const string m_operationName = "LBOperation";
        const string m_cutOperation = "LBCut";
        const string m_copyOperation = "LBCopy";

        // Path of local hard drive space for user data such as swatches
        string m_localStorage;

        // Current operation being processed if any
        string m_currentOperation;

        //Swatch related data items
        Swatches.SwatchManager m_swatchManager = new Swatches.SwatchManager();
        Swatches.SwatchSet m_currentSwatchSet = null;

        // Toolbox related data items
        List<Toolbox.ToolButton> m_toolBox = new List<Toolbox.ToolButton>();
        Toolbox.ToolButton m_currentTool = null;

        // Treecontrol view data items
        DataModel.Controller m_controller = new DataModel.Controller();
        ViewModel.ControllerViewModel m_controllerView;

        ViewModel.MovieViewModel m_currentMovieViewModel = null;
        ViewModel.FrameViewModel m_currentFrame;
        Visuals.StripView m_currentStrip;

        // Current filename for the document
        string m_fileName = null;

        MRUFileHandler m_fileHandler;
        const string m_mruFileName = "mrufiles.xml";
        const string m_transformFolder = "Transforms";

        // Multiselection
        private Point m_startSelectionPoint = new Point();
        private bool m_selectionStarted = false;
        private bool m_selectionSelected = false;

        public MainWindow()
        {
            InitializeComponent();

            m_controllerView = new ViewModel.ControllerViewModel(m_controller);

            ControllerData.DataContext = m_controllerView;

            m_localStorage = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + m_appDirectory;

            // See if our directory exists and if not, make it so
            if (Directory.Exists(m_localStorage) == false)
            {
                Directory.CreateDirectory(m_localStorage);
            }

            TabBorder.Background = ColorSelection.Background;
            TabBorder.BorderBrush = ColorSelection.BorderBrush;
            TabBorder.BorderThickness = ColorSelection.BorderThickness;
            SwatchTab.Background = ColorSelection.Background;

            // Load swatches from disk
            LoadSwatches(m_localStorage + "\\" + m_swatchFile);

            // Load before handling files so our transforms are available
            LoadTransforms();

            m_fileHandler = new MRUFileHandler();
            m_fileHandler.MRUFileName = m_localStorage + "\\" + m_mruFileName;
            m_fileHandler.RestoreMRU();

            RecentFileList.DataContext = m_fileHandler;

            m_currentFrame = null;
            m_currentStrip = null;

            // Load up tool box with known tool objects
            // If we have plugin tools we need to be able to
            // add in the buttons - a toggle button derives from control
            // We would want to allow these to be loaded on teh fly
            m_toolBox.Add(ArrowSelection);
            m_toolBox.Add(AreaSelection);
            m_toolBox.Add(BrushSelection);
            m_toolBox.Add(BucketSelection);
            m_toolBox.Add(EyeDropperSelection);
            m_toolBox.Add(ScissorSelection);

            m_currentTool = ArrowSelection;
            ArrowSelection.IsChecked = true;
            if (m_fileHandler.Children.Count > 0)
            {
                OpenExistingDocument(m_fileHandler[0].FileName);
            }
            else
            {
                SetWindowName(m_untitledDocument);
            }

            // Setup animation stuff
            FrameStrip.AnimationStoppedHandler += OnAnimationStopped;
            FrameStrip.AnimationPausedHandler += OnAnimationPaused;
            FrameStrip.AnimationStartedHandler += OnAnimationStarted;
            FrameStrip.AnimationProgressHandler += OnAnimationProgress;
            FrameStrip.AnimationResumedHandler += OnAnimationResumed;
            FrameStrip.FrameSelected += OnFrameSelected;

            // Reset our 3D view so it starts off right
            ResetThreeDView();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (m_controllerView.IsDirty == true)
            {
                System.Windows.MessageBoxResult result = MessageBox.Show("Changes have been made.  Are you sure that you want to exit without saving?", "Application Exit", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true; // they decided not to do that
                }
            }

            FrameStrip.Shutdown();

            m_fileHandler.SaveMRU();
        }

        #region SWATCH_MANAGEMENT
        // Update current selected color based on swatch selection
        void SwatchSelected(object sender, Swatches.SwatchEvent e)
        {
            ColorSelection.SelectedColor = e.Color;
        }

        void SwatchModified(object sender, Swatches.SwatchEvent e)
        {
            SaveSwatches(m_localStorage + "\\" + m_swatchFile);
        }

        void SwatchDeleted(object sender, Swatches.SwatchEvent e)
        {
            if (m_currentSwatchSet != null)
            {
                Swatches.Swatch childSwatch = sender as Swatches.Swatch;

                m_currentSwatchSet.Remove(childSwatch);
                m_currentSwatchSet.SwatchPanel.Children.Remove(childSwatch);
                SaveSwatches(m_localStorage + "\\" + m_swatchFile);
            }
        }

        void Swatch_SetupSet(Swatches.SwatchSet set)
        {
            TabItem swatchTab = new TabItem();
            TextBox headerText = new TextBox();

            set.Modified += set_Modified;
            headerText.Text = set.Name;
            headerText.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            headerText.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            headerText.MinWidth = 25;
            headerText.TextChanged += set.headerText_TextChanged;
            swatchTab.Header = headerText;

            SwatchTab.Items.Add(swatchTab);

            swatchTab.Content = set.SwatchPanel;

            for (int swatchIdx = 0; swatchIdx < set.Count; swatchIdx++)
            {
                Swatches.Swatch swatch = set[swatchIdx];

                swatch.Selected += SwatchSelected;
                swatch.Modified += SwatchModified;
                swatch.Deleted += SwatchDeleted;
                set.SwatchPanel.Children.Add(swatch);
            }
        }

        void set_Modified(object sender, EventArgs e)
        {
            SaveSwatches(m_localStorage + "\\" + m_swatchFile);
        }

        // Load all of our swatches 
        void LoadSwatches(string fileName)
        {
            XmlReaderSettings settings = new XmlReaderSettings();

            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;

            // See if the file exists, if it doesn't we will create it
            if (File.Exists(fileName) == false)
            {
                FileStream fileStream = File.Create(fileName);

                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }

            XmlReader reader = XmlReader.Create(fileName, settings);

            m_swatchManager.Load(reader);

            SwatchTab.Items.Clear(); // remove the old

            // Add in each set and each swatch in the set
            for (int index = 0; index < m_swatchManager.Count; index++)
            {
                Swatches.SwatchSet set = m_swatchManager[index];
                Swatch_SetupSet(set);
            }

            if (m_swatchManager.Count > 0)
            {
                m_currentSwatchSet = m_swatchManager[0]; // default to the first one
            }

            reader.Close();
        }

        // if changes then update the local file
        void SaveSwatches(string fileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = false;
            settings.NewLineOnAttributes = true;

            XmlWriter writer = XmlWriter.Create(fileName, settings);

            m_swatchManager.Save(writer);

            writer.Close();
        }

        private void ColorSelection_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_currentSwatchSet != null)
            {
                // Right click on color selection indicating adding a swatch to current panel
                Swatches.Swatch colorSwatch = new Swatches.Swatch();

                colorSwatch.CurrentColor = ColorSelection.SelectedColor;

                m_currentSwatchSet.Add(colorSwatch);
                colorSwatch.Selected += SwatchSelected;
                colorSwatch.Modified += SwatchModified;
                colorSwatch.Deleted += SwatchDeleted;
                m_currentSwatchSet.SwatchPanel.Children.Add(colorSwatch);

                SaveSwatches(m_localStorage + "\\" + m_swatchFile);
            }
        }

        private void SwatchTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update to the correct current swatch set
            if (SwatchTab.SelectedIndex != -1)
            {
                m_currentSwatchSet = m_swatchManager[SwatchTab.SelectedIndex];
            }
        }

        #region SWATCHMENU
        private void MenuItem_NewSwatchSet(object sender, RoutedEventArgs e)
        {
            // Create a new swatch set
            Swatches.SwatchSet swatchSet = new Swatches.SwatchSet();

            swatchSet.Name = "Untitled";

            Swatch_SetupSet(swatchSet);

            m_swatchManager.Add(swatchSet);

            SwatchTab.SelectedIndex = SwatchTab.Items.Count - 1; // select the last one

            SaveSwatches(m_localStorage + "\\" + m_swatchFile);

            e.Handled = true;
        }

        private void MenuItem_DeleteSwatchSet(object sender, RoutedEventArgs e)
        {
            if (m_currentSwatchSet != null)
            {
                int selectedIndex = SwatchTab.SelectedIndex;

                SwatchTab.Items.Remove(SwatchTab.Items[selectedIndex]);
                if (selectedIndex != 0)
                {
                    selectedIndex--; // move back one
                }
                m_swatchManager.Remove(m_currentSwatchSet);
                SwatchTab.SelectedIndex = selectedIndex;

                SaveSwatches(m_localStorage + "\\" + m_swatchFile);
            }
            e.Handled = true;
        }

        private void MenuItem_ImportSwatchSet(object sender, RoutedEventArgs e)
        {
            // Popup dialog to select filename to import swatches from
            OpenFileDialog importDlg = new OpenFileDialog();

            importDlg.FileName = "Swatches";
            importDlg.DefaultExt = ".sxml"; // swatch xml
            importDlg.Filter = "Light Bringer Swatch documents (.sxml)|*.sxml"; // Filter files by extension 

            Nullable<bool> result = importDlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
                XmlReaderSettings settings = new XmlReaderSettings();

                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                XmlReader reader = XmlReader.Create(importDlg.FileName, settings);

                Swatches.SwatchManager tempManager = new Swatches.SwatchManager();

                tempManager.Load(reader);

                // Add in each set and each swatch in the set
                for (int index = 0; index < tempManager.Count; index++)
                {
                    Swatches.SwatchSet set = tempManager[index];
                    Swatch_SetupSet(set);
                    m_swatchManager.Add(set); // add to the persistent one
                }

                reader.Close();

                SaveSwatches(m_localStorage + "\\" + m_swatchFile);
            } 
            e.Handled = true;
        }

        private void MenuItem_ExportSwatchSet(object sender, RoutedEventArgs e)
        {
            if (m_currentSwatchSet != null)
            {
                // Popup dialog to select filename to store swatches into
                SaveFileDialog exportDlg = new SaveFileDialog();

                exportDlg.FileName = "Swatches";
                exportDlg.DefaultExt = ".sxml"; // swatch xml
                exportDlg.Filter = "Light Bringer Swatch documents (.sxml)|*.sxml"; // Filter files by extension 

                Nullable<bool> result = exportDlg.ShowDialog();

                // Process open file dialog box results 
                if (result == true)
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = false;
                    settings.NewLineOnAttributes = true;

                    XmlWriter writer = XmlWriter.Create(exportDlg.FileName, settings);

                    Swatches.SwatchManager tempManager = new Swatches.SwatchManager();

                    tempManager.Add(m_currentSwatchSet);
                    tempManager.Save(writer);

                    writer.Close();
                }
            }
            e.Handled = true;
        }
        #endregion // SWATCHMENU

        #endregion // SWATCH_MANAGEMENT

        #region APPLICATION_MENU_COMMANDS
        private void MenuItem_AboutClick(object sender, RoutedEventArgs e)
        {
            About aboutDlg = new About();

            aboutDlg.ShowDialog();

            e.Handled = true;
        }

        #region FILE_MENU
        private void MenuItem_NewDocument(object sender, RoutedEventArgs e)
        {
            bool skipNew = false;

            if (m_controller.IsDirty == true)
            {
                System.Windows.MessageBoxResult result = MessageBox.Show("Changes have been made.  Are you sure that you want to create a new document without saving?", "Application Exit", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    skipNew = true; // they changed their mind
                }
            }

            if (skipNew == false)
            {
                Controllers.ControllerConfiguration cfgDialog = new Controllers.ControllerConfiguration();

                cfgDialog.SelectedType = m_controller.AssociatedControllerType;

                bool? dialogResults = cfgDialog.ShowDialog();

                if (dialogResults == true)
                {
                    // Assign the selected controller
                    m_fileName = null; // new document, nothing saved as of yet
                    m_controller = new DataModel.Controller();
                    m_controllerView = new ViewModel.ControllerViewModel(m_controller);
                    m_controller.AssociatedControllerType = cfgDialog.SelectedType;
                    ControllerData.DataContext = m_controllerView;

                    SetWindowName(m_untitledDocument);
                }
            }
        }

        private void OpenExistingDocument(string fileName)
        {
            // Load document and all that good stuff
            if (File.Exists(fileName) == true)
            {
                m_fileName = fileName; // we have something

                XmlReaderSettings settings = new XmlReaderSettings();

                settings.IgnoreComments = true;
                settings.IgnoreWhitespace = true;

                XmlReader reader = XmlReader.Create(m_fileName, settings);

                m_controller = new DataModel.Controller();
                m_controller.Load(reader);

                m_controllerView = new ViewModel.ControllerViewModel(m_controller);
                ControllerData.DataContext = m_controllerView;

                m_controllerView.IsDirty = false;
                reader.Close();

                m_fileHandler.AddFile(m_fileName);
                SetWindowName(m_fileName);
            }
            else
            {
                MessageBox.Show("Unable to open the file: " + fileName);
            }
        }

        private void MenuItem_OpenDocument(object sender, RoutedEventArgs e)
        {
            bool skipOpen = false;

            if (m_controller.IsDirty == true)
            {
                System.Windows.MessageBoxResult result = MessageBox.Show("Changes have been made.  Are you sure that you want to open a different document without saving?", "Application Exit", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    skipOpen = true; // they changed their mind
                }
            }

            if (skipOpen == false)
            {
                OpenFileDialog ofDialog = new OpenFileDialog();

                ofDialog.FileName = "LBFDocument";
                ofDialog.DefaultExt = ".lbf";
                ofDialog.Filter = "Light Bringer documents (.lbf)|*.lbf"; // Filter files by extension

                bool? doOpen = ofDialog.ShowDialog();

                if (doOpen == true)
                {
                    OpenExistingDocument(ofDialog.FileName);
                }
            }
        }

        private void SaveDocument()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = false;
            settings.NewLineOnAttributes = true;

            XmlWriter writer = XmlWriter.Create(m_fileName, settings);

            m_controller.Save(writer);

            writer.Close();

            // Update/create
            m_fileHandler.AddFile(m_fileName);

            m_controllerView.IsDirty = false; // we are now nice and clean
        }

        private void MenuItem_SaveDocument(object sender, RoutedEventArgs e)
        {
            // See if it has been saved before, if not then do a save as
            if (m_fileName != null)
            {
                SaveDocument();
            }
            else
            {
                MenuItem_SaveAsDocument(sender, e); // no document name as of yet, prompt them
            }
        }

        private void MenuItem_SaveAsDocument(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfDialog = new SaveFileDialog();

            sfDialog.FileName = "LBFDocument"; // Default file name
            sfDialog.DefaultExt = ".lbf"; // Default file extension
            sfDialog.Filter = "Light Bringer documents (.lbf)|*.lbf"; // Filter files by extension

            bool? doSave = sfDialog.ShowDialog();

            if (doSave == true)
            {
                m_fileName = sfDialog.FileName;
                SaveDocument();
                SetWindowName(m_fileName);
            }
        }

        private void MenuItem_CloseDocument(object sender, RoutedEventArgs e)
        {
            bool skipClose = false;

            if (m_controller.IsDirty == true)
            {
                System.Windows.MessageBoxResult result = MessageBox.Show("Changes have been made.  Are you sure that you want to close this document without saving?", "Application Exit", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    skipClose = true; // they changed their mind
                }
            }

            if (skipClose == false)
            {
                // do something close like
                SetWindowName(m_untitledDocument);
            }
        }

        private void MenuItem_ExitProgram(object sender, RoutedEventArgs e)
        {
            Close();
            e.Handled = true;
        }

        #endregion // FILE_MENU
        #endregion // APPLICATION_MENU_COMMANDS

        #region PIXEL_STACK
        private void PixelStack_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double currentScale = PixelZoomBox.Scale;

            // Determine scale based on delta
            if (e.Delta > 0)
            {
                // zoom in
                if (currentScale < 100.0)
                {
                    currentScale += 1.0;
                    PixelZoomBox.Scale = currentScale;
                }
            }
            else
            {
                // zoom out
                if (currentScale > 1.0)
                {
                    currentScale -= 1.0;
                    PixelZoomBox.Scale = currentScale;
                }
            }
            e.Handled = true;
        }

/*        private void EditorZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (FrameContainer != null)
            {
                foreach (Visuals.StripView stripView in FrameContainer.Children)
                {
                    stripView.Zoom(EditorZoom.Value);
                }
            }
        }
        */
        private void PixelStack_Drop(object sender, DragEventArgs e)
        {
            if (m_currentTool != null)
            {
                // Any paint or multiselection tool allows the drop
                if (m_currentTool.ToolType == Toolbox.ToolType.MultiSelection)
                {
                    if (e.Data.GetDataPresent("SwatchColor"))
                    {
                        string colorData = e.Data.GetData("SwatchColor") as string;
                        Color colorValue = (Color)ColorConverter.ConvertFromString(colorData);

                        if (m_selectionSelected == true)
                        {
                            FrameContainer.ColorSelectedPixels(colorValue);
                        }
                        else
                        {
                            Visuals.StripView strip = sender as Visuals.StripView;

                            if (strip != null)
                            {
                                strip.SetColor(colorValue, false);
                            }
                        }
                    }
                }
            }
        }
        #endregion // PIXEL_STACK

        #region TOOLBOX
        private void ToolSelection_Click(object sender, RoutedEventArgs e)
        {
            foreach (Toolbox.ToolButton button in m_toolBox)
            {
                if (sender != button)
                {
                    button.IsChecked = false;
                }
                else
                {
                    m_currentTool = button;
                }
            }

            Cursor = m_currentTool.WindowCursor;
            e.Handled = true;
        }

        private void PixelZoomBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool clearSelection = true;

            // Clear out the last selection if any
            // However if this is a paint bucket, we will want to not deselect
            if (m_currentTool.ToolType == Toolbox.ToolType.MultiSelection)
            {
                if (m_currentTool.Action == Toolbox.ToolAction.Paint)
                {
                    clearSelection = false;
                }
            }

            if (clearSelection == true)
            {
                if ((m_selectionSelected == true) || (m_selectionStarted == true))
                {
                    m_selectionStarted = false;
                    m_selectionSelected = false; // currently nothing selected but we are starting
                    FrameContainer.DeSelectPixels();
                }
            }

            // Which tool?
            switch (m_currentTool.ToolType)
            {
                case Toolbox.ToolType.SingleSelection:
                    {
                    }
                    break;
                case Toolbox.ToolType.MultiSelection:
                    {
                        // Are we doing a multi selection?
                        if (m_currentTool.Action == Toolbox.ToolAction.Select)
                        {
                            m_startSelectionPoint = e.GetPosition(FrameContainer);
                            m_selectionStarted = true;
                        }
                    }
                    break;
            }
        }

        private void PixelZoomBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Right now if no selection started then ignore it
            if (m_selectionStarted == true)
            {
                Point mousePos = e.GetPosition(FrameContainer);
                //                Vector diff = m_startSelectionPoint - mousePos;

                Rect selectionArea = new Rect(m_startSelectionPoint, mousePos);
                Vector frameOffset = VisualTreeHelper.GetOffset(FrameContainer);

                FrameContainer.SelectPixels(selectionArea, frameOffset);

                m_selectionSelected = true;
            }

            m_selectionStarted = false;  // we are now down
        }

        private void PixelZoomBox_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void PixelZoomBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (m_selectionStarted == true)
            {
                // Get the current mouse position
                Point mousePos = e.GetPosition(FrameContainer);
                Rect selectionArea = new Rect(m_startSelectionPoint, mousePos);
                Vector frameOffset = VisualTreeHelper.GetOffset(FrameContainer);

                FrameContainer.SelectPixels(selectionArea, frameOffset);

//                Vector diff = m_startSelectionPoint - mousePos;

                // This is for drag/drop - keep it for now
                // for reference
                /*
                if (e.LeftButton == MouseButtonState.Pressed &&
                    Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                }*/
            }
        }

        void PixelSelected(object sender, Visuals.PixelEventArgs e)
        {
            switch (m_currentTool.ToolType)
            {
                case Toolbox.ToolType.SingleSelection:
                    {
                        if (m_currentTool.Action == Toolbox.ToolAction.Paint)
                        {
                            // This wil set the associated view color too
                            e.Pixel.Color = ColorSelection.SelectedColor;
                        }
                    }
                    break;
                case Toolbox.ToolType.MultiSelection:
                    {
                        if (m_currentTool.Action == Toolbox.ToolAction.Paint)
                        {
                            if (m_selectionSelected == true)
                            {
                                // Did they select a particular set of pixels?
                                FrameContainer.ColorSelectedPixels(ColorSelection.SelectedColor);

                                // Do not clear out selection they may have more work to do
                            }
                            else
                            {
                                FrameworkElement pixelStack = e.Pixel.Parent as FrameworkElement;

                                // This is kind of a hack but it works and is quick
                                // I could walk from the current frame and see what pixel belongs to what strip
                                // and go from there
                                if (pixelStack != null)
                                {
                                    FrameworkElement parentStack = pixelStack.Parent as FrameworkElement;

                                    if (parentStack != null)
                                    {
                                        // See who the parent of the pixel is
                                        // and set the color for all of the pixels in this strip
                                        Visuals.StripView strip = parentStack.Parent as Visuals.StripView;

                                        if (strip != null)
                                        {
                                            strip.SetColor(ColorSelection.SelectedColor, false); // color 'em all
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case Toolbox.ToolType.EyeDropper:
                    {
                        ColorSelection.SelectedColor = e.Pixel.Color;
                    }
                    break;
            }
        }
        #endregion // TOOLBOX

        #region TREE_MENU_ITEMS

        private object GetAssociatedMenuControl(object menuItemControl)
        {
            object returnObject = null;

            // Try to cast the sender to a MenuItem
            MenuItem menuItem = menuItemControl as MenuItem;
            if (menuItem != null)
            {
                ContextMenu parentContext = menuItem.Parent as ContextMenu;

                if (parentContext != null)
                {
                    StackPanel panel = parentContext.PlacementTarget as StackPanel;

                    if (panel != null)
                    {
                        returnObject = panel.DataContext;
                    }
                }
            }

            return returnObject;
        }

        #region CONTROLLER_TREE

        // This will configure the number of strips per controller and the length of each strip
        // if there are already strips and lengths then we will have to adjust accordingly
        private void Controller_Configure(object sender, RoutedEventArgs e)
        {
            Controllers.ControllerConfiguration cfgDialog = new Controllers.ControllerConfiguration();

            cfgDialog.SelectedType = m_controller.AssociatedControllerType;

            bool? dialogResults = cfgDialog.ShowDialog();

            if (dialogResults == true)
            {
                // Assign the selected controller
                m_controller.AssociatedControllerType = cfgDialog.SelectedType;
                m_controllerView.IsDirty = true;
            }
            e.Handled = true;
        }
#if NOT_USED
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                ViewModel.StripViewModel svm = associatedObject as ViewModel.StripViewModel;

                if (svm != null)
                {
                    // Create dialog to configure the strip i.e. number of pixels etc.
                    Visuals.StripSettings settingsDlg = new Visuals.StripSettings();

                    settingsDlg.StartingCount = svm.Children.Count;  // see what we currently have
                    settingsDlg.StartStripId = svm.StripId;

                    bool? dialogResults = settingsDlg.ShowDialog();

                    if (dialogResults == true)
                    {
                        svm.SetPixelCount(settingsDlg.PixelCount, ColorSelection.SelectedColor);
                        svm.StripId = settingsDlg.StripId;

                        if (svm.Parent == m_currentFrame)
                        {
                            foreach (Visuals.StripView panel in FrameContainer.Children)
                            {
                                if (panel.AssociatedView == svm)
                                {
                                    // Panel already exists - update accordingly
                                    // We need to update the view
                                    panel.Reset(settingsDlg.PixelCount);
                                    int index = 0;

                                    foreach (ViewModel.PixelViewModel pvm in svm.Children)
                                    {
                                        DataModel.Pixel pixel = pvm.AssociatedData as DataModel.Pixel;

                                        // Set the color first and then set
                                        // the associated view so we don't set the views color back to itself
                                        panel[index].Color = pixel.PixelColor;
                                        panel[index++].AssociatedView = pvm;
                                    }
                                    if (m_currentStrip != null)
                                    {
                                        m_currentStrip.IsSelected = false;
                                    }
                                    panel.IsSelected = true;
                                    m_currentStrip = panel;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            e.Handled = true;
        }
#endif
        private void Controller_AddMovie(object sender, RoutedEventArgs e)
        {
            // There is only one controller, no need to go searching for it
            m_controllerView.AddChild(); // add a new movie
            m_controllerView.IsExpanded = true;
            e.Handled = true;
        }

        private void Controller_PasteMovie(object sender, RoutedEventArgs e)
        {
            try
            {
                int objectId = 0;

                if (Clipboard.ContainsData(ViewModel.MovieViewModel.IdentityName) == true)
                {
                    DataModel.Movie pastedMovie = Clipboard.GetData(ViewModel.MovieViewModel.IdentityName) as DataModel.Movie;

                    if (pastedMovie != null)
                    {
                        ViewModel.MovieViewModel mvm = m_controllerView.AddChild() as ViewModel.MovieViewModel;

                        mvm.AssociatedData = pastedMovie;

                        objectId = pastedMovie.Id;
                    }
                }

                // Either copy or cut both paste so we need to paste it
                // the question is do we go back and remove the original
                if (m_currentOperation == m_cutOperation)
                {
                    // We are cutting
                    if (objectId != 0)
                    {
                        // We need to find the object based on the id
                        ViewModel.ViewModelBase viewObject = m_controllerView.Find(objectId);

                        if (viewObject != null)
                        {
                            m_controllerView.RemoveChild(viewObject);
                        }
                    }
                }
            }

            catch (System.Runtime.InteropServices.COMException excep)
            {
                MessageBox.Show(excep.Message);
            }

            e.Handled = true;
        }
        #endregion // CONTROLLER_TREE

        #region MOVIE_TREE
        private void Movie_AddFrame(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                ViewModel.MovieViewModel mvm = associatedObject as ViewModel.MovieViewModel;

                if (mvm != null)
                {
                    ViewModel.FrameViewModel fvm = mvm.AddChild() as ViewModel.FrameViewModel;
                    
                    // Create the default strips for this movie frame
                    Controllers.ControllerType controllerType = m_controller.AssociatedControllerType;

                    int stripIndex = 0;
                    foreach (Controllers.ControllerStrip strip in controllerType.Strips)
                    {
                        ViewModel.StripViewModel svm = fvm.AddChild() as ViewModel.StripViewModel;

                        svm.SetPixelCount(strip.Length, ColorSelection.SelectedColor);
                        svm.StripId = stripIndex++;
                    }

                    ShowMovieStrip(mvm); // Update for new frame

                    mvm.IsExpanded = true;
                }
            }

            // Determine associated control
            e.Handled = true;
        }

        private void Movie_PasteFrame(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                try
                {
                    int objectId = 0;
                    ViewModel.MovieViewModel mvm = associatedObject as ViewModel.MovieViewModel;

                    // We need to know the movie that is receiving this Frame
                    if (mvm != null)
                    {
                        if (Clipboard.ContainsData(ViewModel.FrameViewModel.IdentityName) == true)
                        {
                            DataModel.Frame pastedFrame = Clipboard.GetData(ViewModel.FrameViewModel.IdentityName) as DataModel.Frame;

                            if (pastedFrame != null)
                            {
                                ViewModel.FrameViewModel cvm = mvm.AddChild() as ViewModel.FrameViewModel;

                                cvm.AssociatedData = pastedFrame;

                                objectId = pastedFrame.Id;
                            }
                        }

                        // Either copy or cut both paste so we need to paste it
                        // the question is do we go back and remove the original
                        if (m_currentOperation == m_cutOperation)
                        {
                            // We are cutting
                            if (objectId != 0)
                            {
                                // We need to find the object based on the id
                                ViewModel.ViewModelBase viewObject = mvm.Find(objectId);

                                if (viewObject != null)
                                {
                                    mvm.RemoveChild(viewObject);
                                }
                            }
                        }
                    }
                }

                catch (System.Runtime.InteropServices.COMException excep)
                {
                    MessageBox.Show(excep.Message);
                }
            }
            e.Handled = true;
        }

        private void Movie_CopyMovie(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                ViewModel.MovieViewModel mvm = associatedObject as ViewModel.MovieViewModel;

                if (mvm != null)
                {
                    DataModel.Movie movieObject = mvm.AssociatedData as DataModel.Movie;

                    try
                    {
                        Clipboard.SetData(ViewModel.MovieViewModel.IdentityName, movieObject);
                        m_currentOperation = m_copyOperation;
                    }

                    catch (System.Runtime.InteropServices.COMException excep)
                    {
                        MessageBox.Show(excep.Message);
                    }
                }
            }
            e.Handled = true;
        }

        private void Movie_CutMovie(object sender, RoutedEventArgs e)
        {
            // Need to cut, should we put that info on the Clipboard?
            // If we do, what do we put there
            // Perhaps we should place a "CutObject" with a value indicating which one
            // we could check on the paste if it is a cutobject opposed to copy

            // Do we cut and then paste? problem with having two programs sharing, how would we cut and paste and know the other
            // application pasted it?  One option is not to allow pasting across applications
            // how to cancel the cut if they don't paste?  Just keep it until we get the paste and the paste cuts it i.e. moves it
            Movie_CopyMovie(sender, e);

            m_currentOperation = m_cutOperation; // override the copy operation
            e.Handled = true;
        }

        private void Movie_DeleteMovie(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                ViewModel.MovieViewModel mvm = associatedObject as ViewModel.MovieViewModel;

                if (mvm != null)
                {
                    m_controllerView.RemoveChild(mvm); // remove this movie
                }
            }
            e.Handled = true;
        }
        #endregion // MOVIE_TREE

        #region Frame_TREE
        private void Frame_AddStrip(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                ViewModel.FrameViewModel cvm = associatedObject as ViewModel.FrameViewModel;

                if (cvm != null)
                {
                    cvm.AddChild();
                    cvm.IsExpanded = true;
                }
            }

            // Determine associated control
            e.Handled = true;
        }

        // If the strip already exists then we need to prompt to "overwrite" it
        private void Frame_PasteStrip(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                try
                {
                    int objectId = 0;
                    ViewModel.FrameViewModel cvm = associatedObject as ViewModel.FrameViewModel;

                    // We need to know the Frame that is receiving this strip
                    if (cvm != null)
                    {
                        if (Clipboard.ContainsData(ViewModel.StripViewModel.IdentityName) == true)
                        {
                            DataModel.Strip pastedStrip = Clipboard.GetData(ViewModel.StripViewModel.IdentityName) as DataModel.Strip;

                            if (pastedStrip != null)
                            {
                                ViewModel.StripViewModel svm = cvm.AddChild() as ViewModel.StripViewModel;

                                svm.AssociatedData = pastedStrip;

                                objectId = pastedStrip.Id;
                            }
                        }

                        // Either copy or cut both paste so we need to paste it
                        // the question is do we go back and remove the original
                        if (m_currentOperation == m_cutOperation)
                        {
                            // We are cutting
                            if (objectId != 0)
                            {
                                // We need to find the object based on the id
                                ViewModel.ViewModelBase viewObject = cvm.Find(objectId);

                                if (viewObject != null)
                                {
                                    cvm.RemoveChild(viewObject);
                                }
                            }
                        }
                    }
                }

                catch (System.Runtime.InteropServices.COMException excep)
                {
                    MessageBox.Show(excep.Message);
                }
            }
            e.Handled = true;
        }

        private void Frame_CopyFrame(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                ViewModel.FrameViewModel cvm = associatedObject as ViewModel.FrameViewModel;

                if (cvm != null)
                {
                    DataModel.Frame FrameObject = cvm.AssociatedData as DataModel.Frame;

                    try
                    {
                        Clipboard.SetData(ViewModel.FrameViewModel.IdentityName, FrameObject);
                        m_currentOperation = m_copyOperation;
                    }

                    catch (System.Runtime.InteropServices.COMException excep)
                    {
                        MessageBox.Show(excep.Message);
                    }
                }
            }
            e.Handled = true;
        }

        private void Frame_CutFrame(object sender, RoutedEventArgs e)
        {
            Frame_CopyFrame(sender, e);

            m_currentOperation = m_cutOperation; // override the copy operation
            e.Handled = true;
        }

        private void Frame_DeleteFrame(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                ViewModel.FrameViewModel cvm = associatedObject as ViewModel.FrameViewModel;

                if (cvm != null)
                {
                    ViewModel.MovieViewModel parentVM = cvm.Parent as ViewModel.MovieViewModel;

                    if (parentVM != null)
                    {
                        parentVM.RemoveChild(cvm);
                    }
                }
            }
            e.Handled = true;
        }
        #endregion // Frame_TREE

        #region STRIP_TREE
        private void Strip_CopyStrip(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                ViewModel.StripViewModel svm = associatedObject as ViewModel.StripViewModel;

                if (svm != null)
                {
                    DataModel.Strip stripObject = svm.AssociatedData as DataModel.Strip;

                    try
                    {
                        Clipboard.SetData(ViewModel.StripViewModel.IdentityName, stripObject);
                        m_currentOperation = m_copyOperation;
                    }

                    catch (System.Runtime.InteropServices.COMException excep)
                    {
                        MessageBox.Show(excep.Message);
                    }
                }
            }
            e.Handled = true;
        }

        private void Strip_CutStrip(object sender, RoutedEventArgs e)
        {
            Strip_CopyStrip(sender, e);

            m_currentOperation = m_cutOperation; // override the copy operation
            e.Handled = true;
        }

        private void Strip_DeleteStrip(object sender, RoutedEventArgs e)
        {
            object associatedObject = GetAssociatedMenuControl(sender);

            if (associatedObject != null)
            {
                ViewModel.StripViewModel svm = associatedObject as ViewModel.StripViewModel;

                if (svm != null)
                {
                    ViewModel.FrameViewModel parentVM = svm.Parent as ViewModel.FrameViewModel;

                    if (parentVM != null)
                    {
                        parentVM.RemoveChild(svm);
                    }
                }
            }
            e.Handled = true;
        }
        #endregion // STRIP_TREE
        #endregion // TREE_MENU_ITEMS

        // window name can be either the untitled string or a document name w/ path
        // I prefer not to the path nor the extension
        private void SetWindowName(string name)
        {
            int lastSlash = name.LastIndexOf('\\');
            int lastPeriod = name.LastIndexOf('.');

            if (lastSlash < 0)
            {
                lastSlash = 0;
            }
            else
            {
                lastSlash++; // we want to move past that last slash
            }

            if (lastPeriod < 0)
            {
                lastPeriod = name.Length;
            }

            // Convert lastPeriod to length opposed to zero based index
            lastPeriod -= lastSlash;

//            Title = name.Substring(lastSlash, lastPeriod) + " - " + m_appTitle;
            Title = m_appTitle + " - " + name.Substring(lastSlash, lastPeriod);
        }

        private void ShowFrame(ViewModel.FrameViewModel cvm)
        {
            // If it is our current Frame then skip it
            if (cvm != null)
            {
                if (cvm != m_currentFrame)
                {
                    bool updateOnly = true;

                    /*
                     * TODO only update if needed opposed to recreating everything
                     */
                    if (m_currentFrame == null)
                    {
                        updateOnly = false;  // nothing currently selected
                    }
                    else
                    {
                        if (cvm.Children.Count != m_currentFrame.Children.Count)
                        {
                            updateOnly = false;
                        }
                    }

                    if (updateOnly == false)
                    {
                        FrameContainer.Clear();

                        ThreeDViewPort.Children.Clear();
                        ThreeDViewPort.Children.Add(ViewPortLight); // add our light back in
                        ThreeDViewPort.Children.Add(ViewPortGrid); // add in our grid too
                    }

                    double offset = 0.0;
                    int panelIndex = 0;
                    foreach (ViewModel.StripViewModel stripVM in cvm.Children)
                    {
                        Visuals.StripView panel = null;
                        bool panelAdded = false;
                        ModelVisual3D threeDStrip = null;

                        if (updateOnly == false)
                        {
                            // Create a stack panel for this strip
                            panel = new Visuals.StripView(stripVM.Children.Count);
                            FrameContainer.AddStrip(panel);
                            threeDStrip = panel.Generate3DView(offset);
                            panelAdded = true;
                        }
                        else
                        {
                            panel = FrameContainer[panelIndex];

                            if (stripVM.Children.Count != panel.AssociatedView.Children.Count)
                            {
                                panel = new Visuals.StripView(stripVM.Children.Count);
                                FrameContainer[panelIndex] = panel;
                                threeDStrip = panel.Generate3DView(offset);
                                panelAdded = true;
                            }
                            else // if we didnt create a new one, then find the right one
                            {
                                int threeDIndex = 0;
                                foreach (ModelVisual3D mv3dObj in ThreeDViewPort.Children)
                                {
                                    threeDStrip = mv3dObj as HelixToolkit.Wpf.BoxVisual3D;
                                    if (threeDStrip != null)
                                    {
                                        if (threeDIndex == panelIndex)  // the one we want?
                                        {
                                            // Done
                                            break;
                                        }
                                        threeDIndex++;
                                    }
                                }
                            }
                        }

                        // If it is new, setup the callbacks etc.
                        if (panelAdded == true)
                        {
                            panel.ZoomHandler = PixelStack_MouseWheel;
                            panel.Handler = PixelSelected;
                            panel.Drop += PixelStack_Drop;
                            panel.MouseWheel += PixelStack_MouseWheel;
                            panel.AssociatedView = stripVM;
                            ThreeDViewPort.Children.Add(threeDStrip);
                        }

                        // Whether it is a new panel or an existing one
                        // we need to update the pixel colors
                        int index = 0;
                        foreach (ViewModel.PixelViewModel pvm in stripVM.Children)
                        {
                            DataModel.Pixel pixel = pvm.AssociatedData as DataModel.Pixel;

                            // Set the color first and then set
                            // the associated view so we don't set the views color back to itself
                            panel[index].AssociatedView = null;
                            panel[index].Color = pixel.PixelColor;
                            panel[index].AssociatedView = pvm;
                            if (threeDStrip != null)
                            {
                                HelixToolkit.Wpf.SphereVisual3D pixelSphere = threeDStrip.Children[index] as HelixToolkit.Wpf.SphereVisual3D;

                                pixelSphere.Fill = pvm.CurrentColorAsBrush;
                            }

                            index++;
                        }

                        offset -= 1.0;

                        panelIndex++; // now moving to the next one
                    }

                    // Deselect the older current strip if selected...
                    if (m_currentStrip != null)
                    {
                        m_currentStrip.IsSelected = false;
                    }

                    m_currentFrame = cvm;
                    if (FrameContainer.StripCount > 0)
                    {
                        m_currentStrip = FrameContainer[0];
                        m_currentStrip.IsSelected = true;
                    }
                    else
                    {
                        m_currentStrip = null;
                    }
                }
            }
            else
            {
                FrameContainer.Clear();
            }
        }

        private ViewModel.ViewModelBase GetSelectedItem()
        {
            ViewModel.ViewModelBase vmb = null;

            if (ControllerData.SelectedItem != null)
            {
                vmb = ControllerData.SelectedItem as ViewModel.ViewModelBase;
            }
            return vmb;
        }

        private void ShowMovieStrip(ViewModel.MovieViewModel mvm)
        {
            // Only update as needed
            if (mvm != m_currentMovieViewModel)
            {
                bool updateOnly = true;

                m_currentMovieViewModel = mvm;

                // See if the number of frames match
                if (FrameStrip.FrameCount != mvm.Children.Count)
                {
                    // Clear the old
                    FrameStrip.ClearData(true);
                    updateOnly = false;
                }
                else
                {
                    // Since they are equal we will re-use the frames
                    // but clear out the stacks
                    FrameStrip.ClearData(false);
                }

                int frameId = 1;
                foreach (ViewModel.FrameViewModel fvm in mvm.Children)
                {
                    Visuals.AnimationFrame frame = null;

                    if (updateOnly == false)
                    {
                        // Need to create a visual of each frame
                        frame = new Visuals.AnimationFrame();
                        FrameStrip.AddFrame(frame);
                    }
                    else
                    {
                        frame = FrameStrip.GetFrame(frameId - 1); // 0 based
                    }

                    if (frame != null)
                    {
                        DataModel.Frame dataFrame = fvm.AssociatedData as DataModel.Frame;

                        // Any transforms
                        if (dataFrame.Transforms > 0)
                        {
                            Transform.TransformStack transformStack = new Transform.TransformStack();

                            foreach (SharedInterfaces.ITransform transform in dataFrame.GetTransforms)
                            {
                                transformStack.Children.Add(transform as UIElement);
                            }
                            FrameStrip.AddTransformPanel(frame, transformStack);
                        }
                        frame.AssociatedData = dataFrame; // Show this one on the screen
                        frame.FrameId = frameId++;
                    }
                }
            }
        }

        private void ControllerData_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.ViewModelBase currentItem = GetSelectedItem();

            if (currentItem != null)
            {
                ViewModel.MovieViewModel mwm = currentItem as ViewModel.MovieViewModel;

                if (mwm != null)
                {
                    // Houston we have a movie
                    // We will select the first Frame of the movie to display
                    if (mwm.Children.Count > 0)
                    {
                        ShowFrame(mwm.Children[0]);
                        ShowMovieStrip(mwm);
                    }
                    else
                    {
                        ShowFrame(null);
                    }
                }
                else
                {
                    ViewModel.FrameViewModel cvm = currentItem as ViewModel.FrameViewModel;

                    if (cvm != null)
                    {
                        ShowFrame(cvm);
                        ViewModel.MovieViewModel mvm = cvm.Parent as ViewModel.MovieViewModel;

                        if (mvm != null)
                        {
                            ShowMovieStrip(mvm);
                        }
                    }
                    else
                    {
                        ViewModel.StripViewModel svm = currentItem as ViewModel.StripViewModel;

                        if (m_currentStrip != null)
                        {
                            m_currentStrip.IsSelected = false;
                        }

                        if (svm != null)
                        {
                            ViewModel.FrameViewModel fvm = svm.Parent as ViewModel.FrameViewModel;
                            if (fvm != null)
                            {
                                ViewModel.MovieViewModel mvm = fvm.Parent as ViewModel.MovieViewModel;

                                if (mvm != null)
                                {
                                    ShowMovieStrip(mvm);
                                }
                            }
                            // Fill We show everything in the Frame
                            ShowFrame(fvm);

                            foreach (Visuals.StripView stripView in FrameContainer.Children)
                            {
                                if (stripView.AssociatedView == svm)
                                {
                                    m_currentStrip = stripView;
                                    m_currentStrip.IsSelected = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        #region COMMAND_BINDINGS

        /*
         * The file handling command bindings are defined in the application menu section
         * as I already defined them for the menu.
         */
        private void CommandBinding_Cut(object sender, ExecutedRoutedEventArgs e)
        {
            CommandBinding_Copy(sender, e);

            m_currentOperation = m_cutOperation;
        }

        private void CommandBinding_Copy(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.ViewModelBase currentItem = GetSelectedItem();

            // Determine what is currently selected and proceed to cut it
            if (currentItem != null)
            {
                string instanceIdenityName = currentItem.InstanceIdentityName;

                if (instanceIdenityName != null)
                {
                    DataModel.DataModelBase dmb = currentItem.AssociatedData;

                    try
                    {
                        Clipboard.SetData(instanceIdenityName, dmb);
                        m_currentOperation = m_copyOperation;
                    }

                    catch (System.Runtime.InteropServices.COMException excep)
                    {
                        MessageBox.Show(excep.Message);
                    }
                }
            }
        }

        private void CommandBinding_Paste(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.ViewModelBase currentItem = GetSelectedItem();

            // Determine what is currently selected and proceed to cut it
            if (currentItem != null)
            {
                int objectId = 0;

                // We will add the object on the Clipboard to the selected item
                // However we have to ensure it is viable
                if (Clipboard.ContainsData(ViewModel.MovieViewModel.IdentityName) == true)
                {
                    // There is a movie in the Clipboard
                    if (currentItem.GetType() == m_controllerView.GetType())
                    {
                        try
                        {
                            DataModel.Movie movie = Clipboard.GetData(ViewModel.MovieViewModel.IdentityName) as DataModel.Movie;

                            if (movie != null)
                            {
                                //Is it valid? i.e. can we paste this into the controller?
                                if (currentItem == m_controllerView)
                                {
                                    ViewModel.MovieViewModel mvm = m_controllerView.AddChild(movie) as ViewModel.MovieViewModel;

                                    objectId = movie.Id;
                                }
                            }
                        }
                        catch (System.Runtime.InteropServices.COMException excep)
                        {
                            MessageBox.Show(excep.Message);
                        }
                    }
                    else if (Clipboard.ContainsData(ViewModel.FrameViewModel.IdentityName) == true)
                    {
                        // There is a Frame on the Clipboard
                        try
                        {
                            DataModel.Frame Frame = Clipboard.GetData(ViewModel.FrameViewModel.IdentityName) as DataModel.Frame;

                            if (Frame != null)
                            {
                                // Is it a movie
                                if (currentItem.InstanceIdentityName == ViewModel.MovieViewModel.IdentityName)
                                {
                                    ViewModel.MovieViewModel mvm = currentItem as ViewModel.MovieViewModel;
                                    if (mvm != null)
                                    {
                                        ViewModel.FrameViewModel cvm = mvm.AddChild(Frame) as ViewModel.FrameViewModel;

                                        objectId = Frame.Id;
                                    }
                                }
                            }
                        }
                        catch (System.Runtime.InteropServices.COMException excep)
                        {
                            MessageBox.Show(excep.Message);
                        }
                    }
                    else if (Clipboard.ContainsData(ViewModel.StripViewModel.IdentityName) == true)
                    {
                        // There is a strip on the Clipboard
                        try
                        {
                            DataModel.Strip strip = Clipboard.GetData(ViewModel.StripViewModel.IdentityName) as DataModel.Strip;

                            if (strip != null)
                            {
                                // Is it a movie
                                if (currentItem.InstanceIdentityName == ViewModel.MovieViewModel.IdentityName)
                                {
                                    ViewModel.FrameViewModel mvm = currentItem as ViewModel.FrameViewModel;
                                    if (mvm != null)
                                    {
                                        ViewModel.StripViewModel svm = mvm.AddChild(strip) as ViewModel.StripViewModel;

                                        objectId = strip.Id;
                                    }
                                }
                            }
                        }
                        catch (System.Runtime.InteropServices.COMException excep)
                        {
                            MessageBox.Show(excep.Message);
                        }
                    }

                    // Either copy or cut both paste so we need to paste it
                    // the question is do we go back and remove the original
                    if (m_currentOperation == m_cutOperation)
                    {
                        // We are cutting
                        if (objectId != 0)
                        {
                            // We need to find the object based on the id
                            ViewModel.ViewModelBase viewObject = m_controllerView.Find(objectId);

                            if (viewObject != null)
                            {
                                ViewModel.ViewModelBase parentObj = viewObject.Parent;

                                parentObj.RemoveChild(viewObject);
                            }
                        }
                    }
                }
            }
        }
        #endregion // COMMAND_BINDINGS

        private void MRU_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock menuText = sender as TextBlock;

            // This is a text block
            if (menuText != null)
            {
                ContentPresenter presenter =  menuText.TemplatedParent as ContentPresenter;
                if (presenter != null)
                {
                    FileListEntry entry = presenter.Content as FileListEntry;

                    if (entry != null)
                    {
                        // Load document and all that good stuff
                        OpenExistingDocument(entry.FileName);
                    }
                }
            }
//            e.Handled = true; // we are going to indicate we didn't handle it so the menu closes
        }

        #region FILM_STRIP

        #region PLAY_COMMANDS
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            // Play it all
            FrameStrip.Play(0, FrameStrip.FrameCount);
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            FrameStrip.Pause();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            FrameStrip.Stop();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            FrameStrip.Home();
        }

        private void Rewind_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
        }

        private void End_Click(object sender, RoutedEventArgs e)
        {
            FrameStrip.End();
        }
        #endregion // PLAY_COMMANDS

        // should the film strip be of the 3D model of each frame?
        // We need to also insert transitions between frmaes
        void OnAnimationStopped(object sender, EventArgs e)
        {
            Pause.IsEnabled = false;
            Stop.IsEnabled = false;
            Play.IsEnabled = true;
        }

        void OnAnimationStarted(object sender, EventArgs e)
        {
            Stop.IsEnabled = true;
            Pause.IsEnabled = true;
        }

        void OnAnimationPaused(object sender, EventArgs e)
        {
            Pause.IsEnabled = false;
            Stop.IsEnabled = true; // they can still stop it
            Play.IsEnabled = true;
        }

        void OnAnimationResumed(object sender, EventArgs e)
        {
        }

        void OnAnimationProgress(object sender, Visuals.AnimationEvent e)
        {
        }

        private void OnFrameSelected(int frameId)
        {
            // User selected a frame from somewhere else, select it in the tree
            if (m_currentMovieViewModel != null)
            {
                int selectedFrame = frameId - 1; // 0 based

                m_currentMovieViewModel.IsExpanded = true;

                if (selectedFrame < m_currentMovieViewModel.Children.Count)
                {
                    //                    m_currentMovieViewModel.Children[selectedFrame].IsExpanded = true;
                    m_currentMovieViewModel.Children[selectedFrame].IsSelected = true;
                }
            }
        }
        #endregion // FILM_STRIP

        #region THREE_D_VIEW_SUPPORT
        private void ThreeDView_IsSelectedChanged(object sender, EventArgs e)
        {
            // IF our 3D view is active, make sure it has the current frame showing
            // consisting of X strips of leds
            // We may want to not have to update the view every time the user goes away from it
            if (ThreeDView.IsActive == true)
            {
            }
            else
            {
//                ThreeDViewPort.Children.Clear();
            }
        }

        private void ResetThreeDView()
        {
            ThreeDViewPort.Camera.Position = new System.Windows.Media.Media3D.Point3D(0, 0, 20);
            ThreeDViewPort.Camera.LookDirection = new System.Windows.Media.Media3D.Vector3D(0, -3, -20);
            ThreeDViewPort.Camera.UpDirection = new System.Windows.Media.Media3D.Vector3D(0, 1, 0);
        }

        private void ResetView_Click(object sender, RoutedEventArgs e)
        {
            if (ThreeDView.IsActive == true)
            {
                ResetThreeDView();
            }
            e.Handled = true;
        }

        private void ShowGrid_Click(object sender, RoutedEventArgs e)
        {
            if (ThreeDView.IsActive == true)
            {
                if (ShowGrid.IsChecked == true)
                {
                    ViewPortGrid.Visible = true;
                }
                else
                {
                    ViewPortGrid.Visible = false;
                }
            }
            e.Handled = true;
        }
        #endregion // THREE_D_VIEW_SUPPRT

        #region TRANSFORM_UI
        private void LoadTransforms()
        {
            SharedInterfaces.TransformFactory factory = SharedInterfaces.TransformFactory.GetInstance();
            string transformPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + "\\LightBringer\\" + m_transformFolder;

            // If that doesn't exist then try the non x86 path
            if (Directory.Exists(transformPath) == false)
            {
                transformPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\\LightBringer\\" + m_transformFolder;
            }

            if (Directory.Exists(transformPath) == false)
            {
//                Directory.GetCurrentDirectory();
                transformPath = Environment.CurrentDirectory;
            }
            factory.LoadTransformTypes(transformPath);

            List<string> transformNames = new List<string>();

            factory.GetTransformNames(ref transformNames);

            foreach (string transformName in transformNames)
            {
                SharedInterfaces.ITransform transform = factory.GetTransform(transformName);

                if (transform != null)
                {
                    // Make sure we are not ignoring this one
                    if (transform.Category() != SharedInterfaces.TransformCategory.eIgnore)
                    {
                        UIElement transformUI = transform as UIElement;

                        transform.SetIconSize(SharedInterfaces.TransformIconSize.eMedium);

                        if (transform.Origin() == SharedInterfaces.TransformOrigin.eSystem)
                        {
                            SystemTransforms.Children.Add(transformUI);
                        }
                        else
                        {
                            UserTransforms.Children.Add(transformUI);
                        }
                    }
                }
            }
        }

        private void TransformControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        #endregion // TRANFORM_UI
    }
}
