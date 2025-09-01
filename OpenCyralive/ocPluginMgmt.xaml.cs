using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using static OpenCyralive.GlobalFunction;
using Application = System.Windows.Application;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.Forms.MessageBox;

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
                    Assembly assembly = Assembly.LoadFile(Directory.GetCurrentDirectory() + "\\" + folder_path + "\\" + Regex.Split(folder_path, @"\\").Last() + ".dll");
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
                    openThings(res_folder + "\\plugins", "");
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
    }
}
