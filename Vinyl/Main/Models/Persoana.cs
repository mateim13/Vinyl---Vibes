using System;

namespace VinylApp.Models
{
    public class Persoana
    {
        public string Nume { get; set; }
        public string Contact { get; set; }
        public string[] DiscuriImprumutate { get; set; }

        public Persoana(string nume, string contact)
        {
            Nume = nume;
            Contact = contact;
            DiscuriImprumutate = new string[0];
        }

        public string InfoPersoana()
        {
            string lista = (DiscuriImprumutate != null && DiscuriImprumutate.Length > 0)
                ? string.Join(", ", DiscuriImprumutate)
                : "Niciun disc momentan";

            return "Persoana: " + Nume + " | Contact: " + Contact + "\nAre imprumutate: " + lista;
        }
    }
}
