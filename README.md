# Aplikacja desktopowa (WPF) - StalkerDesc

## Opis projektu
Aplikacja desktopowa napisana w języku **C# (WPF)** służąca do zarządzania komputerami w pracowni szkolnej.  
Program umożliwia administratorowi zdalne sterowanie komputerami oraz monitorowanie ich działania w sieci lokalnej.

Po uruchomianiu aplikacji w oknie aplikacji wyświetla się lista dostępnych komputerów w sieci lan automatycznie pobranych za pomocą technologii UDP.
Po wybraniu dowolnego komputera z listy, dostępna jest opcja wyłączenia urządzenia, resetowania, śledzenia procesów.

---

## Funkcjonalności
- Wyświetlanie listy komputerów w sieci
- Zdalne wyłączanie komputerów
- Restart komputerów
- Blokowanie komputerów
- Wyświetlanie listy procesów
- Zamykanie wybranych procesów
- Podgląd logów i aktywności

---

## Technologie
- C#
- WPF (Windows Presentation Foundation)
- .NET
- TCP/IP (komunikacja sieciowa)

---

## Autorzy
- Jakub Sajda - Team lider / Scrum master
- Maksymilian Cedro – Backend / Serwer
- Kamil Bożuta – Frontend / UI 
- Huber Zaremba –  Komunikacja / GitHub

---

## Instrukcja działania

### 1. Wymagania
- System Windows
- Zainstalowany .NET (np. .NET 6 lub nowszy)
- Visual Studio (opcjonalnie)

---

### 2. Uruchomienie serwera (aplikacji głównej)
1. Otwórz projekt w Visual Studio  
2. Uruchom aplikację (F5)  
3. Aplikacja zacznie nasłuchiwać połączeń  

---

### 3. Uruchomienie klienta (agenta)
1. Uruchom aplikację klienta na komputerze w sieci  
2. Wprowadź adres IP serwera (jeśli wymagane)  
3. Połącz się z serwerem  

---

### 4. Obsługa programu
- Po uruchomieniu klienta komputer pojawi się na liście  
- Wybierz komputer z listy  
- Użyj przycisków:
  - **Wyłącz** – wyłącza komputer  
  - **Restart** – restartuje komputer  
  - **Zablokuj** – blokuje ekran  

---

### 5. Procesy
- Wybierz komputer  
- Przejdź do zakładki „Procesy”  
- Możesz:
  - zobaczyć listę procesów  
  - zamknąć wybrany proces  

---

## ⚠️ Uwagi
- Aplikacja działa w sieci lokalnej  
- Klient musi być uruchomiony na komputerze docelowym  
- Niektóre funkcje mogą wymagać uprawnień administratora  

---

## 📄 Licencja
Projekt edukacyjny – do użytku szkolnego.
