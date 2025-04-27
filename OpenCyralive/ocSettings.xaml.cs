using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using static OpenCyralive.GlobalFunction;
using Application = System.Windows.Application;
using ColorConverter = System.Drawing.ColorConverter;
using Color = System.Drawing.Color;
using ComboBox = System.Windows.Controls.ComboBox;
using System.Reflection;
using IWshRuntimeLibrary;
using File = System.IO.File;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Globalization;
using System.Windows.Markup;

namespace OpenCyralive
{
    /// <summary>
    /// ocSettings.xaml 的交互逻辑
    /// </summary>
    public partial class ocSettings : Window
    {
        List<string> fonts = new List<string>();
        string[] fontsizes = { "13", "14", "16", "18" };
        string[] langs = Directory.GetFiles(res_folder + "\\lang");
        int selectedLang;
        public ocSettings()
        {
            InitializeComponent();
            System.Windows.Forms.Application.EnableVisualStyles();
            ocConfig = JsonNode.Parse(File.ReadAllText(res_folder + "\\config\\config.json"));
            if (ocConfig["WindowXY"].ToString() != "")
            {
                oc_hold_position.IsChecked = true;
            }
            if (ocConfig["WindowSize"].ToString() != "")
            {
                oc_hold_size.IsChecked = true;
            }
            if (ocConfig["Topmost"].ToString() != "No")
            {
                oc_topmost.IsChecked = true;
            }
            if (ocConfig["TransparentWindow"].ToString() == "Yes")
            {
                oc_transparent_window.IsChecked = true;
            }
            if (ocConfig["Translucent"].ToString() == "Yes")
            {
                oc_translucent.IsChecked = true;
            }
            if (ocConfig["Taskbar"].ToString() == "Yes")
            {
                oc_tb_show.IsChecked = true;
            }
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    oc_msg_bg.Fill = (window as MainWindow).Cierra_hover_text_border.Background;
                    oc_msg_fg.Fill = (window as MainWindow).Cierra_hover_text.Foreground;
                    oc_msg_brd.Fill = (window as MainWindow).Cierra_hover_text_border.BorderBrush;
                }
            }
            foreach (System.Drawing.FontFamily fontFamily in System.Drawing.FontFamily.Families)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = fontFamily.Name;
                oc_msg_font.Items.Add(comboBoxItem);
                fonts.Add(fontFamily.Name);
            }
            int selectedFont = 0;
            foreach (ComboBoxItem comboBoxItem in oc_msg_font.Items)
            {
                comboBoxItem.FontFamily = new System.Windows.Media.FontFamily(fonts[selectedFont]);
                selectedFont++;
            }
            if (ocConfig["Bubble_font"].ToString() != "")
            {
                oc_msg_font.SelectedIndex = fonts.IndexOf(ocConfig["Bubble_font"].ToString());
                oc_msg_font.ToolTip = fonts[fonts.IndexOf(ocConfig["Bubble_font"].ToString())];
            }
            else
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        oc_msg_font.SelectedIndex = fonts.IndexOf((window as MainWindow).Cierra_hover_text.FontFamily.ToString());
                        oc_msg_font.ToolTip = fonts[fonts.IndexOf((window as MainWindow).Cierra_hover_text.FontFamily.ToString())];
                    }
                }
            }
            if (ocConfig["Bubble_font_Bold"].ToString() != "")
            {
                oc_msg_font_bold.IsChecked = true;
            }
            if (ocConfig["Bubble_font_Italic"].ToString() != "")
            {
                oc_msg_font_italic.IsChecked = true;
            }
            foreach (string onefontsize in fontsizes)
            {
                oc_msg_font_size.Items.Add(onefontsize);
            }
            if (ocConfig["Bubble_font_size"].ToString() != "")
            {
                oc_msg_font_size.SelectedIndex = Array.IndexOf(fontsizes, ocConfig["Bubble_font_size"].ToString());
            }
            else
            {
                oc_msg_font_size.SelectedIndex = 0;
            }
            if (ocConfig["Hemi"].ToString() != "South")
            {
                oc_hemi.SelectedIndex = 0;
            }
            else
            {
                oc_hemi.SelectedIndex = 1;
            }
            foreach (string langfile in langs)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                FileStream fileStream = new FileStream(langfile, FileMode.Open);
                comboBoxItem.Content = ((ResourceDictionary)XamlReader.Load(fileStream))["displayName"];
                oc_language.Items.Add(comboBoxItem);
                fileStream.Close();
            }
            if (ocConfig["Culture"].ToString() != "")
            {
                selectedLang = Array.IndexOf(langs, res_folder + "\\lang\\" + ocConfig["Culture"].ToString() + ".xaml");
                oc_language.SelectedIndex = selectedLang;
            }
            else
            {
                if (File.Exists(res_folder + "\\lang\\" + CultureInfo.CurrentCulture + ".xaml"))
                {
                    selectedLang = Array.IndexOf(langs, res_folder + "\\lang\\" + CultureInfo.CurrentCulture + ".xaml");
                    oc_language.SelectedIndex = selectedLang;
                }
                else
                {
                    selectedLang = Array.IndexOf(langs, res_folder + "\\lang\\zh-CN.xaml");
                    oc_language.SelectedIndex = selectedLang;
                }
            }
            if (File.Exists(res_folder + "\\config\\brand.txt"))
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk"))
                {
                    oc_desktop_shortcut.IsChecked = true;
                }
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk"))
                {
                    oc_startmenu_shortcut.IsChecked = true;
                }
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk"))
                {
                    oc_autostart.IsChecked = true;
                }
            }
            else
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk"))
                {
                    oc_desktop_shortcut.IsChecked = true;
                }
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk"))
                {
                    oc_startmenu_shortcut.IsChecked = true;
                }
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk"))
                {
                    oc_autostart.IsChecked = true;
                }
            }
            if (!File.Exists(res_folder + "\\specialplugins\\moreinfo\\moreinfo.dll"))
            {
                oc_moreinfo.Visibility = Visibility.Hidden;
            }
            if (!File.Exists(res_folder + "\\specialplugins\\resetdefault\\resetdefault.dll"))
            {
                oc_reset_default.Visibility = Visibility.Hidden;
            }
        }

        void create_shortcut(Environment.SpecialFolder specialFolder)
        {
            WshShell shell = new WshShell();
            IWshShortcut Cierra_shortcut;
            if (File.Exists(res_folder + "\\config\\brand.txt"))
            {
                Cierra_shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(specialFolder) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk");
            }
            else
            {
                Cierra_shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(specialFolder) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk");
            }
            Cierra_shortcut.WorkingDirectory = Environment.CurrentDirectory;
            Cierra_shortcut.TargetPath = Directory.GetCurrentDirectory() + "\\OpenCyralive.exe";
            if (File.Exists(res_folder + "\\config\\brand.txt"))
            {
                if (ocConfig["Character"].ToString() != "" && File.Exists(res_folder + "\\images\\appicon\\" + read_config_file(res_folder + "\\config\\config.json", "Character") + "\\appicon.ico"))
                {
                    Cierra_shortcut.IconLocation = Directory.GetCurrentDirectory() + "\\" + res_folder + "\\images\\appicon\\" + read_config_file(res_folder + "\\config\\config.json", "Character") + "\\appicon.ico";
                }
                else if (File.Exists(res_folder + "\\images\\appicon\\" + strings[strings.Length - 1] + "\\appicon.ico"))
                {
                    Cierra_shortcut.IconLocation = Directory.GetCurrentDirectory() + "\\" + res_folder + "\\images\\appicon\\" + strings[strings.Length - 1] + "\\appicon.ico";
                }
                else
                {
                    Cierra_shortcut.IconLocation = Directory.GetCurrentDirectory() + "\\" + res_folder + "\\images\\appicon\\appicon.ico";
                }
                Cierra_shortcut.Description = File.ReadAllText(res_folder + "\\config\\brand.txt");
            }
            else
            {
                if (ocConfig["Character"].ToString() != "" && File.Exists(res_folder + "\\images\\appicon\\" + read_config_file(res_folder + "\\config\\config.json", "Character") + "\\appicon.ico"))
                {
                    Cierra_shortcut.IconLocation = Directory.GetCurrentDirectory() + "\\" + res_folder + "\\images\\appicon\\" + read_config_file(res_folder + "\\config\\config.json", "Character") + "\\appicon.ico";
                }
                else if (File.Exists(res_folder + "\\images\\appicon\\" + strings[strings.Length - 1] + "\\appicon.ico"))
                {
                    Cierra_shortcut.IconLocation = Directory.GetCurrentDirectory() + "\\" + res_folder + "\\images\\appicon\\" + strings[strings.Length - 1] + "\\appicon.ico";
                }
                else
                {
                    Cierra_shortcut.IconLocation = Directory.GetCurrentDirectory() + "\\" + res_folder + "\\images\\appicon\\appicon.ico";
                }
            }
            Cierra_shortcut.Save();
        }

        private void oc_hold_position_Click(object sender, RoutedEventArgs e)
        {
            if (read_config_file(res_folder + "\\config\\config.json", "WindowXY") != "")
            {
                write_config_file(res_folder + "\\config\\config.json", "WindowXY", "");
                chara_hold_position = false;
            }
            else
            {
                write_config_file(res_folder + "\\config\\config.json", "WindowXY", Application.Current.MainWindow.Left + "," + Application.Current.MainWindow.Top);
                chara_hold_position = true;
                get_position = Regex.Split(read_config_file(res_folder + "\\config\\config.json", "WindowXY"), ",");
            }
        }

        private void oc_hold_size_Click(object sender, RoutedEventArgs e)
        {
            if (read_config_file(res_folder + "\\config\\config.json", "WindowSize") != "")
            {
                write_config_file(res_folder + "\\config\\config.json", "WindowSize", "");
            }
            else if (read_config_file(res_folder + "\\config\\config.json", "WindowSize") == "")
            {
                write_config_file(res_folder + "\\config\\config.json", "WindowSize", Application.Current.MainWindow.Height + "," + Application.Current.MainWindow.Width);
            }
        }

        private void oc_topmost_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.Topmost)
            {
                Application.Current.MainWindow.Topmost = false;
                write_config_file(res_folder + "\\config\\config.json", "Topmost", "No");
            }
            else
            {
                Application.Current.MainWindow.Topmost = true;
                write_config_file(res_folder + "\\config\\config.json", "Topmost", "Yes");
            }
        }

        private void about_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(Directory.GetCurrentDirectory() + "\\" + res_folder + "\\specialplugins\\about\\about.dll");
                foreach (Type type in assembly.GetExportedTypes())
                {
                    if (type.Name == "plugin_base")
                    {
                        type.InvokeMember("pluginStart", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void os_license_Click(object sender, RoutedEventArgs e)
        {
            openThings(res_folder + "\\license", "");
        }

        private void oc_transparent_window_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.Background == Brushes.Transparent)
            {
                Application.Current.MainWindow.Background = (Brush)new BrushConverter().ConvertFromString("#01FFFFFF");
                write_config_file(res_folder + "\\config\\config.json", "TransparentWindow", "No");
            }
            else
            {
                Application.Current.MainWindow.Background = Brushes.Transparent;
                write_config_file(res_folder + "\\config\\config.json", "TransparentWindow", "Yes");
            }
        }

        private void oc_translucent_Click(object sender, RoutedEventArgs e)
        {
            if (read_config_file(res_folder + "\\config\\config.json", "Translucent") == "Yes")
            {
                write_config_file(res_folder + "\\config\\config.json", "Translucent", "No");
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).oc_Show.Opacity = 1;
                    }
                }
            }
            else
            {
                write_config_file(res_folder + "\\config\\config.json", "Translucent", "Yes");
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).oc_Show.Opacity = 0.5;
                    }
                }
            }
        }

        private void oc_tb_show_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.ShowInTaskbar)
            {
                Application.Current.MainWindow.ShowInTaskbar = false;
                write_config_file(res_folder + "\\config\\config.json", "Taskbar", "No");
            }
            else
            {
                Application.Current.MainWindow.ShowInTaskbar = true;
                write_config_file(res_folder + "\\config\\config.json", "Taskbar", "Yes");
            }
        }

        private void oc_msg_bg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    colorDialog.Color = (Color)new ColorConverter().ConvertFromString((window as MainWindow).Cierra_hover_text_border.Background.ToString());
                }
            }
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string hexColor = "#" + (colorDialog.Color.ToArgb() & 0x00FFFFFF).ToString("X6");
                write_config_file(res_folder + "\\config\\config.json", "Bubble_bg", hexColor);
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).Cierra_hover_text_border.Background = (Brush)new BrushConverter().ConvertFromString(hexColor);
                        (window as MainWindow).Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    }
                }
                oc_msg_bg.Fill = (Brush)new BrushConverter().ConvertFromString(hexColor);
            }
        }

        private void oc_msg_fg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    colorDialog.Color = (Color)new ColorConverter().ConvertFromString((window as MainWindow).Cierra_hover_text.Foreground.ToString());
                }
            }
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string hexColor = "#" + (colorDialog.Color.ToArgb() & 0x00FFFFFF).ToString("X6");
                write_config_file(res_folder + "\\config\\config.json", "Bubble_fg", hexColor);
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).Cierra_hover_text.Foreground = (Brush)new BrushConverter().ConvertFromString(hexColor);
                        (window as MainWindow).Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    }
                }
                oc_msg_fg.Fill = (Brush)new BrushConverter().ConvertFromString(hexColor);
            }
        }

        private void oc_msg_brd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            colorDialog.FullOpen = true;
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    colorDialog.Color = (Color)new ColorConverter().ConvertFromString((window as MainWindow).Cierra_hover_text_border.BorderBrush.ToString());
                }
            }
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string hexColor = "#" + (colorDialog.Color.ToArgb() & 0x00FFFFFF).ToString("X6");
                write_config_file(res_folder + "\\config\\config.json", "Bubble_brd", hexColor);
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).Cierra_hover_text_border.BorderBrush = (Brush)new BrushConverter().ConvertFromString(hexColor);
                        (window as MainWindow).Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    }
                }
                oc_msg_brd.Fill = (Brush)new BrushConverter().ConvertFromString(hexColor);
            }
        }

        private void oc_msg_font_DropDownClosed(object sender, EventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    if (oc_msg_font.SelectedIndex != fonts.IndexOf((window as MainWindow).Cierra_hover_text.FontFamily.ToString()))
                    {
                        write_config_file(res_folder + "\\config\\config.json", "Bubble_font", ((ComboBox)sender).Text);
                        (window as MainWindow).Cierra_hover_text.Document.FontFamily = new FontFamily(((ComboBox)sender).Text);
                        fontFamily = (window as MainWindow).Cierra_hover_text.Document.FontFamily;
                        oc_msg_font.ToolTip = ((ComboBox)sender).Text;
                        (window as MainWindow).Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void oc_msg_font_bold_Click(object sender, RoutedEventArgs e)
        {
            if (read_config_file(res_folder + "\\config\\config.json", "Bubble_font_Bold") != "")
            {
                write_config_file(res_folder + "\\config\\config.json", "Bubble_font_Bold", "");
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).Cierra_hover_text.FontWeight = FontWeights.Normal;
                        (window as MainWindow).Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                write_config_file(res_folder + "\\config\\config.json", "Bubble_font_Bold", "Yes");
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).Cierra_hover_text.FontWeight = FontWeights.Bold;
                        (window as MainWindow).Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void oc_msg_font_italic_Click(object sender, RoutedEventArgs e)
        {
            if (read_config_file(res_folder + "\\config\\config.json", "Bubble_font_Italic") != "")
            {
                write_config_file(res_folder + "\\config\\config.json", "Bubble_font_Italic", "");
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).Cierra_hover_text.FontStyle = FontStyles.Normal;
                        (window as MainWindow).Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    }
                }
            }
            else
            {
                write_config_file("resources/config/config.json", "Bubble_font_Italic", "Yes");
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow))
                    {
                        (window as MainWindow).Cierra_hover_text.FontStyle = FontStyles.Italic;
                        (window as MainWindow).Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void oc_msg_font_size_DropDownClosed(object sender, EventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {

                    if (oc_msg_font_size.SelectedIndex != Array.IndexOf(fontsizes, (window as MainWindow).Cierra_hover_text.Document.FontSize.ToString()))
                    {
                        write_config_file(res_folder + "\\config\\config.json", "Bubble_font_size", ((ComboBox)sender).Text);
                        (window as MainWindow).Cierra_hover_text.Document.FontSize = Convert.ToDouble(((ComboBox)sender).Text);
                        fontSize = Convert.ToDouble(((ComboBox)sender).Text);
                        (window as MainWindow).Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void oc_hemi_north_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (read_config_file(res_folder + "\\config\\config.json", "Hemi") == "South")
            {
                write_config_file(res_folder + "\\config\\config.json", "Hemi", "");
            }
        }

        private void oc_hemi_south_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (read_config_file(res_folder + "\\config\\config.json", "Hemi") != "South")
            {
                write_config_file(res_folder + "\\config\\config.json", "Hemi", "South");
            }
        }

        private void oc_schedule_Click(object sender, RoutedEventArgs e)
        {
            new ocSchedule().ShowDialog();
        }

        private void oc_desktop_shortcut_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(res_folder + "\\config\\brand.txt"))
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk");
                }
                else
                {
                    create_shortcut(Environment.SpecialFolder.DesktopDirectory);
                }
            }
            else
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk");
                }
                else
                {
                    create_shortcut(Environment.SpecialFolder.DesktopDirectory);
                }
            }
        }

        private void oc_startmenu_shortcut_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(res_folder + "\\config\\brand.txt"))
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk");
                }
                else
                {
                    create_shortcut(Environment.SpecialFolder.StartMenu);
                }
            }
            else
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk");
                }
                else
                {
                    create_shortcut(Environment.SpecialFolder.StartMenu);
                }
            }
        }

        private void oc_autostart_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(res_folder + "\\config\\brand.txt"))
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + File.ReadAllText(res_folder + "\\config\\brand.txt") + ".lnk");
                }
                else
                {
                    create_shortcut(Environment.SpecialFolder.Startup);
                }
            }
            else
            {
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk"))
                {
                    File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + Assembly.GetExecutingAssembly().GetName().Name + ".lnk");
                }
                else
                {
                    create_shortcut(Environment.SpecialFolder.Startup);
                }
            }
        }

        private void oc_restart_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            notifyIcon.Dispose();
            openThings(Assembly.GetExecutingAssembly().GetName().Name + ".exe", "");
        }

        private void oc_config_file_Click(object sender, RoutedEventArgs e)
        {
            openThings(res_folder, "");
        }

        private void oc_moreinfo_Click(object sender, RoutedEventArgs e)
        {
            Assembly assembly = Assembly.LoadFrom(Directory.GetCurrentDirectory() + "\\" + res_folder + "\\specialplugins\\moreinfo\\moreinfo.dll");
            foreach (Type type in assembly.GetExportedTypes())
            {
                if (type.Name == "plugin_base")
                {
                    type.InvokeMember("pluginStart", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null);
                }
            }
        }

        private void oc_reset_default_Click(object sender, RoutedEventArgs e)
        {
            var messageBox = MessageBox.Show(Application.Current.FindResource("rst_msg").ToString(), Application.Current.FindResource("rst_warn").ToString(), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (messageBox == System.Windows.Forms.DialogResult.Yes)
            {
                Assembly assembly = Assembly.LoadFrom(Directory.GetCurrentDirectory() + "\\" + res_folder + "\\specialplugins\\resetdefault\\resetdefault.dll");
                foreach (Type type in assembly.GetExportedTypes())
                {
                    if (type.Name == "plugin_base")
                    {
                        if ((bool)type.InvokeMember("pluginStart", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null))
                        {
                            notifyIcon.Dispose();
                            Application.Current.Shutdown();
                            openThings(Assembly.GetExecutingAssembly().GetName().Name + ".exe", "");
                        }
                    }
                }
            }
        }

        private void oc_plugin_mgmt_Click(object sender, RoutedEventArgs e)
        {
            var dialogResult = MessageBox.Show(Application.Current.FindResource("msg_plugin_mgmt").ToString(), Application.Current.FindResource("msg_info").ToString(), MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                openThings(Directory.GetCurrentDirectory() + "\\CyralivePluginMgmt\\OpenCyralivePluginMgmt.exe", "");
                notifyIcon.Dispose();
                foreach (Window window in Application.Current.Windows)
                {
                    window.Close();
                }
            }
        }

        private void oc_character_mgmt_Click(object sender, RoutedEventArgs e)
        {
            new ocCharaMgmt().Show();
        }

        private void oc_language_DropDownClosed(object sender, EventArgs e)
        {
            if (oc_language.SelectedIndex != selectedLang)
            {
                if (oc_language.SelectedIndex == Array.IndexOf(langs, res_folder + "\\lang\\" + CultureInfo.CurrentCulture + ".xaml"))
                {
                    write_config_file(res_folder + "\\config\\config.json", "Culture", "");
                    FileStream fileStream = new FileStream(langs[selectedLang], FileMode.Open);
                    FileStream fileStream1 = new FileStream(res_folder + "\\lang\\" + CultureInfo.CurrentCulture + ".xaml", FileMode.Open);
                    Application.Current.Resources.MergedDictionaries.Remove((ResourceDictionary)XamlReader.Load(fileStream));
                    Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(fileStream1));
                    fileStream.Close();
                    fileStream1.Close();
                    selectedLang = oc_language.SelectedIndex;
                }
                else
                {
                    string[] strings = Regex.Split(langs[oc_language.SelectedIndex], @"\\");
                    string[] strings1 = Regex.Split(strings[strings.Length - 1], "\\.");
                    write_config_file(res_folder + "\\config\\config.json", "Culture", strings1[0]);
                    FileStream fileStream = new FileStream(langs[selectedLang].ToString(), FileMode.Open);
                    FileStream fileStream1 = new FileStream(res_folder + "\\lang\\" + strings1[0] + ".xaml", FileMode.Open);
                    Application.Current.Resources.MergedDictionaries.Remove((ResourceDictionary)XamlReader.Load(fileStream));
                    Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(fileStream1));
                    fileStream.Close();
                    fileStream1.Close();
                    selectedLang = oc_language.SelectedIndex;
                }
            }
        }
    }
}
