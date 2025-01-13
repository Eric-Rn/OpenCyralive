using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using static OpenCyralive.GlobalFunction;

namespace OpenCyralive
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (File.Exists(res_folder + "\\lang\\" + CultureInfo.CurrentCulture + ".xaml"))
            {
                FileStream fileStream = new FileStream(res_folder + "\\lang\\" + CultureInfo.CurrentCulture + ".xaml", FileMode.Open);
                Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(fileStream));
                fileStream.Close();
            }
            else
            {
                FileStream fileStream = new FileStream(res_folder + "\\lang\\zh-CN.xaml", FileMode.Open);
                Current.Resources.MergedDictionaries.Add((ResourceDictionary)XamlReader.Load(fileStream));
                fileStream.Close();
            }
        }
    }
}
