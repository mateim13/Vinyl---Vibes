using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace LibrarieModele
{
    public class Vinyl
    {
        private int      _anLansare;
        private float    _pret;
        private string?  _numeImprumutatManual;
        private Persoana? _imprumutatorPersoana;

        public Persoana? ImprumutatorPersoana
        {
            get => _imprumutatorPersoana;
            set
            {
                _imprumutatorPersoana = value;
                if (value != null)
                    _numeImprumutatManual = null;
            }
        }

        public string? NumeImprumutat
        {
            get => _imprumutatorPersoana?.Nume ?? _numeImprumutatManual;
            set
            {
                _numeImprumutatManual = value;
                _imprumutatorPersoana = null;
            }
        }

        public bool EsteImprumutat => _imprumutatorPersoana != null || !string.IsNullOrEmpty(_numeImprumutatManual);

        public int An_Lansare
        {
            get => _anLansare;
            set
            {
                if (value > DateTime.Now.Year)
                    throw new ArgumentOutOfRangeException(nameof(An_Lansare), $"Anul de lansare ({value}) nu poate fi in viitor (maxim acceptat: {DateTime.Now.Year}).");
                _anLansare = value;
            }
        }

        public float Pret
        {
            get => _pret;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Pret), "Pretul nu poate fi negativ.");
                _pret = value;
            }
        }

        public string? CaleAudioSnippet { get; set; }
        public string Titlu { get; set; }
        public string Artist { get; set; }
        public int CodConditie { get; set; }
        public Melodie[] Melodii { get; set; }
        public GenMuzical Gen { get; set; }
        public FormatVinyl Format { get; set; }
        public DateTime? DataAchizitie { get; set; }

        public bool EsteVintage => _anLansare < 1990;
        public bool NecesitaAtentie => CodConditie <= 3;

        public string ConditieDisc
        {
            get
            {
                switch (CodConditie)
                {
                    case 1: return "1 - Poor (P)";
                    case 2: return "2 - Fair (F)";
                    case 3: return "3 - Good (G)";
                    case 4: return "4 - Good Plus (G+)";
                    case 5: return "5 - Very Good (VG)";
                    case 6: return "6 - Very Good Plus (VG+)";
                    case 7: return "7 - Near Mint (NM)";
                    case 8: return "8 - Mint (M)";
                    default: return "0 - Neprecizat";
                }
            }
        }

        public string DurataTotalaFormatata
        {
            get
            {
                if (Melodii == null || Melodii.Length == 0) return "N/A";
                float totalMinute = Melodii.Sum(m => m.DurataMinute);
                if (totalMinute <= 0) return "N/A";
                int totalSec = (int)Math.Round(totalMinute * 60);
                int ore = totalSec / 3600;
                int minute = (totalSec % 3600) / 60;
                int secunde = totalSec % 60;
                return ore > 0 ? $"{ore}h {minute}m {secunde}s" : $"{minute}m {secunde}s";
            }
        }

        public Vinyl()
        {
            Titlu = string.Empty;
            Artist = "Neprecizat";
            Melodii = new Melodie[0];
            Gen = GenMuzical.Necunoscut;
            Format = FormatVinyl.LP;
        }

        public Vinyl(string titlu, string artist, int anLansare, float pret)
        {
            Titlu = titlu;
            Artist = artist;
            An_Lansare = anLansare; 
            Pret = pret;   
            Melodii = new Melodie[0];
            Gen = GenMuzical.Necunoscut;
            Format = FormatVinyl.LP;
        }

        public string Info()
        {
            string numeAfisat = string.IsNullOrEmpty(Titlu) ? "VINYL NESET" : Titlu;
            string stareStr;
            if (_imprumutatorPersoana != null)
                stareStr = $"[Imprumutat la {_imprumutatorPersoana.Nume} ({_imprumutatorPersoana.Stare})]";
            else if (!string.IsNullOrEmpty(_numeImprumutatManual))
                stareStr = $"[Imprumutat la {_numeImprumutatManual}]";
            else
                stareStr = "[In raft]";

            var sb = new StringBuilder();
            sb.AppendLine($"Vinyl: {Artist} - {numeAfisat} ({_anLansare}) {stareStr}");
            sb.AppendLine($"Gen: {Gen} | Format: {Format}");
            sb.AppendLine($"Conditie: {ConditieDisc}");
            sb.AppendLine($"Durata: {DurataTotalaFormatata}");
            if (!string.IsNullOrEmpty(CaleAudioSnippet))
                sb.AppendLine($"Snippet: {CaleAudioSnippet}");
            sb.AppendLine("Tracklist:");
            if (Melodii != null && Melodii.Length > 0)
            {
                foreach (var (m, i) in Melodii.Select((m, i) => (m, i + 1)))
                    sb.AppendLine($"  {m.Info(i)}");
            }
            else
            {
                sb.AppendLine("  (fara melodii inregistrate)");
            }
            sb.Append("---");
            return sb.ToString();
        }

        public IEnumerable<string> ToLines()
        {
            string imprumutat = EsteImprumutat ? "1" : "0";
            string numeImp = NumeImprumutat ?? string.Empty;
            string cale = CaleAudioSnippet ?? string.Empty;
            string dataAch = DataAchizitie.HasValue
                ? DataAchizitie.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : string.Empty;

            yield return $"VINYL|{Titlu}|{Artist}|{_anLansare}|" +
                         $"{_pret.ToString(CultureInfo.InvariantCulture)}|{CodConditie}|" +
                         $"{(int)Gen}|{(int)Format}|{imprumutat}|{numeImp}|{cale}|{dataAch}";

            if (Melodii != null)
                foreach (var m in Melodii)
                    yield return m.ToLine();
        }

        public static bool TryFromLines(string[] linii, ref int index, out Vinyl? vinyl)
        {
            vinyl = null;
            if (index >= linii.Length) return false;

            var p = linii[index].Split('|');
            if (p[0] != "VINYL") return false;

            vinyl = new Vinyl();
            vinyl.Titlu = p.Length > 1 ? p[1] : string.Empty;
            vinyl.Artist = p.Length > 2 ? p[2] : string.Empty;

            if (int.TryParse(p.Length > 3 ? p[3] : "0", out int an))
                vinyl._anLansare = an;
            if (float.TryParse(p.Length > 4 ? p[4] : "0",
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float pret))
                vinyl._pret = pret;
            if (int.TryParse(p.Length > 5 ? p[5] : "0", out int cod))
                vinyl.CodConditie = cod;
            if (int.TryParse(p.Length > 6 ? p[6] : "0", out int gen))
                vinyl.Gen = (GenMuzical)gen;
            if (int.TryParse(p.Length > 7 ? p[7] : "0", out int fmt))
                vinyl.Format = (FormatVinyl)fmt;

            bool imp = p.Length > 8 && p[8] == "1";
            if (imp && p.Length > 9 && !string.IsNullOrEmpty(p[9]))
                vinyl._numeImprumutatManual = p[9];

            vinyl.CaleAudioSnippet = p.Length > 10 && !string.IsNullOrEmpty(p[10]) ? p[10] : null;

            if (p.Length > 11 && !string.IsNullOrEmpty(p[11])
                && DateTime.TryParseExact(p[11], "yyyy-MM-dd",
                       CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dataAch))
                vinyl.DataAchizitie = dataAch;

            index++;

            var melodii = new List<Melodie>();
            while (index < linii.Length && linii[index].StartsWith("MELODIE|"))
            {
                melodii.Add(Melodie.FromLine(linii[index]));
                index++;
            }
            vinyl.Melodii = melodii.ToArray();

            return true;
        }
    }
}
