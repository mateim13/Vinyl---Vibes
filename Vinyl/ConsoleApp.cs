using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibrarieModele;
using NivelStocareDate;

namespace VinylApp
{
    class ConsoleApp
    {
        const string FisierVinyluri = "vinyluri.txt";
        const string FisierPersone  = "persoane.txt";

        readonly RepoVinyluri repoVinyluri;
        readonly RepoPersone  repoPersone;

        public ConsoleApp()
        {
            repoVinyluri = new RepoVinyluri(FisierVinyluri);
            repoPersone  = new RepoPersone(FisierPersone);
        }

        public void Ruleaza()
        {
            string optiune;
            do
            {
                Console.WriteLine("\n--- MENIU GESTIUNE VINYL VIBES ---");
                Console.WriteLine("A. Adauga un vinyl");
                Console.WriteLine("B. Afiseaza colectia");
                Console.WriteLine("C. Cauta un vinyl dupa titlu");
                Console.WriteLine("D. Cauta un vinyl dupa artist");
                Console.WriteLine("E. Afiseaza discuri imprumutate");
                Console.WriteLine("F. Afiseaza cele mai recente N vinyluri");
                Console.WriteLine("G. Cauta dupa gen muzical");
                Console.WriteLine("H. Modifica un vinyl");
                Console.WriteLine("I. Sterge un vinyl");
                Console.WriteLine("J. Returneaza un vinyl (de la imprumut)");
                Console.WriteLine("K. Gestiune persoane");
                Console.WriteLine("X. Iesire");
                optiune = Console.ReadLine()?.ToUpper() ?? "";

                switch (optiune)
                {
                    case "A": AdaugaVinyl();  
                        break;
                    case "B": AfiseazaListaVinyluri(repoVinyluri.ObtineToti(), "COLECTIE VINYLURI");   
                        break;
                    case "C": CautaDupaTitlu();                                                           
                        break;
                    case "D": CautaDupaArtist();                                                           
                        break;
                    case "E": AfiseazaListaVinyluri(repoVinyluri.ObtineImprumutate(), "DISCURI IMPRUMUTATE"); 
                        break;
                    case "F": AfiseazaCeleMaiRecente();                                                    
                        break;
                    case "G": CautaDupaGen();                                                              
                        break;
                    case "H": ModificaVinyl();                                                            
                        break;
                    case "I": StergeVinyl();                                                               
                        break;
                    case "J": ReturneazaVinyl();                                                          
                        break;
                    case "K": MeniuPersone();                                                              
                        break;
                    case "X": Console.WriteLine("La revedere!");                                           
                        break;
                    default:  Console.WriteLine("Optiune invalida, incercati din nou.");                   
                        break;
                }
            } while (optiune != "X");
        }

        private int CitesteAn(int valImplicita = 0)
        {
            Console.Write("An lansare: ");
            string input = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(input)) return valImplicita;

            if (!int.TryParse(input, out int an))
            {
                Console.WriteLine("  Eroare: an invalid.");
                return valImplicita;
            }

            try
            {
                var test = new Vinyl();
                test.An_Lansare = an;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"  Eroare: {ex.Message}");
                return valImplicita;
            }

