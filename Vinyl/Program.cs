using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using LibrarieModele;
using NivelStocareDate;

namespace VinylApp
{
    class Program
    {

        static void Main(string[] args)
        {
            AdministrareVinyluriMemorie adminVinyluri = new AdministrareVinyluriMemorie();
            string optiune;

            do
            {
                Console.WriteLine("\n--- MENIU GESTIUNE VINYL VIBES ---");
                Console.WriteLine("A. Adauga un vinyl");
                Console.WriteLine("B. Afiseaza colectia (tabel)");
                Console.WriteLine("C. Cauta un vinyl dupa titlu");
                Console.WriteLine("D. Cauta un vinyl dupa artist");
                Console.WriteLine("E. Afiseaza discuri imprumutate");
                Console.WriteLine("F. Afiseaza cele mai recente N vinyluri");
                Console.WriteLine("G. Cauta dupa gen muzical");
                Console.WriteLine("X. Iesire");
                optiune = Console.ReadLine().ToUpper();

                switch (optiune)
                {
                    case "A":
                        AdaugaVinyl(adminVinyluri);
                        break;
                    case "B":
                        AfiseazaTabelar(adminVinyluri.GetAllVinyls(), "COLECTIE VINYLURI");
                        break;
                    case "C":
                        CautaDupaTitlu(adminVinyluri);
                        break;
                    case "D":
                        CautaDupaArtist(adminVinyluri);
                        break;
                    case "E":
                        AfiseazaImprumutate(adminVinyluri);
                        break;
                    case "F":
                        AfiseazaCeleMaiRecente(adminVinyluri);
                        break;
                    case "G":
                        CautaDupaGen(adminVinyluri);
                        break;
                    case "X":
                        Console.WriteLine("La revedere!");
                        break;
                    default:
                        Console.WriteLine("Optiune invalida, incercati din nou.");
                        break;
                }
            } while (optiune != "X");
        }

        static void AfiseazaTabelar(List<Vinyl> discuri, string titluTabel)
        {
            Console.WriteLine($"\n--- {titluTabel} ---");

            if (discuri.Count == 0)
            {
                Console.WriteLine("Nicio intrare de afisat.");
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("Artist");
            dt.Columns.Add("Titlu");
            dt.Columns.Add("An");
            dt.Columns.Add("Conditie");
            dt.Columns.Add("Gen");
            dt.Columns.Add("Durata");

            foreach (Vinyl v in discuri)
            {
                dt.Rows.Add(
                    v.Artist ?? "",
                    v.Titlu ?? "NESET",
                    v.An_Lansare.ToString(),
                    v.ConditieDisc,
                    v.Gen.ToString(),
                    v.DurataTotalaFormatata
                );
            }

            int[] w = new int[dt.Columns.Count];
            for (int c = 0; c < dt.Columns.Count; c++)
            {
                w[c] = dt.Columns[c].ColumnName.Length;
                foreach (DataRow row in dt.Rows)
                    w[c] = Math.Max(w[c], row[c].ToString().Length);
                w[c] = Math.Min(w[c], 28);
            }

            string sep = "+" + string.Join("+", w.Select(x => new string('-', x + 2))) + "+";

            Console.WriteLine(sep);
            string header = "|";
            for (int c = 0; c < dt.Columns.Count; c++)
                header += " " + dt.Columns[c].ColumnName.PadRight(w[c]) + " |";
            Console.WriteLine(header);
            Console.WriteLine(sep);

            int idx = 0;
            foreach (DataRow row in dt.Rows)
            {
                string line = "|";
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    string val = row[c].ToString();
                    if (val.Length > w[c]) val = val.Substring(0, w[c] - 2) + "..";
                    line += " " + val.PadRight(w[c]) + " |";
                }
                Console.WriteLine(line);

                Vinyl v = discuri[idx];
                int nrPiese = v.Melodii?.Length ?? 0;
                string stare = v.EsteImprumutat ? $"Imprumutat: {v.NumeImprumutat}" : "In raft";
                int innerWidth = w.Sum() + (w.Length - 1) * 3;
                string summary = $"  >> Piese: {nrPiese}  |  Durata: {v.DurataTotalaFormatata}  |  {stare}";
                Console.WriteLine($"| {summary.PadRight(innerWidth)} |");

                Console.WriteLine(sep);
                idx++;
            }
        }

