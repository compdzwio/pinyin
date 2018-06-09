using System;
using System.Collections.Generic;
using System.IO;

namespace Overtrue.Pinyin
{
    public class FileDictLoader : DictLoaderInterface
    {
        /**
         * Words segment name
         */
        protected string segmentName = "words_{0}.txt";

        /**
         * Dict path
         */
        protected string path;

        public FileDictLoader(string path)
        {
            this.path = path;
        }

        public void Map(Action<Dictionary<string, string>> callback)
        {
            for (int i = 0; i < 100; i++)
            {
                string segment = this.path + "/" + String.Format(this.segmentName, i);
                if (File.Exists(segment))
                {
                    using (StreamReader sr = File.OpenText(segment))
                    {
                        string s = "";
                        Dictionary<string, string> dict = new Dictionary<string, string>();
                        while ((s = sr.ReadLine()) != null)
                        {
                            string[] keypair = s.Split(':');
                            dict.Add(keypair[0], keypair[1]);
                        }
                        callback(dict);
                    }
                }
            }
        }

        public void MapSurname(Action<Dictionary<string, string>> callback)
        {
            string segment = this.path + "/" + "surnames.txt";
            if (File.Exists(segment))
            {
                using (StreamReader sr = File.OpenText(segment))
                {
                    string s = "";
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] keypair = s.Split(':');
                        dict.Add(keypair[0], keypair[1]);
                    }
                    callback(dict);
                }
            }
        }
    }
}
