using System;

namespace vinyl
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- Gestiune Colectie si Imprumuturi ---");

            Console.Write("Titlu Album: ");
            string titlu = Console.ReadLine();

            Console.Write("Artist: ");
            string artist = Console.ReadLine();

            Console.Write("An lansare: ");
            int an = int.Parse(Console.ReadLine());

            Console.Write("Cod conditie (1-8): ");
            int conditie = int.Parse(Console.ReadLine());
   
            Console.Write("Cale fisier audio snippet (ex: C:/muzica/piesa.mp3): ");
            string caleAudio = Console.ReadLine();
 
            Vinyl disc = new Vinyl(titlu, artist, an, 0);
            disc.CodConditie = conditie;
            disc.CaleAudioSnippet = caleAudio;
          
            Console.WriteLine("Introduceti melodiile (separate prin virgula):");
            string lista = Console.ReadLine();
            disc.Tracklist(lista);
   
            Console.Write("\nEste acest disc imprumutat cuiva? (da/nu): ");
            string raspuns = Console.ReadLine().ToLower();

            if (raspuns == "da")
            {
                Console.Write("Numele persoanei care l-a luat: ");
                string numePersoana = Console.ReadLine();

                Console.Write("Contact (telefon/email): ");
                string contactPersoana = Console.ReadLine();

                Persoana Persoana = new Persoana(numePersoana, contactPersoana);

                disc.NumeImprumutat = Persoana.Nume;

                Persoana.DiscuriImprumutate = new string[] { disc.Titlu };

                Console.WriteLine("\n--- Status Actualizat ---");
                Console.WriteLine(Persoana.InfoPersoana());
            }

            Console.WriteLine("\n--- DETALII VINYL DIN SISTEM ---");
            Console.WriteLine(disc.Info());

            Console.WriteLine("Apasati orice tasta pentru inchidere...");
            Console.ReadKey();
        }
    }
}