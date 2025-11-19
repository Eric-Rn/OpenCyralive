using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Text.RegularExpressions;
using Cursors = System.Windows.Input.Cursors;
using System.Text.Json.Nodes;
using static OpenCyralive.GlobalFunction;
using Timer = System.Timers.Timer;
using System.Windows.Threading;
using System.Drawing;
using System.Text.Json;
using Brushes = System.Windows.Media.Brushes;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using FontFamily = System.Windows.Media.FontFamily;
using static OpenCyralive.CyraliveOperaScript;
using System.Reflection;
using System.Linq;
using Application = System.Windows.Application;
using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;
using System.Windows.Media.Animation;
using System.Globalization;
using System.Windows.Markup;
using HeyRed.Mime;
using XamlAnimatedGif;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Runtime.Loader;

namespace OpenCyralive
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> character_images = new List<string>();
        bool hover_text_override;
        bool fullyStarted = false;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        void character_change(string file_path)
        {
            if (MimeGuesser.GuessMimeType(file_path) == "image/gif")
            {
                AnimationBehavior.SetRepeatBehavior(oc_Show, RepeatBehavior.Forever);
                AnimationBehavior.SetSourceUri(oc_Show, new Uri("file://" + Directory.GetCurrentDirectory() + "/" + file_path));
            }
            else
            {
                oc_Show.ClearValue(AnimationBehavior.RepeatBehaviorProperty);
                oc_Show.ClearValue(AnimationBehavior.SourceUriProperty);
                oc_Show.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\" + file_path, UriKind.RelativeOrAbsolute));
            }
        }

        string character_status()
        {
            if (AnimationBehavior.GetSourceUri(oc_Show) != null)
            {
                return Path.GetFileNameWithoutExtension(new Uri(AnimationBehavior.GetSourceUri(oc_Show).ToString()).LocalPath);
            }
            else
            {
                return Path.GetFileNameWithoutExtension(new Uri(oc_Show.Source.ToString()).LocalPath);
            }
        }

        string oc_Show_character_name()
        {
            if (AnimationBehavior.GetSourceUri(oc_Show) == null)
            {
                return Path.GetFileName(Path.GetDirectoryName(new Uri(oc_Show.Source.ToString()).LocalPath));
            }
            else
            {
                return Path.GetFileName(Path.GetDirectoryName(new Uri(AnimationBehavior.GetSourceUri(oc_Show).ToString()).LocalPath));
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Forms.Application.EnableVisualStyles();
            CyraliveOperaScript_init();
            if (File.Exists(res_folder + "\\config\\brand.txt"))
            {
                notifyIcon.Text = File.ReadAllText(res_folder + "\\config\\brand.txt");
            }
            else
            {
                notifyIcon.Text = "OpenCyralive";
            }
            notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    oc_cm.IsOpen = true;
                }
                else
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        window.Visibility = Visibility.Visible;
                    }
                    if (File.Exists(res_folder + "\\lines\\" + oc_Show_character_name() + "\\activate.json"))
                    {
                        Cierra_hover_text.Markdown = get_message(res_folder + "\\lines\\" + oc_Show_character_name() + "\\activate.json");
                        get_msg_trigger();
                    }
                    Cierra_hover_text_grid.Visibility = Visibility.Visible;
                }
            };
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Visible = true;
            try
            {
                foreach (string folder_path in Directory.GetDirectories(res_folder + "\\characters"))
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = Path.GetFileName(folder_path);
                    menuItem.Click += (s, e) =>
                    {
                        character_images.Clear();
                        foreach (string file_path in Directory.GetFiles(res_folder + "\\characters\\" + menuItem.Header))
                        {
                            character_images.Add(file_path);
                        }
                        character_change(Directory.GetFiles(folder_path, "normal.*").First());
                        write_config_file(res_folder + "\\config\\config.json", "Character", oc_Show_character_name());
                        if (File.Exists(res_folder + "\\images\\trayicon\\" + read_config_file(res_folder + "\\config\\config.json", "Character") + "\\trayicon.ico"))
                        {
                            notifyIcon.Icon = new Icon(res_folder + "\\images\\trayicon\\" + read_config_file(res_folder + "\\config\\config.json", "Character") + "\\trayicon.ico");
                        }
                        else
                        {
                            notifyIcon.Icon = new Icon(res_folder + "\\images\\trayicon\\trayicon.ico");
                        }
                        if (File.Exists(res_folder + "\\images\\appicon\\" + read_config_file(res_folder + "\\config\\config.json", "Character") + "\\appicon.ico"))
                        {
                            Icon = new BitmapImage(new Uri(res_folder + "\\images\\appicon\\" + read_config_file(res_folder + "\\config\\config.json", "Character") + "\\appicon.ico", UriKind.RelativeOrAbsolute));
                        }
                        else
                        {
                            Icon = new BitmapImage(new Uri(res_folder + "\\images\\appicon\\appicon.ico", UriKind.RelativeOrAbsolute));
                        }
                    };
                    mi_characters.Items.Add(menuItem);
                }
                if (Directory.Exists(res_folder + "\\plugins") && Directory.GetDirectories(res_folder + "\\plugins").Length > 0)
                {
                    foreach (string folder_path in Directory.GetDirectories(res_folder + "\\plugins"))
                    {
                        //Assembly assembly = Assembly.LoadFrom(folder_path + "\\" + Path.GetFileName(folder_path) + ".dll");
                        AssemblyLoadContext assemblyLoadContext = new ocassemblylc();
                        assemblyLoadContext.Resolving += (context, assemblyName) =>
                        {
                            if (File.Exists(Directory.GetCurrentDirectory() + "\\" + folder_path + "\\" + assemblyName.Name + ".dll"))
                            {
                                return context.LoadFromStream(new MemoryStream(File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\" + folder_path + "\\" + assemblyName.Name + ".dll")));
                            }
                            return null;
                        };
                        Assembly assembly = assemblyLoadContext.LoadFromStream(new MemoryStream(File.ReadAllBytes(Directory.GetCurrentDirectory() + "\\" + folder_path + "\\" + Path.GetFileName(folder_path) + ".dll")));
                        MenuItem menuItem = new MenuItem();
                        foreach (Type type in assembly.GetExportedTypes())
                        {
                            if (type.Name == "plugin_base")
                            {
                                menuItem.Header = type.InvokeMember("pluginName", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null) as string;
                            }
                        }
                        menuItem.Click += (s, e) =>
                        {
                            foreach (Type type in assembly.GetExportedTypes())
                            {
                                if (type.Name == "plugin_base")
                                {
                                    try
                                    {
                                        if (type.InvokeMember("IsWidget", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null) != null && (bool)type.InvokeMember("IsWidget", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null))
                                        {
                                            object obj = Activator.CreateInstance(assembly.GetType(assembly.GetName().Name + ".WidgetWindow"));
                                            Window window = (Window)obj;
                                            window.Left = Left + oc_Stage.ActualWidth - Cierra_hover_text_border.Width;
                                            window.Top = Top + Height / 2;
                                            window.Loaded += (s, e) =>
                                            {
                                                Background = Brushes.Transparent;
                                            };
                                            window.Closed += (s, e) =>
                                            {
                                                if (read_config_file(res_folder + "\\config\\config.json", "TransparentWindow") != "Yes")
                                                {
                                                    Background = (Brush)new BrushConverter().ConvertFromString("#01FFFFFF");
                                                }
                                            };
                                            if (Topmost)
                                            {
                                                window.Topmost = true;
                                            }
                                            window.Show();
                                        }
                                    }
                                    catch
                                    {
                                        type.InvokeMember("pluginStart", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null);
                                    }
                                }
                            }
                        };
                        Cyralive_plugins.Items.Add(menuItem);
                    }
                }
                else
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = "没有插件";
                    menuItem.IsEnabled = false;
                    Cyralive_plugins.Items.Add(menuItem);
                }
                ocConfig = JsonNode.Parse(File.ReadAllText(res_folder + "\\config\\config.json"));
                if (ocConfig["Character"].ToString() == null || ocConfig["Character"].ToString() == "")
                {
                    character_change(Directory.GetFiles(Directory.GetDirectories(res_folder + "\\characters").First(), "normal.*").First());
                }
                else
                {
                    character_change(Directory.GetFiles(res_folder + "\\characters\\" + ocConfig["Character"].ToString(), "normal.*").First());
                }
                if (ocConfig["Character"].ToString() != "" && File.Exists(res_folder + "\\images\\trayicon\\" + ocConfig["Character"] + "\\trayicon.ico"))
                {
                    notifyIcon.Icon = new Icon(res_folder + "\\images\\trayicon\\" + ocConfig["Character"].ToString() + "\\trayicon.ico");
                }
                else if (File.Exists(res_folder + "\\images\\trayicon\\" + strings.Last() + "\\trayicon.ico"))
                {
                    notifyIcon.Icon = new Icon(res_folder + "\\images\\trayicon\\" + strings.Last() + "\\trayicon.ico");
                }
                else
                {
                    notifyIcon.Icon = new Icon(res_folder + "\\images\\trayicon\\trayicon.ico");
                }
                if (ocConfig["Character"].ToString() != "" && File.Exists(res_folder + "\\images\\appicon\\" + ocConfig["Character"].ToString() + "\\appicon.ico"))
                {
                    Icon = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\" + res_folder + "\\images\\appicon\\" + ocConfig["Character"].ToString() + "\\appicon.ico", UriKind.RelativeOrAbsolute));
                }
                else if (File.Exists(res_folder + "\\images\\appicon\\" + strings.Last() + "\\appicon.ico"))
                {
                    Icon = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\" + res_folder + "\\images\\appicon\\" + strings.Last() + "\\appicon.ico", UriKind.RelativeOrAbsolute));
                }
                else
                {
                    Icon = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + "\\" + res_folder + "\\images\\appicon\\appicon.ico", UriKind.RelativeOrAbsolute));
                }
                if (ocConfig["Hemi"].ToString() != "South")
                {
                    month = DateTime.Now.Month;
                }
                else
                {
                    if (DateTime.Now.Month + 6 > 12)
                    {
                        month = DateTime.Now.Month - 6;
                    }
                    else
                    {
                        month = DateTime.Now.Month + 6;
                    }
                }
                if (Directory.Exists(res_folder + "\\lines\\" + oc_Show_character_name() + "\\startup"))
                {
                    Cierra_hover_text.Markdown = get_message(res_folder + "\\lines\\" + oc_Show_character_name() + "\\startup\\" + month + ".json");
                    get_msg_trigger();
                }
                else
                {
                    Cierra_hover_text_grid.Visibility = Visibility.Hidden;
                }
                if (ocConfig["WindowXY"].ToString() == null || ocConfig["WindowXY"].ToString() == "")
                {
                    Top = SystemParameters.PrimaryScreenHeight - Height - SystemParameters.PrimaryScreenHeight * 0.1;
                    Left = SystemParameters.PrimaryScreenWidth - Width - SystemParameters.PrimaryScreenWidth * 0.03;
                }
                else
                {
                    get_position = Regex.Split(ocConfig["WindowXY"].ToString(), ",");
                    Top = Convert.ToDouble(get_position[1]);
                    Left = Convert.ToDouble(get_position[0]);
                    chara_hold_position = true;
                }
                if (ocConfig["WindowSize"].ToString() != "")
                {
                    get_size = Regex.Split(ocConfig["WindowSize"].ToString(), ",");
                    Height = Convert.ToDouble(get_size[0]);
                    Width = Convert.ToDouble(get_size[1]);
                }
                if (ocConfig["Topmost"].ToString() != "Yes")
                {
                    Topmost = false;
                }
                if (ocConfig["TransparentWindow"].ToString() == "Yes")
                {
                    Background = Brushes.Transparent;
                }
                if (ocConfig["Translucent"].ToString() == "Yes")
                {
                    oc_Show.Opacity = 0.5;
                }
                if (ocConfig["Taskbar"].ToString() == "Yes")
                {
                    ShowInTaskbar = true;
                }
                if (ocConfig["Bubble_bg"].ToString() != "")
                {
                    Cierra_hover_text_border.Background = (Brush)new BrushConverter().ConvertFromString(ocConfig["Bubble_bg"].ToString());
                    Cierra_hover_text.Background = (Brush)new BrushConverter().ConvertFromString(ocConfig["Bubble_bg"].ToString());
                }
                if (ocConfig["Bubble_fg"].ToString() != "")
                {
                    Cierra_hover_text.Foreground = (Brush)new BrushConverter().ConvertFromString(ocConfig["Bubble_fg"].ToString());
                }
                if (ocConfig["Bubble_brd"].ToString() != "")
                {
                    Cierra_hover_text_border.BorderBrush = (Brush)new BrushConverter().ConvertFromString(ocConfig["Bubble_brd"].ToString());
                }
                if (ocConfig["Bubble_font_Bold"].ToString() != "")
                {
                    Cierra_hover_text.FontWeight = FontWeights.Bold;
                }
                if (ocConfig["Bubble_font_Italic"].ToString() != "")
                {
                    Cierra_hover_text.FontStyle = FontStyles.Italic;
                }
                if (ocConfig["Culture"].ToString() != "")
                {
                    try
                    {
                        FileStream fileStream;
                        if (File.Exists(res_folder + "\\lang\\" + CultureInfo.CurrentCulture + ".xaml"))
                        {
                            fileStream = new FileStream(res_folder + "\\lang\\" + CultureInfo.CurrentCulture + ".xaml", FileMode.Open);
                        }
                        else
                        {
                            fileStream = new FileStream(res_folder + "\\lang\\zh-CN.xaml", FileMode.Open);
                        }
                        FileStream fileStream1 = new FileStream(res_folder + "\\lang\\" + ocConfig["Culture"].ToString() + ".xaml", FileMode.Open);
                        Application.Current.Resources.MergedDictionaries.Remove((ResourceDictionary)XamlReader.Load(fileStream));
                        Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(fileStream1));
                        fileStream.Close();
                        fileStream1.Close();
                    }
                    catch
                    {

                    }
                }
                if (File.Exists(res_folder + "\\config\\brand.txt"))
                {
                    Title = File.ReadAllText(res_folder + "\\config\\brand.txt");
                }
                DoubleAnimation doubleAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(700)));
                doubleAnimation.Completed += (s, e) =>
                {
                    fullyStarted = true;
                };
                OCview.BeginAnimation(OpacityProperty, doubleAnimation);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        void get_msg_trigger()
        {
            if (fontSize != 0)
            {
                Cierra_hover_text.Document.FontSize = fontSize;
            }
            else
            {
                if (ocConfig["Bubble_font_size"].ToString() != "")
                {
                    Cierra_hover_text.Document.FontSize = Convert.ToDouble(ocConfig["Bubble_font_size"].ToString());
                }
                else
                {
                    Cierra_hover_text.Document.FontSize = 13;
                }
            }
            if (fontFamily != null)
            {
                Cierra_hover_text.Document.FontFamily = fontFamily;
            }
            else
            {
                if (ocConfig["Bubble_font"].ToString() != "")
                {
                    Cierra_hover_text.Document.FontFamily = new FontFamily(ocConfig["Bubble_font"].ToString());
                }
                else
                {
                    Cierra_hover_text.Document.FontFamily = new FontFamily("Microsoft Yahei");
                }
            }
            Clipboard.ContentChanged += (s, e) =>
            {
                if (File.Exists(res_folder + "\\lines\\" + oc_Show_character_name() + "\\clipboard.json"))
                {
                    JsonNode jsonNode = JsonNode.Parse(File.ReadAllText(res_folder + "\\lines\\" + oc_Show_character_name() + "\\clipboard.json"));
                    JsonArray clipboardReactionMessages = (JsonArray)jsonNode["messages"];
                    if (System.Windows.Clipboard.ContainsText())
                    {
                        foreach (string txt in ((JsonArray)jsonNode["keywords"]).AsArray())
                        {
                            if (System.Windows.Clipboard.GetText().Contains(txt))
                            {
                                Cierra_hover_text_grid.Visibility = Visibility.Visible;
                                Cierra_hover_text.Markdown = clipboardReactionMessages[new Random().Next(0, clipboardReactionMessages.Count)].ToString();
                                get_msg_trigger();
                                break;
                            }
                        }
                    }
                }
            };
        }

        private void mi_exit_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(700)));
            doubleAnimation.Completed += (s, e) =>
            {
                notifyIcon.Dispose();
                foreach (Window window in Application.Current.Windows)
                {
                    window.Close();
                }
            };
            OCview.BeginAnimation(OpacityProperty, doubleAnimation);
        }

        private void oc_Show_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Cursor == Cursors.SizeAll)
            {
                DragMove();
            }
            else
            {
                try
                {
                    if (character_images.Count > 1)
                    {
                        character_change(character_images[new Random().Next(0, character_images.Count)]);
                    }
                    Cierra_hover_text_grid.Visibility = Visibility.Visible;
                    Cierra_hover_text.Markdown = get_message(res_folder + "\\lines\\" + oc_Show_character_name() + "\\" + character_status() + ".json");
                    get_msg_trigger();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mi_drag_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.SizeAll;
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Cursor == Cursors.SizeAll)
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Name.StartsWith("CyraliveWidget"))
                {
                    window.Left = Left + oc_Stage.ActualWidth - Cierra_hover_text_border.Width;
                    window.Top = Top + Height / 2;
                }
            }
            Timer timer = new Timer(1000);
            timer.Elapsed += (s, e2) =>
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, () =>
                {
                    if (Mouse.LeftButton == MouseButtonState.Released)
                    {
                        timer.Stop();
                        Cursor = Cursors.Arrow;
                        if (chara_hold_position)
                        {
                            if (Left != Convert.ToDouble(get_position[0]) || Top != Convert.ToDouble(get_position[1]))
                            {
                                write_config_file(res_folder + "\\config\\config.json", "WindowXY", Left + "," + Top);
                            }
                        }
                    }
                });
            };
            timer.Enabled = true;
        }

        private void oc_Show_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (read_config_file(res_folder + "\\config\\config.json", "Translucent") == "Yes")
            {
                oc_Show.Opacity = 0.5;
            }
        }

        public static string schedule_reader()
        {
            JsonDocument schedule_read = JsonDocument.Parse(File.ReadAllText("resources/config/time.json"));
            return schedule_read.RootElement.GetProperty("clock")[DateTime.Now.Hour].ToString();
        }

        private void oc_Show_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (hover_text_override)
            {
                hover_text_override = false;
            }
            else
            {
                Cierra_hover_text_grid.Visibility = Visibility.Visible;
                try
                {
                    if (Directory.Exists(res_folder + "\\lines\\" + oc_Show_character_name() + "\\hover"))
                    {
                        Cierra_hover_text.Markdown = get_message(res_folder + "\\lines\\" + oc_Show_character_name() + "\\hover\\" + schedule_reader() + ".json");
                    }
                    else
                    {
                        Cierra_hover_text.Markdown = get_message(res_folder + "\\lines\\" + oc_Show_character_name() + "\\" + character_status() + ".json");
                    }
                    get_msg_trigger();
                    if (read_config_file(res_folder + "\\config\\config.json", "Translucent") == "Yes")
                    {
                        oc_Show.Opacity = 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void mi_minimize_tb_Click(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Hidden && !ShowInTaskbar)
            {
                MessageBox.Show("不能同时最小化到任务栏和托盘", "提示", 0, MessageBoxIcon.Information);
            }
            else
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow) || window.Name.StartsWith("CyraliveWidget"))
                    {
                        window.ShowInTaskbar = true;
                        window.Visibility = Visibility.Visible;
                        window.WindowState = WindowState.Minimized;
                    }
                }
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (read_config_file(res_folder + "\\config\\config.json", "Taskbar") != "Yes")
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(MainWindow) || window.Name.StartsWith("CyraliveWidget"))
                    {
                        window.ShowInTaskbar = false;
                    }
                }
            }
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Name.StartsWith("CyraliveWidget"))
                {
                    window.WindowState = WindowState.Normal;
                }
            }
        }

        private void mi_minimize_tray_Click(object sender, RoutedEventArgs e)
        {
            if (Visibility == Visibility.Visible && WindowState == WindowState.Minimized)
            {
                MessageBox.Show("不能同时最小化到任务栏和托盘", "提示", 0, MessageBoxIcon.Information);
            }
            else
            {
                if (read_config_file("resources/config/config.json", "Taskbar") == "Yes")
                {
                    ShowInTaskbar = true;
                }
                foreach (Window window in Application.Current.Windows)
                {
                    window.Visibility = Visibility.Hidden;
                }
            }
        }

        private void mi_settings_Click(object sender, RoutedEventArgs e)
        {
            new ocSettings().Show();
        }

        private void oc_Show_Drop(object sender, System.Windows.DragEventArgs e)
        {
            hover_text_override = true;
            string[] fileDrops = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
            if (Directory.Exists(res_folder + "\\lines\\" + oc_Show_character_name() + "\\dragdrop"))
            {
                try
                {
                    Cierra_hover_text.Markdown = get_message(res_folder + "\\lines\\" + oc_Show_character_name() + "\\dragdrop\\" + Path.GetExtension(fileDrops.First()).TrimStart('.') + ".json");
                }
                catch
                {
                    Cierra_hover_text.Markdown = get_message(res_folder + "\\lines\\" + oc_Show_character_name() + "\\dragdrop\\dragdrop.json");
                }
                get_msg_trigger();
                Cierra_hover_text_grid.Visibility = Visibility.Visible;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else if (Height > 429 || Width > 442)
            {
                Width = 340;
                Height = 315;
            }
            if (fullyStarted)
            {
                OCWindow.BorderBrush = Brushes.Aquamarine;
                OCWindow.BorderThickness = new Thickness(1);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 5);
                dispatcherTimer.Tick += (s, e) =>
                {
                    OCWindow.ClearValue(BorderBrushProperty);
                    OCWindow.ClearValue(BorderThicknessProperty);
                    if (read_config_file(res_folder + "\\config\\config.json", "WindowSize") != "")
                    {
                        write_config_file(res_folder + "\\config\\config.json", "WindowSize", Height + "," + Width);
                    }
                    dispatcherTimer.Stop();
                };
                dispatcherTimer.Start();
            }
        }
    }
}
