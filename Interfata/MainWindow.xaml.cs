using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibrarieModele;
using NivelStocareDate;

namespace Interfata
{
    public class PersoneViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PersonaVM> _persoaneAfisate
            = new ObservableCollection<PersonaVM>();

        public ObservableCollection<PersonaVM> PersoaneAfisate
        {
            get => _persoaneAfisate;
            set { _persoaneAfisate = value; OnPropertyChanged(); }
        }

        public string NrPersoane =>
            _persoaneAfisate.Count == 1 ? "1 persoană" : $"{_persoaneAfisate.Count} persoane";

        public void Refresh(IEnumerable<PersonaVM> sursa)
        {
            _persoaneAfisate.Clear();
            foreach (var p in sursa)
                _persoaneAfisate.Add(p);
            OnPropertyChanged(nameof(NrPersoane));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public record PersonaVM(
        string Nume,
        string DisplayNume,
        string DisplayDetalii,
        string DisplayStare,
        string Stare);

    public partial class MainWindow : Window
    {
        private const int MAX_ARTIST = 50;
        private const int MAX_TITLU = 100;
        private const int AN_MIN = 1900;
        private const float PRET_MAX = 99999f;

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
        private static readonly Brush CULOARE_EROARE =
            new SolidColorBrush(Color.FromRgb(255, 107, 107));

        private bool _menuExtins = false;
        private const double MENU_RESTRINS = 50;
        private const double MENU_EXTINS = 200;

        private IEnumerable<TextBlock> NavLabels =>
            new[] { NavLblAdauga, NavLblColectie, NavLblImprumutat, NavLblEditare, NavLblPersoane };

        private enum Tab { Adauga, Colectie, Imprumutat, Editare, Persoane }
        private Tab _tabCurent = Tab.Adauga;

        private string? _titluOriginalEditat = null;

        private readonly PersoneViewModel _vmPersoane = new PersoneViewModel();
        private string? _numeOriginalEditat = null;

        private readonly RepoVinyluri repo =
            new RepoVinyluri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vinyluri.txt"));

        private readonly RepoPersone repoPersone =
            new RepoPersone(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "persoane.txt"));

        private record VinylVM(
            string DisplayNume,
            string DisplayDetalii,
            string DisplayStare,
            string Artist,
            string Titlu,
            string TitluOriginal);

        private List<VinylVM> _toateVinylurile = new List<VinylVM>();

        public MainWindow()
        {
            InitializeComponent();
            TabPersoane.DataContext = _vmPersoane;
            IncarcaDate();
            ActualizeazaBadge();
        }

        private void IncarcaDate()
        {
            _toateVinylurile = repo.ObtineToti()
                .Select(v =>
                {
                    string dataStr = v.DataAchizitie.HasValue
                        ? $"  ·  Achiziționat: {v.DataAchizitie.Value:dd.MM.yyyy}"
                        : string.Empty;
                    return new VinylVM(
                        DisplayNume: $"{v.Artist}  –  {v.Titlu}  ({v.An_Lansare})",
                        DisplayDetalii: $"{v.Gen}  ·  {v.Format}  ·  {v.ConditieDisc}  ·  {v.DurataTotalaFormatata}{dataStr}",
                        DisplayStare: v.EsteImprumutat ? $"📤 {v.NumeImprumutat}" : "📥 În raft",
                        Artist: v.Artist,
                        Titlu: v.Titlu,
                        TitluOriginal: v.Titlu
                    );
                }).ToList();
        }

        private void ActualizeazaBadge()
        {
            int nr = _toateVinylurile.Count;
            TbNrVinyluri.Text = nr == 1 ? "1 vinyl" : $"{nr} vinyluri";
        }

        private void SetTab(Tab tab)
        {
            _tabCurent = tab;

            TabAdauga.Visibility = Visibility.Collapsed;
            TabColectie.Visibility = Visibility.Collapsed;
            TabImprumutat.Visibility = Visibility.Collapsed;
            TabEditare.Visibility = Visibility.Collapsed;
            TabPersoane.Visibility = Visibility.Collapsed;

            BtnNavEditare.Visibility = tab == Tab.Editare ? Visibility.Visible : Visibility.Collapsed;

            BtnNavAdauga.Style = (Style)FindResource("NavItemButton");
            BtnNavColectie.Style = (Style)FindResource("NavItemButton");
            BtnNavImprumutat.Style = (Style)FindResource("NavItemButton");
            BtnNavPersoane.Style = (Style)FindResource("NavItemButton");

            switch (tab)
            {
                case Tab.Adauga:
                    TabAdauga.Visibility = Visibility.Visible;
                    BtnNavAdauga.Style = (Style)FindResource("NavItemButtonActive");
                    break;

                case Tab.Colectie:
                    TabColectie.Visibility = Visibility.Visible;
                    BtnNavColectie.Style = (Style)FindResource("NavItemButtonActive");
                    IncarcaDate();
                    AplicaFiltruCautare();
                    ActualizeazaBadge();
                    break;

                case Tab.Imprumutat:
                    TabImprumutat.Visibility = Visibility.Visible;
                    BtnNavImprumutat.Style = (Style)FindResource("NavItemButtonActive");
                    IncarcaDate();
                    AplicaListaImprumutat();
                    break;

                case Tab.Editare:
                    TabEditare.Visibility = Visibility.Visible;
                    BtnNavEditare.Style = (Style)FindResource("NavItemButtonActive");
                    break;

                case Tab.Persoane:
                    TabPersoane.Visibility = Visibility.Visible;
                    BtnNavPersoane.Style = (Style)FindResource("NavItemButtonActive");
                    IncarcaPersoane();
                    break;
            }
        }

        private void OnNavAdauga(object sender, RoutedEventArgs e) => SetTab(Tab.Adauga);
        private void OnNavColectie(object sender, RoutedEventArgs e) => SetTab(Tab.Colectie);
        private void OnNavImprumutat(object sender, RoutedEventArgs e) => SetTab(Tab.Imprumutat);
        private void OnNavPersoane(object sender, RoutedEventArgs e) => SetTab(Tab.Persoane);

        private void OnNavInapoiDinEditare(object sender, RoutedEventArgs e)
        {
            _titluOriginalEditat = null;
            TbMesajSuccesEdit.Visibility = Visibility.Collapsed;
            TbMesajEroareEdit.Visibility = Visibility.Collapsed;
            SetTab(Tab.Colectie);
        }

        private void AplicaFiltruCautare()
        {
            string termen = TxtCautare?.Text?.Trim() ?? string.Empty;

            var items = string.IsNullOrEmpty(termen)
                ? _toateVinylurile
                : _toateVinylurile
                    .Where(v =>
                        v.Artist.Contains(termen, StringComparison.OrdinalIgnoreCase) ||
                        v.Titlu.Contains(termen, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            lstVinyluri.ItemsSource = items;
            BdrListaGoala.Visibility = items.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnCautareChanged(object sender, TextChangedEventArgs e)
            => AplicaFiltruCautare();

        private void AplicaListaImprumutat()
        {
            var items = _toateVinylurile
                .Where(v => v.DisplayStare.StartsWith("📤"))
                .ToList();

            lstImprumutat.ItemsSource = items;
            BdrImpGoala.Visibility = items.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnEditeazaVinyl(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string titlu = btn.Tag?.ToString() ?? string.Empty;

            var vinyl = repo.ObtineToti()
                            .FirstOrDefault(v => v.Titlu == titlu);
            if (vinyl == null) return;

            _titluOriginalEditat = titlu;

            TxtEditArtist.Text = vinyl.Artist;
            TxtEditTitlu.Text = vinyl.Titlu;
            TxtEditAn.Text = vinyl.An_Lansare.ToString();
            TxtEditPret.Text = vinyl.Pret.ToString(CultureInfo.InvariantCulture);

            ChkEditRock.IsChecked = (vinyl.Gen & GenMuzical.Rock) != 0;
            ChkEditJazz.IsChecked = (vinyl.Gen & GenMuzical.Jazz) != 0;
            ChkEditPop.IsChecked = (vinyl.Gen & GenMuzical.Pop) != 0;
            ChkEditBlues.IsChecked = (vinyl.Gen & GenMuzical.Blues) != 0;
            ChkEditElectronic.IsChecked = (vinyl.Gen & GenMuzical.Electronic) != 0;
            ChkEditHipHop.IsChecked = (vinyl.Gen & GenMuzical.HipHop) != 0;
            ChkEditClasic.IsChecked = (vinyl.Gen & GenMuzical.Clasic) != 0;
            ChkEditRnB.IsChecked = (vinyl.Gen & GenMuzical.RnB) != 0;
            ChkEditSoul.IsChecked = (vinyl.Gen & GenMuzical.Soul) != 0;
            ChkEditAltele.IsChecked = (vinyl.Gen & GenMuzical.Altele) != 0;

            SelecteazaTagInComboBox(CmbEditFormat, (int)vinyl.Format);

            SelecteazaTagInComboBox(CmbEditConditie, vinyl.CodConditie);

            DpEditDataAchizitie.SelectedDate = vinyl.DataAchizitie;

            ChkEditImprumutat.IsChecked = vinyl.EsteImprumutat;
            if (vinyl.EsteImprumutat)
            {
                PanelEditNumeImprumutat.Visibility = Visibility.Visible;
                IncarcaPersoneInComboBox(CmbEditNumeImprumutat, vinyl.NumeImprumutat);
            }
            else
            {
                PanelEditNumeImprumutat.Visibility = Visibility.Collapsed;
                CmbEditNumeImprumutat.SelectedIndex = -1;
            }

            TbMesajSuccesEdit.Visibility = Visibility.Collapsed;
            TbMesajEroareEdit.Visibility = Visibility.Collapsed;
            AscundeToateEroriEdit();

            TbEditareTitluHeader.Text = $"Editează  \"{vinyl.Artist} – {vinyl.Titlu}\"";

            SetTab(Tab.Editare);
        }

        private void SelecteazaTagInComboBox(ComboBox cmb, int tagValue)
        {
            foreach (ComboBoxItem item in cmb.Items)
            {
                if (int.TryParse(item.Tag?.ToString(), out int t) && t == tagValue)
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }
            cmb.SelectedIndex = 0;
        }

        private void ChkEditImprumutat_Changed(object sender, RoutedEventArgs e)
        {
            PanelEditNumeImprumutat.Visibility =
                ChkEditImprumutat.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            if (ChkEditImprumutat.IsChecked == true)
                IncarcaPersoneInComboBox(CmbEditNumeImprumutat);
            else
            {
                CmbEditNumeImprumutat.SelectedIndex = -1;
                AscundeMesajEroare(TbErrEditNumeImprumutat, LblEditNumeImprumutat);
            }
        }

        private void OnSalveazaModificari(object sender, RoutedEventArgs e)
        {
            TbMesajSuccesEdit.Visibility = Visibility.Collapsed;
            TbMesajEroareEdit.Visibility = Visibility.Collapsed;

            int cod = ValideazaDateEdit();
            AplicaEroriEdit(cod);
            if (cod != ERR_OK) return;

            int.TryParse(TxtEditAn.Text.Trim(), out int an);
            float.TryParse(TxtEditPret.Text.Trim(),
                           NumberStyles.Float, CultureInfo.InvariantCulture, out float pret);

            var original = repo.ObtineToti()
                               .FirstOrDefault(v => v.Titlu == _titluOriginalEditat);

            var vinylNou = new Vinyl
            {
                Artist = TxtEditArtist.Text.Trim(),
                Titlu = TxtEditTitlu.Text.Trim(),
                An_Lansare = an,
                Pret = pret,
                Gen = (GenMuzical)CitesteGenuri(edit: true),
                Format = (FormatVinyl)CitesteTagComboBox(CmbEditFormat),
                CodConditie = CitesteTagComboBox(CmbEditConditie),
                DataAchizitie = DpEditDataAchizitie.SelectedDate,
                Melodii = original?.Melodii ?? new Melodie[0],
                CaleAudioSnippet = original?.CaleAudioSnippet
            };

            if (ChkEditImprumutat.IsChecked == true)
                vinylNou.NumeImprumutat = CmbEditNumeImprumutat.SelectedItem?.ToString() ?? string.Empty;

            bool ok = repo.ModificaVinyl(_titluOriginalEditat!, vinylNou);

            if (ok)
            {
                TbMesajSuccesEdit.Text = $"✓  \"{vinylNou.Artist} – {vinylNou.Titlu}\" actualizat cu succes!";
                TbMesajSuccesEdit.Visibility = Visibility.Visible;

                _titluOriginalEditat = vinylNou.Titlu;
                TbEditareTitluHeader.Text = $"Editează  \"{vinylNou.Artist} – {vinylNou.Titlu}\"";

                IncarcaDate();
                ActualizeazaBadge();
            }
            else
            {
                TbMesajEroareEdit.Text = "Eroare: vinylul nu a putut fi actualizat. Verificați titlul original.";
                TbMesajEroareEdit.Visibility = Visibility.Visible;
            }
        }

        private void OnStergeVinyl(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string titlu = btn.Tag?.ToString() ?? string.Empty;

            var rezultat = MessageBox.Show(
                $"Ești sigur că vrei să ștergi vinylul \"{titlu}\"?\nAceastă acțiune nu poate fi anulată.",
                "Confirmare ștergere",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (rezultat != MessageBoxResult.Yes) return;

            bool ok = repo.StergeVinyl(titlu);
            if (ok)
            {
                IncarcaDate();
                ActualizeazaBadge();
                AplicaFiltruCautare();
            }
            else
            {
                MessageBox.Show($"Eroare la ștergerea vinylului \"{titlu}\".",
                                "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnHamburgerClick(object sender, RoutedEventArgs e)
        {
            _menuExtins = !_menuExtins;
            ColMenu.Width = new GridLength(_menuExtins ? MENU_EXTINS : MENU_RESTRINS);

            var viz = _menuExtins ? Visibility.Visible : Visibility.Collapsed;
            foreach (var lbl in NavLabels)
                lbl.Visibility = viz;
        }

        private void IncarcaPersoneInComboBox(ComboBox cmb, string? selectat = null)
        {
            cmb.Items.Clear();
            foreach (var p in repoPersone.ObtineToti())
                cmb.Items.Add(p.Nume);

            if (selectat != null && cmb.Items.Contains(selectat))
                cmb.SelectedItem = selectat;
            else if (cmb.Items.Count > 0)
                cmb.SelectedIndex = 0;
        }

        private void ChkImprumutat_Changed(object sender, RoutedEventArgs e)
        {
            PanelNumeImprumutat.Visibility =
                ChkImprumutat.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            if (ChkImprumutat.IsChecked == true)
                IncarcaPersoneInComboBox(CmbNumeImprumutat);
            else
            {
                CmbNumeImprumutat.SelectedIndex = -1;
                AscundeMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
            }
        }

        private int CitesteGenuri(bool edit = false)
        {
            int gen = 0;
            if (!edit)
            {
                if (ChkRock.IsChecked == true) gen |= (int)GenMuzical.Rock;
                if (ChkJazz.IsChecked == true) gen |= (int)GenMuzical.Jazz;
                if (ChkPop.IsChecked == true) gen |= (int)GenMuzical.Pop;
                if (ChkBlues.IsChecked == true) gen |= (int)GenMuzical.Blues;
                if (ChkElectronic.IsChecked == true) gen |= (int)GenMuzical.Electronic;
                if (ChkHipHop.IsChecked == true) gen |= (int)GenMuzical.HipHop;
                if (ChkClasic.IsChecked == true) gen |= (int)GenMuzical.Clasic;
                if (ChkRnB.IsChecked == true) gen |= (int)GenMuzical.RnB;
                if (ChkSoul.IsChecked == true) gen |= (int)GenMuzical.Soul;
                if (ChkAltele.IsChecked == true) gen |= (int)GenMuzical.Altele;
            }
            else
            {
                if (ChkEditRock.IsChecked == true) gen |= (int)GenMuzical.Rock;
                if (ChkEditJazz.IsChecked == true) gen |= (int)GenMuzical.Jazz;
                if (ChkEditPop.IsChecked == true) gen |= (int)GenMuzical.Pop;
                if (ChkEditBlues.IsChecked == true) gen |= (int)GenMuzical.Blues;
                if (ChkEditElectronic.IsChecked == true) gen |= (int)GenMuzical.Electronic;
                if (ChkEditHipHop.IsChecked == true) gen |= (int)GenMuzical.HipHop;
                if (ChkEditClasic.IsChecked == true) gen |= (int)GenMuzical.Clasic;
                if (ChkEditRnB.IsChecked == true) gen |= (int)GenMuzical.RnB;
                if (ChkEditSoul.IsChecked == true) gen |= (int)GenMuzical.Soul;
                if (ChkEditAltele.IsChecked == true) gen |= (int)GenMuzical.Altele;
            }
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
            int cod = ERR_OK;
            int anMaxim = DateTime.Now.Year;

            string artist = TxtArtist.Text.Trim();
            if (string.IsNullOrEmpty(artist)) cod |= ERR_ARTIST_GOL;
            else if (artist.Length > MAX_ARTIST) cod |= ERR_ARTIST_LUNG;

            string titlu = TxtTitlu.Text.Trim();
            if (string.IsNullOrEmpty(titlu)) cod |= ERR_TITLU_GOL;
            else if (titlu.Length > MAX_TITLU) cod |= ERR_TITLU_LUNG;

            if (!int.TryParse(TxtAn.Text.Trim(), out int an) || an < AN_MIN || an > anMaxim)
                cod |= ERR_AN_INVALID;

            if (!float.TryParse(TxtPret.Text.Trim(),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float pret)
                || pret < 0 || pret > PRET_MAX)
                cod |= ERR_PRET_INVALID;

            if (CitesteGenuri() == 0) cod |= ERR_GEN_NESET;
            if (CitesteTagComboBox(CmbFormat) == 0) cod |= ERR_FORMAT_NESET;
            if (CitesteTagComboBox(CmbConditie) == 0) cod |= ERR_CONDITIE_NESET;

            if (ChkImprumutat.IsChecked == true &&
                CmbNumeImprumutat.SelectedItem == null)
                cod |= ERR_NUME_IMP_GOL;

            return cod;
        }

        private int ValideazaDateEdit()
        {
            int cod = ERR_OK;
            int anMaxim = DateTime.Now.Year;

            string artist = TxtEditArtist.Text.Trim();
            if (string.IsNullOrEmpty(artist)) cod |= ERR_ARTIST_GOL;
            else if (artist.Length > MAX_ARTIST) cod |= ERR_ARTIST_LUNG;

            string titlu = TxtEditTitlu.Text.Trim();
            if (string.IsNullOrEmpty(titlu)) cod |= ERR_TITLU_GOL;
            else if (titlu.Length > MAX_TITLU) cod |= ERR_TITLU_LUNG;

            if (!int.TryParse(TxtEditAn.Text.Trim(), out int an) || an < AN_MIN || an > anMaxim)
                cod |= ERR_AN_INVALID;

            if (!float.TryParse(TxtEditPret.Text.Trim(),
                    NumberStyles.Float, CultureInfo.InvariantCulture, out float pret)
                || pret < 0 || pret > PRET_MAX)
                cod |= ERR_PRET_INVALID;

            if (CitesteGenuri(edit: true) == 0) cod |= ERR_GEN_NESET;
            if (CitesteTagComboBox(CmbEditFormat) == 0) cod |= ERR_FORMAT_NESET;
            if (CitesteTagComboBox(CmbEditConditie) == 0) cod |= ERR_CONDITIE_NESET;

            if (ChkEditImprumutat.IsChecked == true &&
                CmbEditNumeImprumutat.SelectedItem == null)
                cod |= ERR_NUME_IMP_GOL;

            return cod;
        }

        private void AfiseazaMesajEroare(TextBlock tb, Label lbl)
        {
            tb.Visibility = Visibility.Visible;
            lbl.Foreground = CULOARE_EROARE;
        }

        private void AscundeMesajEroare(TextBlock tb, Label lbl)
        {
            tb.Visibility = Visibility.Collapsed;
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

            if ((cod & ERR_GEN_NESET) != 0) AfiseazaMesajEroare(TbErrGen, LblGen);
            else AscundeMesajEroare(TbErrGen, LblGen);
            if ((cod & ERR_FORMAT_NESET) != 0) AfiseazaMesajEroare(TbErrFormat, LblFormat);
            else AscundeMesajEroare(TbErrFormat, LblFormat);
            if ((cod & ERR_CONDITIE_NESET) != 0) AfiseazaMesajEroare(TbErrConditie, LblConditie);
            else AscundeMesajEroare(TbErrConditie, LblConditie);
            if ((cod & ERR_NUME_IMP_GOL) != 0) AfiseazaMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
            else AscundeMesajEroare(TbErrNumeImprumutat, LblNumeImprumutat);
        }

        private void AplicaEroriEdit(int cod)
        {
            int anMaxim = DateTime.Now.Year;

            if ((cod & ERR_ARTIST_GOL) != 0)
            { TbErrEditArtist.Text = "Câmpul Artist este obligatoriu!"; AfiseazaMesajEroare(TbErrEditArtist, LblEditArtist); }
            else if ((cod & ERR_ARTIST_LUNG) != 0)
            { TbErrEditArtist.Text = $"Artistul nu poate depăși {MAX_ARTIST} caractere!"; AfiseazaMesajEroare(TbErrEditArtist, LblEditArtist); }
            else AscundeMesajEroare(TbErrEditArtist, LblEditArtist);

            if ((cod & ERR_TITLU_GOL) != 0)
            { TbErrEditTitlu.Text = "Câmpul Titlu este obligatoriu!"; AfiseazaMesajEroare(TbErrEditTitlu, LblEditTitlu); }
            else if ((cod & ERR_TITLU_LUNG) != 0)
            { TbErrEditTitlu.Text = $"Titlul nu poate depăși {MAX_TITLU} caractere!"; AfiseazaMesajEroare(TbErrEditTitlu, LblEditTitlu); }
            else AscundeMesajEroare(TbErrEditTitlu, LblEditTitlu);

            if ((cod & ERR_AN_INVALID) != 0)
            { TbErrEditAn.Text = $"Introduceți un an valid ({AN_MIN}–{anMaxim})!"; AfiseazaMesajEroare(TbErrEditAn, LblEditAn); }
            else AscundeMesajEroare(TbErrEditAn, LblEditAn);

            if ((cod & ERR_PRET_INVALID) != 0)
            { TbErrEditPret.Text = $"Preț valid: 0–{PRET_MAX}, format 12.50!"; AfiseazaMesajEroare(TbErrEditPret, LblEditPret); }
            else AscundeMesajEroare(TbErrEditPret, LblEditPret);

            if ((cod & ERR_GEN_NESET) != 0) AfiseazaMesajEroare(TbErrEditGen, LblEditGen);
            else AscundeMesajEroare(TbErrEditGen, LblEditGen);
            if ((cod & ERR_FORMAT_NESET) != 0) AfiseazaMesajEroare(TbErrEditFormat, LblEditFormat);
            else AscundeMesajEroare(TbErrEditFormat, LblEditFormat);
            if ((cod & ERR_CONDITIE_NESET) != 0) AfiseazaMesajEroare(TbErrEditConditie, LblEditConditie);
            else AscundeMesajEroare(TbErrEditConditie, LblEditConditie);
            if ((cod & ERR_NUME_IMP_GOL) != 0) AfiseazaMesajEroare(TbErrEditNumeImprumutat, LblEditNumeImprumutat);
            else AscundeMesajEroare(TbErrEditNumeImprumutat, LblEditNumeImprumutat);
        }

        private void AscundeToateEroriEdit()
        {
            AscundeMesajEroare(TbErrEditArtist, LblEditArtist);
            AscundeMesajEroare(TbErrEditTitlu, LblEditTitlu);
            AscundeMesajEroare(TbErrEditAn, LblEditAn);
            AscundeMesajEroare(TbErrEditPret, LblEditPret);
            AscundeMesajEroare(TbErrEditGen, LblEditGen);
            AscundeMesajEroare(TbErrEditFormat, LblEditFormat);
            AscundeMesajEroare(TbErrEditConditie, LblEditConditie);
            AscundeMesajEroare(TbErrEditNumeImprumutat, LblEditNumeImprumutat);
        }

        private void OnAdauga(object sender, RoutedEventArgs e)
        {
            TbMesajSucces.Visibility = Visibility.Collapsed;

            int cod = ValideazaDateVinyl();
            AplicaEroriUI(cod);
            if (cod != ERR_OK) return;

            int.TryParse(TxtAn.Text.Trim(), out int an);
            float.TryParse(TxtPret.Text.Trim(),
                           NumberStyles.Float, CultureInfo.InvariantCulture, out float pret);

            var vinyl = new Vinyl
            {
                Artist = TxtArtist.Text.Trim(),
                Titlu = TxtTitlu.Text.Trim(),
                An_Lansare = an,
                Pret = pret,
                Gen = (GenMuzical)CitesteGenuri(),
                Format = (FormatVinyl)CitesteTagComboBox(CmbFormat),
                CodConditie = CitesteTagComboBox(CmbConditie),
                DataAchizitie = DpDataAchizitie.SelectedDate,
                Melodii = new Melodie[0]
            };

            if (ChkImprumutat.IsChecked == true)
                vinyl.NumeImprumutat = CmbNumeImprumutat.SelectedItem?.ToString() ?? string.Empty;

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
            TxtArtist.Text = TxtTitlu.Text = TxtAn.Text = TxtPret.Text = string.Empty;
            CmbNumeImprumutat.SelectedIndex = -1;

            ChkRock.IsChecked = ChkJazz.IsChecked = ChkPop.IsChecked = ChkBlues.IsChecked =
            ChkElectronic.IsChecked = ChkHipHop.IsChecked = ChkClasic.IsChecked =
            ChkRnB.IsChecked = ChkSoul.IsChecked = ChkAltele.IsChecked = false;

            CmbFormat.SelectedIndex = CmbConditie.SelectedIndex = 0;
            DpDataAchizitie.SelectedDate = null;
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

        private void IncarcaPersoane()
        {
            var vms = repoPersone.ObtineToti()
                .Select(p => new PersonaVM(
                    Nume: p.Nume,
                    DisplayNume: $"{p.Nume}",
                    DisplayDetalii: $"{p.Rol}  ·  Contact: {(string.IsNullOrEmpty(p.Contact) ? "—" : p.Contact)}",
                    DisplayStare: StareLabel(p.Stare),
                    Stare: p.Stare.ToString()))
                .ToList();

            _vmPersoane.Refresh(vms);
            BdrPersoaneGoale.Visibility = vms.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private static string StareLabel(StareMembru stare) => stare switch
        {
            StareMembru.Activ => "✔ Activ",
            StareMembru.Inactiv => "⏸ Inactiv",
            StareMembru.VIP => "⭐ VIP",
            _ => stare.ToString()
        };

        private void OnSalveazaPersoana(object sender, RoutedEventArgs e)
        {
            string nume = TxtPNume.Text.Trim();

            if (string.IsNullOrEmpty(nume))
            {
                TbErrPNume.Visibility = Visibility.Visible;
                LblPNume.Foreground = CULOARE_EROARE;
                return;
            }
            TbErrPNume.Visibility = Visibility.Collapsed;
            LblPNume.Foreground = CULOARE_NORMALA;

            string contact = TxtPContact.Text.Trim();
            RolPersoana rol = CitesteTagEnum<RolPersoana>(CmbPRol, RolPersoana.Colectionar);
            StareMembru stare = CitesteTagEnum<StareMembru>(CmbPStare, StareMembru.Activ);

            if (_numeOriginalEditat == null)
            {
                if (repoPersone.CautaDupaNumePersoana(nume) != null)
                {
                    AfiseazaMesajPersoana($"Există deja o persoană cu numele \"{nume}\"!", eroare: true);
                    return;
                }
                var p = new Persoana(nume, contact, rol, stare);
                repoPersone.Adauga(p);
                AfiseazaMesajPersoana($"✓  \"{nume}\" adăugat cu succes!");
            }
            else
            {
                var pNoua = new Persoana(nume, contact, rol, stare);
                bool ok = repoPersone.ModificaPersoana(_numeOriginalEditat, pNoua);
                if (!ok)
                {
                    AfiseazaMesajPersoana("Eroare la salvare. Persoana nu a fost găsită.", eroare: true);
                    return;
                }
                AfiseazaMesajPersoana($"✓  \"{nume}\" actualizat cu succes!");
                ResetFormPersoana();
            }

            GoliesteCampuriPersoana();
            IncarcaPersoane();
        }

        private void OnEditeazaPersoana(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string nume = btn.Tag?.ToString() ?? string.Empty;

            var p = repoPersone.CautaDupaNumePersoana(nume);
            if (p == null) return;

            TxtPNume.Text = p.Nume;
            TxtPContact.Text = p.Contact;
            SelecteazaTagStringInComboBox(CmbPRol, p.Rol.ToString());
            SelecteazaTagStringInComboBox(CmbPStare, p.Stare.ToString());

            _numeOriginalEditat = p.Nume;
            TbFormTitluPersoana.Text = $"Editează  \"{p.Nume}\"";
            BtnSalveazaPersoana.Content = "💾  Salvează";
            BtnAnuleazaEditPersoana.Visibility = Visibility.Visible;
            TbMesajPersoana.Visibility = Visibility.Collapsed;
        }

        private void OnStergePersoana(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string nume = btn.Tag?.ToString() ?? string.Empty;

            var rezultat = MessageBox.Show(
                $"Ești sigur că vrei să ștergi persoana \"{nume}\"?\nAceastă acțiune nu poate fi anulată.",
                "Confirmare ștergere",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (rezultat != MessageBoxResult.Yes) return;

            bool ok = repoPersone.StergePersoana(nume);
            if (ok)
            {
                IncarcaPersoane();
                if (_numeOriginalEditat == nume)
                    ResetFormPersoana();
            }
            else
            {
                MessageBox.Show($"Eroare la ștergerea persoanei \"{nume}\".",
                                "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAnuleazaEditPersoana(object sender, RoutedEventArgs e)
            => ResetFormPersoana();

        private void ResetFormPersoana()
        {
            _numeOriginalEditat = null;
            TbFormTitluPersoana.Text = "Adaugă persoană nouă";
            BtnSalveazaPersoana.Content = "＋  Adaugă";
            BtnAnuleazaEditPersoana.Visibility = Visibility.Collapsed;
            GoliesteCampuriPersoana();
        }

        private void GoliesteCampuriPersoana()
        {
            TxtPNume.Text = string.Empty;
            TxtPContact.Text = string.Empty;
            CmbPRol.SelectedIndex = 0;
            CmbPStare.SelectedIndex = 0;
            TbErrPNume.Visibility = Visibility.Collapsed;
            LblPNume.Foreground = CULOARE_NORMALA;
        }

        private void AfiseazaMesajPersoana(string text, bool eroare = false)
        {
            TbMesajPersoana.Text = text;
            TbMesajPersoana.Foreground = eroare
                ? new SolidColorBrush(Color.FromRgb(255, 107, 107))
                : new SolidColorBrush(Color.FromRgb(74, 222, 128));
            TbMesajPersoana.Visibility = Visibility.Visible;
        }

        private T CitesteTagEnum<T>(ComboBox cmb, T fallback) where T : struct, Enum
        {
            if (cmb.SelectedItem is ComboBoxItem item &&
                Enum.TryParse<T>(item.Tag?.ToString(), out T val))
                return val;
            return fallback;
        }

        private void SelecteazaTagStringInComboBox(ComboBox cmb, string tagValue)
        {
            foreach (ComboBoxItem item in cmb.Items)
            {
                if (item.Tag?.ToString() == tagValue)
                {
                    cmb.SelectedItem = item;
                    return;
                }
            }
            cmb.SelectedIndex = 0;
        }
    }
}