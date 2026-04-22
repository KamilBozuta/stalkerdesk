# Aplikacja desktopowa (WPF) - StalkerDesc

## Opis projektu
Aplikacja desktopowa napisana w języku **C# (WPF)** służąca do zarządzania komputerami w pracowni szkolnej.  
Program umożliwia administratorowi zdalne sterowanie komputerami oraz monitorowanie ich działania w sieci lokalnej.

Po uruchomianiu aplikacji w oknie aplikacji wyświetla się lista dostępnych komputerów w sieci lan automatycznie pobranych za pomocą technologii UDP.
Po wybraniu dowolnego komputera z listy, można kontrolować działanie komputerów.


## Funkcjonalności
- Wyświetlanie listy komputerów w sieci
- Zdalne wyłączanie komputerów
- Restart komputerów
- Blokowanie komputerów

## Technologie
- C#
- WPF (Windows Presentation Foundation)
- .NET
- TCP/IP (komunikacja sieciowa)
- PING
- DNS
- Process.Start

## Autorzy
- Jakub Sajda - Team lider / Scrum master
- Maksymilian Cedro – Backend / Serwer
- Kamil Bożuta – Frontend / UI 
- Huber Zaremba –  Komunikacja / GitHub

## Instrukcja działania
### Obsługa programu

- Po uruchomieniu klienta komputer pojawi się na liście  
- Wybierz komputer z listy  
- Użyj przycisków:
  - **Wyłącz** – wyłącza komputer  
  - **Restart** – restartuje komputer  
  - **Zablokuj** – blokuje ekran  


## Uwagi
- Aplikacja działa w sieci lokalnej a komputer kliencki wymaga skryptu clientApp
- Klient musi być uruchomiony na komputerze docelowym
- Niektóre funkcje mogą wymagać uprawnień administratora  

## Licencja
Projekt edukacyjny – do użytku szkolnego.
