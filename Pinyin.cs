using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overtrue.Pinyin
{
    public class Pinyin
    {
        public const string NONE = "none";
        public const string ASCII = "ascii";
        public const string UNICODE = "unicode";
	
        /**
         * Dict loader.
         */
        protected DictLoaderInterface loader;

        private string loaderName;

        /**
         * Punctuations map.
         */
        protected Dictionary<string, string> punctuations = new Dictionary<string, string>() {
            {"，", ","}, 
            {"。", "."},
            {"！", "!"},
            {"？", "?"},
            {"：", ":"},
            {"“", "\""},
            {"”", "\""},
            {"‘", "'"},
            {"’", "'"}
        };

        public Pinyin(string loaderName = null)
        {
            if (loaderName == null)
            {
                this.loaderName = "Overtrue.Pinyin.FileDictLoader";
            }
        }

        /**
         * Convert string to pinyin.
         */
        public string[] Convert(string str, string option = Pinyin.NONE)
        {
            string pinyin = this.Romanize(str);

            return this.SplitWords(pinyin, option);
        }

        /**
         * Convert string (person name) to pinyin.
         */
        public string[] Name(string stringName, string option = Pinyin.NONE)
        {
            string pinyin = this.Romanize(stringName, true);

            return this.SplitWords(pinyin, option);
        }

        /**
         * Return a pinyin permalink from string.
         */
        public string Permalink(string str, string delimiter = "-")
        {
            if (Array.IndexOf(new string[] {"_", "-", ".", ""}, delimiter) == -1)
            {
                throw new ArgumentException("Delimiter must be one of: '_', '-', '.', ''");
            }

            return String.Join(delimiter, this.Convert(str));
        }

        /**
         * Return first letters
         */
        public string Abbr(string str, string delimiter = "")
        {
            return String.Join(delimiter, Array.ConvertAll(this.Convert(str),
                                                           new Converter<string, string>(delegate(string pinyin) {
                                                                   return pinyin[0].ToString();
                                                               }
                                                               )));
        }

        /**
         * Chinese phrase to pinyin.
         */
        public string Phrase(string str, string delimiter = " ", string option = Pinyin.NONE)
        {
            return String.Join(delimiter, this.Convert(str, option));
        }

        /**
         * Chinese to pinyin sentense
         */
        public string Sentence(string sentence, bool withTone = false)
        {
            List<string> marks = new List<string>(this.punctuations.Keys);
            marks.AddRange(this.punctuations.Values);
            string punctuationsRegex = Regex.Escape(String.Join("", marks.ToArray())).Replace("/", @"\/");
            string regex = "/[^üāēīōūǖáéíóúǘǎěǐǒǔǚàèìòùǜɑa-z0-9" + punctuationsRegex + "\\s_]+";
            string pinyin = Regex.Replace(this.Romanize(sentence), regex, "", RegexOptions.IgnoreCase);
            Dictionary<string, string> punctuations = new Dictionary<string, string>(this.punctuations);
            punctuations.Add("\t", " ");
            punctuations.Add(" ", " ");

            foreach (string key in punctuations.Keys)
            {
                pinyin = pinyin.Replace(key, punctuations[key]).Trim();
            }

            return withTone ? pinyin : this.Format(pinyin, false);
        }

        /**
         * Loader setter.
         */
        public Pinyin SetLoader(DictLoaderInterface loader)
        {
            this.loader = loader;
            return this;
        }

        /**
         * Return dict loader.
         */
        public DictLoaderInterface GetLoader()
        {
            if (this.loader == null)
            {
                string dataDir = Environment.CurrentDirectory + "/data/";

                string loaderName = this.loaderName;
                this.loader = (DictLoaderInterface) Activator.CreateInstance(Type.GetType(loaderName), dataDir);
            }

            return this.loader;
        }

        /**
         * Preprocess
         */
        protected string Prepare(string str)
        {
            str = Regex.Replace(str, "[a-z0-9_-]+", new MatchEvaluator(delegate(Match match)
            {
                return "\t" + match.Value[0];
            }), RegexOptions.IgnoreCase);

            return Regex.Replace(str, @"[^\p{Han}\p{P}\p{Z}\p{M}\p{N}\p{L}\t]", "", RegexOptions.CultureInvariant);
        }

        /**
         * Convert Chinese to pinyin
         */
        protected string Romanize(string str, bool isName = false)
        {
            str = this.Prepare(str);

            DictLoaderInterface dictLoader = this.GetLoader();

            if (isName)
            {
                str = this.ConvertSurname(str, dictLoader);
            }

            dictLoader.Map(delegate(Dictionary<string, string> dictionary)
                           {
                               foreach (string key in dictionary.Keys)
                               {
                                   str = str.Replace(key, dictionary[key]);
                               }
                           });

            return str;
        }

        /**
         * Convert Chinese Surname to pinyin.
         */
        protected string ConvertSurname(string str, DictLoaderInterface dictLoader)
        {
            dictLoader.MapSurname(delegate(Dictionary<string, string> dictionary)
                                  {
                                      foreach (string key in dictionary.Keys)
                                      {
                                          if (str.IndexOf(key) == 0)
                                          {
                                              str = dictionary[key] + str.Substring(key.Length);
                                              break;
                                          }
                                      }
                                  });

            return str;
        }

        /**
         * Split pinyin string to words.
         */
        public string[] SplitWords(string pinyin, string option)
        {
            string[] split = Regex.Split(pinyin, "[^üāēīōūǖáéíóúǘǎěǐǒǔǚàèìòùǜɑa-z\\d]+").Where(s => s != "" && s != null).ToArray();
            if (Pinyin.UNICODE != option)
            {
                for (int index = 0; index < split.Length; index++)
                {
                    split[index] = this.Format(pinyin, Pinyin.ASCII == option);
                }
            }

            return split;
        }

        /**
         * Format.
         */
        protected string Format(string pinyin, bool tone)
        {
            Dictionary<string, string[]> replacements = new Dictionary<string, string[]>() {
                {"üē", new string[] {"ue", "1"}}, {"üé", new string[] {"ue", "2"}}, {"üě", new string[] {"ue", "3"}}, {"üè", new string[] {"ue", "4"}}, {"ā", new string[] {"a", "1"}}, {"ē", new string[] {"e", "1"}}, {"ī", new string[] {"i", "1"}}, {"ō", new string[] {"o", "1"}}, {"ū", new string[] {"u", "1"}}, {"ǖ", new string[] {"v", "1"}}, {"á", new string[] {"a", "2"}}, {"é", new string[] {"e", "2"}}, {"í", new string[] {"i", "2"}}, {"ó", new string[] {"o", "2"}}, {"ú", new string[] {"u", "2"}}, {"ǘ", new string[] {"v", "2"}}, {"ǎ", new string[] {"a", "3"}}, {"ě", new string[] {"e", "3"}}, {"ǐ", new string[] {"i", "3"}}, {"ǒ", new string[] {"o", "3"}}, {"ǔ", new string[] {"u", "3"}}, {"ǚ", new string[] {"v", "3"}}, {"à", new string[] {"a", "4"}}, {"è", new string[] {"e", "4"}}, {"ì", new string[] {"i", "4"}}, {"ò", new string[] {"o", "4"}}, {"ù", new string[] {"u", "4"}}, {"ǜ", new string[] {"v", "4"}}
            };

            foreach (string key in replacements.Keys)
            {
                if (pinyin.IndexOf(key) != -1)
                {
                    pinyin = pinyin.Replace(key, replacements[key][0]) + (tone ? replacements[key][1] : "");
                }
            }

            return pinyin;
        }
    }
}