        static void AdaugaVinyl(AdministrareVinyluriMemorie adminVinyluri)
        {
            Console.WriteLine("\n--- ADAUGARE DISC NOU ---");
            Console.Write("Titlu album: ");
            string titlu = Console.ReadLine();
            Console.Write("Artist: ");
            string artist = Console.ReadLine();
            Console.Write("An lansare: ");
            int.TryParse(Console.ReadLine(), out int anLansare);
            Console.WriteLine("Conditie disc:");
            Console.WriteLine("  1 = Poor (P)           - Disc foarte deteriorat");
            Console.WriteLine("  2 = Fair (F)           - Disc cu defecte vizibile");
            Console.WriteLine("  3 = Good (G)           - Disc functional, uzura normala");
            Console.WriteLine("  4 = Good Plus (G+)     - Disc bun, zgarietur mici");
            Console.WriteLine("  5 = Very Good (VG)     - Disc foarte bun, imperfectiuni minore");
            Console.WriteLine("  6 = Very Good Plus (VG+) - Aproape perfect, urme minime");
            Console.WriteLine("  7 = Near Mint (NM)     - Aproape nou");
            Console.WriteLine("  8 = Mint (M)           - Perfect, nefolosit");
            Console.Write("Alegeti conditia (1-8): ");
            int.TryParse(Console.ReadLine(), out int conditie);

            Console.WriteLine("\nGen muzical (puteti combina cu +, ex: 1+8 pentru Rock+Blues):");
            Console.WriteLine("  1=Rock   2=Jazz   4=Pop    8=Blues  16=Electronic");
            Console.WriteLine("  32=HipHop  64=Clasic  128=R&B  256=Soul  512=Altele");
            Console.Write("Alegeti gen: ");
            string[] partiGen = Console.ReadLine().Split('+');
            GenMuzical genAles = GenMuzical.Necunoscut;
            foreach (string parte in partiGen)
            {
                if (int.TryParse(parte.Trim(), out int gVal)
                    && Enum.IsDefined(typeof(GenMuzical), gVal)
                    && gVal != 0)
                {
                    genAles |= (GenMuzical)gVal;
                }
            }

            Console.WriteLine("\nFormat vinil (puteti combina cu +, ex: 1+16 pentru LP colorat):");
            Console.WriteLine("  1=LP  2=EP  4=Single  8=Reissue  16=Colored  32=PictureDisc");
            Console.Write("Alegeti format: ");
            string[] partiFormat = Console.ReadLine().Split('+');
            FormatVinyl formatAles = FormatVinyl.Nedefinit;
            foreach (string parte in partiFormat)
            {
                if (int.TryParse(parte.Trim(), out int fVal) && Enum.IsDefined(typeof(FormatVinyl), fVal))
                    formatAles |= (FormatVinyl)fVal;
            }
            if (formatAles == FormatVinyl.Nedefinit)
                formatAles = FormatVinyl.LP;

            Vinyl discNou = new Vinyl(titlu, artist, anLansare, 0)
            {
                CodConditie = conditie,
                Gen = genAles,
                Format = formatAles
            };

            Console.Write("\nCate melodii are albumul? ");
            if (int.TryParse(Console.ReadLine(), out int nrMelodii) && nrMelodii > 0)
            {
                string[] tracklist = new string[nrMelodii];
                float[] durate = new float[nrMelodii];

                for (int i = 0; i < nrMelodii; i++)
                {
                    Console.Write($"Nume melodie {i + 1}: ");
                    string numePiesa = Console.ReadLine();

                    Console.Write("Featuring (Enter daca e piesa solo): ");
                    string feat = Console.ReadLine();

                    tracklist[i] = !string.IsNullOrWhiteSpace(feat)
                        ? $"{numePiesa} (feat. {feat})"
                        : numePiesa;

                    Console.Write("Durata melodiei (minute, ex: 3.5 = 3m30s): ");
                    float.TryParse(
                        Console.ReadLine().Replace(',', '.'),
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float durata);
                    durate[i] = durata;
                }

                discNou.Melodii = tracklist;
                discNou.DurataMelodiiMinute = durate;
            }

            Console.Write("\nEste acest disc imprumutat cuiva? (da/nu): ");
            if (Console.ReadLine().Trim().ToLower() == "da")
            {
                Console.Write("Numele persoanei: ");
                string numePers = Console.ReadLine();

                Console.Write("Stare membru (1=Activ, 2=VIP, 3=Inactiv) [implicit 1]: ");
                int.TryParse(Console.ReadLine(), out int stareCod);
                StareMembru stare = stareCod == 2 ? StareMembru.VIP
                                 : stareCod == 3 ? StareMembru.Inactiv
                                 : StareMembru.Activ;

                Persoana imprumutator = new Persoana(numePers, "", RolPersoana.Imprumutator, stare);

                bool reusit = adminVinyluri.ImprumutaVinyl(discNou, imprumutator);
                if (!reusit)
                    Console.WriteLine("  Discul a ramas in colectie (neimprumutat).");
            }

            adminVinyluri.AddVinyl(discNou);
            Console.WriteLine("Disc adaugat cu succes!");
        }

