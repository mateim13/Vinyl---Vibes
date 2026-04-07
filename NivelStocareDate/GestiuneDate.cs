using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibrarieModele;

namespace NivelStocareDate
{
    public abstract class GestiuneDate<T>
    {
        protected readonly List<T> elemente;
        protected readonly string  caleFisier;

        protected GestiuneDate(string caleFisier)
        {
            this.caleFisier = caleFisier;
            elemente = Incarca();
        }

        protected abstract IEnumerable<string> SerializeazaElement(T element);
        protected abstract List<T> DeserializeazaLinii(string[] linii);

        protected void Salveaza()
        {
            var linii = elemente.SelectMany(SerializeazaElement);
            File.WriteAllLines(caleFisier, linii, Encoding.UTF8);
        }

        private List<T> Incarca()
        {
            if (!File.Exists(caleFisier)) return new List<T>();
            var linii = File.ReadAllLines(caleFisier, Encoding.UTF8)
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToArray();
            return DeserializeazaLinii(linii);
        }

        public List<T> ObtineToti() => new List<T>(elemente);

        public void Adauga(T element)
        {
            elemente.Add(element);
            Salveaza();
        }

        protected bool ModificaLaIndex(int idx, T elementNou)
        {
            if (idx < 0) return false;
            elemente[idx] = elementNou;
            Salveaza();
            return true;
        }

        protected bool StergeDelaIndex(int idx)
        {
            if (idx < 0) return false;
            elemente.RemoveAt(idx);
            Salveaza();
            return true;
        }
    }

    public class RepoVinyluri : GestiuneDate<Vinyl>
    {
        public RepoVinyluri(string caleFisier) : base(caleFisier) { }

        protected override IEnumerable<string> SerializeazaElement(Vinyl v) => v.ToLines();

        protected override List<Vinyl> DeserializeazaLinii(string[] linii)
        {
            var lista = new List<Vinyl>();
            int i = 0;
            while (i < linii.Length)
            {
                if (Vinyl.TryFromLines(linii, ref i, out Vinyl? v) && v != null)
                    lista.Add(v);
                else
                    i++;
            }
            return lista;
        }

        public List<Vinyl> CautaDupaTitlu(string titlu) =>
            elemente.Where(v => v.Titlu.Equals(titlu, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Vinyl> CautaDupaArtist(string artist) =>
            elemente.Where(v => v.Artist.Equals(artist, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Vinyl> CautaDupaAn(int an) =>
            elemente.Where(v => v.An_Lansare == an).ToList();

        public List<Vinyl> ObtineImprumutate() =>
            elemente.Where(v => v.EsteImprumutat).OrderBy(v => v.NumeImprumutat).ToList();

        public List<Vinyl> ObtineCeleMaiRecente(int n) =>
            elemente.OrderByDescending(v => v.An_Lansare).Take(n).ToList();

        public List<Vinyl> CautaDupaGen(GenMuzical gen)
        {
            if (gen == GenMuzical.Necunoscut)
                return elemente.Where(v => v.Gen == GenMuzical.Necunoscut).ToList();
            return elemente.Where(v => (v.Gen & gen) != 0).ToList();
        }

        public bool ModificaVinyl(string titluOriginal, Vinyl vinylNou)
        {
            int idx = elemente.FindIndex(v => v.Titlu.Equals(titluOriginal, StringComparison.OrdinalIgnoreCase));
            return ModificaLaIndex(idx, vinylNou);
        }

        public bool StergeVinyl(string titlu)
        {
            int idx = elemente.FindIndex(v => v.Titlu.Equals(titlu, StringComparison.OrdinalIgnoreCase));
            return StergeDelaIndex(idx);
        }

        public bool ReturnazaVinyl(string titlu)
        {
            var disc = elemente.FirstOrDefault(v => v.Titlu.Equals(titlu, StringComparison.OrdinalIgnoreCase));
            if (disc == null || !disc.EsteImprumutat) 
                return false;
            disc.NumeImprumutat = string.Empty;
            Salveaza();
            return true;
        }

        public bool ImprumutaVinyl(Vinyl disc, Persoana persoana)
        {
            if (disc.EsteImprumutat)
            {
                Console.WriteLine($"  Discul '{disc.Titlu}' este deja imprumutat la {disc.NumeImprumutat}.");
                return false;
            }
            int discuriCurente = elemente.Count(v => v.EsteImprumutat && v.NumeImprumutat != null && v.NumeImprumutat.Equals(persoana.Nume, StringComparison.OrdinalIgnoreCase));

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

    public class RepoPersone : GestiuneDate<Persoana>
    {
        public RepoPersone(string caleFisier) : base(caleFisier) { }

        protected override IEnumerable<string> SerializeazaElement(Persoana p) => new[] { p.ToLine() };

        protected override List<Persoana> DeserializeazaLinii(string[] linii)
        {
            var lista = new List<Persoana>();
            foreach (var linie in linii)
            {
                var p = Persoana.FromLine(linie);
                if (p != null) 
                    lista.Add(p);
            }
            return lista;
        }

        public Persoana? CautaDupaNumePersoana(string nume) =>
            elemente.FirstOrDefault(p => p.Nume.Equals(nume, StringComparison.OrdinalIgnoreCase));

        public bool ModificaPersoana(string numeOriginal, Persoana persoanaNoua)
        {
            int idx = elemente.FindIndex(p => p.Nume.Equals(numeOriginal, StringComparison.OrdinalIgnoreCase));
            return ModificaLaIndex(idx, persoanaNoua);
        }

        public bool StergePersoana(string nume)
        {
            int idx = elemente.FindIndex(p => p.Nume.Equals(nume, StringComparison.OrdinalIgnoreCase));
            return StergeDelaIndex(idx);
        }
    }
}
