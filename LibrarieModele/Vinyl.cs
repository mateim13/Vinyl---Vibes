using System;
using System.Collections.Generic;
using System.Text;

namespace LibrarieModele
{
    [Flags]
    public enum GenMuzical
    {
        Necunoscut  = 0,
        Rock        = 1,
        Jazz        = 2,
        Pop         = 4,
        Blues       = 8,
        Electronic  = 16,
        HipHop      = 32,
        Clasic      = 64,
        RnB         = 128,  
        Soul        = 256,
        Altele      = 512
    }

    [Flags]
    public enum FormatVinyl
    {
        Nedefinit   = 0,
        LP          = 1,      
        EP          = 2,       
        Single      = 4,       
        Reissue     = 8,       
        Colored     = 16,      
        PictureDisc = 32      
    }

    public class Vinyl
    {
        private string _numeImprumutatManual;
        private Persoana _imprumutatorPersoana;

        public Persoana ImprumutatorPersoana
        {
            get => _imprumutatorPersoana;
            set
            {
                _imprumutatorPersoana = value;
                if (value != null)
                    _numeImprumutatManual = null; 
            }
        }

        public string NumeImprumutat
        {
            get => _imprumutatorPersoana?.Nume ?? _numeImprumutatManual;
            set
            {
                _numeImprumutatManual = value;
                _imprumutatorPersoana = null; 
            }
        }

        public bool EsteImprumutat => _imprumutatorPersoana != null || !string.IsNullOrEmpty(_numeImprumutatManual);

        public string CaleAudioSnippet { get; set; }
        public string Titlu { get; set; }
        public string Artist { get; set; }
        public int An_Lansare { get; set; }
        public float Pret { get; set; }
        public int CodConditie { get; set; }
        public string[] Melodii { get; set; }
        public float[] DurataMelodiiMinute { get; set; }
        public GenMuzical Gen { get; set; }
        public FormatVinyl Format { get; set; }

        public bool EsteVintage => An_Lansare < 1990;
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
                if (DurataMelodiiMinute == null || DurataMelodiiMinute.Length == 0)
                    return "N/A";

                float totalMinute = 0;
                foreach (float d in DurataMelodiiMinute)
                    totalMinute += d;

                if (totalMinute <= 0)
                    return "N/A";

                int totalSec = (int)Math.Round(totalMinute * 60);
                int ore = totalSec / 3600;
                int minute = (totalSec % 3600) / 60;
                int secunde = totalSec % 60;

                return ore > 0
                    ? $"{ore}h {minute}m {secunde}s"
                    : $"{minute}m {secunde}s";
            }
        }

        public Vinyl()
        {
            Titlu = string.Empty;
            Artist = "Neprecizat";
            Melodii = new string[0];
            DurataMelodiiMinute = new float[0];
            Gen = GenMuzical.Necunoscut;
            Format = FormatVinyl.LP;
        }

        public Vinyl(string titlu, string artist, int anLansare, float pret)
        {
            Titlu = titlu;
            Artist = artist;
            An_Lansare = anLansare;
            Pret = pret;
            Melodii = new string[0];
            DurataMelodiiMinute = new float[0];
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

            string detalii = Artist + " - " + numeAfisat + " (" + An_Lansare + ") " + stareStr;

            string tracklist = "\nTracklist:";
            if (Melodii != null && Melodii.Length > 0)
            {
                for (int i = 0; i < Melodii.Length; i++)
                {
                    string durata = "";
                    if (DurataMelodiiMinute != null && i < DurataMelodiiMinute.Length && DurataMelodiiMinute[i] > 0)
                    {
                        int min = (int)DurataMelodiiMinute[i];
                        int sec = (int)Math.Round((DurataMelodiiMinute[i] - min) * 60);
                        durata = $" [{min}:{sec:D2}]";
                    }
                    tracklist += $"\n  {i + 1}. {Melodii[i]}{durata}";
                }
            }
            else
            {
                tracklist += " Nu sunt melodii inregistrate.";
            }

            return "\n" + detalii +
                   "\nGen: " + Gen +
                   " | Format: " + Format +
                   " | Durata totala: " + DurataTotalaFormatata +
                   "\nSnippet: " + (string.IsNullOrEmpty(CaleAudioSnippet) ? "Fara audio" : CaleAudioSnippet) +
                   "\nConditie: " + ConditieDisc + tracklist + "\n--------------------------";
        }
    }
}
