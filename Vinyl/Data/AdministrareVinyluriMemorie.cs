using System;
using System.Collections.Generic;
using System.Linq;

using VinylApp.Models;

namespace VinylApp.Data
{
    public class AdministrareVinyluriMemorie
    {
        private List<Vinyl> vinyluri;

        public AdministrareVinyluriMemorie()
        {
            vinyluri = new List<Vinyl>();
        }

        public void AddVinyl(Vinyl disc)
        {
            vinyluri.Add(disc);
        }

        public List<Vinyl> GetAllVinyls()
        {
            return vinyluri;
        }

        public List<Vinyl> GetVinylByTitle(string titlu)
        {
            return vinyluri.Where(v => v.Titlu.Equals(titlu, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Vinyl> GetVinylByArtist(string artist)
        {
            return vinyluri.Where(v => v.Artist.Equals(artist, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<Vinyl> GetVinylByYear(int an)
        {
            return vinyluri.Where(v => v.An_Lansare == an).ToList();
        }
    }
}
