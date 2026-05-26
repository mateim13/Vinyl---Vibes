using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using LibrarieModele;

namespace NivelStocareDate
{
    public static class DiscogsService
    {
        private static readonly HttpClient _http = new HttpClient();
        private const string Token     = "mhnrYfxnrRPusAUwfWGMVykmWZFmEidlpUMmFEwF";
        private const string UserAgent = "VinylVibes/1.0";

        static DiscogsService()
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Discogs", $"token={Token}");
        }

        public static async Task<Melodie[]> ImporteazaTracklist(string artist, string titlu, int an)
        {
            string searchUrl =
                $"https://api.discogs.com/database/search" +
                $"?artist={Uri.EscapeDataString(artist)}" +
                $"&release_title={Uri.EscapeDataString(titlu)}" +
                $"&year={an}" +
                $"&type=release";

            var searchResp = await _http.GetAsync(searchUrl);
            if (!searchResp.IsSuccessStatusCode) return Array.Empty<Melodie>();

            using var searchDoc = JsonDocument.Parse(await searchResp.Content.ReadAsStringAsync());
            var results = searchDoc.RootElement.GetProperty("results");
            if (results.GetArrayLength() == 0) return Array.Empty<Melodie>();

            int releaseId = results[0].GetProperty("id").GetInt32();

            var releaseResp = await _http.GetAsync($"https://api.discogs.com/releases/{releaseId}");
            if (!releaseResp.IsSuccessStatusCode) return Array.Empty<Melodie>();

            using var releaseDoc = JsonDocument.Parse(await releaseResp.Content.ReadAsStringAsync());
            if (!releaseDoc.RootElement.TryGetProperty("tracklist", out var tracklist))
                return Array.Empty<Melodie>();

            var melodii = new List<Melodie>();
            foreach (var track in tracklist.EnumerateArray())
            {
                string trackTitlu = track.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";
                if (string.IsNullOrWhiteSpace(trackTitlu)) continue;

                float durata = 0f;
                if (track.TryGetProperty("duration", out var d) && !string.IsNullOrEmpty(d.GetString()))
                    durata = ParseDurata(d.GetString()!);

                melodii.Add(new Melodie(trackTitlu, string.Empty, durata));
            }

            return melodii.ToArray();
        }

        private static float ParseDurata(string durata)
        {
            var parts = durata.Split(':');
            if (parts.Length != 2) return 0f;
            if (!int.TryParse(parts[0], out int min) || !int.TryParse(parts[1], out int sec)) return 0f;
            return min + sec / 60f;
        }
    }
}
