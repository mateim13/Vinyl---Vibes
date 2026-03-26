using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using LibrarieModele;

namespace NivelStocareDate
{
    public class AdministrareVinyluriMemorie
    {
        private List<Vinyl> vinyluri;
        private readonly string caleFisier;

        public AdministrareVinyluriMemorie(string caleFisier)
        {
            this.caleFisier = caleFisier;
            vinyluri = Incarca();
        }

        private void Salveaza()
        {
            var linii = new List<string>();
            foreach (var v in vinyluri)
            {
                string imprumutat = v.EsteImprumutat ? "1" : "0";
                string numeImp   = v.NumeImprumutat ?? "";
                string cale      = v.CaleAudioSnippet ?? "";
                linii.Add($"VINYL|{v.Titlu}|{v.Artist}|{v.An_Lansare}|" +
                          $"{v.Pret.ToString(CultureInfo.InvariantCulture)}|{v.CodConditie}|" +
                          $"{(int)v.Gen}|{(int)v.Format}|{imprumutat}|{numeImp}|{cale}");

                if (v.Melodii != null)
                {
                    foreach (var m in v.Melodii)
                        linii.Add($"MELODIE|{m.Titlu}|{m.Featuring}|" +
                                  $"{m.DurataMinute.ToString(CultureInfo.InvariantCulture)}");
                }
            }
            File.WriteAllLines(caleFisier, linii, Encoding.UTF8);
        }

        private List<Vinyl> Incarca()
        {
            var lista = new List<Vinyl>();
            if (!File.Exists(caleFisier)) return lista;

            Vinyl? curent = null;
            var melodiiCurente = new List<Melodie>();

            foreach (var linie in File.ReadAllLines(caleFisier, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(linie)) continue;
                var p = linie.Split('|');

                if (p[0] == "VINYL")
                {
                    if (curent != null)
                    {
                        curent.Melodii = melodiiCurente.ToArray();
                        lista.Add(curent);
                    }
                    curent = new Vinyl();
                    melodiiCurente.Clear();

                    curent.Titlu = p.Length > 1 ? p[1] : "";
                    curent.Artist = p.Length > 2 ? p[2] : "";
                    if (int.TryParse(p.Length > 3 ? p[3] : "0", out int an)) curent.An_Lansare = an;
                    if (float.TryParse(p.Length > 4 ? p[4] : "0", NumberStyles.Float, CultureInfo.InvariantCulture, out float pret)) curent.Pret = pret;
                    if (int.TryParse(p.Length > 5 ? p[5] : "0", out int cod)) curent.CodConditie = cod;
                    if (int.TryParse(p.Length > 6 ? p[6] : "0", out int gen)) curent.Gen = (GenMuzical)gen;
                    if (int.TryParse(p.Length > 7 ? p[7] : "0", out int fmt)) curent.Format = (FormatVinyl)fmt;
                    bool imp = p.Length > 8 && p[8] == "1";
                    if (imp && p.Length > 9 && !string.IsNullOrEmpty(p[9]))
                        curent.NumeImprumutat = p[9];
                    curent.CaleAudioSnippet = p.Length > 10 ? p[10] : "";
                }
                else if (p[0] == "MELODIE" && curent != null)
                {
                    string titluM = p.Length > 1 ? p[1] : "";
                    string feat   = p.Length > 2 ? p[2] : "";
                    float.TryParse(p.Length > 3 ? p[3] : "0", NumberStyles.Float, CultureInfo.InvariantCulture, out float dur);
                    melodiiCurente.Add(new Melodie(titluM, feat, dur));
                }
            }

            if (curent != null)
            {
                curent.Melodii = melodiiCurente.ToArray();
                lista.Add(curent);
            }
            return lista;
        }

        public void AddVinyl(Vinyl disc)
        {
            vinyluri.Add(disc);
            Salveaza();
        }

        public List<Vinyl> GetAllVinyls() => vinyluri;

        public List<Vinyl> GetVinylByTitle(string titlu) =>
            vinyluri.Where(v => v.Titlu.Equals(titlu, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Vinyl> GetVinylByArtist(string artist) =>
            vinyluri.Where(v => v.Artist.Equals(artist, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Vinyl> GetVinylByYear(int an) =>
            vinyluri.Where(v => v.An_Lansare == an).ToList();

        public List<Vinyl> GetVinyluriBorrowed() =>
            vinyluri.Where(v => v.EsteImprumutat).OrderBy(v => v.NumeImprumutat).ToList();

        public List<Vinyl> GetTopVinyluriByYear(int n) =>
            vinyluri.OrderByDescending(v => v.An_Lansare).Take(n).ToList();

        public List<Vinyl> GetVinylByGen(GenMuzical gen)
        {
            if (gen == GenMuzical.Necunoscut)
                return vinyluri.Where(v => v.Gen == GenMuzical.Necunoscut).ToList();
            return vinyluri.Where(v => (v.Gen & gen) != 0).ToList();
        }

        public bool ModificaVinyl(string titluOriginal, Vinyl vinylNou)
        {
            int idx = vinyluri.FindIndex(v => v.Titlu.Equals(titluOriginal, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;
            vinyluri[idx] = vinylNou;
            Salveaza();
            return true;
        }

        public bool StergeVinyl(string titlu)
        {
            int idx = vinyluri.FindIndex(v => v.Titlu.Equals(titlu, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;
            vinyluri.RemoveAt(idx);
            Salveaza();
            return true;
        }

        public bool ReturnazaVinyl(string titlu)
        {
            var disc = vinyluri.FirstOrDefault(v => v.Titlu.Equals(titlu, StringComparison.OrdinalIgnoreCase));
            if (disc == null || !disc.EsteImprumutat) return false;
            disc.NumeImprumutat = string.Empty;
            Salveaza();
            return true;
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
            Salveaza();
            return true;
        }
    }
}
