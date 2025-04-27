using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace CyralivePluginMgmt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> strings = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            System.Windows.Forms.Application.EnableVisualStyles();
            if (Directory.Exists("..\\resources\\plugins") && Directory.GetDirectories("..\\resources\\plugins").Length > 0)
            {
                foreach (string folder_path in Directory.GetDirectories("..\\resources\\plugins"))
                {
                    strings.Add(folder_path);
                    Assembly assembly = Assembly.Load(File.ReadAllBytes(folder_path + "\\" + Regex.Split(folder_path, @"\\").Last() + ".dll"));
                    ListViewItem listViewItem = new ListViewItem();
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (type.Name == "plugin_base")
                        {
                            listViewItem.Content = type.InvokeMember("pluginName", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null) as string;
                        }
                    }
                    plugin_list.Items.Add(listViewItem);
                }
            }
        }

        private void close_plugin_mgmt_Click(object sender, RoutedEventArgs e)
        {
            /*Process process = new Process();
            process.StartInfo.FileName = Directory.GetCurrentDirectory() + "\\..\\OpenCyralive.exe";
            process.Start();*/
            Close();
        }

        private void del_plugin_Click(object sender, RoutedEventArgs e)
        {
            if (plugin_list.SelectedItem != null)
            {
                try
                {
                    Directory.Delete(strings[plugin_list.SelectedIndex], true);
                    strings.Clear();
                    foreach (string folder_path in Directory.GetDirectories("..\\resources\\plugins"))
                    {
                        strings.Add(folder_path);
                    }
                    MessageBox.Show((string)((ListViewItem)plugin_list.SelectedItem).Content + " 已被删除。", "操作成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    plugin_list.Items.Remove(plugin_list.SelectedItem);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void install_oc_plugin_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".zip";
            openFileDialog.Filter = "ZIP Folders (.ZIP)|*.zip";
            if (openFileDialog.ShowDialog().Value)
            {
                try
                {
                    string[] oc_import_plugin_path = Regex.Split(openFileDialog.FileName, "\\\\");
                    ZipFile.ExtractToDirectory(openFileDialog.FileName, "..\\resources", true);
                    plugin_list.Items.Clear();
                    strings.Clear();
                    foreach (string folder_path in Directory.GetDirectories("..\\resources\\plugins"))
                    {
                        strings.Add(folder_path);
                        Assembly assembly = Assembly.Load(File.ReadAllBytes(folder_path + "\\" + Regex.Split(folder_path, @"\\").Last() + ".dll"));
                        ListViewItem listViewItem = new ListViewItem();
                        foreach (Type type in assembly.GetExportedTypes())
                        {
                            if (type.Name == "plugin_base")
                            {
                                listViewItem.Content = type.InvokeMember("pluginName", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null) as string;
                            }
                        }
                        plugin_list.Items.Add(listViewItem);
                    }
                    MessageBox.Show(Regex.Replace(oc_import_plugin_path.Last(), "\\.zip", "") + " 插件已安装。", "操作成功", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }
    }
}