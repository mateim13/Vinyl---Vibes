using System;

namespace vinyl
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
                Console.WriteLine("B. Afiseaza toate vinylurile");
                Console.WriteLine("C. Cauta un vinyl dupa titlu");
                Console.WriteLine("D. Cauta un vinyl dupa artist");
                Console.WriteLine("X. Iesire");
                optiune = Console.ReadLine().ToUpper();

                switch (optiune)
                {
                    case "A":
                        Console.WriteLine("\n--- ADAUGARE DISC NOU ---");
                        Console.Write("Titlu album: ");
                        string titlu = Console.ReadLine();
                        Console.Write("Artist: ");
                        string artist = Console.ReadLine();
                        Console.Write("An lansare: ");
                        int.TryParse(Console.ReadLine(), out int anLansare);
                        Console.Write("Cod conditie (1-8): ");
                        int.TryParse(Console.ReadLine(), out int conditie);
                        Vinyl discNou = new Vinyl(titlu, artist, anLansare, 0)
                        {
                            CodConditie = conditie
                        };
                        Console.Write("Cate melodii are albumul? ");
                        if (int.TryParse(Console.ReadLine(), out int nrMelodii) && nrMelodii > 0)
                        {
                            string[] tracklist = new string[nrMelodii];
                            for (int i = 0; i < nrMelodii; i++)
                            {
                                Console.Write($"Nume melodie {i + 1}: ");
                                string numePiesa = Console.ReadLine();

                                Console.Write("Featuring (lasati liber si apasati Enter daca e piesa solo): ");
                                string feat = Console.ReadLine();

                                if (!string.IsNullOrWhiteSpace(feat))
                                {
                                    tracklist[i] = $"{numePiesa} (feat. {feat})";
                                }
                                else
                                {
                                    tracklist[i] = numePiesa;
                                }
                            }
                            discNou.Melodii = tracklist;
                        }
                        Console.Write("\nEste acest disc imprumutat cuiva? (da/nu): ");
                        if(Console.ReadLine().ToLower() == "da")
                        {
                            Console.Write("Numele persoanei care l-a luat: ");
                            discNou.NumeImprumutat = Console.ReadLine();
                        }
                        adminVinyluri.AddVinyl(discNou);
                        Console.WriteLine("Disc adaugat cu succes!");
                        break;

                    case "B":
                        Console.WriteLine("\n--- COLECTIE VINYLURI ---");
                        List<Vinyl> discuri = adminVinyluri.GetAllVinyls();
                        if(discuri.Count == 0)
                        {
                            Console.WriteLine("Colectia este goala.");
                        }
                        else
                                                    {
                            foreach (Vinyl disc in discuri)
                            {
                                Console.WriteLine(disc.Info());
                            }
                        }
                        break;
                    case "C":
                        Console.Write("\nIntroduceti titlul cautat: ");
                        string titluCautat = Console.ReadLine();
                        List<Vinyl> gasiteDupaTitlu = adminVinyluri.GetVinylByTitle(titluCautat);
                        if (gasiteDupaTitlu.Count == 0)
                        {
                            Console.WriteLine("Nu s-au gasit discuri cu acest titlu.");
                        }
                        else
                        {
                            Console.WriteLine($"\nS-au gasit {gasiteDupaTitlu.Count} rezultate:");
                            foreach (Vinyl disc in gasiteDupaTitlu)
                            {
                                Console.WriteLine(disc.Info());
                            }
                        }
                        break;
                    case "D":
                        Console.Write("\nIntroduceti artistul cautat: ");
                        string artistCautat = Console.ReadLine();
                        List<Vinyl> gasiteDupaArtist = adminVinyluri.GetVinylByArtist(artistCautat);
                        if(gasiteDupaArtist.Count == 0)
                        {
                            Console.WriteLine("Nu s-au gasit vinyluri de la acest artist.");
                        }
                        else
                        {
                            Console.WriteLine($"\nS-au gasit {gasiteDupaArtist.Count} rezultate:");
                            foreach (Vinyl v in gasiteDupaArtist) 
                                Console.WriteLine(v.Info());
                        }
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
    }
}