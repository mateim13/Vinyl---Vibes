using System;
using System.Collections.Generic;
using System.Linq;

using LibrarieModele;

namespace NivelStocareDate
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

        public List<Vinyl> GetVinyluriBorrowed()
        {
            return vinyluri
                .Where(v => v.EsteImprumutat)
                .OrderBy(v => v.NumeImprumutat)
                .ToList();
        }

        public List<Vinyl> GetTopVinyluriByYear(int n)
        {
            return vinyluri
                .OrderByDescending(v => v.An_Lansare)
                .Take(n)
                .ToList();
        }

        public List<Vinyl> GetVinylByGen(GenMuzical gen)
        {
            if (gen == GenMuzical.Necunoscut)
                return vinyluri.Where(v => v.Gen == GenMuzical.Necunoscut).ToList();

            return vinyluri.Where(v => (v.Gen & gen) != 0).ToList();
        }

        public bool ImprumutaVinyl(Vinyl disc, Persoana persoana)
        {
            if (disc.EsteImprumutat)
            {
                Console.WriteLine($"  Eroare: discul '{disc.Titlu}' este deja imprumutat la {disc.NumeImprumutat}.");
                return false;
            }

            int discuriCurente = vinyluri.Count(v =>
                v.EsteImprumutat &&
                v.NumeImprumutat != null &&
                v.NumeImprumutat.Equals(persoana.Nume, StringComparison.OrdinalIgnoreCase));

            if (!persoana.PoateImprumuta(discuriCurente, out string motiv))
            {
                Console.WriteLine($"  Imprumut refuzat: {motiv}");
                return false;
            }

            disc.ImprumutatorPersoana = persoana;
            return true;
        }
    }
}
