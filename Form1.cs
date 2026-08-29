using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Windows.Forms;

namespace GrafikBrygad
{
    public partial class Form1 : Form
    {
        // =====================================================
        // INFORMACJE O PROGRAMIE
        // =====================================================

        private const string NazwaProgramu =
            "Grafik Brygad VEOLIA Energia Łódź";

        private const string WersjaProgramu =
            "wersja 1.20";

        private const string AutorProgramu =
            "Marek Walaszczyk";

        private const string RozpoczecieProjektu =
            "2026.08";

        private const string AdresProjektu =
            "https://github.com/Maraptor/GrafikBrygad";

        // Liczba wierszy widocznych jednocześnie w tabeli.
        // Przy 11 dniach bieżąca data może znajdować się
        // dokładnie w środkowym, szóstym wierszu.
        private const int LiczbaWidocznychDni = 11;

        // Bazowy projekt interfejsu został przygotowany dla
        // 96 DPI (100%) i obszaru klienta 1020 × 916.
        //
        // Od v1.20 program nie nakłada ręcznego Scale()
        // na automatyczne skalowanie Windows. Cały interfejs
        // jest zawsze odtwarzany z poniższych wartości bazowych.
        private const int BazowaSzerokoscKlienta = 1020;
        private const int BazowaWysokoscKlienta = 916;
        private const float BazoweDpi = 96.0f;


        // =====================================================
        // USTAWIENIA GRAFIKU
        // =====================================================

        // Cykl 20-dniowy:
        // N N N N X P P P P X R R R R W W W W W W
        //
        // X = dzień wolny między zmianami (chroniony),
        //     którego nie wykorzystujemy jako dnia uzupełniającego.
        // W = dzień wolny, który może być dniem uzupełniającym
        //     wymiar czasu pracy.

        private readonly string cykl =
            "NNNNXPPPPXRRRRWWWWWW";

        // Pierwszy dzień N dla każdej brygady

        private readonly DateTime[] datyStartowe =
        {
            new DateTime(2026, 8, 19), // A
            new DateTime(2026, 8, 23), // B
            new DateTime(2026, 8, 27), // C
            new DateTime(2026, 8, 11), // D
            new DateTime(2026, 8, 15)  // E
        };

        private readonly string[] brygady =
        {
            "Brygada A",
            "Brygada B",
            "Brygada C",
            "Brygada D",
            "Brygada E"
        };


        // =====================================================
        // ELEMENTY OKNA
        // =====================================================

        private Panel panelNaglowek =
            new Panel();

        private PictureBox logoVeolia =
            new PictureBox();

        private Label lblTytul =
            new Label();

        private Label lblWersja =
            new Label();

        private Button btnRokWstecz =
            new Button();

        private Button btnPoprzedni =
            new Button();

        private Button btnNastepny =
            new Button();

        private Button btnRokPrzod =
            new Button();

        private Button btnAktualny =
            new Button();

        private Button btnPodglad =
            new Button();

        private Button btnDrukuj =
            new Button();

        private Button btnWidok =
            new Button();

        private Label lblMiesiac =
            new Label();

        private DataGridView tabela =
            new DataGridView();

        private Panel panelLegenda =
            new Panel();

        private Label lblAutor =
            new Label();

        private LinkLabel linkNowaWersja =
            new LinkLabel();


        // =====================================================
        // DRUKOWANIE
        // =====================================================

        private PrintDocument dokumentDruku =
            new PrintDocument();

        private PrintPreviewDialog podgladDruku =
            new PrintPreviewDialog();


        // =====================================================
        // ZMIENNE POMOCNICZE
        // =====================================================

        private DateTime aktualnyMiesiac;

        private int indeksDzisiejszegoWiersza = -1;

        // false = nowoczesny widok R/P/N/W/X
        // true  = widok zgodny z oryginalnym grafikiem VEOLIA:
        //         1/2/3/w/x oraz kolumny I-V.
        private bool widokVeolia = false;

        // Aktualna, końcowa skala całego interfejsu.
        //
        // 1.00 = układ bazowy 100%
        // >1.00 = powiększenie, ale tylko do granicy ekranu.
        private float skalaInterfejsu = 1.0f;

        // Pozycje i rozmiary wszystkich kontrolek są zapisywane
        // jeden raz w układzie bazowym. Przy każdej zmianie DPI
        // odtwarzamy je z tych danych, więc skalowanie nigdy
        // się nie kumuluje.
        private readonly Dictionary<Control, Rectangle>
            bazoweProstokatyKontrolek =
                new Dictionary<Control, Rectangle>();

        private readonly Dictionary<Control, Padding>
            bazowePaddingiKontrolek =
                new Dictionary<Control, Padding>();

        private readonly Dictionary<Control, BazowaCzcionka>
            bazoweCzcionkiKontrolek =
                new Dictionary<Control, BazowaCzcionka>();

        private bool ukladBazowyZapisany = false;


        // =====================================================
        // DANE BAZOWE CZCIONKI
        // =====================================================

        private sealed class BazowaCzcionka
        {
            public BazowaCzcionka(
                Font font)
            {
                NazwaRodziny =
                    font.FontFamily.Name;

                RozmiarPunktowy =
                    font.SizeInPoints;

                Styl =
                    font.Style;
            }

            public string NazwaRodziny
            {
                get;
            }

            public float RozmiarPunktowy
            {
                get;
            }

            public FontStyle Styl
            {
                get;
            }
        }


        // =====================================================
        // KONSTRUKTOR
        // =====================================================

        public Form1()
        {
            InitializeComponent();

            // MECHANIZM SKALOWANIA STABILNEJ v1.20:
            //
            // Nie pozwalamy już, aby WinForms najpierw skalował
            // cały formularz przez AutoScaleMode.Dpi, a potem
            // nasz kod próbował go ponownie zmniejszać.
            //
            // To właśnie powodowało rozjazd między ramką okna
            // a jego zawartością przy 125% i 150%.
            //
            // Windows nadal informuje nas o aktualnym DeviceDpi,
            // ale geometrię głównego interfejsu kontrolujemy sami.
            this.AutoScaleMode =
                AutoScaleMode.None;

            this.Text =
                $"{NazwaProgramu} ({WersjaProgramu})";

            this.ClientSize =
                new Size(
                    BazowaSzerokoscKlienta,
                    BazowaWysokoscKlienta);

            // Rozmiar minimalny ustawimy dopiero po
            // dopasowaniu do rzeczywistego obszaru roboczego.
            // Dzięki temu Windows może poprawnie zastosować DPI,
            // a aplikacja może później zmniejszyć się, jeśli
            // przy 125% / 150% nie mieści się na ekranie.
            this.MinimumSize =
                Size.Empty;

            this.StartPosition =
                FormStartPosition.CenterScreen;

            this.BackColor =
                Color.FromArgb(245, 247, 250);

            this.Font =
                new Font(
                    "Segoe UI",
                    10);

            DateTime dzis =
                DateTime.Today;

            aktualnyMiesiac =
                new DateTime(
                    dzis.Year,
                    dzis.Month,
                    1);

            UtworzInterfejs();

            GenerujGrafik();

            // Zapamiętujemy układ dokładnie taki, jak został
            // zaprojektowany dla 96 DPI. Od tej chwili każda
            // zmiana skali odtwarza geometrię z tej bazy.
            ZapiszBazowyUkladInterfejsu();

            // Po pierwszym pokazaniu formularza:
            // 1. dopasowujemy cały interfejs do obszaru roboczego
            //    bieżącego monitora (ważne przy 125% i 150% DPI),
            // 2. ustawiamy dzisiejszy dzień możliwie na środku.
            this.Shown +=
                (sender, e) =>
                {
                    BeginInvoke(
                        new Action(
                            () =>
                            {
                                DopasujOknoDoObszaruRoboczego();
                                UstawDzisiejszyDzienNaSrodku();
                            }));
                };

            // Jeżeli użytkownik zmieni skalę Windows albo
            // przeniesie okno na monitor z innym DPI, nie
            // przeskalowujemy poprzedniego układu. Odtwarzamy
            // wszystko od nowa z wartości bazowych.
            this.DpiChanged +=
                (sender, e) =>
                {
                    BeginInvoke(
                        new Action(
                            DopasujOknoDoObszaruRoboczego));
                };
        }


        // =====================================================
        // ZAPIS BAZOWEGO UKŁADU INTERFEJSU
        // =====================================================

        private void ZapiszBazowyUkladInterfejsu()
        {
            bazoweProstokatyKontrolek.Clear();
            bazowePaddingiKontrolek.Clear();
            bazoweCzcionkiKontrolek.Clear();

            foreach (
                Control kontrolka
                in this.Controls)
            {
                ZapiszBazowyUkladKontrolki(
                    kontrolka);
            }

            ukladBazowyZapisany =
                true;
        }


        // =====================================================
        // ZAPIS JEDNEJ KONTROLKI I JEJ DZIECI
        // =====================================================

        private void ZapiszBazowyUkladKontrolki(
            Control kontrolka)
        {
            bazoweProstokatyKontrolek[
                kontrolka] =
                kontrolka.Bounds;

            bazowePaddingiKontrolek[
                kontrolka] =
                kontrolka.Padding;

            bazoweCzcionkiKontrolek[
                kontrolka] =
                new BazowaCzcionka(
                    kontrolka.Font);

            foreach (
                Control dziecko
                in kontrolka.Controls)
            {
                ZapiszBazowyUkladKontrolki(
                    dziecko);
            }
        }


        // =====================================================
        // REJESTRACJA NOWO UTWORZONYCH KONTROLEK
        // =====================================================

        private void ZarejestrujNoweKontrolki(
            Control rodzic)
        {
            foreach (
                Control kontrolka
                in rodzic.Controls)
            {
                if (!bazoweProstokatyKontrolek
                    .ContainsKey(
                        kontrolka))
                {
                    ZapiszBazowyUkladKontrolki(
                        kontrolka);
                }
                else
                {
                    ZarejestrujNoweKontrolki(
                        kontrolka);
                }
            }
        }


        // =====================================================
        // USUNIĘCIE STARYCH DZIECI Z REJESTRU
        // np. przed przebudową legendy
        // =====================================================

        private void UsunDzieciZBazowegoUkladu(
            Control rodzic)
        {
            foreach (
                Control kontrolka
                in rodzic.Controls)
            {
                UsunDzieciZBazowegoUkladu(
                    kontrolka);

                bazoweProstokatyKontrolek.Remove(
                    kontrolka);

                bazowePaddingiKontrolek.Remove(
                    kontrolka);

                bazoweCzcionkiKontrolek.Remove(
                    kontrolka);
            }
        }


        // =====================================================
        // OBLICZENIE KOŃCOWEJ SKALI INTERFEJSU
        // =====================================================

        private float ObliczSkaleInterfejsu()
        {
            Screen ekran =
                Screen.FromControl(
                    this);

            Rectangle obszarRoboczy =
                ekran.WorkingArea;

            // Rozmiar ramki i paska tytułu jest obsługiwany
            // przez Windows i zależy od aktualnego DPI.
            int szerokoscElementowOkna =
                Math.Max(
                    0,
                    this.Width -
                    this.ClientSize.Width);

            int wysokoscElementowOkna =
                Math.Max(
                    0,
                    this.Height -
                    this.ClientSize.Height);

            const int marginesOkna = 10;

            int dostepnaSzerokoscKlienta =
                Math.Max(
                    1,
                    obszarRoboczy.Width -
                    szerokoscElementowOkna -
                    marginesOkna * 2);

            int dostepnaWysokoscKlienta =
                Math.Max(
                    1,
                    obszarRoboczy.Height -
                    wysokoscElementowOkna -
                    marginesOkna * 2);

            // Skala, jakiej życzy sobie Windows:
            // 100% = 1.00
            // 125% = 1.25
            // 150% = 1.50
            float skalaDpi =
                Math.Max(
                    1.0f,
                    this.DeviceDpi /
                    BazoweDpi);

            // Maksymalna skala, przy której CAŁY bazowy
            // interfejs 1020 × 916 nadal mieści się na pulpicie.
            float skalaPoSzerokosci =
                dostepnaSzerokoscKlienta /
                (float)BazowaSzerokoscKlienta;

            float skalaPoWysokosci =
                dostepnaWysokoscKlienta /
                (float)BazowaWysokoscKlienta;

            float skalaMieszczaca =
                Math.Min(
                    skalaPoSzerokosci,
                    skalaPoWysokosci);

            // Nigdy nie powiększamy bardziej niż żąda DPI,
            // ale gdy DPI byłoby za duże dla wysokości ekranu,
            // zatrzymujemy się dokładnie na granicy mieszczącej
            // cały interfejs.
            float wynik =
                Math.Min(
                    skalaDpi,
                    skalaMieszczaca);

            // Dolne zabezpieczenie dla bardzo małych ekranów.
            // Na 1920×1080 wartość będzie znacznie wyższa.
            return Math.Max(
                0.60f,
                wynik);
        }


        // =====================================================
        // SKALOWANA WARTOŚĆ W PIKSELACH
        // =====================================================

        private int SkalujPiksele(
            int wartoscBazowa)
        {
            return
                Math.Max(
                    1,
                    (int)Math.Round(
                        wartoscBazowa *
                        skalaInterfejsu));
        }


        // =====================================================
        // SKALOWANY PADDING
        // =====================================================

        private Padding SkalujPadding(
            Padding bazowy)
        {
            return
                new Padding(
                    (int)Math.Round(
                        bazowy.Left *
                        skalaInterfejsu),
                    (int)Math.Round(
                        bazowy.Top *
                        skalaInterfejsu),
                    (int)Math.Round(
                        bazowy.Right *
                        skalaInterfejsu),
                    (int)Math.Round(
                        bazowy.Bottom *
                        skalaInterfejsu));
        }


        // =====================================================
        // SKALOWANY ROZMIAR CZCIONKI W PUNKTACH
        // =====================================================

        private float SkalowanyRozmiarCzcionki(
            float bazowyRozmiarPunktowy)
        {
            // Font w punktach jest już przez GDI renderowany
            // według bieżącego DPI. Aby geometria i tekst miały
            // tę samą KOŃCOWĄ skalę, kompensujemy DeviceDpi.
            float dpi =
                Math.Max(
                    BazoweDpi,
                    this.DeviceDpi);

            float wynik =
                bazowyRozmiarPunktowy *
                skalaInterfejsu *
                BazoweDpi /
                dpi;

            return Math.Max(
                6.0f,
                wynik);
        }


        // =====================================================
        // UTWORZENIE CZCIONKI DLA BIEŻĄCEJ SKALI
        // =====================================================

