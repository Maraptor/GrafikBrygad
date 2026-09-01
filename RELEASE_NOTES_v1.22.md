# Grafik Brygad v1.22 --- informacje o wydaniu

**Wersja:** 1.22\
**Autor:** Marek Walaszczyk\
**Rok:** 2026

## Najważniejsze zmiany

Wersja **1.22** rozwija Grafik Brygad przede wszystkim o obsługę własnej
brygady i ręcznych korekt harmonogramu. Zmiany zostały zintegrowane z
widokiem grafiku, szczegółami dnia, okresem rozliczeniowym oraz
kwartalnym wydrukiem A4.

### Moja brygada

Użytkownik może wskazać swoją brygadę jako nadrzędną. Jest ona
wyróżniana w interfejsie, okresie rozliczeniowym i na wydruku.

### Ręczna edycja grafiku

W oknie **Szczegóły dnia** można wprowadzić korektę dla Mojej brygady:

-   `R` --- rano,
-   `P` --- popołudnie,
-   `N` --- noc,
-   `U` --- urlop,
-   `AUTO` --- usunięcie korekty i powrót do grafiku bazowego.

Ręczne korekty są zapisywane lokalnie. Bazowy 20-dniowy cykl grafiku nie
jest modyfikowany.

### Szybsze otwieranie Szczegółów dnia

Nie trzeba już klikać wyłącznie daty. Kliknięcie **dowolnego pola w
wierszu dnia** otwiera Szczegóły dnia dla wybranej daty.

### Moje zmiany

Dodano nowe okno **MOJE ZMIANY**, które zbiera aktywne ręczne korekty
Mojej brygady w jednym miejscu.

Dostępne są filtry:

-   Wszystkie,
-   Bieżący rok,
-   Bieżący kwartał.

Lista zachowuje stałą kolejność chronologiczną; sortowanie kolumn
zostało wyłączone. Dwuklik na wpisie lub **POKAŻ DZIEŃ** prowadzi
bezpośrednio do odpowiedniej daty.

### Okres rozliczeniowy

Dodano wiersz:

**„Liczba dni do dopracowania po uwzględnieniu ręcznych zmian i
urlopu"**

Pozwala on porównać harmonogram bazowy z sytuacją po wprowadzeniu korekt
użytkownika. Trzy dotychczasowe wartości bazowe pozostają liczone z
harmonogramu.

### Wydruk kwartalny

Kwartalny wydruk A4 uwzględnia:

-   ręczne korekty Mojej brygady,
-   urlop `U`,
-   wyróżnienie Mojej brygady symbolem `★`,
-   skorygowaną liczbę dni do dopracowania.

Ręczne wpisy są dodatkowo wyróżniane na wydruku.

### Interfejs

-   pozostawiono **Widok standardowy** i **Widok prosty**,
-   poprawiono czytelność pól `W` i `X`,
-   rozwinięto okno Szczegóły dnia,
-   ujednolicono wygląd tabel okresu rozliczeniowego,
-   zachowano szybki dostęp do podglądu wydruku, wydruku A4 i INFO.

### Prawa autorskie i licencja

Dodano finalną informację:

> © 2026 MAREK WALASZCZYK. Wszelkie prawa zastrzeżone. Program jest
> darmowy, ale zabrania się modyfikacji kodu oraz rozpowszechniania go
> bez zgody autora.

Do projektu dołączono również osobny plik **LICENSE.txt** określający
warunki korzystania z programu.

## Zrzuty ekranu v1.22

Dokumentacja wydania wykorzystuje finalne obrazy:

-   `Images/v1.22-standard.png`
-   `Images/v1.22-prosty.png`
-   `Images/v1.22-szczegoly-dnia.png`
-   `Images/v1.22-okres-rozliczeniowy.png`
-   `Images/v1.22-moje-zmiany.png`
-   `Images/v1.22-wydruk-kwartalny.png`

## Testy

Finalna wersja została sprawdzona pod kątem:

-   działania obu widoków grafiku,
-   ręcznych korekt i `AUTO`,
-   trwałego zapisu zmian,
-   okna Moje zmiany,
-   przechodzenia do Szczegółów dnia,
-   okresu rozliczeniowego,
-   podglądu i wydruku kwartalnego.

## Pliki wydania

Do publikacji przewidziane są:

-   instalator **GrafikBrygad-v1.22-Setup**,
-   archiwum ZIP instalatora,
-   `README.md`,
-   `RELEASE_NOTES_v1.22.md`,
-   `LICENSE.txt`,
-   finalne zrzuty ekranu w katalogu `Images`.

------------------------------------------------------------------------

**Grafik Brygad VEOLIA Energia Łódź v1.22**\
© 2026 Marek Walaszczyk. Wszelkie prawa zastrzeżone.
