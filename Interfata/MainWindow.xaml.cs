using System;
using System.Collections.Generic;
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
        private const int   MAX_ARTIST       = 50;
        private const int   MAX_TITLU        = 100;
        private const int   AN_MIN           = 1900;
        private const float PRET_MAX         = 99999f;

        private const int ERR_OK             = 0;
        private const int ERR_ARTIST_GOL     = 1;
        private const int ERR_ARTIST_LUNG    = 2;
        private const int ERR_TITLU_GOL      = 4;
        private const int ERR_TITLU_LUNG     = 8;
        private const int ERR_AN_INVALID     = 16;
        private const int ERR_PRET_INVALID   = 32;
        private const int ERR_GEN_NESET      = 64;
        private const int ERR_FORMAT_NESET   = 128;
        private const int ERR_CONDITIE_NESET = 256;
        private const int ERR_NUME_IMP_GOL   = 512;

        private static readonly Brush CULOARE_NORMALA = Brushes.White;
        private static readonly Brush CULOARE_EROARE  =
            new SolidColorBrush(Color.FromRgb(255, 107, 107));

        private bool   _menuExtins  = false;
        private const double MENU_RESTRINS = 50;
        private const double MENU_EXTINS   = 200;

        private IEnumerable<TextBlock> NavLabels =>
            new[] { NavLblAdauga, NavLblColectie, NavLblImprumutat };

        private enum Tab { Adauga, Colectie, Imprumutat }
        private Tab _tabCurent = Tab.Adauga;

        private readonly RepoVinyluri repo =
            new RepoVinyluri(@"E:\PIU\Git Desktop Repos\Vinyl---Vibes\vinyluri.txt");

        private List<VinylVM> _toateVinylurile = new List<VinylVM>();

        private record VinylVM(
            string DisplayNume,
            string DisplayDetalii,
            string DisplayStare,
            string Artist,
            string Titlu);

        public MainWindow()
        {
            InitializeComponent();
            IncarcaDate();
            ActualizeazaBadge();
        }

        private void IncarcaDate()
        {
            _toateVinylurile = repo.ObtineToti()
                .Select(v => new VinylVM(
                    DisplayNume:    $"{v.Artist}  –  {v.Titlu}  ({v.An_Lansare})",
                    DisplayDetalii: $"{v.Gen}  ·  {v.Format}  ·  {v.ConditieDisc}  ·  {v.DurataTotalaFormatata}",
                    DisplayStare:   v.EsteImprumutat ? $"📤 {v.NumeImprumutat}" : "📥 În raft",
                    Artist:         v.Artist,
                    Titlu:          v.Titlu
                )).ToList();
        }

        private void ActualizeazaBadge()
        {
            int nr = _toateVinylurile.Count;
            TbNrVinyluri.Text = nr == 1 ? "1 vinyl" : $"{nr} vinyluri";
        }

        private void SetTab(Tab tab)
        {
            _tabCurent = tab;

            TabAdauga.Visibility    = Visibility.Collapsed;
            TabColectie.Visibility  = Visibility.Collapsed;
            TabImprumutat.Visibility = Visibility.Collapsed;

            BtnNavAdauga.Style     = (Style)FindResource("NavItemButton");
            BtnNavColectie.Style   = (Style)FindResource("NavItemButton");
            BtnNavImprumutat.Style = (Style)FindResource("NavItemButton");

            switch (tab)
            {
                case Tab.Adauga:
                    TabAdauga.Visibility    = Visibility.Visible;
                    BtnNavAdauga.Style      = (Style)FindResource("NavItemButtonActive");
                    break;

                case Tab.Colectie:
                    TabColectie.Visibility  = Visibility.Visible;
                    BtnNavColectie.Style    = (Style)FindResource("NavItemButtonActive");
                    IncarcaDate();
                    AplicaFiltruCautare();
                    ActualizeazaBadge();
                    break;

                case Tab.Imprumutat:
                    TabImprumutat.Visibility = Visibility.Visible;
                    BtnNavImprumutat.Style   = (Style)FindResource("NavItemButtonActive");
                    IncarcaDate();
                    AplicaListaImprumutat();
                    break;
            }
        }

        private void OnNavAdauga(object sender, RoutedEventArgs e)     => SetTab(Tab.Adauga);
        private void OnNavColectie(object sender, RoutedEventArgs e)   => SetTab(Tab.Colectie);
        private void OnNavImprumutat(object sender, RoutedEventArgs e) => SetTab(Tab.Imprumutat);

        private void AplicaFiltruCautare()
        {
            string termen = TxtCautare?.Text?.Trim() ?? string.Empty;

            var items = string.IsNullOrEmpty(termen)
                ? _toateVinylurile
                : _toateVinylurile
                    .Where(v =>
                        v.Artist.Contains(termen, StringComparison.OrdinalIgnoreCase) ||
                        v.Titlu .Contains(termen, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            lstVinyluri.ItemsSource = items;

            bool gol = !items.Any();
            BdrListaGoala.Visibility = gol ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnCautareChanged(object sender, TextChangedEventArgs e)
        {
            AplicaFiltruCautare();
        }

        private void AplicaListaImprumutat()
        {
            var items = _toateVinylurile
                .Where(v => v.DisplayStare.StartsWith("📤"))
                .ToList();

            lstImprumutat.ItemsSource = items;
            BdrImpGoala.Visibility = items.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnHamburgerClick(object sender, RoutedEventArgs e)
        {
            _menuExtins = !_menuExtins;
            ColMenu.Width = new GridLength(_menuExtins ? MENU_EXTINS : MENU_RESTRINS);

            var viz = _menuExtins ? Visibility.Visible : Visibility.Collapsed;
            foreach (var lbl in NavLabels)
                lbl.Visibility = viz;
        }

        private void ChkImprumutat_Changed(object sender, RoutedEventArgs e)
        {
            PanelNumeImprumutat.Visibility =
                ChkImprumutat.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            if (ChkImprumutat.IsChecked != true)
            {
                TxtNumeImprumutat.Text = string.Empty;
                AscundeMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
            }
        }

        private int CitesteGenuri()
        {
            int gen = 0;
            if (ChkRock.IsChecked       == true) gen |= (int)GenMuzical.Rock;
            if (ChkJazz.IsChecked       == true) gen |= (int)GenMuzical.Jazz;
            if (ChkPop.IsChecked        == true) gen |= (int)GenMuzical.Pop;
            if (ChkBlues.IsChecked      == true) gen |= (int)GenMuzical.Blues;
            if (ChkElectronic.IsChecked == true) gen |= (int)GenMuzical.Electronic;
            if (ChkHipHop.IsChecked     == true) gen |= (int)GenMuzical.HipHop;
            if (ChkClasic.IsChecked     == true) gen |= (int)GenMuzical.Clasic;
            if (ChkRnB.IsChecked        == true) gen |= (int)GenMuzical.RnB;
            if (ChkSoul.IsChecked       == true) gen |= (int)GenMuzical.Soul;
            if (ChkAltele.IsChecked     == true) gen |= (int)GenMuzical.Altele;
            return gen;
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
            int cod     = ERR_OK;
            int anMaxim = DateTime.Now.Year;

            string artist = TxtArtist.Text.Trim();
            if (string.IsNullOrEmpty(artist))     cod |= ERR_ARTIST_GOL;
            else if (artist.Length > MAX_ARTIST)  cod |= ERR_ARTIST_LUNG;

            string titlu = TxtTitlu.Text.Trim();
            if (string.IsNullOrEmpty(titlu))      cod |= ERR_TITLU_GOL;
            else if (titlu.Length > MAX_TITLU)    cod |= ERR_TITLU_LUNG;

            if (!int.TryParse(TxtAn.Text.Trim(), out int an) || an < AN_MIN || an > anMaxim)
                cod |= ERR_AN_INVALID;

            if (!float.TryParse(TxtPret.Text.Trim(),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float pret)
                || pret < 0 || pret > PRET_MAX)
                cod |= ERR_PRET_INVALID;

            if (CitesteGenuri()                 == 0) cod |= ERR_GEN_NESET;
            if (CitesteTagComboBox(CmbFormat)   == 0) cod |= ERR_FORMAT_NESET;
            if (CitesteTagComboBox(CmbConditie) == 0) cod |= ERR_CONDITIE_NESET;

            if (ChkImprumutat.IsChecked == true &&
                string.IsNullOrEmpty(TxtNumeImprumutat.Text.Trim()))
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
            { TbErrArtist.Text = "Câmpul Artist este obligatoriu!"; AfiseazaMesajEroare(TbErrArtist, LblArtist); }
            else if ((cod & ERR_ARTIST_LUNG) != 0)
            { TbErrArtist.Text = $"Artistul nu poate depăși {MAX_ARTIST} caractere!"; AfiseazaMesajEroare(TbErrArtist, LblArtist); }
            else AscundeMesajEroare(TbErrArtist, LblArtist);

            if ((cod & ERR_TITLU_GOL) != 0)
            { TbErrTitlu.Text = "Câmpul Titlu este obligatoriu!"; AfiseazaMesajEroare(TbErrTitlu, LblTitlu); }
            else if ((cod & ERR_TITLU_LUNG) != 0)
            { TbErrTitlu.Text = $"Titlul nu poate depăși {MAX_TITLU} caractere!"; AfiseazaMesajEroare(TbErrTitlu, LblTitlu); }
            else AscundeMesajEroare(TbErrTitlu, LblTitlu);

            if ((cod & ERR_AN_INVALID) != 0)
            { TbErrAn.Text = $"Introduceți un an valid ({AN_MIN}–{anMaxim})!"; AfiseazaMesajEroare(TbErrAn, LblAn); }
            else AscundeMesajEroare(TbErrAn, LblAn);

            if ((cod & ERR_PRET_INVALID) != 0)
            { TbErrPret.Text = $"Preț valid: 0–{PRET_MAX}, format 12.50!"; AfiseazaMesajEroare(TbErrPret, LblPret); }
            else AscundeMesajEroare(TbErrPret, LblPret);

            if ((cod & ERR_GEN_NESET)      != 0) AfiseazaMesajEroare(TbErrGen,           LblGen);
            else                                  AscundeMesajEroare(TbErrGen,            LblGen);

            if ((cod & ERR_FORMAT_NESET)   != 0) AfiseazaMesajEroare(TbErrFormat,        LblFormat);
            else                                  AscundeMesajEroare(TbErrFormat,         LblFormat);

            if ((cod & ERR_CONDITIE_NESET) != 0) AfiseazaMesajEroare(TbErrConditie,      LblConditie);
            else                                  AscundeMesajEroare(TbErrConditie,       LblConditie);

            if ((cod & ERR_NUME_IMP_GOL)   != 0) AfiseazaMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
            else                                  AscundeMesajEroare(TbErrNumeImprumutat,  LblNumeImprumutat);
        }

        private void OnAdauga(object sender, RoutedEventArgs e)
        {
            TbMesajSucces.Visibility = Visibility.Collapsed;

            int cod = ValideazaDateVinyl();
            AplicaEroriUI(cod);
            if (cod != ERR_OK) return;

            int.TryParse  (TxtAn.Text.Trim(),   out int   an);
            float.TryParse(TxtPret.Text.Trim(),
                           NumberStyles.Float, CultureInfo.InvariantCulture, out float pret);

            var vinyl = new Vinyl
            {
                Artist      = TxtArtist.Text.Trim(),
                Titlu       = TxtTitlu.Text.Trim(),
                An_Lansare  = an,
                Pret        = pret,
                Gen         = (GenMuzical) CitesteGenuri(),
                Format      = (FormatVinyl)CitesteTagComboBox(CmbFormat),
                CodConditie = CitesteTagComboBox(CmbConditie),
                Melodii     = new Melodie[0]
            };

            if (ChkImprumutat.IsChecked == true)
                vinyl.NumeImprumutat = TxtNumeImprumutat.Text.Trim();

            repo.Adauga(vinyl);

            IncarcaDate();
            ActualizeazaBadge();

            TbMesajSucces.Text = $"✓  \"{vinyl.Artist} – {vinyl.Titlu}\" adăugat cu succes!";
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
            TxtArtist.Text         = string.Empty;
            TxtTitlu.Text          = string.Empty;
            TxtAn.Text             = string.Empty;
            TxtPret.Text           = string.Empty;
            TxtNumeImprumutat.Text = string.Empty;

            ChkRock.IsChecked       = false;
            ChkJazz.IsChecked       = false;
            ChkPop.IsChecked        = false;
            ChkBlues.IsChecked      = false;
            ChkElectronic.IsChecked = false;
            ChkHipHop.IsChecked     = false;
            ChkClasic.IsChecked     = false;
            ChkRnB.IsChecked        = false;
            ChkSoul.IsChecked       = false;
            ChkAltele.IsChecked     = false;

            CmbFormat.SelectedIndex   = 0;
            CmbConditie.SelectedIndex = 0;

            ChkImprumutat.IsChecked        = false;
            PanelNumeImprumutat.Visibility = Visibility.Collapsed;
        }

        private void AscundeTotiErorii()
        {
            AscundeMesajEroare(TbErrArtist,         LblArtist);
            AscundeMesajEroare(TbErrTitlu,          LblTitlu);
            AscundeMesajEroare(TbErrAn,             LblAn);
            AscundeMesajEroare(TbErrPret,           LblPret);
            AscundeMesajEroare(TbErrGen,            LblGen);
            AscundeMesajEroare(TbErrFormat,         LblFormat);
            AscundeMesajEroare(TbErrConditie,       LblConditie);
            AscundeMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
        }
    }
}
