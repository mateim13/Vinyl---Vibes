# Vinyl Vibes — Sistem de Gestiune Viniluri

O aplicație desktop C# cu interfață grafică WPF destinată colecționarilor de discuri de vinil.
Permite organizarea colecției, monitorizarea stării fizice a discurilor, gestionarea împrumuturilor, editarea/ștergerea înregistrărilor și importul automat al tracklist-ului din Discogs API.

---

## Funcționalități

- **Meniu lateral** — bară de navigare pliabilă (50 px ↔ 200 px) cu pictograme și text
- **Tab-uri de navigare** — Adaugă · Colecție · Împrumutate · Persoane · Editare
- **Adăugare vinyluri** — formular cu validare completă: artist, titlu, an, preț, gen muzical (multi-select cu CheckBox), format și condiție disc (ComboBox), dată achiziție
- **Import tracklist din Discogs** — buton de import asincron care caută discul pe [Discogs API](https://www.discogs.com/developers/) și populează automat lista de melodii
- **Împrumuturi** — marcare disc ca împrumutat cu selecție persoană din listă (ComboBox populat din `persoane.txt`)
- **Editare vinyluri** — formularul de editare pre-populat cu datele existente, salvare cu re-validare
- **Ștergere vinyluri** — confirmare dialog înainte de ștergere permanentă
- **Căutare în timp real** — filtrare listă după artist sau titlu, cu actualizare la fiecare tastă
- **Vizualizare colecție** — DataGrid cu detalii complete per disc
- **Vizualizare împrumuturi** — tab separat cu lista discurilor împrumutate
- **Gestiune persoane** — tab dedicat cu CRUD complet: adăugare, editare, ștergere; câmpuri: nume, contact, rol, stare (Activ / Inactiv / VIP)
- **Sistem de grading** — condiție disc pe scara 1–8 (Poor → Mint)
- **Persistență** — datele sunt salvate automat în fișiere text (`vinyluri.txt`, `persoane.txt`) în directorul executabilului
- **Validare prin WPF binding (IDataErrorInfo)** — bordură roșie per câmp, mesaj de eroare afișat sub control, fără erori la deschiderea aplicației

---

## Controale WPF Utilizate

| Control | Utilizare | Locație |
|---------|-----------|---------|
| **DataGrid** | Afișare colecție vinyluri | Tab Colecție |
| **ListBox** | Lista împrumuturi, lista persoane | Tab Împrumutate, Tab Persoane |
| **ComboBox** | Selecție Format disc, Condiție disc, Persoană împrumutată, Rol și Stare persoană | Formular Adaugă, Formular Editare, Tab Persoane |
| **CheckBox** | Selecție multiplă genuri muzicale (`[Flags]`) + marcare disc împrumutat | Formular Adaugă, Formular Editare |
| **DatePicker** | Selectare data achiziției discului (opțional, template dark custom) | Formular Adaugă, Formular Editare |
| **TextBox** | Input text: artist, titlu, an, preț, căutare, date persoană | Formulare + bara de căutare |
| **Button** | Acțiuni: adaugă, reset, editează, șterge, hamburger, import Discogs | Toate secțiunile |

---

## Arhitectura Soluției

```
Vinyl.slnx
│
├── LibrarieModele/              # Nivelul Model (entități de domeniu)
│   ├── Enums.cs                 # GenMuzical [Flags], FormatVinyl, RolPersoana, StareMembru
│   ├── Melodie.cs               # Model melodie + FromLine() / ToLine()
│   ├── Persoana.cs              # Model persoană + FromLine() / ToLine()
│   └── Vinyl.cs                 # Model vinyl + TryFromLines() / ToLines()
│
├── NivelStocareDate/            # Nivelul de Stocare (persistență fișiere)
│   └── GestiuneDate.cs          # GestiuneDate<T> · RepoVinyluri · RepoPersone
│
└── Interfata/                   # Nivelul Prezentare (WPF — MVVM)
    ├── MainWindow.xaml           # UI: stiluri, meniu hamburger, tab-uri, formulare
    ├── MainWindow.xaml.cs        # Logica de prezentare, navigare, CRUD
    ├── VinylFormViewModel.cs     # ViewModel formular adăugare (INotifyPropertyChanged,
    │                             #   IDataErrorInfo, HashSet<string> _dirty)
    └── DiscogsService.cs         # Serviciu static REST → Discogs API (HttpClient, async/await)
```

---

## Validare și Binding (MVVM)

Formularul de adăugare folosește binding bidirecțional WPF către `VinylFormViewModel`:

```xml
<TextBox x:Name="TxtArtist"
         Text="{Binding Artist,
                UpdateSourceTrigger=PropertyChanged,
                ValidatesOnDataErrors=True}" />
```

- **UI → ViewModel**: `UpdateSourceTrigger=PropertyChanged` — ViewModel primește valoarea la fiecare tastă
- **ViewModel → UI**: `INotifyPropertyChanged` — `OnPropertyChanged()` actualizează automat controlul
- **Validare**: `IDataErrorInfo.this[propertyName]` — WPF citește eroarea și aplică bordura roșie
- **Fără erori la startup**: `HashSet<string> _dirty` — validarea se declanșează doar pentru câmpurile atinse de utilizator

---

## Tehnologii

| | |
|---|---|
| Limbaj | C# (.NET 10) |
| UI | WPF (Windows Presentation Foundation) |
| Arhitectură | Layered + MVVM (INotifyPropertyChanged, IDataErrorInfo) |
| Stocare | Fișiere text plain (`vinyluri.txt`, `persoane.txt`) |
| API extern | [Discogs API v2.0](https://www.discogs.com/developers/) — import tracklist |
| HTTP | `HttpClient` singleton + `async/await` + `System.Text.Json` |

---

## Build & Run

```powershell
# Rulare aplicație WPF
dotnet run --project Interfata\Interfata.csproj

# Doar compilare
dotnet build "Vinyl.slnx"
```

> Fișierele de date (`vinyluri.txt`, `persoane.txt`) sunt create/citite automat din directorul executabilului (`bin\Debug\net10.0-windows\`).

---

## Structura Fișierelor de Date

**`vinyluri.txt`**
```
VINYL|<Titlu>|<Artist>|<An>|<Pret>|<CodConditie>|<Gen(int)>|<Format(int)>|<Imprumutat(0/1)>|<NumeImprumutat>|<CaleSnippet>|<DataAchizitie(yyyy-MM-dd)>
MELODIE|<Titlu>|<Featuring>|<DurataMinute>
```

**`persoane.txt`**
```
PERSOANA|<Nume>|<Contact>|<Rol>|<Stare>|<disc1;disc2;...>
```
