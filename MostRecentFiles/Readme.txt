When using this module, you will need to create a menu item in the main window such as:
                <MenuItem Name="RecentFileList" Header="Recent Files" ItemsSource="{Binding Children}">
                    <MenuItem.Resources>
                        <HierarchicalDataTemplate
                                          DataType="{x:Type mru:MRUFileHandler}" 
                                          ItemsSource="{Binding Children}"
                                          >
                            <StackPanel Orientation="Horizontal" Margin="5,0,0,0">
                                <TextBlock Margin="5,5,0,5" Text="RCL"></TextBlock>
                            </StackPanel>
                        </HierarchicalDataTemplate>
                        <DataTemplate DataType="{x:Type mru:FileListEntry}">
                            <StackPanel Orientation="Horizontal">
                                <TextBlock Text="{Binding Header}" MouseLeftButtonUp="MRU_MouseLeftButtonUp"></TextBlock>
                            </StackPanel>
                        </DataTemplate>
                    </MenuItem.Resources>
                </MenuItem>


You will also want to handle the file selection as follows:
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