        private Font UtworzSkalowanaCzcionke(
            BazowaCzcionka bazowa)
        {
            return
                new Font(
                    bazowa.NazwaRodziny,
                    SkalowanyRozmiarCzcionki(
                        bazowa.RozmiarPunktowy),
                    bazowa.Styl,
                    GraphicsUnit.Point);
        }


        private Font UtworzSkalowanaCzcionke(
            float bazowyRozmiar,
            FontStyle styl)
        {
            return
                new Font(
                    "Segoe UI",
                    SkalowanyRozmiarCzcionki(
                        bazowyRozmiar),
                    styl,
                    GraphicsUnit.Point);
        }


        // =====================================================
        // ZASTOSOWANIE SKALI DO JEDNEJ KONTROLKI
        // =====================================================

        private void ZastosujSkaleKontrolki(
            Control kontrolka)
        {
            if (bazoweProstokatyKontrolek.TryGetValue(
                kontrolka,
                out Rectangle bazowyProstokat))
            {
                kontrolka.Bounds =
                    new Rectangle(
                        (int)Math.Round(
                            bazowyProstokat.X *
                            skalaInterfejsu),
                        (int)Math.Round(
                            bazowyProstokat.Y *
                            skalaInterfejsu),
                        SkalujPiksele(
                            bazowyProstokat.Width),
                        SkalujPiksele(
                            bazowyProstokat.Height));
            }

            if (bazowePaddingiKontrolek.TryGetValue(
                kontrolka,
                out Padding bazowyPadding))
            {
                kontrolka.Padding =
                    SkalujPadding(
                        bazowyPadding);
            }

            if (bazoweCzcionkiKontrolek.TryGetValue(
                kontrolka,
                out BazowaCzcionka? bazowaCzcionka))
            {
                kontrolka.Font =
                    UtworzSkalowanaCzcionke(
                        bazowaCzcionka);
            }

            foreach (
                Control dziecko
                in kontrolka.Controls)
            {
                ZastosujSkaleKontrolki(
                    dziecko);
            }
        }


        // =====================================================
        // DODATKOWE PARAMETRY DATAGRIDVIEW
        // =====================================================

        private void ZastosujSkaleTabeli()
        {
            tabela.ColumnHeadersHeight =
                SkalujPiksele(
                    46);

            tabela.RowTemplate.Height =
                SkalujPiksele(
                    40);

            tabela.ColumnHeadersDefaultCellStyle.Font =
                UtworzSkalowanaCzcionke(
                    12.0f,
                    FontStyle.Bold);

            tabela.DefaultCellStyle.Font =
                UtworzSkalowanaCzcionke(
                    13.5f,
                    FontStyle.Regular);

            foreach (
                DataGridViewRow wiersz
                in tabela.Rows)
            {
                wiersz.Height =
                    SkalujPiksele(
                        40);

                foreach (
                    DataGridViewCell komorka
                    in wiersz.Cells)
                {
                    // Komórki, które mają własną czcionkę,
                    // są w grafiku używane do pogrubienia
                    // sobót, niedziel, świąt i dzisiejszego dnia.
                    if (komorka.HasStyle &&
                        komorka.Style.Font != null)
                    {
                        FontStyle styl =
                            komorka.Style.Font.Style;

                        komorka.Style.Font =
                            UtworzSkalowanaCzcionke(
                                13.5f,
                                styl);
                    }
                }
            }
        }


        // =====================================================
        // ZASTOSOWANIE SKALI DO NOWEJ LEGENDY
        // =====================================================

        private void ZastosujSkaleLegendy()
        {
            if (!ukladBazowyZapisany)
            {
                return;
            }

            ZarejestrujNoweKontrolki(
                panelLegenda);

            foreach (
                Control kontrolka
                in panelLegenda.Controls)
            {
                ZastosujSkaleKontrolki(
                    kontrolka);
            }
        }


        // =====================================================
        // DOPASOWANIE CAŁEGO OKNA I ZAWARTOŚCI
        // =====================================================

        private void DopasujOknoDoObszaruRoboczego()
        {
            if (this.IsDisposed ||
                this.Disposing ||
                !ukladBazowyZapisany)
            {
                return;
            }

            SuspendLayout();

            try
            {
                MinimumSize =
                    Size.Empty;

                // Zawsze liczymy od bazy, a nie od poprzedniego
                // rozmiaru. To usuwa efekt 100→125→150→100,
                // w którym zawartość pozostawała przeskalowana.
                skalaInterfejsu =
                    ObliczSkaleInterfejsu();

                // Formularz również ma bazową czcionkę 10 pt.
                this.Font =
                    new Font(
                        "Segoe UI",
                        SkalowanyRozmiarCzcionki(
                            10.0f),
                        FontStyle.Regular,
                        GraphicsUnit.Point);

                foreach (
                    Control kontrolka
                    in this.Controls)
                {
                    ZastosujSkaleKontrolki(
                        kontrolka);
                }

                ZastosujSkaleTabeli();

                // Rozmiar obszaru klienta wynika dokładnie
                // z tego samego współczynnika co jego zawartość.
                this.ClientSize =
                    new Size(
                        SkalujPiksele(
                            BazowaSzerokoscKlienta),
                        SkalujPiksele(
                            BazowaWysokoscKlienta));

                Screen ekran =
                    Screen.FromControl(
                        this);

                Rectangle obszarRoboczy =
                    ekran.WorkingArea;

                int x =
                    obszarRoboczy.Left +
                    Math.Max(
                        0,
                        (obszarRoboczy.Width -
                         this.Width) / 2);

                int y =
                    obszarRoboczy.Top +
                    Math.Max(
                        0,
                        (obszarRoboczy.Height -
                         this.Height) / 2);

                this.Location =
                    new Point(
                        x,
                        y);

                tabela.Invalidate();
            }
            finally
            {
                ResumeLayout(
                    true);
            }
        }


        // =====================================================
        // TWORZENIE INTERFEJSU
        // =====================================================

        private void UtworzInterfejs()
        {
            UtworzNaglowek();


            // -------------------------------------------------
            // NAZWA MIESIĄCA
            // -------------------------------------------------

            lblMiesiac.Location =
                new Point(200, 118);

            lblMiesiac.Width = 620;
            lblMiesiac.Height = 58;

            lblMiesiac.TextAlign =
                ContentAlignment.MiddleCenter;

            lblMiesiac.Font =
                new Font(
                    "Segoe UI",
                    20,
                    FontStyle.Bold);

            lblMiesiac.ForeColor =
                Color.FromArgb(
                    32,
                    39,
                    48);


            // -------------------------------------------------
            // ROK -1
            // -------------------------------------------------

            btnRokWstecz.Text =
                "ROK -1";

            btnRokWstecz.Location =
                new Point(70, 184);

            btnRokWstecz.Width = 120;
            btnRokWstecz.Height = 44;

            btnRokWstecz.Font =
                new Font(
                    "Segoe UI",
                    10.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnRokWstecz,
                false);

            btnRokWstecz.Click +=
                BtnRokWstecz_Click;


            // -------------------------------------------------
            // POPRZEDNI MIESIĄC
            // -------------------------------------------------

            btnPoprzedni.Text =
                "POPRZEDNI MIESIĄC";

            btnPoprzedni.Location =
                new Point(210, 184);

            btnPoprzedni.Width = 200;
            btnPoprzedni.Height = 44;

            btnPoprzedni.Font =
                new Font(
                    "Segoe UI",
                    10.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnPoprzedni,
                false);

            btnPoprzedni.Click +=
                BtnPoprzedni_Click;


            // -------------------------------------------------
            // AKTUALNY DZIEŃ
            // -------------------------------------------------

            btnAktualny.Text =
                "AKTUALNY";

            btnAktualny.Location =
                new Point(430, 184);

            btnAktualny.Width = 160;
            btnAktualny.Height = 44;

            btnAktualny.Font =
                new Font(
                    "Segoe UI",
                    10.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnAktualny,
                true);

            btnAktualny.Click +=
                BtnAktualny_Click;


            // -------------------------------------------------
            // NASTĘPNY MIESIĄC
            // -------------------------------------------------

            btnNastepny.Text =
                "NASTĘPNY MIESIĄC";

            btnNastepny.Location =
                new Point(610, 184);

            btnNastepny.Width = 200;
            btnNastepny.Height = 44;

            btnNastepny.Font =
                new Font(
                    "Segoe UI",
                    10.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnNastepny,
                false);

            btnNastepny.Click +=
                BtnNastepny_Click;


            // -------------------------------------------------
            // ROK +1
            // -------------------------------------------------

            btnRokPrzod.Text =
                "ROK +1";

            btnRokPrzod.Location =
                new Point(830, 184);

            btnRokPrzod.Width = 120;
            btnRokPrzod.Height = 44;

            btnRokPrzod.Font =
                new Font(
                    "Segoe UI",
                    10.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnRokPrzod,
                false);

            btnRokPrzod.Click +=
                BtnRokPrzod_Click;


            // -------------------------------------------------
            // PRZEŁĄCZNIK WIDOKU - POD TABELĄ
            // -------------------------------------------------

            btnWidok.Text =
                "WIDOK VEOLIA";

            btnWidok.Location =
                new Point(210, 748);

            btnWidok.Width = 190;
            btnWidok.Height = 42;

            btnWidok.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnWidok,
                false);

            btnWidok.Click +=
                BtnWidok_Click;


            // -------------------------------------------------
            // PODGLĄD WYDRUKU - POD TABELĄ
            // -------------------------------------------------

            btnPodglad.Text =
                "PODGLĄD WYDRUKU";

            btnPodglad.Location =
                new Point(415, 748);

            btnPodglad.Width = 190;
            btnPodglad.Height = 42;

            btnPodglad.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnPodglad,
                false);

            btnPodglad.Click +=
                BtnPodglad_Click;


            // -------------------------------------------------
            // DRUKUJ A4 - POD TABELĄ
            // -------------------------------------------------

            btnDrukuj.Text =
                "DRUKUJ A4";

            btnDrukuj.Location =
                new Point(620, 748);

            btnDrukuj.Width = 190;
            btnDrukuj.Height = 42;

