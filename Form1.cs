using System;
using System.Drawing;
using System.Drawing.Printing;
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
            "wersja 1.18";

        private const string AutorProgramu =
            "Marek Walaszczyk";


        // =====================================================
        // USTAWIENIA GRAFIKU
        // =====================================================

        // Cykl 20-dniowy:
        // N N N N W P P P P W R R R R W W W W W W

        private readonly string cykl =
            "NNNNWPPPPWRRRRWWWWWW";

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

        private Button btnPoprzedni =
            new Button();

        private Button btnNastepny =
            new Button();

        private Button btnAktualny =
            new Button();

        private Button btnPodglad =
            new Button();

        private Button btnDrukuj =
            new Button();

        private Label lblMiesiac =
            new Label();

        private DataGridView tabela =
            new DataGridView();

        private Panel panelLegenda =
            new Panel();

        private Label lblAutor =
            new Label();


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


        // =====================================================
        // KONSTRUKTOR
        // =====================================================

        public Form1()
        {
            InitializeComponent();

            this.Text =
                $"{NazwaProgramu} ({WersjaProgramu})";

            // Nieco większy obszar roboczy daje więcej oddechu
            // nagłówkowi, tabeli, legendzie i podpisowi autora.
            this.ClientSize =
                new Size(1180, 906);

            this.MinimumSize =
                this.Size;

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

            // Po pierwszym pokazaniu formularza ustawiamy
            // dzisiejszy dzień możliwie na środku tabeli.
            this.Shown +=
                (sender, e) =>
                {
                    UstawDzisiejszyDzienNaSrodku();
                };
        }


        // =====================================================
        // TWORZENIE INTERFEJSU
        // =====================================================

        private void UtworzInterfejs()
        {
            UtworzNaglowek();


            // -------------------------------------------------
            // POPRZEDNI MIESIĄC
            // -------------------------------------------------

            btnPoprzedni.Text =
                "POPRZEDNI MIESIĄC";

            btnPoprzedni.Location =
                new Point(24, 126);

            btnPoprzedni.Width = 220;
            btnPoprzedni.Height = 42;

            btnPoprzedni.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnPoprzedni,
                false);

            btnPoprzedni.Click +=
                BtnPoprzedni_Click;


            // -------------------------------------------------
            // NAZWA MIESIĄCA
            // -------------------------------------------------

            lblMiesiac.Location =
                new Point(280, 118);

            lblMiesiac.Width = 620;
            lblMiesiac.Height = 58;

            lblMiesiac.TextAlign =
                ContentAlignment.MiddleCenter;

            lblMiesiac.Font =
                new Font(
                    "Segoe UI",
                    18,
                    FontStyle.Bold);

            lblMiesiac.ForeColor =
                Color.FromArgb(
                    32,
                    39,
                    48);


            // -------------------------------------------------
            // NASTĘPNY MIESIĄC
            // -------------------------------------------------

            btnNastepny.Text =
                "NASTĘPNY MIESIĄC";

            btnNastepny.Location =
                new Point(936, 126);

            btnNastepny.Width = 220;
            btnNastepny.Height = 42;

            btnNastepny.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnNastepny,
                false);

            btnNastepny.Click +=
                BtnNastepny_Click;


            // -------------------------------------------------
            // AKTUALNY DZIEŃ
            // -------------------------------------------------

            btnAktualny.Text =
                "AKTUALNY";

            btnAktualny.Location =
                new Point(302, 184);

            btnAktualny.Width = 180;
            btnAktualny.Height = 42;

            btnAktualny.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnAktualny,
                true);

            btnAktualny.Click +=
                BtnAktualny_Click;


            // -------------------------------------------------
            // PODGLĄD WYDRUKU
            // -------------------------------------------------

            btnPodglad.Text =
                "PODGLĄD WYDRUKU";

            btnPodglad.Location =
                new Point(500, 184);

            btnPodglad.Width = 180;
            btnPodglad.Height = 42;

            btnPodglad.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Bold);

            StylizujPrzycisk(
                btnPodglad,
                false);

            btnPodglad.Click +=
                BtnPodglad_Click;


            // -------------------------------------------------
            // DRUKUJ A4
            // -------------------------------------------------

            btnDrukuj.Text =
                "DRUKUJ A4";

            btnDrukuj.Location =
                new Point(698, 184);

            btnDrukuj.Width = 180;
            btnDrukuj.Height = 42;

            btnDrukuj.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
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
                new Point(24, 242);

            tabela.Width = 1132;
            tabela.Height = 520;

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
                42;

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
                    10,
                    FontStyle.Bold);

            tabela.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                tabela.ColumnHeadersDefaultCellStyle.BackColor;

            tabela.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;

            tabela.DefaultCellStyle.Font =
                new Font(
                    "Segoe UI",
                    10);

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
                29;

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


            // -------------------------------------------------
            // LEGENDA
            // -------------------------------------------------

            UtworzLegende();


            // -------------------------------------------------
            // AUTOR
            // -------------------------------------------------

            lblAutor.Text =
                "Autor programu: " +
                AutorProgramu;

            lblAutor.Location =
                new Point(24, 852);

            lblAutor.Width = 1132;
            lblAutor.Height = 32;

            lblAutor.TextAlign =
                ContentAlignment.MiddleCenter;

            lblAutor.Font =
                new Font(
                    "Segoe UI",
                    9.5f,
                    FontStyle.Italic);

            lblAutor.ForeColor =
                Color.FromArgb(
                    105,
                    111,
                    118);


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
                btnPoprzedni);

            this.Controls.Add(
                lblMiesiac);

            this.Controls.Add(
                btnNastepny);

            this.Controls.Add(
                btnAktualny);

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
                1132;

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
                new Point(22, 12);

            logoVeolia.Width =
                190;

            logoVeolia.Height =
                64;

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
                new Point(225, 10);

            lblTytul.Width =
                720;

            lblTytul.Height =
                68;

            lblTytul.TextAlign =
                ContentAlignment.MiddleCenter;

            lblTytul.Font =
                new Font(
                    "Segoe UI",
                    15,
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
                new Point(950, 31);

            lblWersja.Width =
                155;

            lblWersja.Height =
                30;

            lblWersja.TextAlign =
                ContentAlignment.MiddleRight;

            lblWersja.Font =
                new Font(
                    "Segoe UI",
                    9,
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
                new Point(24, 784);

            panelLegenda.Width =
                1132;

            panelLegenda.Height =
                58;

            panelLegenda.BackColor =
                Color.White;

            panelLegenda.BorderStyle =
                BorderStyle.FixedSingle;


            DodajElementLegendy(
                "N",
                "noc",
                Color.LightSkyBlue,
                18,
                140);

            DodajElementLegendy(
                "P",
                "popołudnie",
                Color.LightGoldenrodYellow,
                168,
                170);

            DodajElementLegendy(
                "R",
                "rano",
                Color.LightCoral,
                348,
                140);

            DodajElementLegendy(
                "W",
                "wolne",
                Color.LightGray,
                498,
                140);


            // -------------------------------------------------
            // KOLORY DAT
            // -------------------------------------------------

            DodajElementDnia(
                "Sobota",
                Color.Green,
                660,
                180);

            DodajElementDnia(
                "Niedziela / Święto",
                Color.Red,
                850,
                260);
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
                    9,
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
                Color.White;

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
                    9,
                    FontStyle.Bold);

            lbl.ForeColor =
                kolor;

            panel.Controls.Add(
                lbl);

            panelLegenda.Controls.Add(
                panel);
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
                55;


            // -------------------------------------------------
            // KOLUMNA: DZIEŃ TYGODNIA
            // -------------------------------------------------

            tabela.Columns.Add(
                "DzienTygodnia",
                "TYDZ.");

            tabela.Columns[1]
                .FillWeight =
                65;


            // -------------------------------------------------
            // BRYGADY
            // -------------------------------------------------

            for (int i = 0;
                 i < brygady.Length;
                 i++)
            {
                tabela.Columns.Add(
                    "Brygada" + i,
                    brygady[i]);

                tabela.Columns[i + 2]
                    .FillWeight =
                    130;
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

                    tabela.Rows[numerWiersza]
                        .Cells[b + 2]
                        .Value =
                        zmiana.ToString();
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
                    komorkaDnia.Style.ForeColor =
                        Color.Red;

                    komorkaTygodnia.Style.ForeColor =
                        Color.Red;

                    komorkaDnia.Style.Font =
                        new Font(
                            "Segoe UI",
                            10,
                            FontStyle.Bold);

                    komorkaTygodnia.Style.Font =
                        new Font(
                            "Segoe UI",
                            10,
                            FontStyle.Bold);
                }
                else if (data.DayOfWeek ==
                    DayOfWeek.Saturday)
                {
                    komorkaDnia.Style.ForeColor =
                        Color.Green;

                    komorkaTygodnia.Style.ForeColor =
                        Color.Green;

                    komorkaDnia.Style.Font =
                        new Font(
                            "Segoe UI",
                            10,
                            FontStyle.Bold);

                    komorkaTygodnia.Style.Font =
                        new Font(
                            "Segoe UI",
                            10,
                            FontStyle.Bold);
                }
                else
                {
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
                tabela.DisplayedRowCount(false);

            // Zabezpieczenie na moment, gdy kontrolka nie została
            // jeszcze w pełni narysowana.
            if (widoczneWiersze <= 0)
            {
                int wysokoscDanych =
                    tabela.ClientSize.Height -
                    tabela.ColumnHeadersHeight;

                widoczneWiersze =
                    Math.Max(
                        1,
                        wysokoscDanych /
                        Math.Max(1, tabela.RowTemplate.Height));
            }

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
                new Size(540, 450);

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
                    brygady[b];

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
                            Color.Red;
                    }
                    else if (aktualnaData.DayOfWeek == DayOfWeek.Saturday)
                    {
                        lblData.ForeColor =
                            Color.Green;
                    }
                    else
                    {
                        lblData.ForeColor =
                            Color.FromArgb(
                                31,
                                38,
                                47);
                    }

                    if (nazwaSwieta != null)
                    {
                        lblSwieto.Text =
                            nazwaSwieta;

                        lblSwieto.ForeColor =
                            Color.DarkRed;

                        lblSwieto.BackColor =
                            Color.MistyRose;
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
                            PobierzPelnaNazweZmiany(
                                zmiana);

                        etykietyZmian[b].BackColor =
                            PobierzKolorZmiany(
                                zmiana);
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

            odswiezSzczegoly();

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
                    return Color.LightGoldenrodYellow;

                case 'R':
                    return Color.LightCoral;

                case 'W':
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

                default:
                    return zmiana.ToString();
            }
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
            int roznicaDni =
                (data - dataStartowa).Days;

            int pozycja =
                ((roznicaDni % 20) + 20) % 20;

            return cykl[pozycja];
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
                for (
                    int kolumna = 2;
                    kolumna < tabela.Columns.Count;
                    kolumna++)
                {
                    string? wartosc =
                        wiersz
                        .Cells[kolumna]
                        .Value?
                        .ToString();

                    switch (wartosc)
                    {
                        case "N":

                            wiersz.Cells[kolumna]
                                .Style.BackColor =
                                Color.LightSkyBlue;

                            break;


                        case "P":

                            wiersz.Cells[kolumna]
                                .Style.BackColor =
                                Color.LightGoldenrodYellow;

                            break;


                        case "R":

                            wiersz.Cells[kolumna]
                                .Style.BackColor =
                                Color.LightCoral;

                            break;


                        case "W":

                            wiersz.Cells[kolumna]
                                .Style.BackColor =
                                Color.LightGray;

                            break;
                    }
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

            // Dzisiejszy wiersz jest nieco wyższy od pozostałych,
            // dzięki czemu łatwiej znaleźć go wzrokiem.
            wiersz.Height = 34;
            wiersz.DividerHeight = 1;

            foreach (
                DataGridViewCell komorka
                in wiersz.Cells)
            {
                komorka.Style.Font =
                    new Font(
                        "Segoe UI",
                        10,
                        FontStyle.Bold);

                komorka.Style.SelectionBackColor =
                    komorka.Style.BackColor;

                komorka.Style.SelectionForeColor =
                    komorka.Style.ForeColor.IsEmpty
                        ? Color.Black
                        : komorka.Style.ForeColor;
            }

            wiersz.Cells[0]
                .Style.BackColor =
                Color.Gold;

            wiersz.Cells[1]
                .Style.BackColor =
                Color.Gold;
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

            e.Paint(
                e.CellBounds,
                DataGridViewPaintParts.All);

            using Pen pen =
                new Pen(
                    Color.FromArgb(
                        230,
                        140,
                        0),
                    3);

            Rectangle rect =
                e.CellBounds;

            // GÓRA

            e.Graphics.DrawLine(
                pen,
                rect.Left,
                rect.Top + 1,
                rect.Right,
                rect.Top + 1);

            // DÓŁ

            e.Graphics.DrawLine(
                pen,
                rect.Left,
                rect.Bottom - 2,
                rect.Right,
                rect.Bottom - 2);

            // LEWA KRAWĘDŹ

            if (e.ColumnIndex == 0)
            {
                e.Graphics.DrawLine(
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
                e.Graphics.DrawLine(
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
            Graphics g =
                e.Graphics;

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
                lblMiesiac.Text;

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
                        Color kolorDaty =
                            tabela.Rows[wiersz]
                            .Cells[kol]
                            .Style.ForeColor;

                        if (kolorDaty ==
                            Color.Green)
                        {
                            kolorTekstu =
                                Brushes.Green;
                        }
                        else if (kolorDaty ==
                            Color.Red)
                        {
                            kolorTekstu =
                                Brushes.Red;
                        }

                        if (wiersz ==
                            indeksDzisiejszegoWiersza)
                        {
                            tlo =
                                Brushes.Gold;
                        }
                    }


                    // -----------------------------------------
                    // ZMIANY
                    // -----------------------------------------

                    if (kol >= 2)
                    {
                        switch (wartosc)
                        {
                            case "N":
                                tlo =
                                    Brushes.LightSkyBlue;
                                break;

                            case "P":
                                tlo =
                                    Brushes.LightGoldenrodYellow;
                                break;

                            case "R":
                                tlo =
                                    Brushes.LightCoral;
                                break;

                            case "W":
                                tlo =
                                    Brushes.LightGray;
                                break;
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
                AutorProgramu,
                fontAutor,
                Brushes.DimGray,
                obszar.Left,
                dolTabeli);


            e.HasMorePages =
                false;
        }
    }
}