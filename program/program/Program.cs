using Lyricify.Lyrics.Helpers;
using Lyricify.Lyrics.Models;
using Lyricify.Lyrics.Searchers;
using Lyricify.Lyrics.Generators;
using Lyricify.Lyrics.Parsers;
var searcher = new QQMusicSearcher();

string videoId, title, artist;

if (args.Length == 3)
{
    videoId = args[0];
    title = args[1];
    artist = args[2];
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
string jsonOutput = CustomJsonGenerator.Generate(lyricsData, videoId);
Console.WriteLine(jsonOutput);
// LRCっぽい方が欲しい場合: song Mid を使う
var lrc = await ProviderHelper.QQMusicApi.GetLyric(qq.Mid);
if(lrc != null){Console.WriteLine(lrc?.Lyric);}
else{Console.WriteLine("LRC Lyric is null");}
