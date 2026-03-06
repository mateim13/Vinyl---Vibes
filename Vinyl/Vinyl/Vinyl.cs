using System;
using System.Collections.Generic;
using System.Text;

namespace vinyl
{
    public class Vinyl
    {
        public string NumeImprumutat { get; set; }
        public bool EsteImpumutat => !string.IsNullOrEmpty(NumeImprumutat);
        public string CaleAudioSnippet { get; set; }
        public string Titlu { get; set; }
        public string Artist { get; set; }
        public int An_Lansare { get; set; }
        public float Pret { get; set; }
        public int CodConditie { get; set; }
        public string[] Melodii { get; set; }
        public bool EsteVintage => An_Lansare < 1990;
        public bool NecesitaAtentie => CodConditie <= 3;

        public string ConditieDisc
        {
            get
            {
                switch (CodConditie)
                {
                    case 1: return "Poor (P)";
                    case 2: return "Fair (F)";
                    case 3: return "Good (G)";
                    case 4: return "Good Plus (G+)";
                    case 5: return "Very Good (VG)";
                    case 6: return "Very Good Plus (VG+)";
                    case 7: return "Near Mint (NM)";
                    case 8: return "Mint (M)";
                    default: return "Neprecizat";
                }
            }
        }

        public Vinyl()
        {
            Titlu = string.Empty;
            Artist = "Neprecizat";
            Melodii = new string[0];
        }

        public Vinyl(string titlu, string artist, int anLansare, float pret)
        {
            Titlu = titlu;
            Artist = artist;
            An_Lansare = anLansare;
            Pret = pret;
        }

        public string Info()
        {
            string numeAfisat = string.IsNullOrEmpty(Titlu) ? "VINYL NESETAT" : Titlu;
            string stare = EsteImpumutat ? "[Imprumutat la " + NumeImprumutat + "]" : "[In raft]";
            string detalii = Artist + " - " + numeAfisat + " (" + An_Lansare + ")";

            string tracklist = "\nTracklist:";
            if (Melodii != null && Melodii.Length > 0) 
            {
                int contor = 1;
                foreach (string piesa in Melodii) 
                {
                    tracklist += "\n  " + contor + ". " + piesa;
                    contor++;
                }
            }
            else
            {
                tracklist += " Nu sunt melodii inregistrate.";
            }

            return  "\n" + detalii + "\nSnippet: " + (string.IsNullOrEmpty(CaleAudioSnippet) ? "Fara audio" : CaleAudioSnippet) +
                    "\nConditie: " + ConditieDisc + tracklist;

        }

        public void Tracklist(string listaMelodii)
        {
            if (!string.IsNullOrEmpty(listaMelodii))
            {
                Melodii = listaMelodii.Split(',');

                for (int i = 0; i < Melodii.Length; i++)
                {
                    Melodii[i] = Melodii[i].Trim();
                }
            }
        }

        



    }
}