using System;

namespace LibrarieModele
{
    public class Melodie
    {
        public string Titlu { get; set; }
        public string Featuring { get; set; }
        public float DurataMinute { get; set; }

        public Melodie()
        {
            Titlu = string.Empty;
            Featuring = string.Empty;
        }

        public Melodie(string titlu, string featuring, float durataMinute)
        {
            Titlu = titlu ?? string.Empty;
            Featuring = featuring ?? string.Empty;
            DurataMinute = durataMinute;
        }

        public string NumeComplet =>
            string.IsNullOrWhiteSpace(Featuring)
                ? Titlu
                : $"{Titlu} (feat. {Featuring})";

        public string DurataFormatata
        {
            get
            {
                if (DurataMinute <= 0) return "N/A";
                int min = (int)DurataMinute;
                int sec = (int)Math.Round((DurataMinute - min) * 60);
                return $"{min}:{sec:D2}";
            }
        }

        public string Info(int index = 0)
        {
            string prefix = index > 0 ? $"{index}. " : "";
            string durata = DurataMinute > 0 ? $" [{DurataFormatata}]" : "";
            return $"{prefix}{NumeComplet}{durata}";
        }
    }
}
