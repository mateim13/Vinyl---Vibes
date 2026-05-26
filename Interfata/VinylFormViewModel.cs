using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Interfata
{
    public class VinylFormViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private const int   AN_MIN    = 1900;
        private const int   MAX_ARTIST = 50;
        private const int   MAX_TITLU  = 100;
        private const float PRET_MAX   = 99999f;

        private string _artist = string.Empty;
        private string _titlu  = string.Empty;
        private string _an     = string.Empty;
        private string _pret   = string.Empty;
        private readonly HashSet<string> _dirty = new HashSet<string>();

        public string Artist
        {
            get => _artist;
            set { _artist = value; _dirty.Add(nameof(Artist)); OnPropertyChanged(); }
        }

        public string Titlu
        {
            get => _titlu;
            set { _titlu = value; _dirty.Add(nameof(Titlu)); OnPropertyChanged(); }
        }

        public string An
        {
            get => _an;
            set { _an = value; _dirty.Add(nameof(An)); OnPropertyChanged(); }
        }

        public string Pret
        {
            get => _pret;
            set { _pret = value; _dirty.Add(nameof(Pret)); OnPropertyChanged(); }
        }

        public string Error => string.Empty;

        public string this[string propertyName]
        {
            get
            {
                int anMaxim = DateTime.Now.Year;
                if (!_dirty.Contains(propertyName)) return string.Empty;
                switch (propertyName)
                {
                    case nameof(Artist):
                        if (string.IsNullOrWhiteSpace(Artist))
                            return "Câmpul Artist este obligatoriu!";
                        if (Artist.Length > MAX_ARTIST)
                            return $"Artistul nu poate depăși {MAX_ARTIST} caractere!";
                        return string.Empty;
                    case nameof(Titlu):
                        if (string.IsNullOrWhiteSpace(Titlu))
                            return "Câmpul Titlu este obligatoriu!";
                        if (Titlu.Length > MAX_TITLU)
                            return $"Titlul nu poate depăși {MAX_TITLU} caractere!";
                        return string.Empty;
                    case nameof(An):
                        if (!int.TryParse(An, out int an) || an < AN_MIN || an > anMaxim)
                            return $"Introduceți un an valid ({AN_MIN}–{anMaxim})!";
                        return string.Empty;
                    case nameof(Pret):
                        if (!float.TryParse(Pret, NumberStyles.Float,
                                CultureInfo.InvariantCulture, out float pret)
                            || pret < 0 || pret > PRET_MAX)
                            return $"Preț valid: 0–{PRET_MAX}, format 12.50!";
                        return string.Empty;
                    default:
                        return string.Empty;
                }
            }
        }

        public bool EsteValid =>
            string.IsNullOrEmpty(this[nameof(Artist)]) &&
            string.IsNullOrEmpty(this[nameof(Titlu)])  &&
            string.IsNullOrEmpty(this[nameof(An)])     &&
            string.IsNullOrEmpty(this[nameof(Pret)]);

        public void MarkAllDirty()
        {
            _dirty.Add(nameof(Artist));
            _dirty.Add(nameof(Titlu));
            _dirty.Add(nameof(An));
            _dirty.Add(nameof(Pret));
        }

        public void Reset()
        {
            _dirty.Clear();
            _artist = _titlu = _an = _pret = string.Empty;
            OnPropertyChanged(nameof(Artist));
            OnPropertyChanged(nameof(Titlu));
            OnPropertyChanged(nameof(An));
            OnPropertyChanged(nameof(Pret));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
