using System;
using System.Globalization;

namespace LibrarieModele
{
    public class Melodie
    {
        private float _durataMinute;

        public string Titlu { get; set; }
        public string Featuring { get; set; }

        public float DurataMinute
        {
            get => _durataMinute;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(DurataMinute),
                        "Durata unei melodii nu poate fi negativa.");
                _durataMinute = value;
            }
        }

        public Melodie()
        {
            Titlu = string.Empty;
            Featuring = string.Empty;
        }

        public Melodie(string titlu, string featuring, float durataMinute)
        {
            Titlu     = titlu     ?? string.Empty;
            Featuring = featuring ?? string.Empty;
            DurataMinute = durataMinute;
        }

        public string NumeComplet =>
            string.IsNullOrWhiteSpace(Featuring) ? Titlu : $"{Titlu} (feat. {Featuring})";

        public string DurataFormatata
        {
            get
            {
                if (_durataMinute <= 0) return "N/A";
                int min = (int)_durataMinute;
                int sec = (int)Math.Round((_durataMinute - min) * 60);
                return $"{min}:{sec:D2}";
            }
        }

        public string Info(int index = 0)
        {
            string prefix = index > 0 ? $"{index}. " : "";
            string durata = _durataMinute > 0 ? $" [{DurataFormatata}]" : "";
            return $"{prefix}{NumeComplet}{durata}";
        }

        public static Melodie FromLine(string linie)
        {
            var p = linie.Split('|');
            string titlu = p.Length > 1 ? p[1] : string.Empty;
            string feat = p.Length > 2 ? p[2] : string.Empty;
            float.TryParse(p.Length > 3 ? p[3] : "0", NumberStyles.Float, CultureInfo.InvariantCulture, out float dur);

            var m = new Melodie { Titlu = titlu, Featuring = feat };
            m._durataMinute = Math.Max(0f, dur);
            return m;
        }

        public string ToLine() =>
            $"MELODIE|{Titlu}|{Featuring}|{_durataMinute.ToString(CultureInfo.InvariantCulture)}";
    }
}
