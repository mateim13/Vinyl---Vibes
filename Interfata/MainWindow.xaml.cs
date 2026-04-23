using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibrarieModele;
using NivelStocareDate;

namespace Interfata
{
    public partial class MainWindow : Window
    {

        private const int MAX_ARTIST = 50;
        private const int MAX_TITLU = 100;
        private const int AN_MIN = 1900;
        private const int AN_MAX_OFFSET = 0;
        private const float PRET_MAX = 99999f;
        private const int MAX_NUME_IMP = 50;

        private const int ERR_OK = 0;
        private const int ERR_ARTIST_GOL = 1;
        private const int ERR_ARTIST_LUNG = 2;
        private const int ERR_TITLU_GOL = 4;
        private const int ERR_TITLU_LUNG = 8;
        private const int ERR_AN_INVALID = 16;
        private const int ERR_PRET_INVALID = 32;
        private const int ERR_GEN_NESET = 64;
        private const int ERR_FORMAT_NESET = 128;
        private const int ERR_CONDITIE_NESET = 256;
        private const int ERR_NUME_IMP_GOL = 512;

        private static readonly Brush CULOARE_NORMALA = Brushes.White;
        private static readonly Brush CULOARE_EROARE = new SolidColorBrush(Color.FromRgb(255, 107, 107));

        private readonly RepoVinyluri repo = new RepoVinyluri(@"E:\PIU\Git Desktop Repos\Vinyl---Vibes\vinyluri.txt");

        private record VinylVM(string DisplayNume, string DisplayDetalii, string DisplayStare);

        public MainWindow()
        {
            InitializeComponent();
            IncarcaLista();
        }

