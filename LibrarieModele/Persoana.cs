using System;

namespace LibrarieModele
{
    public class Persoana
    {
        public string Nume    { get; set; }
        public string Contact { get; set; }
        public string[] DiscuriImprumutate { get; set; }
        public RolPersoana  Rol   { get; set; }
        public StareMembru  Stare { get; set; }

        public int LimitaImprumut
        {
            get
            {
                switch (Stare)
                {
                    case StareMembru.VIP:   return int.MaxValue;
                    case StareMembru.Activ: return 3;
                    default:                return 0;
                }
            }
        }

        public Persoana(string nume, string contact)
        {
            Nume = nume;
            Contact = contact;
            DiscuriImprumutate = new string[0];
            Rol = RolPersoana.Colectionar;
            Stare = StareMembru.Activ;
        }

        public Persoana(string nume, string contact, RolPersoana rol, StareMembru stare)
        {
            Nume = nume;
            Contact = contact;
            DiscuriImprumutate = new string[0];
            Rol = rol;
            Stare = stare;
        }

        public bool PoateImprumuta(int discuriCurente, out string motiv)
        {
            if (Stare == StareMembru.Inactiv)
            {
                motiv = $"{Nume} este inactiv si nu poate imprumuta discuri.";
                return false;
            }
            if (Stare == StareMembru.Activ && discuriCurente >= 3)
            {
                motiv = $"{Nume} (Activ) a atins limita de 3 discuri imprumutate simultan.";
                return false;
            }
            motiv = string.Empty;
            return true;
        }

        public string Info()
        {
            string lista = (DiscuriImprumutate != null && DiscuriImprumutate.Length > 0)
                ? string.Join(", ", DiscuriImprumutate)
                : "niciun disc";
            string limitaStr = Stare == StareMembru.VIP ? "nelimitata" : LimitaImprumut.ToString();

            return $"Persoana : {Nume} | Contact: {Contact}" +
                   $"\nRol      : {Rol} | Stare: {Stare} | Limita imprumut: {limitaStr}" +
                   $"\nImprumuta: {lista}" +
                   "\n---";
        }

        public static Persoana? FromLine(string linie)
        {
            var p = linie.Split('|');
            if (p[0] != "PERSOANA" || p.Length < 5) return null;

            Enum.TryParse<RolPersoana> (p[3], out RolPersoana  rol);
            Enum.TryParse<StareMembru> (p[4], out StareMembru  stare);

            var persoana = new Persoana(p[1], p.Length > 2 ? p[2] : string.Empty, rol, stare);
            if (p.Length > 5 && !string.IsNullOrEmpty(p[5]))
                persoana.DiscuriImprumutate = p[5].Split(';');

            return persoana;
        }

        public string ToLine()
        {
            string discuri = (DiscuriImprumutate != null && DiscuriImprumutate.Length > 0)
                ? string.Join(";", DiscuriImprumutate)
                : string.Empty;
            return $"PERSOANA|{Nume}|{Contact}|{Rol}|{Stare}|{discuri}";
        }
    }
}
