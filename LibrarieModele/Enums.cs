using System;

namespace LibrarieModele
{
    [Flags]
    public enum GenMuzical
    {
        Necunoscut  = 0,
        Rock        = 1,
        Jazz        = 2,
        Pop         = 4,
        Blues       = 8,
        Electronic  = 16,
        HipHop      = 32,
        Clasic      = 64,
        RnB         = 128,
        Soul        = 256,
        Altele      = 512
    }

    [Flags]
    public enum FormatVinyl
    {
        Nedefinit   = 0,
        LP          = 1,
        EP          = 2,
        Single      = 4,
        Reissue     = 8,
        Colored     = 16,
        PictureDisc = 32
    }

    public enum RolPersoana
    {
        Colectionar,
        Imprumutator,
        Vanzator,
        Cumparator
    }

    public enum StareMembru
    {
        Activ,
        Inactiv,
        VIP
    }
}
