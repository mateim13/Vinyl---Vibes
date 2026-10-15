# Vinyl Vibes — Vinyl Record Management System

A C# desktop application with a WPF graphical interface designed for vinyl record collectors.
It allows users to organize their collections, track the physical condition of records, manage loans, edit and delete entries, and automatically import tracklists using the Discogs API.

---

## Features

- **Collapsible sidebar** — expandable navigation bar (50 px ↔ 200 px) with icons and text
- **Navigation tabs** — Add · Collection · Borrowed · People · Edit
- **Add vinyl records** — fully validated form for artist, title, year, price, music genres (multi-select CheckBox), record format and condition (ComboBox), and purchase date
- **Discogs tracklist import** — asynchronous import button that searches for a record using the [Discogs API](https://www.discogs.com/developers/) and automatically populates its tracklist
- **Loans** — mark a record as borrowed and select a person from a list (ComboBox populated from `persoane.txt`)
- **Edit vinyl records** — editing form pre-populated with existing data; changes are revalidated before saving
- **Delete vinyl records** — confirmation dialog before permanent deletion
- **Real-time search** — filter the list by artist or title, updating with every keystroke
- **Collection view** — DataGrid with complete details for each record
- **Loans view** — separate tab listing borrowed records
- **People management** — dedicated tab with full CRUD operations: add, edit, and delete; fields include name, contact, role, and status (Active / Inactive / VIP)
- **Grading system** — record condition on a scale of 1–8 (Poor → Mint)
- **Persistence** — data is automatically saved to text files (`vinyluri.txt`, `persoane.txt`) in the executable directory
- **WPF binding validation (`IDataErrorInfo`)** — red border around invalid fields and an error message below the control, with no validation errors when the application first opens

---

## WPF Controls Used

| Control | Purpose | Location |
|---------|---------|----------|
| **DataGrid** | Display the vinyl record collection | Collection tab |
| **ListBox** | Display loans and people lists | Borrowed tab, People tab |
| **ComboBox** | Select record format, record condition, borrower, and a person's role and status | Add form, Edit form, People tab |
| **CheckBox** | Select multiple music genres (`[Flags]`) and mark a record as borrowed | Add form, Edit form |
| **DatePicker** | Select the optional record purchase date (custom dark template) | Add form, Edit form |
| **TextBox** | Enter artist, title, year, price, search terms, and personal information | Forms and search bar |
| **Button** | Add, reset, edit, delete, toggle the sidebar, and import from Discogs | All sections |

---

## Solution Architecture

```text
Vinyl.slnx
│
├── LibrarieModele/              # Model layer (domain entities)
│   ├── Enums.cs                 # GenMuzical [Flags], FormatVinyl, RolPersoana, StareMembru
│   ├── Melodie.cs               # Track model + FromLine() / ToLine()
│   ├── Persoana.cs              # Person model + FromLine() / ToLine()
│   └── Vinyl.cs                 # Vinyl model + TryFromLines() / ToLines()
│
├── NivelStocareDate/            # Storage layer (file persistence)
│   └── GestiuneDate.cs          # GestiuneDate<T> · RepoVinyluri · RepoPersone
│
└── Interfata/                   # Presentation layer (WPF — MVVM)
    ├── MainWindow.xaml          # UI: styles, hamburger menu, tabs, forms
    ├── MainWindow.xaml.cs       # Presentation logic, navigation, CRUD
    ├── VinylFormViewModel.cs    # Add form ViewModel (INotifyPropertyChanged,
    │                            #   IDataErrorInfo, HashSet<string> _dirty)
    └── DiscogsService.cs        # Static REST service → Discogs API (HttpClient, async/await)
```

---

## Validation and Binding (MVVM)

The add form uses two-way WPF binding to `VinylFormViewModel`:

```xml
<TextBox x:Name="TxtArtist"
         Text="{Binding Artist,
                UpdateSourceTrigger=PropertyChanged,
                ValidatesOnDataErrors=True}" />
```

- **UI → ViewModel:** `UpdateSourceTrigger=PropertyChanged` — the ViewModel receives updated values with every keystroke
- **ViewModel → UI:** `INotifyPropertyChanged` — `OnPropertyChanged()` automatically updates the control
- **Validation:** `IDataErrorInfo.this[propertyName]` — WPF reads the error and applies a red border
- **No startup errors:** `HashSet<string> _dirty` — validation is triggered only for fields the user has interacted with

---

## Technologies

| Component | Technology |
|-----------|------------|
| Language | C# (.NET 10) |
| UI | WPF (Windows Presentation Foundation) |
| Architecture | Layered + MVVM (`INotifyPropertyChanged`, `IDataErrorInfo`) |
| Storage | Plain text files (`vinyluri.txt`, `persoane.txt`) |
| External API | [Discogs API v2.0](https://www.discogs.com/developers/) — tracklist import |
| HTTP | Singleton `HttpClient` + `async/await` + `System.Text.Json` |

---

## Build & Run

```powershell
# Run the WPF application
dotnet run --project Interfata\Interfata.csproj

# Build only
dotnet build "Vinyl.slnx"
```

> Data files (`vinyluri.txt`, `persoane.txt`) are automatically created/read from the executable directory (`bin\Debug\net10.0-windows\`).

---

## Data File Structure

**`vinyluri.txt`**

```text
VINYL|<Titlu>|<Artist>|<An>|<Pret>|<CodConditie>|<Gen(int)>|<Format(int)>|<Imprumutat(0/1)>|<NumeImprumutat>|<CaleSnippet>|<DataAchizitie(yyyy-MM-dd)>
MELODIE|<Titlu>|<Featuring>|<DurataMinute>
```

**`persoane.txt`**

```text
PERSOANA|<Nume>|<Contact>|<Rol>|<Stare>|<disc1;disc2;...>
```

> Field names in the data examples are kept unchanged to match the application's existing serialization format.
