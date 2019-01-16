using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PAObfuscator
{
    public delegate void SendDebugMessageDelegate(string caller, string message, string color="0x000000");

    internal class Obfuscator
    {
        public string missionPath;
        public string missionName;
        public string obfuPath;
        public string obfuName;

        public event SendDebugMessageDelegate OnSendDebug;

        private MacroHandler macroHandler = new MacroHandler();

        public Obfuscator(string arg)
        {
            missionPath = arg;
            missionName = Path.GetFileName(arg);
            obfuPath = Directory.GetParent(missionPath).FullName + "\\" + missionName + "_Obfu"; //Path.Combine(Config.Instance.ExportFolder, missionName);
            obfuName = missionName + "_Obfu";
            if (Directory.Exists(obfuPath))
                Directory.Delete(obfuPath, true);

            Directory.CreateDirectory(obfuPath);
        }

        public void Obfuscate(string file)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string fileExt = Path.GetExtension(file);
            string filePath = Path.GetDirectoryName(file);

            string workPath = filePath.Replace(missionName, obfuName);
            string workFile = workPath + "\\" + fileName + fileExt;

            OnSendDebug?.Invoke("Obfuscator", fileName + fileExt + " wird verarbeitet.");

            if (!Directory.Exists(workPath))
            {
                Directory.CreateDirectory(workPath);
            }

            if (File.Exists(workFile))
            {
                File.Delete(workFile);
            }
            if (fileExt.ToLower() == ".sqf")
            {
                SQF(file, workFile);
                OnSendDebug?.Invoke("Obfuscator",fileName + fileExt + " wurde sqfObfu");
            }
            else
            {
                File.Copy(file, workFile);
                OnSendDebug?.Invoke("Obfuscator", fileName + fileExt + " wurde kopiert");
            }


        }

        public void Obfuscate(string[] files)
        {
            foreach (String file in files)
                Obfuscate(file);
        }

        private void SQF(string file, string workFile)
        {
            var blockComments = @"/\*(.*?)\*/";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";
            StreamWriter writer = new StreamWriter(workFile);

            string input = macroHandler.HandleMacros(file);
            //schnittstelle für MACRO!

            string edited = Regex.Replace(input,
                blockComments + "|" + lineComments + "|" + strings + "|" + verbatimStrings,
                match => {
                    if (match.Value.StartsWith("/*") || match.Value.StartsWith("//"))
                        return match.Value.StartsWith("//") ? "" : "";
                    // Keep the literal strings
                    return match.Value;
                },
                RegexOptions.Singleline).Replace(Environment.NewLine, "");
            writer.Write(edited);
            writer.Close();
            writer = null;
        }

    }
}
