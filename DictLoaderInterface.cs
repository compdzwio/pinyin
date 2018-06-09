using System;
using System.Collections.Generic;

namespace Overtrue.Pinyin
{
    public interface DictLoaderInterface
    {
        void Map(Action<Dictionary<string, string>> callback);
        void MapSurname(Action<Dictionary<string, string>> callback);
    }
}
