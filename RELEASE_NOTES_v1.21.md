# Grafik Brygad v1.21 — informacje o wydaniu

## Najważniejsze zmiany

Wersja 1.21 rozwija stabilną wersję 1.20 bez zmiany podstawowego algorytmu grafiku.

### Interfejs

- przycisk `WIDOK VEOLIA` otrzymał nazwę `WIDOK PROSTY`,
- dodano przycisk `INFO` z krótką instrukcją użytkowania,
- pola W/X w Widoku Standardowym są jaśniejsze,
- zachowano logiczne rozróżnienie W i X,
- poprawiono tekst `wymiar czasu pracy` w oknie okresu rozliczeniowego,
- ujednolicono prezentację wartości w tabeli okresu rozliczeniowego.

### Wydruk

- jeden miesiąc został zastąpiony całym kwartałem,
- trzy miesiące są drukowane obok siebie na jednej stronie A4 poziomo,
- pod tabelami znajduje się podsumowanie okresu rozliczeniowego,
- wydruk działa zarówno w Widoku Standardowym, jak i Prostym,
- poprawiono pierwsze i kolejne otwarcia okna Podglądu wydruku,
- okno podglądu jest centrowane i dopasowane do obszaru roboczego monitora.

### Instalator

- zachowano ten sam `AppId`, dzięki czemu v1.21 aktualizuje poprzednią instalację,
- skrót na pulpicie ma teraz krótką nazwę `Grafik Brygad`,
- pełna nazwa aplikacji pozostaje `Grafik Brygad VEOLIA Energia Łódź`.

## Zgodność

Podstawowa logika grafiku, cykl zmian, niedzielne korekty, święta, Dzień Energetyka, zasada 31 grudnia oraz obliczenia okresów rozliczeniowych pozostają zgodne z v1.20.

## Instalacja

1. Pobierz `GrafikBrygad-v1.21-Setup.zip`.
2. Rozpakuj archiwum.
3. Uruchom `GrafikBrygad-v1.21-Setup.exe`.
4. Instalator może zostać uruchomiony bez wcześniejszego odinstalowania v1.20.

## Plik źródłowy instalatora

`Installer/GrafikBrygad-v1.21.iss`
