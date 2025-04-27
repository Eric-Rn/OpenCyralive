using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static OpenCyralive.GlobalFunction;
using MessageBox = System.Windows.Forms.MessageBox;

namespace OpenCyralive
{
    /// <summary>
    /// ocCharaMgmt.xaml 的交互逻辑
    /// </summary>
    public partial class ocCharaMgmt : Window
    {
        string cur_chara_name;
        public ocCharaMgmt()
        {
            InitializeComponent();
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    string[] chara_name = Regex.Split((window as MainWindow).oc_Show.Source.ToString(), @"\/");
                    cur_chara_name = chara_name[chara_name.Length - 2];
                    cur_chara.Text = cur_chara.Text + cur_chara_name;
                }
            }
            foreach (string pathinfo in Directory.GetDirectories(res_folder + "\\characters"))
            {
                string[] chara_name = Regex.Split(pathinfo, @"\\");
                ListViewItem listViewItem = new ListViewItem();
                listViewItem.Content = chara_name[chara_name.Length - 1];
                oc_charas.Items.Add(listViewItem);
            }
        }

        private void close_occm_wnd_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void delete_chara_Click(object sender, RoutedEventArgs e)
        {
            if (oc_charas.SelectedItem != null)
            {
                if ((string)((ListViewItem)oc_charas.SelectedItem).Content == cur_chara_name)
                {
                    MessageBox.Show(Application.Current.FindResource("cannot_del_chara_info").ToString(), Application.Current.FindResource("cannot_del_chara").ToString(), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
                else
                {
                    Directory.Delete(res_folder + "\\characters\\" + (string)((ListViewItem)oc_charas.SelectedItem).Content, true);
                    Directory.Delete(res_folder + "\\lines\\" + (string)((ListViewItem)oc_charas.SelectedItem).Content, true);
                    if (Directory.Exists(res_folder + "\\images\\appicon\\" + (string)((ListViewItem)oc_charas.SelectedItem).Content))
                    {
                        Directory.Delete(res_folder + "\\images\\appicon\\" + (string)((ListViewItem)oc_charas.SelectedItem).Content, true);
                    }
                    if (Directory.Exists(res_folder + "\\images\\trayicon\\" + (string)((ListViewItem)oc_charas.SelectedItem).Content))
                    {
                        Directory.Delete(res_folder + "\\images\\trayicon\\" + (string)((ListViewItem)oc_charas.SelectedItem).Content, true);
                    }
                    MessageBox.Show((string)((ListViewItem)oc_charas.SelectedItem).Content + " " + Application.Current.FindResource("del_chara_msg").ToString(), Application.Current.FindResource("success_cn").ToString(), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    notifyIcon.Dispose();
                    Application.Current.Shutdown();
                    openThings(Assembly.GetExecutingAssembly().GetName().Name + ".exe", "");
                }
            }
        }

        private void import_chara_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".zip";
            openFileDialog.Filter = "ZIP Folders (.ZIP)|*.zip";
            if (openFileDialog.ShowDialog().Value)
            {
                try
                {
                    string[] oc_import_chara_path = Regex.Split(openFileDialog.FileName, "\\\\");
                    ZipFile.ExtractToDirectory(openFileDialog.FileName, res_folder, true);
                    MessageBox.Show(Regex.Replace(oc_import_chara_path[oc_import_chara_path.Length - 1], "\\.zip", "") + " " + Application.Current.FindResource("import_success_cn").ToString(), Application.Current.FindResource("success_cn").ToString(), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    notifyIcon.Dispose();
                    Application.Current.Shutdown();
                    openThings(Assembly.GetExecutingAssembly().GetName().Name + ".exe", "");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
        }
    }
}
