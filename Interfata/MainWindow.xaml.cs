using System.Windows;
using LibrarieModele;
using NivelStocareDate;
using System.Linq;

namespace Interfata
{
    public partial class MainWindow : Window
    {
        private readonly RepoVinyluri repo = new RepoVinyluri(@"E:\PIU\Git Desktop Repos\Vinyl---Vibes\vinyluri.txt");

        private record VinylVM(string DisplayNume, string DisplayDetalii, string DisplayStare);

        public MainWindow()
        {
            InitializeComponent();
            IncarcaLista();
        }

        private void IncarcaLista()
        {
            lstVinyluri.ItemsSource = repo.ObtineToti()
                .Select(v => new VinylVM(
                    $"{v.Artist}  –  {v.Titlu}  ({v.An_Lansare})",
                    $"Gen: {v.Gen}  |  Format: {v.Format}  |  Condiție: {v.ConditieDisc}  |  Durata: {v.DurataTotalaFormatata}",
                    v.EsteImprumutat ? $"📤 Împrumutat la: {v.NumeImprumutat}" : "📥 În raft"
                )).ToList();
        }
    }
}