using System;
using System.Collections.Generic;

namespace MainApp.SceneTypes;

public sealed class Info
{
    public void ListCollectionAndRequestAnyKeyPress()
    {
        Logger.Clear();
        _logger.WriteCached();

        if (_collectionWhichToList.Count == 0)
        {
            _logger.WriteLineToCache(Logger.Label.Error, _emptyCollectionErrMsg);
            return;
        }

        foreach (var item in _collectionWhichToList)
            _logger.WriteLine(item);

        _logger.WriteLine(Logger.Label.Tip, _lang.ActiveLanguagePack.Info_GoBackTipMsg);
        Console.ReadKey();
    }

    public Info(LanguageManager lang, Logger logger, List<string> collectionWhichToList, string emptyCollectionErrMsg)
    {
        _lang = lang;
        _logger = logger;
        _collectionWhichToList = collectionWhichToList;
        _emptyCollectionErrMsg = emptyCollectionErrMsg;
    }

    private readonly LanguageManager _lang;
    private readonly Logger _logger;
    private readonly List<string> _collectionWhichToList;
    private readonly string _emptyCollectionErrMsg;
}