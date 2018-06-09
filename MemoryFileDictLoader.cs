using System;
using System.IO;
using System.Collections.Generic;

namespace Overtrue.Pinyin
{
    public class MemoryFileDictLoader : DictLoaderInterface
    {
        /**
         * Data directory.
         */
        protected string path;

        /**
         * Words segment name.
         */
        protected string segmentName = "words_{0}.txt";

        /**
         * Segment file contents.
         */
        protected List<Dictionary<string, string>> segments = new List<Dictionary<string, string>>();

        /**
         * Surname cache.
         */
        protected Dictionary<string, string> surnames = new Dictionary<string, string>();

        public MemoryFileDictLoader(string path)
        {
            this.path = path;
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
                        this.segments.Add(dict);
                    }
                }
            }
        }

        public void Map(Action<Dictionary<string, string>> callback)
        {
            foreach(Dictionary<string, string> dict in this.segments)
            {
                callback(dict);
            }
        }

        public void MapSurname(Action<Dictionary<string, string>> callback)
        {
            string segment = this.path + "/" + "surnames.txt";
            if (this.surnames.Count == 0) {
                if (File.Exists(segment))
                {
                    using (StreamReader sr = File.OpenText(segment))
                    {
                        string s = "";
                        
                        while ((s = sr.ReadLine()) != null)
                        {
                            string[] keypair = s.Split(':');
                            this.surnames.Add(keypair[0], keypair[1]);
                        }
                    }
                }
            }
            callback(this.surnames);
        }
    }
}