            btnDrukuj.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnDrukuj,
                false);

            btnDrukuj.Click +=
                BtnDrukuj_Click;


            // -------------------------------------------------
            // TABELA
            // -------------------------------------------------

            tabela.Location =
                new Point(24, 244);

            tabela.Width = 972;

            // Nagłówek + dokładnie 11 pełnych wierszy danych.
            tabela.Height = 489;

            tabela.AllowUserToAddRows =
                false;

            tabela.AllowUserToDeleteRows =
                false;

            tabela.AllowUserToResizeRows =
                false;

            tabela.AllowUserToResizeColumns =
                false;

            tabela.ReadOnly =
                true;

            tabela.RowHeadersVisible =
                false;

            tabela.MultiSelect =
                false;

            tabela.SelectionMode =
                DataGridViewSelectionMode.CellSelect;

            tabela.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            tabela.BackgroundColor =
                Color.White;

            tabela.BorderStyle =
                BorderStyle.FixedSingle;

            tabela.CellBorderStyle =
                DataGridViewCellBorderStyle.Single;

            tabela.GridColor =
                Color.FromArgb(
                    190,
                    197,
                    205);

            tabela.EnableHeadersVisualStyles =
                false;

            tabela.ColumnHeadersHeight =
                46;

            tabela.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            tabela.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(
                    232,
                    236,
                    241);

            tabela.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.FromArgb(
                    28,
                    34,
                    42);

            tabela.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            tabela.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI",
                    12,
                    FontStyle.Bold);

            tabela.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                tabela.ColumnHeadersDefaultCellStyle.BackColor;

            tabela.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            tabela.DefaultCellStyle.Font =
                new Font(
                    "Segoe UI",
                    13.5f);

            tabela.DefaultCellStyle.ForeColor =
                Color.FromArgb(
                    24,
                    30,
                    36);

            tabela.DefaultCellStyle.SelectionForeColor =
                Color.FromArgb(
                    24,
                    30,
                    36);

            tabela.RowTemplate.Height =
                40;

            tabela.ScrollBars =
                ScrollBars.Vertical;

            // Podpowiedzi po najechaniu kursorem
            // (np. nazwa święta przy dacie)
            tabela.ShowCellToolTips =
                true;

            tabela.CellPainting +=
                Tabela_CellPainting;

            // Kliknięcie numeru dnia lub dnia tygodnia
            // otwiera okno ze szczegółami dnia.
            tabela.CellClick +=
                Tabela_CellClick;

            // Kliknięcie nagłówka Brygada A-E / I-V
            // otwiera kwartalny okres rozliczeniowy.
            tabela.ColumnHeaderMouseClick +=
                Tabela_ColumnHeaderMouseClick;


            // -------------------------------------------------
            // LEGENDA
            // -------------------------------------------------

            UtworzLegende();


            // -------------------------------------------------
            // AUTOR
            // -------------------------------------------------

            lblAutor.Text =
                "Autor programu: " +
                AutorProgramu +
                "  •  projekt od: " +
                RozpoczecieProjektu;

            lblAutor.Location =
                new Point(24, 862);

            lblAutor.Width = 972;
            lblAutor.Height = 26;

            lblAutor.TextAlign =
                ContentAlignment.MiddleCenter;

            lblAutor.Font =
                new Font(
                    "Segoe UI",
                    10.5f,
                    FontStyle.Italic);

            lblAutor.ForeColor =
                Color.FromArgb(
                    105,
                    111,
                    118);


            // -------------------------------------------------
            // LINK DO NOWEJ WERSJI
            // -------------------------------------------------

            linkNowaWersja.Text =
                "Pobierz najnowszą wersję: " +
                AdresProjektu;

            linkNowaWersja.Location =
                new Point(24, 888);

            linkNowaWersja.Width = 972;
            linkNowaWersja.Height = 24;

            linkNowaWersja.TextAlign =
                ContentAlignment.MiddleCenter;

            linkNowaWersja.Font =
                new Font(
                    "Segoe UI",
                    9.5f);

            linkNowaWersja.LinkColor =
                Color.FromArgb(
                    0,
                    102,
                    204);

            linkNowaWersja.ActiveLinkColor =
                Color.FromArgb(
                    220,
                    20,
                    60);

            linkNowaWersja.VisitedLinkColor =
                linkNowaWersja.LinkColor;

            linkNowaWersja.LinkClicked +=
                LinkNowaWersja_LinkClicked;


            // -------------------------------------------------
            // USTAWIENIA DRUKOWANIA
            // -------------------------------------------------

            dokumentDruku.DefaultPageSettings.Landscape =
                true;

            dokumentDruku.PrintPage +=
                DokumentDruku_PrintPage;

            podgladDruku.Document =
                dokumentDruku;


            // -------------------------------------------------
            // DODANIE ELEMENTÓW DO OKNA
            // -------------------------------------------------

            this.Controls.Add(
                panelNaglowek);

            this.Controls.Add(
                btnRokWstecz);

            this.Controls.Add(
                btnPoprzedni);

            this.Controls.Add(
                lblMiesiac);

            this.Controls.Add(
                btnNastepny);

            this.Controls.Add(
                btnRokPrzod);

            this.Controls.Add(
                btnAktualny);

            this.Controls.Add(
                btnWidok);

            this.Controls.Add(
                btnPodglad);

            this.Controls.Add(
                btnDrukuj);

            this.Controls.Add(
                tabela);

            this.Controls.Add(
                panelLegenda);

            this.Controls.Add(
                lblAutor);

            this.Controls.Add(
                linkNowaWersja);
        }


        // =====================================================
        // OTWARCIE STRONY PROJEKTU / NOWEJ WERSJI
        // =====================================================

        private void LinkNowaWersja_LinkClicked(
            object? sender,
            LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName =
                            AdresProjektu,

                        UseShellExecute =
                            true
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Nie udało się otworzyć strony projektu.\n\n" +
                    AdresProjektu +
                    "\n\nSzczegóły: " +
                    ex.Message,
                    "Grafik Brygad",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }


        // =====================================================
        // WSPÓLNY STYL PRZYCISKÓW
        // =====================================================

        private void StylizujPrzycisk(
            Button przycisk,
            bool glowny)
        {
            Color kolorAkcentu =
                Color.FromArgb(
                    220,
                    20,
                    40);

            Color kolorAkcentuHover =
                Color.FromArgb(
                    195,
                    15,
                    32);

            Color kolorNeutralny =
                Color.White;

            Color kolorNeutralnyHover =
                Color.FromArgb(
                    238,
                    241,
                    245);

            Color kolorObramowania =
                Color.FromArgb(
                    155,
                    163,
                    172);

            przycisk.FlatStyle =
                FlatStyle.Flat;

            przycisk.UseVisualStyleBackColor =
                false;

            przycisk.Cursor =
                Cursors.Hand;

            przycisk.FlatAppearance.BorderSize =
                1;

            przycisk.FlatAppearance.BorderColor =
                glowny
                    ? kolorAkcentu
                    : kolorObramowania;

            przycisk.BackColor =
                glowny
                    ? kolorAkcentu
                    : kolorNeutralny;

            przycisk.ForeColor =
                glowny
                    ? Color.White
                    : Color.FromArgb(
                        25,
                        30,
                        36);

            przycisk.FlatAppearance.MouseOverBackColor =
                glowny
                    ? kolorAkcentuHover
                    : kolorNeutralnyHover;

            przycisk.FlatAppearance.MouseDownBackColor =
                glowny
                    ? Color.FromArgb(175, 10, 28)
                    : Color.FromArgb(224, 229, 235);
        }


        // =====================================================
        // PROFESJONALNY NAGŁÓWEK
        // =====================================================

        private void UtworzNaglowek()
        {
            panelNaglowek.Location =
                new Point(24, 16);

            panelNaglowek.Width =
                972;

            panelNaglowek.Height =
                94;

            panelNaglowek.BackColor =
                Color.White;

            panelNaglowek.BorderStyle =
                BorderStyle.FixedSingle;


            // -------------------------------------------------
            // LOGO VEOLIA
            // -------------------------------------------------

            logoVeolia.Location =
                new Point(22, 8);

            logoVeolia.Width =
                205;

            logoVeolia.Height =
                76;

            logoVeolia.SizeMode =
                PictureBoxSizeMode.Zoom;

            logoVeolia.BackColor =
                Color.Transparent;

            string sciezkaLogo =
                Path.Combine(
                    Application.StartupPath,
                    "Images",
                    "veolia.png");

            if (File.Exists(sciezkaLogo))
            {
                try
                {
                    logoVeolia.Image =
                        Image.FromFile(
                            sciezkaLogo);
                }
                catch
                {
                    MessageBox.Show(
                        "Nie udało się wczytać pliku logo VEOLIA.",
                        "Błąd logo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(
                    "Nie znaleziono pliku logo:\n\n" +
                    sciezkaLogo,
                    "Brak logo VEOLIA",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }


            // -------------------------------------------------
            // TYTUŁ
            // -------------------------------------------------

            lblTytul.Text =
                "Grafik Brygad VEOLIA Energia Łódź";

            lblTytul.Location =
                new Point(220, 8);

            lblTytul.Width =
                570;

            lblTytul.Height =
                76;

            lblTytul.TextAlign =
                ContentAlignment.MiddleCenter;

            lblTytul.Font =
                new Font(
                    "Segoe UI",
                    20.5f,
                    FontStyle.Bold);

            lblTytul.UseCompatibleTextRendering =
                false;

            lblTytul.AutoEllipsis =
                false;

            lblTytul.Padding =
                new Padding(0, 0, 0, 4);

            lblTytul.ForeColor =
                Color.FromArgb(
                    31,
                    38,
                    47);


            // -------------------------------------------------
            // WERSJA
            // -------------------------------------------------

            lblWersja.Text =
                "(" +
                WersjaProgramu +
                ")";

            lblWersja.Location =
                new Point(792, 31);

            lblWersja.Width =
                154;

            lblWersja.Height =
                30;

            lblWersja.TextAlign =
                ContentAlignment.MiddleRight;

            lblWersja.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Italic);

            lblWersja.ForeColor =
                Color.FromArgb(
                    100,
                    105,
                    112);


            // -------------------------------------------------
            // DODANIE DO PANELU
            // -------------------------------------------------

            panelNaglowek.Controls.Add(
                logoVeolia);

            panelNaglowek.Controls.Add(
                lblTytul);

            panelNaglowek.Controls.Add(
                lblWersja);
        }


        // =====================================================
        // LEGENDA
        // =====================================================

        private void UtworzLegende()
        {
            panelLegenda.Location =
                new Point(24, 802);

            panelLegenda.Width =
                972;

            panelLegenda.Height =
                58;

            panelLegenda.BackColor =
                Color.White;

            panelLegenda.BorderStyle =
                BorderStyle.FixedSingle;

            OdswiezLegende();
        }


        // =====================================================
        // ODŚWIEŻENIE LEGENDY DLA AKTUALNEGO WIDOKU
        // =====================================================

        private void OdswiezLegende()
        {
            if (ukladBazowyZapisany)
            {
                UsunDzieciZBazowegoUkladu(
                    panelLegenda);
            }

            panelLegenda.Controls.Clear();

            if (widokVeolia)
            {
                DodajElementLegendy(
                    "3",
                    "noc",
                    Color.White,
                    12,
                    100);

                DodajElementLegendy(
                    "2",
                    "popołudnie",
                    Color.White,
                    120,
                    140);

                DodajElementLegendy(
                    "1",
                    "rano",
                    Color.White,
                    268,
                    100);

                // W i X pozostają różnymi symbolami i mają
                // nadal oddzielne znaczenie w algorytmie,
                // ale w legendzie są obecnie prezentowane
                // wspólnie jako dni wolne.
                DodajElementLegendy(
                    "w/x",
                    "wolne",
                    Color.LightGray,
                    376,
                    145);
            }
            else
            {
                DodajElementLegendy(
                    "N",
                    "noc",
                    Color.LightSkyBlue,
                    12,
                    100);

                DodajElementLegendy(
                    "P",
                    "popołudnie",
                    Color.Orange,
                    120,
                    140);

                DodajElementLegendy(
                    "R",
                    "rano",
                    Color.LightGreen,
                    268,
                    100);

                DodajElementLegendy(
                    "W/X",
                    "wolne",
                    Color.LightGray,
                    376,
                    145);
            }

            // Kolory dni zgodne z oryginalnym grafikiem VEOLIA:
            // sobota = żółty, niedziela / święto = czerwony.
            DodajElementDnia(
                "Sobota",
                Color.Yellow,
                535,
                130);

            DodajElementDnia(
                "Niedziela / Święto",
                Color.Red,
                673,
                292);

            ZastosujSkaleLegendy();
        }


        // =====================================================
        // ELEMENT LEGENDY
        // =====================================================

        private void DodajElementLegendy(
            string symbol,
            string opis,
            Color kolor,
            int x,
            int szerokosc)
        {
            Panel panel =
                new Panel();

            panel.Location =
                new Point(
                    x,
                    11);

            panel.Width =
                szerokosc;

            panel.Height =
                34;

            panel.BackColor =
                kolor;

            panel.BorderStyle =
                BorderStyle.FixedSingle;

            Label lbl =
                new Label();

            lbl.Text =
                symbol +
                " – " +
                opis;

            lbl.Dock =
                DockStyle.Fill;

            lbl.TextAlign =
                ContentAlignment.MiddleCenter;

            lbl.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            lbl.ForeColor =
                Color.FromArgb(
                    25,
                    30,
                    36);

            panel.Controls.Add(
                lbl);

            panelLegenda.Controls.Add(
                panel);
        }


        // =====================================================
        // ELEMENT LEGENDY KOLORU DATY
        // =====================================================

        private void DodajElementDnia(
            string opis,
            Color kolor,
            int x,
            int szerokosc)
        {
            Panel panel =
                new Panel();

            panel.Location =
                new Point(
                    x,
                    11);

            panel.Width =
                szerokosc;

            panel.Height =
                34;

            panel.BackColor =
                kolor;

            panel.BorderStyle =
                BorderStyle.FixedSingle;

            Label lbl =
                new Label();

            lbl.Text =
                opis;

            lbl.Dock =
                DockStyle.Fill;

            lbl.TextAlign =
                ContentAlignment.MiddleCenter;

            lbl.Font =
                new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold);

            lbl.ForeColor =
                kolor == Color.Red
                    ? Color.White
                    : Color.Black;

            panel.Controls.Add(
                lbl);

            panelLegenda.Controls.Add(
                panel);
        }


        // =====================================================
        // PRZEŁĄCZANIE WIDOKU STANDARD / VEOLIA
        // =====================================================

        private void BtnWidok_Click(
            object? sender,
            EventArgs e)
        {
            int pierwszyWidocznyWiersz = -1;

            if (tabela.Rows.Count > 0)
            {
                try
                {
                    pierwszyWidocznyWiersz =
                        tabela.FirstDisplayedScrollingRowIndex;
                }
                catch
                {
                    pierwszyWidocznyWiersz = -1;
                }
            }

            widokVeolia =
                !widokVeolia;

            btnWidok.Text =
                widokVeolia
                    ? "WIDOK STANDARDOWY"
                    : "WIDOK VEOLIA";

            OdswiezLegende();

            GenerujGrafik();

            // Po zmianie sposobu prezentacji pozostawiamy
            // użytkownika możliwie w tym samym miejscu miesiąca.
            if (pierwszyWidocznyWiersz >= 0 &&
                tabela.Rows.Count > 0)
            {
                int maksymalnyPierwszy =
                    Math.Max(
                        0,
                        tabela.Rows.Count -
                        LiczbaWidocznychDni);

                tabela.FirstDisplayedScrollingRowIndex =
                    Math.Min(
                        pierwszyWidocznyWiersz,
                        maksymalnyPierwszy);
            }

            tabela.ClearSelection();
            tabela.CurrentCell = null;
        }


        // =====================================================
        // ROK WSTECZ
        // =====================================================

        private void BtnRokWstecz_Click(
            object? sender,
            EventArgs e)
        {
            aktualnyMiesiac =
                aktualnyMiesiac.AddYears(-1);

            GenerujGrafik();
        }


        // =====================================================
        // POPRZEDNI MIESIĄC
        // =====================================================

        private void BtnPoprzedni_Click(
            object? sender,
            EventArgs e)
        {
            aktualnyMiesiac =
                aktualnyMiesiac.AddMonths(-1);

            GenerujGrafik();
        }


        // =====================================================
        // NASTĘPNY MIESIĄC
        // =====================================================

        private void BtnNastepny_Click(
            object? sender,
            EventArgs e)
        {
            aktualnyMiesiac =
                aktualnyMiesiac.AddMonths(1);

            GenerujGrafik();
        }


        // =====================================================
        // ROK DO PRZODU
        // =====================================================

        private void BtnRokPrzod_Click(
            object? sender,
            EventArgs e)
        {
            aktualnyMiesiac =
                aktualnyMiesiac.AddYears(1);

            GenerujGrafik();
        }


        // =====================================================
        // AKTUALNY - POWRÓT DO DZISIEJSZEJ DATY
        // =====================================================

        private void BtnAktualny_Click(
            object? sender,
            EventArgs e)
        {
            DateTime dzis =
                DateTime.Today;

            aktualnyMiesiac =
                new DateTime(
                    dzis.Year,
                    dzis.Month,
                    1);

            GenerujGrafik();

            UstawDzisiejszyDzienNaSrodku();
        }


        // =====================================================
        // GENEROWANIE GRAFIKU
        // =====================================================

        private void GenerujGrafik()
        {
            int rok =
                aktualnyMiesiac.Year;

            int miesiac =
                aktualnyMiesiac.Month;

            string nazwaMiesiaca =
                aktualnyMiesiac.ToString(
                    "MMMM yyyy");

            lblMiesiac.Text =
                char.ToUpper(
                    nazwaMiesiaca[0]) +
                nazwaMiesiaca.Substring(1);

            int liczbaDni =
                DateTime.DaysInMonth(
                    rok,
                    miesiac);

            tabela.Columns.Clear();
            tabela.Rows.Clear();

            indeksDzisiejszegoWiersza =
                -1;


            // -------------------------------------------------
            // KOLUMNA: DZIEŃ MIESIĄCA
            // -------------------------------------------------

            tabela.Columns.Add(
                "DzienMiesiaca",
                "DZIEŃ");

            tabela.Columns[0]
                .FillWeight =
                50;


            // -------------------------------------------------
            // KOLUMNA: DZIEŃ TYGODNIA
            // -------------------------------------------------

            tabela.Columns.Add(
                "DzienTygodnia",
                "TYDZ.");

            tabela.Columns[1]
                .FillWeight =
                55;


            // -------------------------------------------------
            // BRYGADY
            // -------------------------------------------------

            string[] oznaczeniaVeolia =
            {
                "I",
                "II",
                "III",
                "IV",
                "V"
            };

            for (int i = 0;
                 i < brygady.Length;
                 i++)
            {
                string naglowekBrygady =
                    widokVeolia
                        ? oznaczeniaVeolia[i]
                        : brygady[i];

                tabela.Columns.Add(
                    "Brygada" + i,
                    naglowekBrygady);

                tabela.Columns[i + 2]
                    .FillWeight =
                    110;

                tabela.Columns[i + 2]
                    .ToolTipText =
                    "Kliknij, aby zobaczyć kwartalny " +
                    "okres rozliczeniowy.";
            }


            // -------------------------------------------------
            // BLOKADA SORTOWANIA KOLUMN
            // -------------------------------------------------
            //
            // Grafik musi zawsze pozostawać w kolejności
            // chronologicznej. Kliknięcie nagłówka kolumny
            // nie może sortować dni ani zmian brygad.

            foreach (
                DataGridViewColumn kolumna
                in tabela.Columns)
            {
                kolumna.SortMode =
                    DataGridViewColumnSortMode.NotSortable;

                kolumna.HeaderCell.SortGlyphDirection =
                    SortOrder.None;
            }


            // -------------------------------------------------
            // DNI MIESIĄCA
            // -------------------------------------------------

            for (int dzien = 1;
                 dzien <= liczbaDni;
                 dzien++)
            {
                DateTime data =
                    new DateTime(
                        rok,
                        miesiac,
                        dzien);

                int numerWiersza =
                    tabela.Rows.Add();

                // Numer dnia miesiąca

                tabela.Rows[numerWiersza]
                    .Cells[0]
                    .Value =
                    data.Day.ToString();

                // Skrót dnia tygodnia

                tabela.Rows[numerWiersza]
                    .Cells[1]
                    .Value =
                    PobierzSkrotDnia(data);


                // -------------------------------------------------
                // ZMIANY BRYGAD
                // -------------------------------------------------

                for (int b = 0;
                     b < brygady.Length;
                     b++)
                {
                    char zmiana =
                        ObliczZmiane(
                            data,
                            datyStartowe[b]);

                    DataGridViewCell komorkaZmiany =
                        tabela.Rows[numerWiersza]
                        .Cells[b + 2];

                    komorkaZmiany.Value =
                        PobierzSymbolWidoku(
                            zmiana);

                    // Zachowujemy rzeczywisty symbol zmiany
                    // niezależnie od sposobu prezentacji.
                    komorkaZmiany.Tag =
                        zmiana;

                    if (CzyKorektaWolnejNiedzieli(
                        data,
                        datyStartowe[b]))
                    {
                        char podstawowa =
                            ObliczZmianePodstawowa(
                                data,
                                datyStartowe[b]);

                        komorkaZmiany.ToolTipText =
                            "Korekta wolnej niedzieli: " +
                            PobierzPelnaNazweZmiany(
                                podstawowa) +
                            " → " +
                            PobierzPelnaNazweZmiany(
                                zmiana);
                    }

                    if (CzyZmianaSwiatecznaWSylwestra(
                        data,
                        zmiana))
                    {
                        if (!string.IsNullOrEmpty(
                            komorkaZmiany.ToolTipText))
                        {
                            komorkaZmiany.ToolTipText +=
                                Environment.NewLine;
                        }

                        komorkaZmiany.ToolTipText +=
                            "31 grudnia – ta zmiana jest traktowana " +
                            "jak praca w dzień świąteczny.";
                    }
                }


                // -------------------------------------------------
                // KOLOR DNIA:
                // SOBOTA = ZIELONY
                // NIEDZIELA / ŚWIĘTO = CZERWONY
                // -------------------------------------------------

                DataGridViewCell komorkaDnia =
                    tabela.Rows[numerWiersza]
                    .Cells[0];

                DataGridViewCell komorkaTygodnia =
                    tabela.Rows[numerWiersza]
                    .Cells[1];

                string? nazwaSwieta =
                    PobierzNazweSwieta(data);

                bool swieto =
                    nazwaSwieta != null;

                // Podpowiedź po najechaniu na numer dnia
                // lub skrót dnia tygodnia.
                //
                // Święto ma pierwszeństwo przed sobotą/niedzielą,
                // dzięki czemu od razu wiadomo, dlaczego data
                // jest oznaczona na czerwono.
                string opisPodpowiedzi =
                    "";

                string pelnaData =
                    data.ToString(
                        "d MMMM yyyy",
                        new CultureInfo("pl-PL"));

                if (swieto)
                {
                    opisPodpowiedzi =
                        pelnaData +
                        " – " +
                        nazwaSwieta;
                }
                else if (CzySylwester(data))
                {
                    opisPodpowiedzi =
                        pelnaData +
                        " – " +
                        PobierzOpisSylwestra();

                    if (data.DayOfWeek ==
                        DayOfWeek.Sunday)
                    {
                        opisPodpowiedzi +=
                            " • Niedziela";
                    }
                    else if (data.DayOfWeek ==
                        DayOfWeek.Saturday)
                    {
                        opisPodpowiedzi +=
                            " • Sobota";
                    }
                }
                else if (data.DayOfWeek ==
                    DayOfWeek.Sunday)
                {
                    opisPodpowiedzi =
                        pelnaData +
                        " – Niedziela";
                }
                else if (data.DayOfWeek ==
                    DayOfWeek.Saturday)
                {
                    opisPodpowiedzi =
                        pelnaData +
                        " – Sobota";
                }

                komorkaDnia.ToolTipText =
                    opisPodpowiedzi;

                komorkaTygodnia.ToolTipText =
                    opisPodpowiedzi;

                if (data.DayOfWeek ==
                    DayOfWeek.Sunday ||
                    swieto)
                {
                    // Oryginalny grafik VEOLIA:
                    // niedziela i święto = czerwone pole,
                    // biały tekst.
                    Color tloDnia =
                        Color.Red;

                    komorkaDnia.Style.BackColor =
                        tloDnia;

                    komorkaTygodnia.Style.BackColor =
                        tloDnia;

                    komorkaDnia.Style.ForeColor =
                        Color.White;

                    komorkaTygodnia.Style.ForeColor =
                        Color.White;

                    komorkaDnia.Style.SelectionBackColor =
                        tloDnia;

                    komorkaTygodnia.Style.SelectionBackColor =
                        tloDnia;

                    komorkaDnia.Style.SelectionForeColor =
                        Color.White;

                    komorkaTygodnia.Style.SelectionForeColor =
                        Color.White;

                    komorkaDnia.Style.Font =
                        new Font(
                            "Segoe UI",
                            13.5f,
                            FontStyle.Bold);

                    komorkaTygodnia.Style.Font =
                        new Font(
                            "Segoe UI",
                            13.5f,
                            FontStyle.Bold);
                }
                else if (data.DayOfWeek ==
                    DayOfWeek.Saturday)
                {
                    // Oryginalny grafik VEOLIA:
                    // sobota = żółte pole, czarny tekst.
                    Color tloDnia =
                        Color.Yellow;

                    komorkaDnia.Style.BackColor =
                        tloDnia;

                    komorkaTygodnia.Style.BackColor =
                        tloDnia;

                    komorkaDnia.Style.ForeColor =
                        Color.Black;

                    komorkaTygodnia.Style.ForeColor =
                        Color.Black;

                    komorkaDnia.Style.SelectionBackColor =
                        tloDnia;

                    komorkaTygodnia.Style.SelectionBackColor =
                        tloDnia;

                    komorkaDnia.Style.SelectionForeColor =
                        Color.Black;

                    komorkaTygodnia.Style.SelectionForeColor =
                        Color.Black;

                    komorkaDnia.Style.Font =
                        new Font(
                            "Segoe UI",
                            13.5f,
                            FontStyle.Bold);

                    komorkaTygodnia.Style.Font =
                        new Font(
                            "Segoe UI",
                            13.5f,
                            FontStyle.Bold);
                }
                else
                {
                    komorkaDnia.Style.BackColor =
                        Color.White;

                    komorkaTygodnia.Style.BackColor =
                        Color.White;

                    komorkaDnia.Style.ForeColor =
                        Color.Black;

                    komorkaTygodnia.Style.ForeColor =
                        Color.Black;
                }


                // -------------------------------------------------
                // DZISIAJ
                // -------------------------------------------------

                if (data.Date ==
                    DateTime.Today)
                {
                    indeksDzisiejszegoWiersza =
                        numerWiersza;
                }
            }

            KolorujZmiany();

            WyróżnijDzisiaj();

            tabela.Invalidate();


            // =================================================
            // USUNIĘCIE NIEBIESKIEGO ZAZNACZENIA
            // PIERWSZEGO DNIA MIESIĄCA
            // =================================================

            tabela.ClearSelection();

            tabela.CurrentCell =
                null;

            // Przy zmianie miesiąca / widoku tworzone są nowe
            // wiersze i część czcionek komórek. Jeżeli interfejs
            // jest już skalowany, natychmiast dostosowujemy także
            // nowe elementy DataGridView.
            if (ukladBazowyZapisany)
            {
                ZastosujSkaleTabeli();
            }
        }


        // =====================================================
        // USTAWIENIE DZISIEJSZEGO DNIA NA ŚRODKU TABELI
        // =====================================================

        private void UstawDzisiejszyDzienNaSrodku()
        {
            if (indeksDzisiejszegoWiersza < 0 ||
                tabela.Rows.Count == 0)
            {
                return;
            }

            int widoczneWiersze =
                Math.Min(
                    LiczbaWidocznychDni,
                    tabela.Rows.Count);

            int pierwszyWiersz =
                indeksDzisiejszegoWiersza -
                widoczneWiersze / 2;

            int maksymalnyPierwszy =
                Math.Max(
                    0,
                    tabela.Rows.Count -
                    widoczneWiersze);

            pierwszyWiersz =
                Math.Max(
                    0,
                    Math.Min(
                        pierwszyWiersz,
                        maksymalnyPierwszy));

            tabela.FirstDisplayedScrollingRowIndex =
                pierwszyWiersz;

            tabela.ClearSelection();

            tabela.CurrentCell =
                null;
        }


        // =====================================================
        // KLIKNIĘCIE NAGŁÓWKA BRYGADY
        // =====================================================

        private void Tabela_ColumnHeaderMouseClick(
            object? sender,
            DataGridViewCellMouseEventArgs e)
        {
            // Kolumny 0 i 1 to DZIEŃ i TYDZ.
            // Okres rozliczeniowy otwieramy tylko dla brygad.
            if (e.ColumnIndex < 2 ||
                e.ColumnIndex >= 2 + brygady.Length)
            {
                return;
            }

            int indeksBrygady =
                e.ColumnIndex - 2;

            PokazOkresRozliczeniowy(
                indeksBrygady);

            tabela.ClearSelection();
            tabela.CurrentCell =
                null;
        }


        // =====================================================
        // POCZĄTEK KWARTALNEGO OKRESU ROZLICZENIOWEGO
        // =====================================================

        private DateTime PobierzPoczatekOkresuRozliczeniowego(
            DateTime data)
        {
            int pierwszyMiesiacKwartału =
                ((data.Month - 1) / 3) * 3 + 1;

            return
                new DateTime(
                    data.Year,
                    pierwszyMiesiacKwartału,
                    1);
        }


        // =====================================================
        // KONIEC KWARTALNEGO OKRESU ROZLICZENIOWEGO
        // =====================================================

        private DateTime PobierzKoniecOkresuRozliczeniowego(
            DateTime data)
        {
            return
                PobierzPoczatekOkresuRozliczeniowego(
                    data)
                .AddMonths(3)
                .AddDays(-1);
        }


        // =====================================================
        // NAZWA OKRESU: lipiec / sierpień / wrzesień 2026
        // =====================================================

        private string PobierzNazweOkresuRozliczeniowego(
            DateTime poczatek)
        {
            CultureInfo polska =
                CultureInfo.GetCultureInfo(
                    "pl-PL");

            string[] miesiace =
            {
                poczatek.ToString(
                    "MMMM",
                    polska),
                poczatek.AddMonths(1).ToString(
                    "MMMM",
                    polska),
                poczatek.AddMonths(2).ToString(
                    "MMMM",
                    polska)
            };

            return
                string.Join(
                    " / ",
                    miesiace) +
                " " +
                poczatek.Year;
        }


        // =====================================================
        // WYMIAR CZASU PRACY W KWARTALE – W DNIACH
        // =====================================================
        //
        // Zasada odpowiada oryginalnemu grafikowi VEOLIA:
        //
        // 1. liczymy wszystkie dni od poniedziałku do piątku,
        // 2. każde święto przypadające od poniedziałku do soboty
        //    zmniejsza wymiar o jeden dzień,
        // 3. święto przypadające w niedzielę nie zmniejsza
        //    wymiaru,
        // 4. PobierzNazweSwieta() zawiera również
        //    Dzień Energetyka 14 sierpnia.
        //
        // Dla III kwartału 2026 wynik wynosi 64 dni,
        // zgodnie z oryginalnym grafikiem VEOLIA.

        private int ObliczWymiarDniOkresuRozliczeniowego(
            DateTime poczatek,
            DateTime koniec)
        {
            int wymiar =
                0;

            for (DateTime data = poczatek.Date;
                 data <= koniec.Date;
                 data = data.AddDays(1))
            {
                if (data.DayOfWeek >= DayOfWeek.Monday &&
                    data.DayOfWeek <= DayOfWeek.Friday)
                {
                    wymiar++;
                }

                if (CzySwieto(data) &&
                    data.DayOfWeek != DayOfWeek.Sunday)
                {
                    wymiar--;
                }
            }

            return
                wymiar;
        }


        // =====================================================
        // LICZBA DNI PRACY BRYGADY W OKRESIE
        // =====================================================
        //
        // Do dni pracy zaliczamy rzeczywiście zaplanowane
        // zmiany R / P / N po uwzględnieniu korekty wolnej
        // niedzieli. W oraz X pozostają dniami wolnymi.

        private int ObliczDniPracyBrygadyWOkresie(
            int indeksBrygady,
            DateTime poczatek,
            DateTime koniec)
        {
            int dniPracy =
                0;

            for (DateTime data = poczatek.Date;
                 data <= koniec.Date;
                 data = data.AddDays(1))
            {
                char zmiana =
                    ObliczZmiane(
                        data,
                        datyStartowe[indeksBrygady]);

                if (zmiana == 'R' ||
                    zmiana == 'P' ||
                    zmiana == 'N')
                {
                    dniPracy++;
                }
            }

            return
                dniPracy;
        }


        // =====================================================
        // SKALOWANIE OKIEN POMOCNICZYCH
        // =====================================================
        //
        // "Okres rozliczeniowy" i "Szczegóły dnia" korzystają
        // z tej samej końcowej skali co główne okno.
        //
        // Dzięki temu Windows nie powiększa ich niezależnie
        // pełnym 125% / 150% przez AutoScaleMode.Dpi.

        private void DopasujOknoPomocniczeDoSkaliGlownego(
            Form okno,
            Size bazowyClientSize)
        {
            okno.SuspendLayout();

            try
            {
                // Najpierw kończymy bazowy layout 96 DPI.
                okno.PerformLayout();

                float skala =
                    Math.Max(
                        0.60f,
                        skalaInterfejsu);

                float dpi =
                    Math.Max(
                        BazoweDpi,
                        this.DeviceDpi);

                foreach (
                    Control kontrolka
                    in okno.Controls)
                {
                    SkalujKontrolkeOknaPomocniczego(
                        kontrolka,
                        skala,
                        dpi);
                }

                okno.ClientSize =
                    new Size(
                        Math.Max(
                            1,
                            (int)Math.Round(
                                bazowyClientSize.Width *
                                skala)),
                        Math.Max(
                            1,
                            (int)Math.Round(
                                bazowyClientSize.Height *
                                skala)));

                okno.PerformLayout();
            }
            finally
            {
                okno.ResumeLayout(
                    true);
            }
        }


        private void SkalujKontrolkeOknaPomocniczego(
            Control kontrolka,
            float skala,
            float dpi)
        {
            Rectangle bazowyProstokat =
                kontrolka.Bounds;

            kontrolka.Bounds =
                new Rectangle(
                    (int)Math.Round(
                        bazowyProstokat.X *
                        skala),
                    (int)Math.Round(
                        bazowyProstokat.Y *
                        skala),
                    Math.Max(
                        1,
                        (int)Math.Round(
                            bazowyProstokat.Width *
                            skala)),
                    Math.Max(
                        1,
                        (int)Math.Round(
                            bazowyProstokat.Height *
                            skala)));

            Padding bazowyPadding =
                kontrolka.Padding;

            kontrolka.Padding =
                new Padding(
                    (int)Math.Round(
                        bazowyPadding.Left *
                        skala),
                    (int)Math.Round(
                        bazowyPadding.Top *
                        skala),
                    (int)Math.Round(
                        bazowyPadding.Right *
                        skala),
                    (int)Math.Round(
                        bazowyPadding.Bottom *
                        skala));

            Font bazowaCzcionka =
                kontrolka.Font;

            float rozmiarCzcionki =
                Math.Max(
                    6.0f,
                    bazowaCzcionka.SizeInPoints *
                    skala *
                    BazoweDpi /
                    dpi);

            kontrolka.Font =
                new Font(
                    bazowaCzcionka.FontFamily.Name,
                    rozmiarCzcionki,
                    bazowaCzcionka.Style,
                    GraphicsUnit.Point);

            // TableLayoutPanel przechowuje część geometrii
            // w RowStyles / ColumnStyles.
            if (kontrolka is TableLayoutPanel tabelaUkladu)
            {
                foreach (
                    RowStyle stylWiersza
                    in tabelaUkladu.RowStyles)
                {
                    if (stylWiersza.SizeType ==
                        SizeType.Absolute)
                    {
                        stylWiersza.Height =
                            Math.Max(
                                1.0f,
                                stylWiersza.Height *
                                skala);
                    }
                }

                foreach (
                    ColumnStyle stylKolumny
                    in tabelaUkladu.ColumnStyles)
                {
                    if (stylKolumny.SizeType ==
                        SizeType.Absolute)
                    {
                        stylKolumny.Width =
                            Math.Max(
                                1.0f,
                                stylKolumny.Width *
                                skala);
                    }
                }
            }

            foreach (
                Control dziecko
                in kontrolka.Controls)
            {
                SkalujKontrolkeOknaPomocniczego(
                    dziecko,
                    skala,
                    dpi);
            }
        }


        // =====================================================
        // OKNO KWARTALNEGO OKRESU ROZLICZENIOWEGO
        // =====================================================

        private void PokazOkresRozliczeniowy(
            int zaznaczonaBrygada)
        {
            DateTime poczatek =
                PobierzPoczatekOkresuRozliczeniowego(
                    aktualnyMiesiac);

            DateTime koniec =
                PobierzKoniecOkresuRozliczeniowego(
                    aktualnyMiesiac);

            int wymiar =
                ObliczWymiarDniOkresuRozliczeniowego(
                    poczatek,
                    koniec);

            int[] dniPracyBrygad =
                new int[brygady.Length];

            int[] dniDoDopracowania =
                new int[brygady.Length];

            // -------------------------------------------------
            // ROCZNY WYMIAR I ROCZNE DNI DO DOPRACOWANIA
            // -------------------------------------------------
            //
            // Roczna wartość jest liczona dokładnie tą samą
            // metodą co wartość kwartalna:
            //
            // pełny wymiar roku kalendarzowego
            // MINUS
            // rzeczywiście zaplanowane zmiany R / P / N.
            //
            // W i X pozostają dniami wolnymi.

            DateTime poczatekRoku =
                new DateTime(
                    aktualnyMiesiac.Year,
                    1,
                    1);

            DateTime koniecRoku =
                new DateTime(
                    aktualnyMiesiac.Year,
                    12,
                    31);

            int wymiarRoczny =
                ObliczWymiarDniOkresuRozliczeniowego(
                    poczatekRoku,
                    koniecRoku);

            int[] dniDoDopracowaniaRocznie =
                new int[brygady.Length];

            for (int i = 0;
                 i < brygady.Length;
                 i++)
            {
                dniPracyBrygad[i] =
                    ObliczDniPracyBrygadyWOkresie(
                        i,
                        poczatek,
                        koniec);

                dniDoDopracowania[i] =
                    wymiar -
                    dniPracyBrygad[i];

                int dniPracyRocznie =
                    ObliczDniPracyBrygadyWOkresie(
                        i,
                        poczatekRoku,
                        koniecRoku);

                dniDoDopracowaniaRocznie[i] =
                    wymiarRoczny -
                    dniPracyRocznie;
            }

            using Form okno =
                new Form();

            // Własne skalowanie zgodne z głównym oknem.
            okno.AutoScaleMode =
                AutoScaleMode.None;

            okno.Text =
                "Okres rozliczeniowy – " +
                PobierzNazweBrygadyWidoku(
                    zaznaczonaBrygada);

            okno.StartPosition =
                FormStartPosition.CenterParent;

            okno.FormBorderStyle =
                FormBorderStyle.FixedDialog;

            okno.MaximizeBox =
                false;

            okno.MinimizeBox =
                false;

            okno.ShowInTaskbar =
                false;

            okno.ClientSize =
                new Size(
                    920,
                    535);

            okno.BackColor =
                Color.FromArgb(
                    245,
                    247,
                    250);


            // -------------------------------------------------
            // TYTUŁ
            // -------------------------------------------------

            Label lblTytulOkresu =
                new Label();

            lblTytulOkresu.Text =
                "KWARTALNY OKRES ROZLICZENIOWY";

            lblTytulOkresu.Location =
                new Point(
                    24,
                    20);

            lblTytulOkresu.Size =
                new Size(
                    872,
                    38);

            lblTytulOkresu.TextAlign =
                ContentAlignment.MiddleCenter;

            lblTytulOkresu.Font =
                new Font(
                    "Segoe UI",
                    17F,
                    FontStyle.Bold);

            lblTytulOkresu.ForeColor =
                Color.FromArgb(
                    25,
                    31,
                    39);

            okno.Controls.Add(
                lblTytulOkresu);


            // -------------------------------------------------
            // NAZWA TRZECH MIESIĘCY
            // -------------------------------------------------

            Label lblOkres =
                new Label();

            lblOkres.Text =
                PobierzNazweOkresuRozliczeniowego(
                    poczatek);

            lblOkres.Location =
                new Point(
                    24,
                    59);

            lblOkres.Size =
                new Size(
                    872,
                    31);

            lblOkres.TextAlign =
                ContentAlignment.MiddleCenter;

            lblOkres.Font =
                new Font(
                    "Segoe UI",
                    12.5F,
                    FontStyle.Bold);

            lblOkres.ForeColor =
                Color.FromArgb(
                    62,
                    70,
                    80);

            okno.Controls.Add(
                lblOkres);


            // -------------------------------------------------
            // WYMIAR WSPÓLNY DLA OKRESU
            // -------------------------------------------------

            Panel panelWymiar =
                new Panel();

            panelWymiar.Location =
                new Point(
                    24,
                    101);

            panelWymiar.Size =
                new Size(
                    872,
                    65);

            panelWymiar.BackColor =
                Color.White;

            panelWymiar.BorderStyle =
                BorderStyle.FixedSingle;

            okno.Controls.Add(
                panelWymiar);

            Label lblOpisWymiaru =
                new Label();

            lblOpisWymiaru.Text =
                "Liczba dni roboczych w okresie " +
                "rozliczeniowym (wymiar czasu)";

            lblOpisWymiaru.Location =
                new Point(
                    18,
                    10);

            lblOpisWymiaru.Size =
                new Size(
                    650,
                    42);

            lblOpisWymiaru.TextAlign =
                ContentAlignment.MiddleLeft;

            lblOpisWymiaru.Font =
                new Font(
                    "Segoe UI",
                    10.5F,
                    FontStyle.Bold);

            panelWymiar.Controls.Add(
                lblOpisWymiaru);

            Label lblWymiar =
                new Label();

            lblWymiar.Text =
                wymiar.ToString();

            lblWymiar.Location =
                new Point(
                    694,
                    7);

            lblWymiar.Size =
                new Size(
                    150,
                    48);

            lblWymiar.TextAlign =
                ContentAlignment.MiddleCenter;

            lblWymiar.Font =
                new Font(
                    "Segoe UI",
                    20F,
                    FontStyle.Bold);

            lblWymiar.ForeColor =
                Color.FromArgb(
                    30,
                    37,
                    46);

            panelWymiar.Controls.Add(
                lblWymiar);


            // -------------------------------------------------
            // TABELA BRYGAD
            // -------------------------------------------------

            TableLayoutPanel tabelaOkresu =
                new TableLayoutPanel();

            tabelaOkresu.Location =
                new Point(
                    24,
                    179);

            tabelaOkresu.Size =
                new Size(
                    872,
                    222);

            tabelaOkresu.BackColor =
                Color.White;

            tabelaOkresu.CellBorderStyle =
                TableLayoutPanelCellBorderStyle.Single;

            tabelaOkresu.ColumnCount =
                6;

            tabelaOkresu.RowCount =
                4;

            tabelaOkresu.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent,
                    44F));

            for (int i = 0;
                 i < 5;
                 i++)
            {
                tabelaOkresu.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent,
                        11.2F));
            }

            tabelaOkresu.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    43F));

            tabelaOkresu.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    59F));

            tabelaOkresu.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    59F));

            tabelaOkresu.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute,
                    59F));

            okno.Controls.Add(
                tabelaOkresu);


            // -------------------------------------------------
            // NAGŁÓWKI BRYGAD
            // -------------------------------------------------

            Label pustyNaglowek =
                new Label();

            pustyNaglowek.Dock =
                DockStyle.Fill;

            pustyNaglowek.TextAlign =
                ContentAlignment.MiddleCenter;

            pustyNaglowek.BackColor =
                Color.FromArgb(
                    232,
                    236,
                    241);

            tabelaOkresu.Controls.Add(
                pustyNaglowek,
                0,
                0);

            for (int i = 0;
                 i < brygady.Length;
                 i++)
            {
                Label naglowek =
                    new Label();

                naglowek.Dock =
                    DockStyle.Fill;

                naglowek.Margin =
                    Padding.Empty;

                naglowek.Text =
                    PobierzNazweBrygadyWidoku(i)
                    .Replace(
                        "Brygada ",
                        "");

                naglowek.TextAlign =
                    ContentAlignment.MiddleCenter;

                naglowek.Font =
                    new Font(
                        "Segoe UI",
                        10F,
                        FontStyle.Bold);

                naglowek.BackColor =
                    i == zaznaczonaBrygada
                        ? Color.Khaki
                        : Color.FromArgb(
                            232,
                            236,
                            241);

                naglowek.ForeColor =
                    Color.FromArgb(
                        25,
                        31,
                        39);

                tabelaOkresu.Controls.Add(
                    naglowek,
                    i + 1,
                    0);
            }


            // -------------------------------------------------
            // OPIS: DNI PRACY BRYGAD
            // -------------------------------------------------

            Label lblOpisDniPracy =
                new Label();

            lblOpisDniPracy.Dock =
                DockStyle.Fill;

            lblOpisDniPracy.Margin =
                Padding.Empty;

            lblOpisDniPracy.Padding =
                new Padding(
                    12,
                    0,
                    8,
                    0);

            lblOpisDniPracy.Text =
                "Liczba dni roboczych w okresie " +
                "rozliczeniowym dla poszczególnych brygad";

            lblOpisDniPracy.TextAlign =
                ContentAlignment.MiddleLeft;

            lblOpisDniPracy.Font =
                new Font(
                    "Segoe UI",
                    9.5F,
                    FontStyle.Regular);

            tabelaOkresu.Controls.Add(
                lblOpisDniPracy,
                0,
                1);


            // -------------------------------------------------
            // OPIS: DNI DO DOPRACOWANIA
            // -------------------------------------------------

            Label lblOpisDopracowania =
                new Label();

            lblOpisDopracowania.Dock =
                DockStyle.Fill;

            lblOpisDopracowania.Margin =
                Padding.Empty;

            lblOpisDopracowania.Padding =
                new Padding(
                    12,
                    0,
                    8,
                    0);

            lblOpisDopracowania.Text =
                "Liczba dni do dopracowania do pełnego " +
                "wymiaru czasu pracy";

            lblOpisDopracowania.TextAlign =
                ContentAlignment.MiddleLeft;

            lblOpisDopracowania.Font =
                new Font(
                    "Segoe UI",
                    9.5F,
                    FontStyle.Regular);

            tabelaOkresu.Controls.Add(
                lblOpisDopracowania,
                0,
                2);


            // -------------------------------------------------
            // OPIS: DNI DO DOPRACOWANIA W ROKU KALENDARZOWYM
            // -------------------------------------------------

            Label lblOpisDopracowaniaRocznego =
                new Label();

            lblOpisDopracowaniaRocznego.Dock =
                DockStyle.Fill;

            lblOpisDopracowaniaRocznego.Margin =
                Padding.Empty;

            lblOpisDopracowaniaRocznego.Padding =
                new Padding(
                    12,
                    0,
                    8,
                    0);

            lblOpisDopracowaniaRocznego.Text =
                "Liczba dni do dopracowania do pełnego " +
                "wymiaru czasu pracy w ciągu roku kalendarzowego";

            lblOpisDopracowaniaRocznego.TextAlign =
                ContentAlignment.MiddleLeft;

            lblOpisDopracowaniaRocznego.Font =
                new Font(
                    "Segoe UI",
                    9.2F,
                    FontStyle.Regular);

            tabelaOkresu.Controls.Add(
                lblOpisDopracowaniaRocznego,
                0,
                3);


            // -------------------------------------------------
            // WARTOŚCI DLA PIĘCIU BRYGAD
            // -------------------------------------------------

            for (int i = 0;
                 i < brygady.Length;
                 i++)
            {
                Label lblDniPracy =
                    new Label();

                lblDniPracy.Dock =
                    DockStyle.Fill;

                lblDniPracy.Margin =
                    Padding.Empty;

                lblDniPracy.Text =
                    dniPracyBrygad[i]
                    .ToString();

                lblDniPracy.TextAlign =
                    ContentAlignment.MiddleCenter;

                lblDniPracy.Font =
                    new Font(
                        "Segoe UI",
                        14F,
                        i == zaznaczonaBrygada
                            ? FontStyle.Bold
                            : FontStyle.Regular);

                lblDniPracy.BackColor =
                    i == zaznaczonaBrygada
                        ? Color.LightGoldenrodYellow
                        : Color.White;

                tabelaOkresu.Controls.Add(
                    lblDniPracy,
                    i + 1,
                    1);


                Label lblDopracowanie =
                    new Label();

                lblDopracowanie.Dock =
                    DockStyle.Fill;

                lblDopracowanie.Margin =
                    Padding.Empty;

                lblDopracowanie.Text =
                    dniDoDopracowania[i]
                    .ToString();

                lblDopracowanie.TextAlign =
                    ContentAlignment.MiddleCenter;

                lblDopracowanie.Font =
                    new Font(
                        "Segoe UI",
                        14F,
                        FontStyle.Bold);

                lblDopracowanie.BackColor =
                    i == zaznaczonaBrygada
                        ? Color.LightGreen
                        : Color.FromArgb(
                            248,
                            248,
                            248);

                tabelaOkresu.Controls.Add(
                    lblDopracowanie,
                    i + 1,
                    2);


                Label lblDopracowanieRoczne =
                    new Label();

                lblDopracowanieRoczne.Dock =
                    DockStyle.Fill;

                lblDopracowanieRoczne.Margin =
                    Padding.Empty;

                lblDopracowanieRoczne.Text =
                    dniDoDopracowaniaRocznie[i]
                    .ToString();

                lblDopracowanieRoczne.TextAlign =
                    ContentAlignment.MiddleCenter;

                lblDopracowanieRoczne.Font =
                    new Font(
                        "Segoe UI",
                        14F,
                        FontStyle.Bold);

                lblDopracowanieRoczne.BackColor =
                    i == zaznaczonaBrygada
                        ? Color.Khaki
                        : Color.FromArgb(
                            242,
                            245,
                            248);

                tabelaOkresu.Controls.Add(
                    lblDopracowanieRoczne,
                    i + 1,
                    3);
            }


            // -------------------------------------------------
            // INFORMACJA POMOCNICZA
            // -------------------------------------------------

            Label lblInfo =
                new Label();

            lblInfo.Location =
                new Point(
                    24,
                    417);

            lblInfo.Size =
                new Size(
                    872,
                    42);

            lblInfo.Text =
                "Do dni pracy brygady zaliczane są zmiany " +
                "R / P / N.  W i X pozostają dniami wolnymi.  " +
                "Wymiar uwzględnia święta oraz Dzień Energetyka.";

            lblInfo.TextAlign =
                ContentAlignment.MiddleCenter;

            lblInfo.Font =
                new Font(
                    "Segoe UI",
                    8.8F,
                    FontStyle.Italic);

            lblInfo.ForeColor =
                Color.DimGray;

            okno.Controls.Add(
                lblInfo);


            // -------------------------------------------------
            // ZAMKNIJ
            // -------------------------------------------------

            Button btnZamknij =
                new Button();

            btnZamknij.Text =
                "ZAMKNIJ";

            btnZamknij.Location =
                new Point(
                    360,
                    475);

            btnZamknij.Size =
                new Size(
                    200,
                    40);

            btnZamknij.Font =
                new Font(
                    "Segoe UI",
                    9.5F,
                    FontStyle.Bold);

            btnZamknij.BackColor =
                Color.White;

            btnZamknij.FlatStyle =
                FlatStyle.Flat;

            btnZamknij.DialogResult =
                DialogResult.OK;

            okno.AcceptButton =
                btnZamknij;

            okno.CancelButton =
                btnZamknij;

            okno.Controls.Add(
                btnZamknij);

            DopasujOknoPomocniczeDoSkaliGlownego(
                okno,
                new Size(
                    920,
                    535));

            okno.ShowDialog(
                this);
        }


        // =====================================================
        // KLIKNIĘCIE DNIA - SZCZEGÓŁY
        // =====================================================

        private void Tabela_CellClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            // Reagujemy tylko na kliknięcie w kolumnę DZIEŃ
            // albo TYDZ. oraz pomijamy nagłówki tabeli.
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0 ||
                e.ColumnIndex > 1)
            {
                return;
            }

            object? wartoscDnia =
                tabela.Rows[e.RowIndex]
                .Cells[0]
                .Value;

            if (wartoscDnia == null ||
                !int.TryParse(
                    wartoscDnia.ToString(),
                    out int numerDnia))
            {
                return;
            }

            DateTime data =
                new DateTime(
                    aktualnyMiesiac.Year,
                    aktualnyMiesiac.Month,
                    numerDnia);

            PokazSzczegolyDnia(data);

            // Po zamknięciu szczegółów usuwamy systemowe
            // niebieskie zaznaczenie klikniętej komórki.
            tabela.ClearSelection();
            tabela.CurrentCell = null;
        }


        // =====================================================
        // OKNO SZCZEGÓŁÓW DNIA
        // =====================================================

        private void PokazSzczegolyDnia(
            DateTime data)
        {
            using Form okno =
                new Form();

            // Własne skalowanie zgodne z głównym oknem.
            okno.AutoScaleMode =
                AutoScaleMode.None;

            okno.Text =
                "Szczegóły dnia";

            okno.StartPosition =
                FormStartPosition.CenterParent;

            okno.FormBorderStyle =
                FormBorderStyle.FixedDialog;

            okno.MaximizeBox =
                false;

            okno.MinimizeBox =
                false;

            okno.ShowInTaskbar =
                false;

            okno.ClientSize =
                new Size(540, 500);

            okno.BackColor =
                Color.FromArgb(
                    245,
                    247,
                    250);


            // -------------------------------------------------
            // PEŁNA DATA
            // -------------------------------------------------

            Label lblData =
                new Label();

            lblData.Location =
                new Point(20, 12);

            lblData.Width =
                500;

            lblData.Height =
                46;

            lblData.TextAlign =
                ContentAlignment.MiddleCenter;

            lblData.Font =
                new Font(
                    "Segoe UI",
                    16,
                    FontStyle.Bold);

            lblData.ForeColor =
                Color.FromArgb(
                    31,
                    38,
                    47);


            // -------------------------------------------------
            // NAZWA ŚWIĘTA / DNIA SZCZEGÓLNEGO
            // -------------------------------------------------

            Label lblSwieto =
                new Label();

            lblSwieto.Location =
                new Point(24, 62);

            lblSwieto.Width =
                492;

            lblSwieto.Height =
                46;

            lblSwieto.TextAlign =
                ContentAlignment.MiddleCenter;

            lblSwieto.Font =
                new Font(
                    "Segoe UI",
                    12,
                    FontStyle.Bold);

            lblSwieto.BorderStyle =
                BorderStyle.FixedSingle;

            lblSwieto.BackColor =
                Color.White;


            // -------------------------------------------------
            // NAGŁÓWEK LISTY BRYGAD
            // -------------------------------------------------

            Label lblZmiany =
                new Label();

            lblZmiany.Location =
                new Point(24, 114);

            lblZmiany.Width =
                492;

            lblZmiany.Height =
                34;

            lblZmiany.Text =
                "Zmiany brygad";

            lblZmiany.TextAlign =
                ContentAlignment.MiddleCenter;

            lblZmiany.Font =
                new Font(
                    "Segoe UI",
                    11.5f,
                    FontStyle.Bold);

            lblZmiany.UseCompatibleTextRendering =
                false;

            lblZmiany.Padding =
                new Padding(0, 0, 0, 2);


            // -------------------------------------------------
            // PANEL ZE ZMIANAMI BRYGAD
            // -------------------------------------------------

            Panel panelZmiany =
                new Panel();

            panelZmiany.Location =
                new Point(24, 150);

            panelZmiany.Width =
                492;

            panelZmiany.Height =
                210;

            panelZmiany.BackColor =
                Color.White;

            panelZmiany.BorderStyle =
                BorderStyle.FixedSingle;


            Label[] etykietyZmian =
                new Label[brygady.Length];

            int y = 10;

            for (int b = 0;
                 b < brygady.Length;
                 b++)
            {
                Label lblBrygada =
                    new Label();

                lblBrygada.Location =
                    new Point(16, y);

                lblBrygada.Width =
                    200;

                lblBrygada.Height =
                    32;

                lblBrygada.Text =
                    PobierzNazweBrygadyWidoku(
                        b);

                lblBrygada.TextAlign =
                    ContentAlignment.MiddleLeft;

                lblBrygada.Font =
                    new Font(
                        "Segoe UI",
                        10,
                        FontStyle.Bold);

                lblBrygada.ForeColor =
                    Color.FromArgb(
                        28,
                        34,
                        42);


                Label lblZmiana =
                    new Label();

                lblZmiana.Location =
                    new Point(240, y);

                lblZmiana.Width =
                    230;

                lblZmiana.Height =
                    32;

                lblZmiana.TextAlign =
                    ContentAlignment.MiddleCenter;

                lblZmiana.Font =
                    new Font(
                        "Segoe UI",
                        10,
                        FontStyle.Bold);

                lblZmiana.BorderStyle =
                    BorderStyle.FixedSingle;

                lblZmiana.ForeColor =
                    Color.FromArgb(
                        20,
                        25,
                        30);

                etykietyZmian[b] =
                    lblZmiana;

                panelZmiany.Controls.Add(
                    lblBrygada);

                panelZmiany.Controls.Add(
                    lblZmiana);

                y += 39;
            }


            // -------------------------------------------------
            // NAWIGACJA DZIEŃ PO DNIU
            // -------------------------------------------------

            Button btnPoprzedniDzien =
                new Button();

            btnPoprzedniDzien.Text =
                "Poprzedni";

            btnPoprzedniDzien.Location =
                new Point(24, 388);

            btnPoprzedniDzien.Width =
                145;

            btnPoprzedniDzien.Height =
                40;

            btnPoprzedniDzien.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnPoprzedniDzien,
                false);


            // -------------------------------------------------
            // POWRÓT DO DZISIEJSZEGO DNIA
            // -------------------------------------------------

            Button btnDzisiajDzien =
                new Button();

            btnDzisiajDzien.Text =
                "Dzisiaj";

            btnDzisiajDzien.Location =
                new Point(195, 388);

            btnDzisiajDzien.Width =
                150;

            btnDzisiajDzien.Height =
                40;

            btnDzisiajDzien.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnDzisiajDzien,
                true);


            Button btnNastepnyDzien =
                new Button();

            btnNastepnyDzien.Text =
                "Następny";

            btnNastepnyDzien.Location =
                new Point(371, 388);

            btnNastepnyDzien.Width =
                145;

            btnNastepnyDzien.Height =
                40;

            btnNastepnyDzien.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnNastepnyDzien,
                false);


            // -------------------------------------------------
            // ZAMKNIĘCIE OKNA
            // -------------------------------------------------

            Button btnZamknijSzczegoly =
                new Button();

            btnZamknijSzczegoly.Text =
                "ZAMKNIJ";

            btnZamknijSzczegoly.Location =
                new Point(
                    195,
                    446);

            btnZamknijSzczegoly.Width =
                150;

            btnZamknijSzczegoly.Height =
                38;

            btnZamknijSzczegoly.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnZamknijSzczegoly,
                false);

            btnZamknijSzczegoly.DialogResult =
                DialogResult.Cancel;

            // Klawisz Esc również zamyka okno.
            okno.CancelButton =
                btnZamknijSzczegoly;


            // -------------------------------------------------
            // ODŚWIEŻANIE ZAWARTOŚCI OKNA
            // -------------------------------------------------

            DateTime aktualnaData =
                data.Date;

            Action odswiezSzczegoly =
                () =>
                {
                    lblData.Text =
                        PobierzPelnaDate(
                            aktualnaData);

                    string? nazwaSwieta =
                        PobierzNazweSwieta(
                            aktualnaData);

                    if (nazwaSwieta != null ||
                        aktualnaData.DayOfWeek == DayOfWeek.Sunday)
                    {
                        lblData.ForeColor =
                            Color.White;

                        lblData.BackColor =
                            Color.Red;
                    }
                    else if (aktualnaData.DayOfWeek == DayOfWeek.Saturday)
                    {
                        lblData.ForeColor =
                            Color.Black;

                        lblData.BackColor =
                            Color.Yellow;
                    }
                    else
                    {
                        lblData.ForeColor =
                            Color.FromArgb(
                                31,
                                38,
                                47);

                        lblData.BackColor =
                            Color.Transparent;
                    }

                    if (nazwaSwieta != null)
                    {
                        lblSwieto.Text =
                            nazwaSwieta;

                        lblSwieto.ForeColor =
                            Color.White;

                        lblSwieto.BackColor =
                            Color.Red;
                    }
                    else if (CzySylwester(
                        aktualnaData))
                    {
                        lblSwieto.Text =
                            PobierzOpisSylwestra();

                        lblSwieto.ForeColor =
                            Color.White;

                        lblSwieto.BackColor =
                            Color.Red;
                    }
                    else
                    {
                        lblSwieto.Text =
                            "Brak święta";

                        lblSwieto.ForeColor =
                            Color.DimGray;

                        lblSwieto.BackColor =
                            Color.White;
                    }

                    for (int b = 0;
                         b < brygady.Length;
                         b++)
                    {
                        char zmiana =
                            ObliczZmiane(
                                aktualnaData,
                                datyStartowe[b]);

                        etykietyZmian[b].Text =
                            PobierzPelnaNazweZmianyWidoku(
                                zmiana);

                        if (CzyZmianaSwiatecznaWSylwestra(
                            aktualnaData,
                            zmiana))
                        {
                            etykietyZmian[b].BackColor =
                                Color.Red;

                            etykietyZmian[b].ForeColor =
                                Color.White;
                        }
                        else if (widokVeolia)
                        {
                            etykietyZmian[b].BackColor =
                                CzyKorektaWolnejNiedzieli(
                                    aktualnaData,
                                    datyStartowe[b])
                                    ? Color.Khaki
                                    : Color.White;

                            etykietyZmian[b].ForeColor =
                                Color.FromArgb(
                                    20,
                                    25,
                                    30);
                        }
                        else
                        {
                            etykietyZmian[b].BackColor =
                                PobierzKolorZmiany(
                                    zmiana);

                            etykietyZmian[b].ForeColor =
                                Color.FromArgb(
                                    20,
                                    25,
                                    30);
                        }
                    }
                };


            btnPoprzedniDzien.Click +=
                (sender, e) =>
                {
                    aktualnaData =
                        aktualnaData.AddDays(-1);

                    odswiezSzczegoly();
                };

            btnDzisiajDzien.Click +=
                (sender, e) =>
                {
                    aktualnaData =
                        DateTime.Today;

                    odswiezSzczegoly();
                };

            btnNastepnyDzien.Click +=
                (sender, e) =>
                {
                    aktualnaData =
                        aktualnaData.AddDays(1);

                    odswiezSzczegoly();
                };


            // -------------------------------------------------
            // DODANIE ELEMENTÓW I WYŚWIETLENIE OKNA
            // -------------------------------------------------

            okno.Controls.Add(
                lblData);

            okno.Controls.Add(
                lblSwieto);

            okno.Controls.Add(
                lblZmiany);

            okno.Controls.Add(
                panelZmiany);

            okno.Controls.Add(
                btnPoprzedniDzien);

            okno.Controls.Add(
                btnDzisiajDzien);

            okno.Controls.Add(
                btnNastepnyDzien);

            okno.Controls.Add(
                btnZamknijSzczegoly);

            odswiezSzczegoly();

            DopasujOknoPomocniczeDoSkaliGlownego(
                okno,
                new Size(
                    540,
                    500));

            okno.ShowDialog(this);
        }


        // =====================================================
        // PEŁNA DATA PO POLSKU
        // =====================================================

        private string PobierzPelnaDate(
            DateTime data)
        {
            CultureInfo polska =
                CultureInfo.GetCultureInfo(
                    "pl-PL");

            string tekst =
                data.ToString(
                    "dddd, d MMMM yyyy",
                    polska);

            if (string.IsNullOrEmpty(tekst))
            {
                return data.ToShortDateString();
            }

            return
                char.ToUpper(
                    tekst[0],
                    polska) +
                tekst.Substring(1);
        }


        // =====================================================
        // KOLOR ZMIANY - WSPÓLNY DLA OKNA SZCZEGÓŁÓW
        // =====================================================

        private Color PobierzKolorZmiany(
            char zmiana)
        {
            switch (zmiana)
            {
                case 'N':
                    return Color.LightSkyBlue;

                case 'P':
                    return Color.Orange;

                case 'R':
                    return Color.LightGreen;

                case 'W':
                case 'X':
                    // W i X są nadal oddzielnymi stanami
                    // logicznymi, lecz celowo mają ten sam kolor.
                    return Color.LightGray;

                default:
                    return Color.White;
            }
        }


        // =====================================================
        // PEŁNA NAZWA ZMIANY
        // =====================================================

        private string PobierzPelnaNazweZmiany(
            char zmiana)
        {
            switch (zmiana)
            {
                case 'R':
                    return "R – rano";

                case 'P':
                    return "P – popołudnie";

                case 'N':
                    return "N – noc";

                case 'W':
                    return "W – wolne";

                case 'X':
                    return "X – wolne";

                default:
                    return zmiana.ToString();
            }
        }


        // =====================================================
        // SYMBOL ZMIANY W AKTUALNYM WIDOKU
        // =====================================================

        private string PobierzSymbolWidoku(
            char zmiana)
        {
            if (!widokVeolia)
            {
                return zmiana.ToString();
            }

            return zmiana switch
            {
                'R' => "1",
                'P' => "2",
                'N' => "3",
                'W' => "w",
                'X' => "x",
                _ => zmiana.ToString()
            };
        }


        // =====================================================
        // PEŁNA NAZWA ZMIANY W AKTUALNYM WIDOKU
        // =====================================================

        private string PobierzPelnaNazweZmianyWidoku(
            char zmiana)
        {
            if (!widokVeolia)
            {
                return PobierzPelnaNazweZmiany(
                    zmiana);
            }

            return zmiana switch
            {
                'R' => "1 – rano",
                'P' => "2 – popołudnie",
                'N' => "3 – noc",
                'W' => "w – wolne",
                'X' => "x – wolne",
                _ => zmiana.ToString()
            };
        }


        // =====================================================
        // NAZWA BRYGADY W AKTUALNYM WIDOKU
        // =====================================================

        private string PobierzNazweBrygadyWidoku(
            int indeks)
        {
            if (!widokVeolia)
            {
                return brygady[indeks];
            }

            string[] oznaczenia =
            {
                "I",
                "II",
                "III",
                "IV",
                "V"
            };

            return
                "Brygada " +
                oznaczenia[indeks];
        }


        // =====================================================
        // SKRÓT DNIA TYGODNIA
        // =====================================================

        private string PobierzSkrotDnia(
            DateTime data)
        {
            string[] skrotyDni =
            {
                "N",
                "Pn",
                "Wt",
                "Śr",
                "Cz",
                "Pt",
                "So"
            };

            return
                skrotyDni[
                    (int)data.DayOfWeek];
        }


        // =====================================================
        // NAZWY ŚWIĄT
        //
        // Oprócz polskich świąt ustawowo wolnych od pracy
        // program traktuje 14 sierpnia jako Dzień Energetyka
        // zgodnie z założeniem tego grafiku.
        // =====================================================

        private string? PobierzNazweSwieta(
            DateTime data)
        {
            DateTime dzien =
                data.Date;

            int rok =
                dzien.Year;

            DateTime wielkanoc =
                ObliczWielkanoc(rok);

            if (dzien == new DateTime(rok, 1, 1))
                return "Nowy Rok";

            if (dzien == new DateTime(rok, 1, 6))
                return "Święto Trzech Króli";

            if (dzien == wielkanoc)
                return "Niedziela Wielkanocna";

            if (dzien == wielkanoc.AddDays(1))
                return "Poniedziałek Wielkanocny";

            if (dzien == new DateTime(rok, 5, 1))
                return "Święto Pracy";

            if (dzien == new DateTime(rok, 5, 3))
                return "Święto Konstytucji 3 Maja";

            if (dzien == wielkanoc.AddDays(49))
                return "Zielone Świątki";

            if (dzien == wielkanoc.AddDays(60))
                return "Boże Ciało";

            if (dzien == new DateTime(rok, 8, 14))
                return "Dzień Energetyka";

            if (dzien == new DateTime(rok, 8, 15))
                return "Wniebowzięcie Najświętszej Maryi Panny";

            if (dzien == new DateTime(rok, 11, 1))
                return "Wszystkich Świętych";

            if (dzien == new DateTime(rok, 11, 11))
                return "Narodowe Święto Niepodległości";

            if (dzien == new DateTime(rok, 12, 24))
                return "Wigilia Bożego Narodzenia";

            if (dzien == new DateTime(rok, 12, 25))
                return "Boże Narodzenie – pierwszy dzień";

            if (dzien == new DateTime(rok, 12, 26))
                return "Boże Narodzenie – drugi dzień";

            return null;
        }


        // =====================================================
        // CZY DZIEŃ JEST ŚWIĘTEM
        // =====================================================

        private bool CzySwieto(
            DateTime data)
        {
            return
                PobierzNazweSwieta(data) != null;
        }


        // =====================================================
        // SYLWESTER – SZCZEGÓLNA ZASADA VEOLIA
        // =====================================================
        //
        // 31 grudnia NIE jest w grafiku traktowany jako zwykłe
        // święto dla wszystkich brygad.
        //
        // Tylko brygady pracujące tego dnia na zmianie:
        // P – popołudniowej
        // N – nocnej
        //
        // wykonują pracę traktowaną jak w dzień świąteczny.
        //
        // R, W i X pozostają bez zmian.
        // Zasada nie przesuwa cyklu 20-dniowego.

        private bool CzySylwester(
            DateTime data)
        {
            return
                data.Month == 12 &&
                data.Day == 31;
        }


        private bool CzyZmianaSwiatecznaWSylwestra(
            DateTime data,
            char zmiana)
        {
            return
                CzySylwester(data) &&
                (zmiana == 'P' ||
                 zmiana == 'N');
        }


        private string PobierzOpisSylwestra()
        {
            return
                "Sylwester – zmiany P i N jak dzień świąteczny";
        }

        // =====================================================
        // OBLICZANIE DATY WIELKANOCY
        // =====================================================

        private DateTime ObliczWielkanoc(
            int rok)
        {
            int a = rok % 19;
            int b = rok / 100;
            int c = rok % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h =
                (19 * a +
                 b -
                 d -
                 g +
                 15) % 30;

            int i = c / 4;
            int k = c % 4;

            int l =
                (32 +
                 2 * e +
                 2 * i -
                 h -
                 k) % 7;

            int m =
                (a +
                 11 * h +
                 22 * l) / 451;

            int miesiac =
                (h +
                 l -
                 7 * m +
                 114) / 31;

            int dzien =
                ((h +
                  l -
                  7 * m +
                  114) % 31) + 1;

            return
                new DateTime(
                    rok,
                    miesiac,
                    dzien);
        }


        // =====================================================
        // OBLICZANIE ZMIANY
        // =====================================================

        private char ObliczZmiane(
            DateTime data,
            DateTime dataStartowa)
        {
            char zmianaPodstawowa =
                ObliczZmianePodstawowa(
                    data,
                    dataStartowa);

            // -------------------------------------------------
            // KOREKTA WOLNEJ NIEDZIELI
            // -------------------------------------------------
            //
            // Zasada VEOLIA:
            // - brygada, która kończy czwartą ranną zmianę
            //   w środę, pracuje na R w najbliższą niedzielę;
            // - brygada, która według cyklu miałaby w tę
            //   niedzielę czwartą ranną zmianę, otrzymuje W.
            //
            // Korekta dotyczy wyłącznie tej niedzieli.
            // Nie przesuwa 20-dniowego cyklu brygady.

            if (data.DayOfWeek == DayOfWeek.Sunday)
            {
                // Brygada, która normalnie kończyłaby serię R
                // właśnie w niedzielę, otrzymuje wolne W.
                if (zmianaPodstawowa == 'R' &&
                    CzyCzwartaRanna(
                        data,
                        dataStartowa))
                {
                    return 'W';
                }

                // Sprawdzamy środę poprzedzającą tę niedzielę.
                DateTime poprzedniaSroda =
                    data.AddDays(-4);

                // Jeżeli brygada zakończyła serię R w środę,
                // przejmuje ranną zmianę w niedzielę.
                if (zmianaPodstawowa == 'W' &&
                    CzyCzwartaRanna(
                        poprzedniaSroda,
                        dataStartowa))
                {
                    return 'R';
                }
            }

            return zmianaPodstawowa;
        }


        // =====================================================
        // CZY W TYM DNIU ZASTOSOWANO KOREKTĘ WOLNEJ NIEDZIELI
        // =====================================================

        private bool CzyKorektaWolnejNiedzieli(
            DateTime data,
            DateTime dataStartowa)
        {
            if (data.DayOfWeek != DayOfWeek.Sunday)
            {
                return false;
            }

            return
                ObliczZmiane(
                    data,
                    dataStartowa) !=
                ObliczZmianePodstawowa(
                    data,
                    dataStartowa);
        }


        // =====================================================
        // PODSTAWOWA ZMIANA Z CYKLU 20-DNIOWEGO
        // =====================================================

        private char ObliczZmianePodstawowa(
            DateTime data,
            DateTime dataStartowa)
        {
            int roznicaDni =
                (data - dataStartowa).Days;

            int pozycja =
                ((roznicaDni % 20) + 20) % 20;

            return cykl[pozycja];
        }


        // =====================================================
        // CZY TO CZWARTA RANNA ZMIANA
        // =====================================================

        private bool CzyCzwartaRanna(
            DateTime data,
            DateTime dataStartowa)
        {
            // Czwarta R jest ostatnim dniem serii rannych zmian:
            // dziś = R, a następny dzień nie jest już R.
            return
                ObliczZmianePodstawowa(
                    data,
                    dataStartowa) == 'R' &&
                ObliczZmianePodstawowa(
                    data.AddDays(1),
                    dataStartowa) != 'R';
        }


        // =====================================================
        // KOLOROWANIE ZMIAN
        // =====================================================

        private void KolorujZmiany()
        {
            foreach (
                DataGridViewRow wiersz
                in tabela.Rows)
            {
                if (wiersz.Index < 0 ||
                    wiersz.Cells[0].Value == null)
                {
                    continue;
                }

                if (!int.TryParse(
                    wiersz.Cells[0].Value?.ToString(),
                    out int numerDnia))
                {
                    continue;
                }

                DateTime data =
                    new DateTime(
                        aktualnyMiesiac.Year,
                        aktualnyMiesiac.Month,
                        numerDnia);

                for (
                    int kolumna = 2;
                    kolumna < tabela.Columns.Count;
                    kolumna++)
                {
                    int indeksBrygady =
                        kolumna - 2;

                    DataGridViewCell komorka =
                        wiersz.Cells[kolumna];

                    char zmiana =
                        komorka.Tag is char tagZmiany
                            ? tagZmiany
                            : ObliczZmiane(
                                data,
                                datyStartowe[indeksBrygady]);

                    // 31 grudnia tylko P i N są traktowane
                    // jak praca w dzień świąteczny.
                    //
                    // Wyróżnienie ma pierwszeństwo zarówno
                    // w widoku standardowym, jak i VEOLIA.
                    if (CzyZmianaSwiatecznaWSylwestra(
                        data,
                        zmiana))
                    {
                        komorka.Style.BackColor =
                            Color.Red;

                        komorka.Style.ForeColor =
                            Color.White;

                        komorka.Style.SelectionBackColor =
                            Color.Red;

                        komorka.Style.SelectionForeColor =
                            Color.White;

                        komorka.Style.Font =
                            new Font(
                                "Segoe UI",
                                13.5f,
                                FontStyle.Bold);

                        continue;
                    }

                    if (widokVeolia)
                    {
                        // Widok VEOLIA przypomina papierowy grafik:
                        // zwykłe pola są białe, natomiast dwie
                        // komórki zamiany wolnej niedzieli są
                        // oznaczone żółtym kolorem.
                        komorka.Style.BackColor =
                            CzyKorektaWolnejNiedzieli(
                                data,
                                datyStartowe[indeksBrygady])
                                ? Color.Khaki
                                : Color.White;

                        komorka.Style.ForeColor =
                            Color.Black;

                        komorka.Style.SelectionBackColor =
                            komorka.Style.BackColor;

                        komorka.Style.SelectionForeColor =
                            Color.Black;

                        continue;
                    }

                    komorka.Style.BackColor =
                        PobierzKolorZmiany(
                            zmiana);

                    komorka.Style.ForeColor =
                        Color.FromArgb(
                            24,
                            30,
                            36);

                    komorka.Style.SelectionBackColor =
                        komorka.Style.BackColor;

                    komorka.Style.SelectionForeColor =
                        komorka.Style.ForeColor;
                }
            }
        }


        // =====================================================
        // WYRÓŻNIENIE DZISIEJSZEGO DNIA
        // =====================================================

        private void WyróżnijDzisiaj()
        {
            if (indeksDzisiejszegoWiersza < 0)
            {
                return;
            }

            DataGridViewRow wiersz =
                tabela.Rows[
                    indeksDzisiejszegoWiersza];

            // Zachowujemy kolory sobót, niedziel, świąt oraz
            // zmian. Dzisiejszy dzień rozpoznajemy po
            // pogrubieniu i mocnej ramce całego wiersza.
            wiersz.DividerHeight = 1;

            foreach (
                DataGridViewCell komorka
                in wiersz.Cells)
            {
                komorka.Style.Font =
                    new Font(
                        "Segoe UI",
                        13.5f,
                        FontStyle.Bold);

                komorka.Style.SelectionBackColor =
                    komorka.Style.BackColor;

                komorka.Style.SelectionForeColor =
                    komorka.Style.ForeColor.IsEmpty
                        ? Color.Black
                        : komorka.Style.ForeColor;
            }
        }


        // =====================================================
        // RAMKA WOKÓŁ DZISIEJSZEGO WIERSZA
        // =====================================================

        private void Tabela_CellPainting(
            object? sender,
            DataGridViewCellPaintingEventArgs e)
        {
            if (indeksDzisiejszegoWiersza < 0)
            {
                return;
            }

            if (e.RowIndex !=
                indeksDzisiejszegoWiersza)
            {
                return;
            }

            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
            {
                return;
            }

            Graphics? graphics =
                e.Graphics;

            if (graphics is null)
            {
                return;
            }

            e.Paint(
                e.CellBounds,
                DataGridViewPaintParts.All);

            using Pen pen =
                new Pen(
                    Color.FromArgb(
                        230,
                        140,
                        0),
                    4);

            Rectangle rect =
                e.CellBounds;

            // GÓRA

            graphics.DrawLine(
                pen,
                rect.Left,
                rect.Top + 1,
                rect.Right,
                rect.Top + 1);

            // DÓŁ

            graphics.DrawLine(
                pen,
                rect.Left,
                rect.Bottom - 2,
                rect.Right,
                rect.Bottom - 2);

            // LEWA KRAWĘDŹ

            if (e.ColumnIndex == 0)
            {
                graphics.DrawLine(
                    pen,
                    rect.Left + 1,
                    rect.Top,
                    rect.Left + 1,
                    rect.Bottom);
            }

            // PRAWA KRAWĘDŹ

            if (e.ColumnIndex ==
                tabela.Columns.Count - 1)
            {
                graphics.DrawLine(
                    pen,
                    rect.Right - 2,
                    rect.Top,
                    rect.Right - 2,
                    rect.Bottom);
            }

            e.Handled =
                true;
        }


        // =====================================================
        // PODGLĄD WYDRUKU
        // =====================================================

        private void BtnPodglad_Click(
            object? sender,
            EventArgs e)
        {
            podgladDruku.Width =
                1200;

            podgladDruku.Height =
                850;

            podgladDruku.ShowDialog();
        }


        // =====================================================
        // DRUKUJ
        // =====================================================

        private void BtnDrukuj_Click(
            object? sender,
            EventArgs e)
        {
            using PrintDialog dialogDruku =
                new PrintDialog();

            dialogDruku.Document =
                dokumentDruku;

            if (dialogDruku.ShowDialog() ==
                DialogResult.OK)
            {
                dokumentDruku.Print();
            }
        }


        // =====================================================
        // RYSOWANIE WYDRUKU A4
        // =====================================================

        private void DokumentDruku_PrintPage(
            object? sender,
            PrintPageEventArgs e)
        {
            Graphics? g =
                e.Graphics;

            if (g is null)
            {
                e.HasMorePages =
                    false;

                return;
            }

            Rectangle obszar =
                e.MarginBounds;


            // -------------------------------------------------
            // CZCIONKI
            // -------------------------------------------------

            using Font fontTytul =
                new Font(
                    "Segoe UI",
                    15,
                    FontStyle.Bold);

            using Font fontMiesiac =
                new Font(
                    "Segoe UI",
                    12,
                    FontStyle.Bold);

            using Font fontNaglowek =
                new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold);

            using Font fontKomorka =
                new Font(
                    "Segoe UI",
                    7);

            using Font fontAutor =
                new Font(
                    "Segoe UI",
                    7,
                    FontStyle.Italic);


            // -------------------------------------------------
            // TYTUŁ
            // -------------------------------------------------

            string tytul =
                NazwaProgramu +
                " (" +
                WersjaProgramu +
                ")";

            SizeF rozmiarTytulu =
                g.MeasureString(
                    tytul,
                    fontTytul);

            float xTytul =
                obszar.Left +
                (obszar.Width -
                rozmiarTytulu.Width) / 2;

            float y =
                obszar.Top;

            g.DrawString(
                tytul,
                fontTytul,
                Brushes.Black,
                xTytul,
                y);

            y += 30;


            // -------------------------------------------------
            // MIESIĄC
            // -------------------------------------------------

            string miesiac =
                lblMiesiac.Text +
                (widokVeolia
                    ? " – widok VEOLIA"
                    : "");

            SizeF rozmiarMiesiaca =
                g.MeasureString(
                    miesiac,
                    fontMiesiac);

            float xMiesiac =
                obszar.Left +
                (obszar.Width -
                rozmiarMiesiaca.Width) / 2;

            g.DrawString(
                miesiac,
                fontMiesiac,
                Brushes.Black,
                xMiesiac,
                y);

            y += 28;


            // -------------------------------------------------
            // WYMIARY TABELI
            // -------------------------------------------------

            int liczbaKolumn =
                tabela.Columns.Count;

            int liczbaWierszy =
                tabela.Rows.Count;

            float szerokoscTabeli =
                obszar.Width;

            float wysokoscNaglowka =
                22;

            float dostepnaWysokosc =
                obszar.Bottom -
                y -
                35;

            float wysokoscWiersza =
                dostepnaWysokosc /
                liczbaWierszy;

            if (wysokoscWiersza > 18)
            {
                wysokoscWiersza = 18;
            }


            // -------------------------------------------------
            // SZEROKOŚCI KOLUMN NA WYDRUKU
            // Dzień i dzień tygodnia są węższe od brygad
            // -------------------------------------------------

            float[] wagiKolumn =
                new float[liczbaKolumn];

            float sumaWag = 0;

            for (int kol = 0;
                 kol < liczbaKolumn;
                 kol++)
            {
                if (kol == 0)
                {
                    wagiKolumn[kol] = 55;
                }
                else if (kol == 1)
                {
                    wagiKolumn[kol] = 65;
                }
                else
                {
                    wagiKolumn[kol] = 130;
                }

                sumaWag +=
                    wagiKolumn[kol];
            }

            float[] szerokosciKolumn =
                new float[liczbaKolumn];

            for (int kol = 0;
                 kol < liczbaKolumn;
                 kol++)
            {
                szerokosciKolumn[kol] =
                    szerokoscTabeli *
                    wagiKolumn[kol] /
                    sumaWag;
            }


            // -------------------------------------------------
            // FORMAT TEKSTU
            // -------------------------------------------------

            using StringFormat sf =
                new StringFormat();

            sf.Alignment =
                StringAlignment.Center;

            sf.LineAlignment =
                StringAlignment.Center;


            // -------------------------------------------------
            // NAGŁÓWKI
            // -------------------------------------------------

            float xKolumny =
                obszar.Left;

            for (int kol = 0;
                 kol < liczbaKolumn;
                 kol++)
            {
                RectangleF rect =
                    new RectangleF(
                        xKolumny,
                        y,
                        szerokosciKolumn[kol],
                        wysokoscNaglowka);

                g.FillRectangle(
                    Brushes.Gainsboro,
                    rect);

                g.DrawRectangle(
                    Pens.Black,
                    rect.X,
                    rect.Y,
                    rect.Width,
                    rect.Height);

                string naglowek =
                    tabela.Columns[kol]
                    .HeaderText;

                g.DrawString(
                    naglowek,
                    fontNaglowek,
                    Brushes.Black,
                    rect,
                    sf);

                xKolumny +=
                    szerokosciKolumn[kol];
            }

            y +=
                wysokoscNaglowka;


            // -------------------------------------------------
            // WIERSZE GRAFIKU
            // -------------------------------------------------

            for (int wiersz = 0;
                 wiersz < liczbaWierszy;
                 wiersz++)
            {
                xKolumny =
                    obszar.Left;

                for (int kol = 0;
                     kol < liczbaKolumn;
                     kol++)
                {
                    RectangleF rect =
                        new RectangleF(
                            xKolumny,
                            y +
                            wiersz *
                            wysokoscWiersza,
                            szerokosciKolumn[kol],
                            wysokoscWiersza);

                    string wartosc =
                        tabela.Rows[wiersz]
                        .Cells[kol]
                        .Value?
                        .ToString() ?? "";

                    Brush tlo =
                        Brushes.White;

                    Brush kolorTekstu =
                        Brushes.Black;


                    // -----------------------------------------
                    // DZIEŃ MIESIĄCA I DZIEŃ TYGODNIA
                    // -----------------------------------------

                    if (kol == 0 ||
                        kol == 1)
                    {
                        if (int.TryParse(
                            tabela.Rows[wiersz]
                            .Cells[0]
                            .Value?
                            .ToString(),
                            out int numerDniaDaty))
                        {
                            DateTime dataWierszaDaty =
                                new DateTime(
                                    aktualnyMiesiac.Year,
                                    aktualnyMiesiac.Month,
                                    numerDniaDaty);

                            if (dataWierszaDaty.DayOfWeek ==
                                    DayOfWeek.Sunday ||
                                CzySwieto(
                                    dataWierszaDaty))
                            {
                                tlo =
                                    Brushes.Red;

                                kolorTekstu =
                                    Brushes.White;
                            }
                            else if (dataWierszaDaty.DayOfWeek ==
                                DayOfWeek.Saturday)
                            {
                                tlo =
                                    Brushes.Yellow;

                                kolorTekstu =
                                    Brushes.Black;
                            }
                        }
                    }


                    // -----------------------------------------
                    // ZMIANY
                    // -----------------------------------------

                    if (kol >= 2)
                    {
                        if (widokVeolia)
                        {
                            if (int.TryParse(
                                tabela.Rows[wiersz]
                                .Cells[0]
                                .Value?
                                .ToString(),
                                out int numerDnia))
                            {
                                DateTime dataWiersza =
                                    new DateTime(
                                        aktualnyMiesiac.Year,
                                        aktualnyMiesiac.Month,
                                        numerDnia);

                                int indeksBrygady =
                                    kol - 2;

                                tlo =
                                    CzyKorektaWolnejNiedzieli(
                                        dataWiersza,
                                        datyStartowe[indeksBrygady])
                                        ? Brushes.Khaki
                                        : Brushes.White;
                            }
                        }
                        else
                        {
                            switch (wartosc)
                            {
                                case "N":
                                    tlo =
                                        Brushes.LightSkyBlue;
                                    break;

                                case "P":
                                    tlo =
                                        Brushes.Orange;
                                    break;

                                case "R":
                                    tlo =
                                        Brushes.LightGreen;
                                    break;

                                case "W":
                                case "X":
                                    tlo =
                                        Brushes.LightGray;
                                    break;
                            }
                        }

                        // Niezależnie od wybranego widoku
                        // zmiany P/N przypadające 31 grudnia
                        // są na wydruku oznaczone jak praca
                        // w dzień świąteczny.
                        if (int.TryParse(
                            tabela.Rows[wiersz]
                            .Cells[0]
                            .Value?
                            .ToString(),
                            out int numerDniaSylwester))
                        {
                            DateTime dataWierszaSylwester =
                                new DateTime(
                                    aktualnyMiesiac.Year,
                                    aktualnyMiesiac.Month,
                                    numerDniaSylwester);

                            char zmianaRzeczywista =
                                tabela.Rows[wiersz]
                                .Cells[kol]
                                .Tag is char tagZmiany
                                    ? tagZmiany
                                    : '\0';

                            if (CzyZmianaSwiatecznaWSylwestra(
                                dataWierszaSylwester,
                                zmianaRzeczywista))
                            {
                                tlo =
                                    Brushes.Red;

                                kolorTekstu =
                                    Brushes.White;
                            }
                        }
                    }


                    g.FillRectangle(
                        tlo,
                        rect);

                    g.DrawRectangle(
                        Pens.Black,
                        rect.X,
                        rect.Y,
                        rect.Width,
                        rect.Height);

                    g.DrawString(
                        wartosc,
                        fontKomorka,
                        kolorTekstu,
                        rect,
                        sf);

                    xKolumny +=
                        szerokosciKolumn[kol];
                }


                // ---------------------------------------------
                // RAMKA DZISIAJ NA WYDRUKU
                // ---------------------------------------------

                if (wiersz ==
                    indeksDzisiejszegoWiersza)
                {
                    using Pen pen =
                        new Pen(
                            Color.DarkOrange,
                            3);

                    RectangleF ramka =
                        new RectangleF(
                            obszar.Left,
                            y +
                            wiersz *
                            wysokoscWiersza,
                            szerokoscTabeli,
                            wysokoscWiersza);

                    g.DrawRectangle(
                        pen,
                        ramka.X,
                        ramka.Y,
                        ramka.Width,
                        ramka.Height);
                }
            }


            // -------------------------------------------------
            // AUTOR
            // -------------------------------------------------

            float dolTabeli =
                y +
                liczbaWierszy *
                wysokoscWiersza +
                10;

            g.DrawString(
                "Autor programu: " +
                AutorProgramu +
                "  •  projekt od: " +
                RozpoczecieProjektu,
                fontAutor,
                Brushes.DimGray,
                obszar.Left,
                dolTabeli);

            g.DrawString(
                "Nowa wersja: " +
                AdresProjektu,
                fontAutor,
                Brushes.DimGray,
                obszar.Left,
                dolTabeli + 12);


            e.HasMorePages =
                false;
        }
    }
}