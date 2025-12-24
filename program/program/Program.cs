using Lyricify.Lyrics.Helpers;
using Lyricify.Lyrics.Models;
using Lyricify.Lyrics.Searchers;
using Lyricify.Lyrics.Generators;
using Lyricify.Lyrics.Parsers;
using System.Text.RegularExpressions;
var searcher = new QQMusicSearcher();

string videoId, title, artist;
int skipLines = 0;
int lrcSkipLines = 0;
if (args.Length >= 4)
{
    videoId = args[0];
    title = args[1];
    artist = args[2];
    skipLines = int.Parse(args[3]);
    if (args.Length >= 5) lrcSkipLines = int.Parse(args[4]);
}
else
{
    Console.WriteLine("Arguments missing. Please input manually.");
    Console.WriteLine("VideoId:");
    videoId = Console.ReadLine() ?? "";
    Console.WriteLine("Title:");
    title = Console.ReadLine() ?? "";
    Console.WriteLine("Artist:");
    artist = Console.ReadLine() ?? "";
    Console.WriteLine("Skip Lines (QRC):");
    skipLines = int.Parse(Console.ReadLine() ?? "0");
    Console.WriteLine("Skip Lines (LRC):");
    lrcSkipLines = int.Parse(Console.ReadLine() ?? "0");
}
var hit = await searcher.SearchForResult(new TrackMultiArtistMetadata
{
    Title = title,
    Artists = new() { artist },
});

var qq = hit as QQMusicSearchResult;
if (qq == null) { Console.WriteLine("QQMusicSearchResult is null"); return; }

var qrc = await ProviderHelper.QQMusicApi.GetLyricsAsync(qq.Id);
if(qrc?.Lyrics == null){Console.WriteLine("QRC Lyric is null"); return;}
var lyricsData = ParseHelper.ParseLyrics(qrc.Lyrics, LyricsRawTypes.Qrc);
if(lyricsData == null){Console.WriteLine("Parsed LyricsData is null"); return;}
string jsonOutput = CustomJsonGenerator.Generate(lyricsData, videoId, skipLines);
File.WriteAllText("lyrics.json", jsonOutput);
Console.WriteLine("JSON output saved to lyrics.json");
Console.WriteLine(jsonOutput);

// LRCっぽい方が欲しい場合: song Mid を使う
var lrc = await ProviderHelper.QQMusicApi.GetLyric(qq.Mid);
if(lrc != null && lrc.Lyric != null)
{
    var timeTagRegex = new Regex(@"^\[\d{1,2}:\d{2}(\.\d{1,3})?\]");
    var lrcLines = lrc.Lyric
        .Replace("\r\n", "\n")
        .Split('\n')
        .Select(x => x.TrimEnd('\r'))
        .Where(x => timeTagRegex.IsMatch(x))
        .Skip(lrcSkipLines)
        .ToArray();
    var lrcOutput = string.Join("\n", lrcLines);
    File.WriteAllText("lyrics.lrc", lrcOutput);
    Console.WriteLine("LRC output saved to lyrics.lrc");
    Console.WriteLine(lrcOutput);
}
else{Console.WriteLine("LRC Lyric is null");}
