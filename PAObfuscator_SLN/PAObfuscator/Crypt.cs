using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PAObfuscator
{
    internal class Crypt
    {
        private Random random = new Random();
        public string cryptKey;
        private List<int> lcryptKey;

        public event SendDebugMessageDelegate OnSendDebug;

        public Crypt(string decryptPath)
        {
            cryptKey = RandomString(64);
            lcryptKey = toArray(cryptKey);
            OnSendDebug?.Invoke("[Crypter]: ","CryptKey: " + cryptKey,"0x0000ff");
            string bakPath = Path.GetDirectoryName(decryptPath) + "\\" + Path.GetFileNameWithoutExtension(decryptPath) + ".bak";
            if (File.Exists(bakPath))
            {
                File.Delete(bakPath);
            }
            File.Copy(decryptPath, bakPath);

            var regex = new Regex("private _key = .*;", RegexOptions.Multiline);
            string result = regex.Replace(File.ReadAllText(decryptPath), "private _key = \"" + cryptKey + "\";");
            File.WriteAllText(decryptPath, result);
        }

        private List<int> toArray(string input)
        {
            return input.Select(c => (int)c).ToList();
        }

        private string lCrypt(string input)
        {
            List<int> iArray = toArray(input);
            int kCount = lcryptKey.Count() - 1;
            int iCount = iArray.Count();
            for (int i = 0; i < iCount; i++)
            {
                iArray[i] = iArray[i] + (i % (lcryptKey[i % kCount])) + lcryptKey[kCount - (i % kCount)];
            }
            return toString(iArray);
        }

        public string cryptSqf(string path)
        {

            string cName = RandomString(32) + ".parpg";
            try
            {
                if (!File.Exists(path))
                    throw new Exception("Die Datei im Pfad " + path + " existiert nicht. Bitte überprüfe ob die Datei existiert und ob sie richtig in der CfgFunctions definiert ist.");
                string source = File.ReadAllText(path);
                string folder = Path.GetDirectoryName(path);
                
                string cPath = folder + "\\" + cName;
                File.Delete(path);

                while (File.Exists(cPath))
                {
                    cName = RandomString(32) + ".parpg";
                    cPath = folder + "\\" + cName;
                }
                File.WriteAllText(cPath, lCrypt(source));
                return cName;
            }
            catch (Exception ex)
            {
                OnSendDebug?.Invoke("[Crypter]: ", ex.Message, "0xff0000");
                cName = null;
            }
            return cName;
        }

        private string toString(List<int> input)
        {
            IEnumerable<char> ret = input.Select(c => (char)c);
            return String.Concat(ret);
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
