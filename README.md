# Vinyl Vibes — Sistem de Gestiune Viniluri

O aplicație desktop C# cu interfață grafică (WPF) destinată colecționarilor de discuri de vinil. Permite organizarea colecției, monitorizarea stării fizice a discurilor și gestionarea împrumuturilor.

---

## Funcționalități

- **Adăugare vinyluri** — formular complet cu validare: artist, titlu, an lansare, preț, gen muzical, format și condiție disc
- **Sistem de grading** — condiție disc pe scara 1–8 (Poor → Mint)
- **Gestionare împrumuturi** — marcare disc ca împrumutat și înregistrarea numelui persoanei
- **Vizualizare colecție** — lista de discuri cu detalii și badge de stare (în raft / împrumutat)
- **Persistență** — datele sunt salvate automat în fișiere text (`vinyluri.txt`, `persoane.txt`)
- **Validare cu feedback vizual** — mesaje de eroare per câmp și mesaj de confirmare la adăugare cu succes

---

## Arhitectura Soluției

```
Vinyl.slnx
│
├── LibrarieModele/          # Nivelul Model (entități de domeniu)
│   ├── Enums.cs             # GenMuzical, FormatVinyl, RolPersoana, StareMembru
│   ├── Melodie.cs           # Model melodie + FromLine() / ToLine()
│   ├── Persoana.cs          # Model persoană + FromLine() / ToLine()
│   └── Vinyl.cs             # Model vinyl + TryFromLines() / ToLines()
│
├── NivelStocareDate/        # Nivelul de Stocare (persistență fișiere)
│   └── GestiuneDate.cs      # GestiuneDate<T> · RepoVinyluri · RepoPersone
│
├── Interfata/               # Nivelul Prezentare (WPF)
│   ├── MainWindow.xaml      # UI · stiluri · layout 2 coloane
│   └── MainWindow.xaml.cs   # Logică prezentare + validare câmpuri
│
└── Vinyl/                   # Aplicație consolă (referință / testare)
    ├── Program.cs
    └── ConsoleApp.cs
```

---

## Tehnologii

| | |
|---|---|
| Limbaj | C# (.NET 10) |
| UI | WPF (Windows Presentation Foundation) |
| Stocare | Fișiere text plain (`vinyluri.txt`, `persoane.txt`) |
| Arhitectură | Layered Architecture (Model → Stocare → Prezentare) |

---

## Build & Run

```powershell
# Interfața grafică WPF (recomandat)
dotnet run --project Interfata\Interfata.csproj

# Aplicația de consolă (alternativă)
dotnet run --project Vinyl\Vinyl.csproj

# Doar compilare
dotnet build
```

> Fișierele de date (`vinyluri.txt`, `persoane.txt`) sunt create automat la prima rulare, în directorul de lucru curent.

---

## Structura Fișierelor de Date

**`vinyluri.txt`**
```
VINYL|<Titlu>|<Artist>|<An>|<Pret>|<CodConditie>|<Gen(int)>|<Format(int)>|<Imprumutat(0/1)>|<NumeImprumutat>|<CaleSnippet>
MELODIE|<Titlu>|<Featuring>|<DurataMinute>
```

**`persoane.txt`**
```
PERSOANA|<Nume>|<Contact>|<Rol>|<Stare>|<disc1;disc2;...>
```