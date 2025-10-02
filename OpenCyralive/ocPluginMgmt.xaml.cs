using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using static OpenCyralive.GlobalFunction;
using Application = System.Windows.Application;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace OpenCyralive
{
    /// <summary>
    /// ocPluginMgmt.xaml 的交互逻辑
    /// </summary>
    public partial class ocPluginMgmt : Window
    {
        List<string> pluginDirs = new List<string>();
        public ocPluginMgmt()
        {
            InitializeComponent();
            if (Directory.Exists(res_folder + "\\plugins") && Directory.GetDirectories(res_folder + "\\plugins").Length > 0)
            {
                foreach (string folder_path in Directory.GetDirectories(res_folder + "\\plugins"))
                {
                    pluginDirs.Add(folder_path);
                    Assembly assembly = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\" + folder_path + "\\" + Path.GetFileName(folder_path) + ".dll");
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (type.Name == "plugin_base")
                        {
                            ListViewItem listViewItem = new ListViewItem();
                            listViewItem.Content = type.InvokeMember("pluginName", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), null);
                            oc_plugins.Items.Add(listViewItem);
                        }
                    }
                }
            }
        }

        private void oc_plugin_modify_Click(object sender, RoutedEventArgs e)
        {
            if (oc_plugins.SelectedIndex != -1)
            {
                var dialogResult = MessageBox.Show(Application.Current.FindResource("msg_plugin_mgmt").ToString(), Application.Current.FindResource("msg_info").ToString(), MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    notifyIcon.Dispose();
                    openThings("explorer.exe", "/select," + Directory.GetCurrentDirectory() + "\\" + pluginDirs[oc_plugins.SelectedIndex]);
                    foreach (Window window in Application.Current.Windows)
                    {
                        window.Close();
                    }
                }
            }
        }

        private void oc_plugin_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void oc_plugin_install_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".zip";
            openFileDialog.Filter = "Zip files (*.zip)|*.zip";
            if (openFileDialog.ShowDialog().Value)
            {
                try
                {
                    ZipFile.ExtractToDirectory(openFileDialog.FileName, res_folder + "\\plugins", true);
                    MessageBox.Show("插件安装成功，桌宠重启后生效", "插件安装成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    notifyIcon.Dispose();
                    Application.Current.Shutdown();
                    openThings(Assembly.GetExecutingAssembly().GetName().Name + ".exe", "");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
