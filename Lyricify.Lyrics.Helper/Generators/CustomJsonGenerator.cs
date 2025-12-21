using Lyricify.Lyrics.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Lyricify.Lyrics.Generators
{
    public static class CustomJsonGenerator
    {
        public static string Generate(LyricsData lyricsData, string videoId = "", int skipLines = 0)
        {
            var output = new
            {
                version = 1,
                videoId = videoId,
                meta = new
                {
                    track = lyricsData.TrackMetadata?.Title ?? "",
                    artist = lyricsData.TrackMetadata?.Artist ?? "",
                    generatedAt = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ")
                },
                lines = lyricsData.Lines?.Skip(skipLines).Select((line, index) => {
                    if (line is SyllableLineInfo syllableLine)
                    {
                        return new
                        {
                            index = index,
                            text = syllableLine.Text,
                            startTimeMs = syllableLine.StartTime ?? 0,
                            endTimeMs = syllableLine.EndTime ?? 0,
                            chars = syllableLine.Syllables.Select((s, i) => new
                            {
                                i = i,
                                c = s.Text,
                                t = s.StartTime
                            }).ToList()
                        };
                    }
                    return null;
                }).Where(x => x != null).ToList()
            };

            return JsonConvert.SerializeObject(output, Formatting.Indented);
        }
    }
}