        static void CautaDupaTitlu(AdministrareVinyluriMemorie adminVinyluri)
        {
            Console.Write("\nIntroduceti titlul cautat: ");
            string titluCautat = Console.ReadLine();
            List<Vinyl> gasite = adminVinyluri.GetVinylByTitle(titluCautat);
            if (gasite.Count == 0)
                Console.WriteLine("Nu s-au gasit discuri cu acest titlu.");
            else
            {
                Console.WriteLine($"\nS-au gasit {gasite.Count} rezultate:");
                foreach (Vinyl disc in gasite)
                    Console.WriteLine(disc.Info());
            }
        }

        static void CautaDupaArtist(AdministrareVinyluriMemorie adminVinyluri)
        {
            Console.Write("\nIntroduceti artistul cautat: ");
            string artistCautat = Console.ReadLine();
            List<Vinyl> gasite = adminVinyluri.GetVinylByArtist(artistCautat);
            if (gasite.Count == 0)
                Console.WriteLine("Nu s-au gasit vinyluri de la acest artist.");
            else
            {
                Console.WriteLine($"\nS-au gasit {gasite.Count} rezultate:");
                foreach (Vinyl v in gasite)
                    Console.WriteLine(v.Info());
            }
        }

        static void CautaDupaGen(AdministrareVinyluriMemorie adminVinyluri)
        {
            Console.WriteLine("\nCautare dupa gen muzical:");
            Console.WriteLine("  1=Rock   2=Jazz   4=Pop    8=Blues  16=Electronic");
            Console.WriteLine("  32=HipHop  64=Clasic  128=R&B  256=Soul  512=Altele");
            Console.Write("Alegeti gen (puteti combina cu +): ");
            string[] parti = Console.ReadLine().Split('+');
            GenMuzical genCautat = GenMuzical.Necunoscut;
            foreach (string parte in parti)
            {
                if (int.TryParse(parte.Trim(), out int gVal)
                    && Enum.IsDefined(typeof(GenMuzical), gVal)
                    && gVal != 0)
                {
                    genCautat |= (GenMuzical)gVal;
                }
            }

            List<Vinyl> gasite = adminVinyluri.GetVinylByGen(genCautat);
            string genStr = genCautat == GenMuzical.Necunoscut ? "Necunoscut" : genCautat.ToString();

            if (gasite.Count == 0)
                Console.WriteLine($"Nu s-au gasit discuri cu genul: {genStr}");
            else
            {
                Console.WriteLine($"\nS-au gasit {gasite.Count} discuri de gen: {genStr}");
                AfiseazaTabelar(gasite, $"REZULTATE GEN: {genStr}");
            }
        }

        static void AfiseazaImprumutate(AdministrareVinyluriMemorie adminVinyluri)
        {
            List<Vinyl> imprumutate = adminVinyluri.GetVinyluriBorrowed();
            if (imprumutate.Count == 0)
                Console.WriteLine("\nNu exista discuri imprumutate momentan.");
            else
            {
                Console.WriteLine($"\nTotal imprumutate: {imprumutate.Count}");
                AfiseazaTabelar(imprumutate, "DISCURI IMPRUMUTATE");
            }
        }

        static void AfiseazaCeleMaiRecente(AdministrareVinyluriMemorie adminVinyluri)
        {
            Console.Write("\nCate vinyluri sa afiseze (top N): ");
            if (int.TryParse(Console.ReadLine(), out int n) && n > 0)
            {
                List<Vinyl> recente = adminVinyluri.GetTopVinyluriByYear(n);
                AfiseazaTabelar(recente, $"TOP {n} CELE MAI RECENTE");
            }
            else
            {
                Console.WriteLine("Numar invalid.");
            }
        }
    }
}
