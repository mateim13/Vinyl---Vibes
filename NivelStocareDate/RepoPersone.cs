using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibrarieModele;

namespace NivelStocareDate
{
    public class RepoPersone
    {
        private List<Persoana> persoane;
        private readonly string caleFisier;

        public RepoPersone(string caleFisier)
        {
            this.caleFisier = caleFisier;
            persoane = Incarca();
        }

        private void Salveaza()
        {
            var linii = new List<string>();
            foreach (var p in persoane)
            {
                string discuri = (p.DiscuriImprumutate != null && p.DiscuriImprumutate.Length > 0)
                    ? string.Join(";", p.DiscuriImprumutate)
                    : "";
                linii.Add($"PERSOANA|{p.Nume}|{p.Contact}|{p.Rol}|{p.Stare}|{discuri}");
            }
            File.WriteAllLines(caleFisier, linii, Encoding.UTF8);
        }

        private List<Persoana> Incarca()
        {
            var lista = new List<Persoana>();
            if (!File.Exists(caleFisier)) return lista;

            foreach (var linie in File.ReadAllLines(caleFisier, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(linie)) continue;
                var p = linie.Split('|');
                if (p[0] != "PERSOANA" || p.Length < 5) continue;

                Enum.TryParse<RolPersoana>(p[3], out RolPersoana rol);
                Enum.TryParse<StareMembru>(p[4], out StareMembru stare);

                var persoana = new Persoana(p[1], p.Length > 2 ? p[2] : "", rol, stare);
                if (p.Length > 5 && !string.IsNullOrEmpty(p[5]))
                    persoana.DiscuriImprumutate = p[5].Split(';');
                lista.Add(persoana);
            }
            return lista;
        }

        public void AdaugaPersoana(Persoana persoana)
        {
            persoane.Add(persoana);
            Salveaza();
        }

        public List<Persoana> GetAllPersone() => persoane;

        public Persoana? GetPersoanaByNume(string nume) =>
            persoane.FirstOrDefault(p => p.Nume.Equals(nume, StringComparison.OrdinalIgnoreCase));

        public bool ModificaPersoana(string numeOriginal, Persoana persoanaNoua)
        {
            int idx = persoane.FindIndex(p => p.Nume.Equals(numeOriginal, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;
            persoane[idx] = persoanaNoua;
            Salveaza();
            return true;
        }

        public bool StergePersoana(string nume)
        {
            int idx = persoane.FindIndex(p => p.Nume.Equals(nume, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;
            persoane.RemoveAt(idx);
            Salveaza();
            return true;
        }
    }
}
