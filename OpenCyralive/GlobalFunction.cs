using System;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using static OpenCyralive.CyraliveOperaScript;
using MdXaml;
using MessageBox = System.Windows.Forms.MessageBox;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Linq;

namespace OpenCyralive
{
    class GlobalFunction
    {
        public static NotifyIcon notifyIcon = new NotifyIcon(new System.ComponentModel.Container());
        public static string[] get_position;
        public static bool oc_hold_position = false;
        public static JsonNode ocConfig;
        public static string res_folder = "resources";
        public static bool chara_hold_position;
        public static string[] get_size;
        public static int month;
        public static string[] strings = Regex.Split(Directory.GetDirectories(res_folder + "\\characters").First(), @"\\");
        public static Markdown markdown = new Markdown();
        public static FontFamily fontFamily;
        public static double fontSize = 0;
        public static void write_config_file(string file_path, string item, string value)
        {
            JsonNode modify_current_json = JsonNode.Parse(File.ReadAllText(file_path));
            modify_current_json[item] = value;
            File.WriteAllText(file_path, modify_current_json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }

        public static string get_message(string file_path)
        {
            try
            {
                JsonDocument message_texts = JsonDocument.Parse(File.ReadAllText(file_path));
                string message_text = message_texts.RootElement.GetProperty("messages")[RandomNumberGenerator.GetInt32(message_texts.RootElement.GetProperty("messages").GetArrayLength())].ToString();
                string final_text = string.Empty;
                bool msgorfinal()
                {
                    bool isvarexists = false;
                    foreach (string str in CyraliveOperaScriptVar)
                    {
                        if (message_text.Contains(str))
                        {
                            isvarexists = true;
                            break;
                        }
                    }
                    return isvarexists;
                }
                if (msgorfinal())
                {
                    foreach (string str in CyraliveOperaScriptVar)
                    {
                        if (message_text.Contains(str))
                        {
                            final_text = message_text.Replace(str, CyraliveOperaScriptVarVal[CyraliveOperaScriptVar.IndexOf(str)]);
                        }
                    }
                    return final_text;
                }
                else
                {
                    return message_text;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static string read_config_file(string file_path, string item)
        {
            JsonDocument getCyraliveConfig = JsonDocument.Parse(File.ReadAllText(file_path));
            return getCyraliveConfig.RootElement.GetProperty(item).ToString();
        }

        public static void openThings(string file_path, string args)
        {
            try
            {
                Process openprogram = new Process();
                openprogram.StartInfo.FileName = file_path;
                if (args != "")
                {
                    openprogram.StartInfo.Arguments = args;
                }
                openprogram.StartInfo.UseShellExecute = true;
                openprogram.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