            return an;
        }

        private float CitestePret(float valImplicita = 0f)
        {
            Console.Write("Pret (RON): ");
            string input = (Console.ReadLine() ?? string.Empty).Replace(',', '.');
            if (string.IsNullOrWhiteSpace(input)) return valImplicita;

            if (!float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float pret) || pret < 0)
            {
                Console.WriteLine("  Eroare: pret invalid (trebuie >= 0).");
                return valImplicita;
            }
            return pret;
        }

        private int CitesteConditie(int valImplicita = 0)
        {
            Console.WriteLine("Conditie: 1=Poor  2=Fair  3=Good  4=G+  5=VG  6=VG+  7=NM  8=Mint");
            Console.Write("Alegeti conditia (1-8): ");
            if (int.TryParse(Console.ReadLine(), out int c) && c >= 1 && c <= 8)
                return c;
            Console.WriteLine("  Conditie invalida, se pastreaza valoarea implicita.");
            return valImplicita;
        }

        private float CitesteDurata()
        {
            Console.Write("Durata (minute, ex: 3.5 = 3m30s): ");
            float.TryParse((Console.ReadLine() ?? string.Empty).Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out float durata);
            return durata;
        }

        private GenMuzical CitesteGen()
        {
            Console.WriteLine("Gen muzical (combina cu +, ex: 1+8 pentru Rock+Blues):");
            Console.WriteLine("  1=Rock  2=Jazz  4=Pop  8=Blues  16=Electronic");
            Console.WriteLine("  32=HipHop  64=Clasic  128=RnB  256=Soul  512=Altele");
            Console.Write("Alegeti gen: ");
            string[] parti = (Console.ReadLine() ?? string.Empty).Split('+');
            GenMuzical genAles = GenMuzical.Necunoscut;
            foreach (string parte in parti)
            {
                if (Enum.TryParse<GenMuzical>(parte.Trim(), out GenMuzical g) && g != GenMuzical.Necunoscut)
                    genAles |= g;
            }
            return genAles;
        }

        private FormatVinyl CitesteFormat()
        {
            Console.WriteLine("Format vinil (combina cu +, ex: 1+16 pentru LP colorat):");
            Console.WriteLine("  1=LP  2=EP  4=Single  8=Reissue  16=Colored  32=PictureDisc");
            Console.Write("Alegeti format: ");
            string[] parti = (Console.ReadLine() ?? string.Empty).Split('+');
            FormatVinyl formatAles = FormatVinyl.Nedefinit;
            foreach (string parte in parti)
            {
                if (Enum.TryParse<FormatVinyl>(parte.Trim(), out FormatVinyl f))
                    formatAles |= f;
            }
            return formatAles == FormatVinyl.Nedefinit ? FormatVinyl.LP : formatAles;
        }

        private StareMembru CitesteStareMembru()
        {
            Console.Write("Stare membru (Activ/Inactiv/VIP) [implicit Activ]: ");
            string input = Console.ReadLine() ?? string.Empty;
            return Enum.TryParse<StareMembru>(input, true, out StareMembru stare) ? stare : StareMembru.Activ;
        }

        private RolPersoana CitesteRol(RolPersoana valImplicita = RolPersoana.Colectionar)
        {
            Console.Write("Rol (Colectionar/Imprumutator/Vanzator/Cumparator): ");
            return Enum.TryParse<RolPersoana>(Console.ReadLine() ?? string.Empty, true, out RolPersoana rol) ? rol : valImplicita;
        }

        private void AfiseazaListaVinyluri(List<Vinyl> discuri, string titlu)
        {
            Console.WriteLine($"\n--- {titlu} ({discuri.Count} inregistrari) ---");
            if (discuri.Count == 0) 
            { 
                Console.WriteLine("Nicio intrare de afisat."); 
                return; 
            }
            foreach (var (v, i) in discuri.Select((v, i) => (v, i + 1)))
            {
                Console.WriteLine($"\n=== {i} ===");
                Console.WriteLine(v.Info());
            }
        }

        private void AdaugaVinyl()
        {
            Console.WriteLine("\n--- ADAUGARE DISC NOU ---");
            Console.Write("Titlu album: "); 
            string titlu  = Console.ReadLine() ?? string.Empty;
            Console.Write("Artist: ");      
            string artist = Console.ReadLine() ?? string.Empty;

            int anLansare = CitesteAn();
            int conditie  = CitesteConditie();

            var discNou = new Vinyl(titlu, artist, anLansare, 0)
            {
                CodConditie = conditie,
                Gen = CitesteGen(),
                Format = CitesteFormat()
            };

            Console.Write("\nCate melodii are albumul? ");
            if (int.TryParse(Console.ReadLine(), out int nrMelodii) && nrMelodii > 0)
            {
                var melodii = new List<Melodie>();
                for (int i = 0; i < nrMelodii; i++)
                {
                    Console.Write($"Titlu melodie {i + 1}: "); string numePiesa = Console.ReadLine() ?? string.Empty;
                    Console.Write("Featuring (Enter daca e solo): "); string feat = Console.ReadLine() ?? string.Empty;
                    melodii.Add(new Melodie(numePiesa, feat, CitesteDurata()));
                }
                discNou.Melodii = melodii.ToArray();
            }

            Console.Write("\nEste acest disc imprumutat cuiva? (da/nu): ");
            if ((Console.ReadLine() ?? string.Empty).Trim().ToLower() == "da")
            {
                Console.Write("Numele persoanei: "); 
                string numePers = Console.ReadLine() ?? string.Empty;
                repoVinyluri.ImprumutaVinyl(discNou, new Persoana(numePers, string.Empty, RolPersoana.Imprumutator, CitesteStareMembru()));
            }

            repoVinyluri.Adauga(discNou);
            Console.WriteLine("Disc adaugat si salvat cu succes!");
        }

        private void CautaDupaTitlu()
        {
            Console.Write("\nTitlul cautat: ");
            string titlu = Console.ReadLine() ?? string.Empty;
            AfiseazaListaVinyluri(repoVinyluri.CautaDupaTitlu(titlu), $"REZULTATE: \"{titlu}\"");
        }

        private void CautaDupaArtist()
        {
            Console.Write("\nArtistul cautat: ");
            string artist = Console.ReadLine() ?? string.Empty;
            AfiseazaListaVinyluri(repoVinyluri.CautaDupaArtist(artist), $"REZULTATE: \"{artist}\"");
        }

        private void CautaDupaGen()
        {
            GenMuzical gen = CitesteGen();
            string genStr  = gen == GenMuzical.Necunoscut ? "Necunoscut" : gen.ToString();
            AfiseazaListaVinyluri(repoVinyluri.CautaDupaGen(gen), $"REZULTATE GEN: {genStr}");
        }

        private void AfiseazaCeleMaiRecente()
        {
            Console.Write("\nCate vinyluri sa afiseze (top N): ");
            if (int.TryParse(Console.ReadLine(), out int n) && n > 0)
                AfiseazaListaVinyluri(repoVinyluri.ObtineCeleMaiRecente(n), $"TOP {n} CELE MAI RECENTE");
            else
                Console.WriteLine("Numar invalid.");
        }

        private void ModificaVinyl()
        {
            Console.Write("\nTitlul vinylului de modificat: ");
            string titlu = Console.ReadLine() ?? string.Empty;
            var gasite = repoVinyluri.CautaDupaTitlu(titlu);
            if (gasite.Count == 0) { Console.WriteLine("Vinyl negasit."); return; }

            var original = gasite[0];
            Console.WriteLine("Vinyl gasit:"); Console.WriteLine(original.Info());
            Console.WriteLine("Datele noi (Enter = pastram valoarea curenta):");

            Console.Write("Titlu: ");  string titluNou  = Console.ReadLine() ?? string.Empty;
            Console.Write("Artist: "); string artistNou = Console.ReadLine() ?? string.Empty;

            int anNou = CitesteAn(original.An_Lansare);
            int conditieNou = CitesteConditie(original.CodConditie);

            var modificat = new Vinyl(
                string.IsNullOrWhiteSpace(titluNou)  ? original.Titlu  : titluNou,
                string.IsNullOrWhiteSpace(artistNou) ? original.Artist : artistNou,
                anNou, original.Pret)
            {
                CodConditie = conditieNou,
                Gen = original.Gen,
                Format = original.Format,
                Melodii = original.Melodii,
                CaleAudioSnippet = original.CaleAudioSnippet
            };
            if (original.EsteImprumutat)
                modificat.NumeImprumutat = original.NumeImprumutat;

            Console.WriteLine(repoVinyluri.ModificaVinyl(titlu, modificat) ? "Vinyl modificat si salvat cu succes!" : "Eroare la modificare.");
        }

        private void StergeVinyl()
        {
            Console.Write("\nTitlul vinylului de sters: ");
            string titlu = Console.ReadLine() ?? string.Empty;
            Console.WriteLine(repoVinyluri.StergeVinyl(titlu) ? $"Vinylul '{titlu}' a fost sters." : "Vinyl negasit.");
        }

        private void ReturneazaVinyl()
        {
            Console.Write("\nTitlul vinylului de returnat: ");
            string titlu = Console.ReadLine() ?? string.Empty;
            Console.WriteLine(repoVinyluri.ReturnazaVinyl(titlu) ? $"Vinylul '{titlu}' a fost returnat in colectie." : "Vinyl negasit sau nu este imprumutat.");
        }

        private void MeniuPersone()
        {
            string opt;
            do
            {
                Console.WriteLine("\n--- GESTIUNE PERSOANE ---");
                Console.WriteLine("1. Adauga persoana");
                Console.WriteLine("2. Afiseaza toate persoanele");
                Console.WriteLine("3. Cauta persoana dupa nume");
                Console.WriteLine("4. Modifica persoana");
                Console.WriteLine("5. Sterge persoana");
                Console.WriteLine("0. Inapoi");
                opt = Console.ReadLine()?.Trim() ?? string.Empty;

                switch (opt)
                {
                    case "1": AdaugaPersoana();                                                                  
                        break;
                    case "2": AfiseazaListaPersone(repoPersone.ObtineToti(), "TOATE PERSOANELE");               
                        break;
                    case "3": CautaPersoana();                                                                   
                        break;
                    case "4": ModificaPersoana();                                                                
                        break;
                    case "5": StergePersoana();                                                                  
                        break;
                    case "0": 
                        break;
                    default:  Console.WriteLine("Optiune invalida."); 
                        break;
                }
            } while (opt != "0");
        }

        private void AfiseazaListaPersone(List<Persoana> persoane, string titlu)
        {
            Console.WriteLine($"\n--- {titlu} ({persoane.Count} inregistrari) ---");
            if (persoane.Count == 0) 
            { 
                Console.WriteLine("Nicio persoana de afisat."); 
                return; 
            }
            foreach (var (p, i) in persoane.Select((p, i) => (p, i + 1)))
            {
                Console.WriteLine($"\n=== {i} ===");
                Console.WriteLine(p.Info());
            }
        }

        private void AdaugaPersoana()
        {
            Console.WriteLine("\n--- ADAUGARE PERSOANA ---");
            Console.Write("Nume: ");
            string nume    = Console.ReadLine() ?? string.Empty;
            Console.Write("Contact: "); 
            string contact = Console.ReadLine() ?? string.Empty;
            repoPersone.Adauga(new Persoana(nume, contact, CitesteRol(), CitesteStareMembru()));
            Console.WriteLine("Persoana adaugata si salvata cu succes!");
        }

        private void CautaPersoana()
        {
            Console.Write("\nNumele persoanei cautate: ");
            string nume = Console.ReadLine() ?? string.Empty;
            var p = repoPersone.CautaDupaNumePersoana(nume);
            Console.WriteLine(p == null ? "Persoana negasita." : "\n" + p.Info());
        }

        private void ModificaPersoana()
        {
            Console.Write("\nNumele persoanei de modificat: ");
            string numeCurent = Console.ReadLine() ?? string.Empty;
            var original = repoPersone.CautaDupaNumePersoana(numeCurent);
            if (original == null) 
            { 
                Console.WriteLine("Persoana negasita."); 
                return; 
            }
            Console.WriteLine("Persoana gasita:"); Console.WriteLine(original.Info());
            Console.WriteLine("Datele noi (Enter = pastram valoarea curenta):");

            Console.Write("Nume: ");    
            string numeNou = Console.ReadLine() ?? string.Empty;
            Console.Write("Contact: "); 
            string contactNou = Console.ReadLine() ?? string.Empty;

            var noua = new Persoana(
                string.IsNullOrWhiteSpace(numeNou) ? original.Nume : numeNou,
                string.IsNullOrWhiteSpace(contactNou) ? original.Contact : contactNou,
                CitesteRol(original.Rol),
                CitesteStareMembru())
            {
                DiscuriImprumutate = original.DiscuriImprumutate
            };

            Console.WriteLine(repoPersone.ModificaPersoana(numeCurent, noua) ? "Persoana modificata si salvata cu succes!": "Eroare la modificare.");
        }

        private void StergePersoana()
        {
            Console.Write("\nNumele persoanei de sters: ");
            string nume = Console.ReadLine() ?? string.Empty;
            Console.WriteLine(repoPersone.StergePersoana(nume) ? $"Persoana '{nume}' a fost stearsa." : "Persoana negasita.");
        }
    }
}