        private void IncarcaLista()
        {
            var items = repo.ObtineToti()
                .Select(v => new VinylVM(
                    $"{v.Artist}  –  {v.Titlu}  ({v.An_Lansare})",
                    $"{v.Gen}  ·  {v.Format}  ·  {v.ConditieDisc}  ·  {v.DurataTotalaFormatata}",
                    v.EsteImprumutat ? $"📤 {v.NumeImprumutat}" : "📥 În raft"
                )).ToList();

            lstVinyluri.ItemsSource = items;

            int nr = items.Count;
            TbNrVinyluri.Text = nr == 1 ? "1 vinyl" : $"{nr} vinyluri";
            BdrListaGoala.Visibility = nr == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ChkImprumutat_Changed(object sender, RoutedEventArgs e)
        {
            PanelNumeImprumutat.Visibility = ChkImprumutat.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            if (ChkImprumutat.IsChecked != true)
            {
                TxtNumeImprumutat.Text = string.Empty;
                AscundeMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
            }
        }

        private int CitesteTagComboBox(ComboBox cmb)
        {
            if (cmb.SelectedItem is ComboBoxItem item &&
                int.TryParse(item.Tag?.ToString(), out int val))
                return val;
            return 0;
        }

        private int ValideazaDateVinyl()
        {
            int cod = ERR_OK;
            int anMaxim = DateTime.Now.Year;

            string artist = TxtArtist.Text.Trim();
            if (string.IsNullOrEmpty(artist))
                cod |= ERR_ARTIST_GOL;
            else if (artist.Length > MAX_ARTIST)
                cod |= ERR_ARTIST_LUNG;

            string titlu = TxtTitlu.Text.Trim();
            if (string.IsNullOrEmpty(titlu))
                cod |= ERR_TITLU_GOL;
            else if (titlu.Length > MAX_TITLU)
                cod |= ERR_TITLU_LUNG;

            if (!int.TryParse(TxtAn.Text.Trim(), out int an) || an < AN_MIN || an > anMaxim)
                cod |= ERR_AN_INVALID;

            if (!float.TryParse(TxtPret.Text.Trim(),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float pret)
                || pret < 0 || pret > PRET_MAX)
                cod |= ERR_PRET_INVALID;

            if (CitesteTagComboBox(CmbGen) == 0)
                cod |= ERR_GEN_NESET;

            if (CitesteTagComboBox(CmbFormat) == 0)
                cod |= ERR_FORMAT_NESET;

            if (CitesteTagComboBox(CmbConditie) == 0)
                cod |= ERR_CONDITIE_NESET;

            if (ChkImprumutat.IsChecked == true && string.IsNullOrEmpty(TxtNumeImprumutat.Text.Trim()))
                cod |= ERR_NUME_IMP_GOL;

            return cod;
        }

        private void AfiseazaMesajEroare(TextBlock tb, Label lbl)
        {
            tb.Visibility  = Visibility.Visible;
            lbl.Foreground = CULOARE_EROARE;
        }

        private void AscundeMesajEroare(TextBlock tb, Label lbl)
        {
            tb.Visibility  = Visibility.Collapsed;
            lbl.Foreground = CULOARE_NORMALA;
        }

        private void AplicaEroriUI(int cod)
        {
            int anMaxim = DateTime.Now.Year;

            if ((cod & ERR_ARTIST_GOL) != 0)
            {
                TbErrArtist.Text = "Câmpul Artist este obligatoriu!";
                AfiseazaMesajEroare(TbErrArtist, LblArtist);
            }
            else if ((cod & ERR_ARTIST_LUNG) != 0)
            {
                TbErrArtist.Text = $"Artistul nu poate depăși {MAX_ARTIST} caractere!";
                AfiseazaMesajEroare(TbErrArtist, LblArtist);
            }
            else AscundeMesajEroare(TbErrArtist, LblArtist);

            if ((cod & ERR_TITLU_GOL) != 0)
            {
                TbErrTitlu.Text = "Câmpul Titlu este obligatoriu!";
                AfiseazaMesajEroare(TbErrTitlu, LblTitlu);
            }
            else if ((cod & ERR_TITLU_LUNG) != 0)
            {
                TbErrTitlu.Text = $"Titlul nu poate depăși {MAX_TITLU} caractere!";
                AfiseazaMesajEroare(TbErrTitlu, LblTitlu);
            }
            else AscundeMesajEroare(TbErrTitlu, LblTitlu);

            if ((cod & ERR_AN_INVALID) != 0)
            {
                TbErrAn.Text = $"Introduceți un an valid ({AN_MIN}–{anMaxim})!";
                AfiseazaMesajEroare(TbErrAn, LblAn);
            }
            else AscundeMesajEroare(TbErrAn, LblAn);

            if ((cod & ERR_PRET_INVALID) != 0)
            {
                TbErrPret.Text = $"Introduceți un preț valid (0–{PRET_MAX}) în format 12.50!";
                AfiseazaMesajEroare(TbErrPret, LblPret);
            }
            else AscundeMesajEroare(TbErrPret, LblPret);

            if ((cod & ERR_GEN_NESET) != 0)
                AfiseazaMesajEroare(TbErrGen, LblGen);
            else
                AscundeMesajEroare(TbErrGen, LblGen);

            if ((cod & ERR_FORMAT_NESET) != 0)
                AfiseazaMesajEroare(TbErrFormat, LblFormat);
            else
                AscundeMesajEroare(TbErrFormat, LblFormat);

            if ((cod & ERR_CONDITIE_NESET) != 0)
                AfiseazaMesajEroare(TbErrConditie, LblConditie);
            else
                AscundeMesajEroare(TbErrConditie, LblConditie);

            if ((cod & ERR_NUME_IMP_GOL) != 0)
                AfiseazaMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
            else
                AscundeMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
        }

        private void OnAdauga(object sender, RoutedEventArgs e)
        {
            TbMesajSucces.Visibility = Visibility.Collapsed;

            int cod = ValideazaDateVinyl();
            AplicaEroriUI(cod);

            if (cod != ERR_OK)
                return;

            int.TryParse(TxtAn.Text.Trim(), out int an);
            float.TryParse(TxtPret.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float pret);

            int genTag = CitesteTagComboBox(CmbGen);
            int formatTag = CitesteTagComboBox(CmbFormat);
            int conditieCod = CitesteTagComboBox(CmbConditie);

            var vinyl = new Vinyl
            {
                Artist = TxtArtist.Text.Trim(),
                Titlu = TxtTitlu.Text.Trim(),
                An_Lansare = an,
                Pret = pret,
                Gen = (GenMuzical)genTag,
                Format = (FormatVinyl)formatTag,
                CodConditie  = conditieCod,
                Melodii = new Melodie[0]
            };

            if (ChkImprumutat.IsChecked == true)
                vinyl.NumeImprumutat = TxtNumeImprumutat.Text.Trim();

            repo.Adauga(vinyl);
            IncarcaLista();

            TbMesajSucces.Text = $"Vinylul \"{vinyl.Artist} – {vinyl.Titlu}\" a fost adăugat cu succes!";
            TbMesajSucces.Visibility = Visibility.Visible;

            GoliesteCampuri();
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            GoliesteCampuri();
            AscundeTotiErorii();
            TbMesajSucces.Visibility = Visibility.Collapsed;
        }

        private void GoliesteCampuri()
        {
            TxtArtist.Text = string.Empty;
            TxtTitlu.Text = string.Empty;
            TxtAn.Text = string.Empty;
            TxtPret.Text = string.Empty;
            TxtNumeImprumutat.Text = string.Empty;

            CmbGen.SelectedIndex = 0;
            CmbFormat.SelectedIndex = 0;
            CmbConditie.SelectedIndex = 0;

            ChkImprumutat.IsChecked = false;
            PanelNumeImprumutat.Visibility = Visibility.Collapsed;
        }

        private void AscundeTotiErorii()
        {
            AscundeMesajEroare(TbErrArtist, LblArtist);
            AscundeMesajEroare(TbErrTitlu, LblTitlu);
            AscundeMesajEroare(TbErrAn, LblAn);
            AscundeMesajEroare(TbErrPret, LblPret);
            AscundeMesajEroare(TbErrGen, LblGen);
            AscundeMesajEroare(TbErrFormat, LblFormat);
            AscundeMesajEroare(TbErrConditie, LblConditie);
            AscundeMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
        }
    }
}