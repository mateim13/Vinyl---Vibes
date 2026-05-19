# Vinyl Vibes — Sistem de Gestiune Viniluri

O aplicație desktop C# cu interfață grafică WPF destinată colecționarilor de discuri de vinil.
Permite organizarea colecției, monitorizarea stării fizice a discurilor, gestionarea împrumuturilor și editarea/ștergerea înregistrărilor.

---

## Funcționalități

- **Meniu lateral** — bară de navigare pliabilă (50 px ↔ 200 px) cu pictograme și text
- **Tab-uri de navigare** — Adaugă · Colecție · Împrumutate · Persoane · Editare
- **Adăugare vinyluri** — formular cu validare completă: artist, titlu, an, preț, gen muzical (multi-select cu CheckBox), format și condiție disc (ComboBox), dată achiziție
- **Împrumuturi** — marcare disc ca împrumutat cu selecție persoană din listă (ComboBox populat din `persoane.txt`)
- **Editare vinyluri** — formularul de editare pre-populat cu datele existente, salvare cu re-validare
- **Ștergere vinyluri** — confirmare dialog înainte de ștergere permanentă
- **Căutare în timp real** — filtrare listă după artist sau titlu, cu actualizare la fiecare tastă
- **Vizualizare colecție** — ListBox cu template personalizat: iconiță disc, detalii, badge stare (📥 În raft / 📤 Împrumutat)
- **Vizualizare împrumuturi** — tab separat cu lista discurilor împrumutate
- **Gestiune persoane** — tab dedicat cu CRUD complet: adăugare, editare, ștergere; câmpuri: nume, contact, rol, stare (Activ / Inactiv / VIP)
- **Sistem de grading** — condiție disc pe scara 1–8 (Poor → Mint)
- **Persistență** — datele sunt salvate automat în fișiere text (`vinyluri.txt`, `persoane.txt`) în directorul executabilului
- **Validare cu feedback vizual** — mesaje de eroare per câmp cu schimbare de culoare label

---

## Controale WPF Utilizate

| Control | Utilizare | Locație |
|---------|-----------|---------|
| **ListBox** | Afișare colecție vinyluri + lista împrumuturi (cu `DataTemplate` personalizat) | Tab Colecție, Tab Împrumutate |
| **ComboBox** | Selecție Format disc, Condiție disc, Persoană împrumutată, Rol și Stare persoană | Formular Adaugă, Formular Editare, Tab Persoane |
| **CheckBox** | Selecție multiplă genuri muzicale (`[Flags]`) + marcare disc împrumutat | Formular Adaugă, Formular Editare |
| **DatePicker** | Selectare data achiziției discului (opțional, template dark custom) | Formular Adaugă, Formular Editare |
| **TextBox** | Input text: artist, titlu, an, preț, căutare, date persoană | Formulare + bara de căutare |
| **Button** | Acțiuni: adaugă, reset, editează, șterge, hamburger, navigare tab-uri | Toate secțiunile |

---

## Arhitectura Soluției

```
Vinyl.slnx
│
├── LibrarieModele/           # Nivelul Model (entități de domeniu)
│   ├── Enums.cs              # GenMuzical [Flags], FormatVinyl, RolPersoana, StareMembru
│   ├── Melodie.cs            # Model melodie + FromLine() / ToLine()
│   ├── Persoana.cs           # Model persoană + FromLine() / ToLine()
│   └── Vinyl.cs              # Model vinyl + TryFromLines() / ToLines()
│
├── NivelStocareDate/         # Nivelul de Stocare (persistență fișiere)
│   └── GestiuneDate.cs       # GestiuneDate<T> · RepoVinyluri · RepoPersone
│
└── Interfata/                # Nivelul Prezentare (WPF)
    ├── MainWindow.xaml        # UI: stiluri, meniu hamburger, tab-uri, formulare
    └── MainWindow.xaml.cs     # Logica de prezentare, navigare, validare, CRUD
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