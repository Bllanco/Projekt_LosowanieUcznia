using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace SystemLosowania_MartaBiala
{
    public partial class EdycjaKlasyPage : ContentPage
    {
        private readonly List<string> Uczniowie;
        private Dictionary<string, bool> Obecnosci;

        public EdycjaKlasyPage(string nazwaKlasy, List<string> uczniowie)
        {
            InitializeComponent();
            BindingContext = this;

            NazwaKlasy = nazwaKlasy;
            Uczniowie = uczniowie;

            Obecnosci = new Dictionary<string, bool>();
            foreach (var uczen in Uczniowie)
            {
                Obecnosci.Add(uczen, true);
            }

            ListaUczniow.ItemsSource = Uczniowie;
        }

        public string NazwaKlasy { get; set; }

        private async void WczytajListeZPliku_Clicked(object sender, EventArgs e)
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Wybierz plik",
                };

                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    var fileStream = await result.OpenReadAsync();
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        string line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            Uczniowie.Add(line);
                        }

                        Obecnosci.Clear();
                        foreach (var uczen in Uczniowie)
                        {
                            Obecnosci.Add(uczen, true);
                        }

                        ListaUczniow.ItemsSource = null;
                        ListaUczniow.ItemsSource = Uczniowie;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wyst¹pi³ b³¹d: {ex.Message}");
            }
        }

        private void UsunUcznia_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var ucznikDoUsuniecia = button.CommandParameter as string;
                if (Uczniowie.Contains(ucznikDoUsuniecia))
                {
                    Uczniowie.Remove(ucznikDoUsuniecia);
                    Obecnosci.Remove(ucznikDoUsuniecia);

                    ListaUczniow.ItemsSource = null;
                    ListaUczniow.ItemsSource = Uczniowie;
                }
            }
        }

        private void LosujUcznia_Clicked(object sender, EventArgs e)
        {
            var obecniUczniowie = Uczniowie.Where(uczen => Obecnosci[uczen]).ToList();

            if (obecniUczniowie.Any())
            {
                Random random = new Random();
                int losowyIndex = random.Next(obecniUczniowie.Count);
                string wylosowanyUczen = obecniUczniowie[losowyIndex];

                DisplayAlert("Wylosowany Uczeñ", wylosowanyUczen, "OK");
            }
            else
            {
                DisplayAlert("Uwaga", "Brak obecnych uczniów do losowania.", "OK");
            }
        }

        private async void EksportujDoPliku_Clicked(object sender, EventArgs e)
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Wybierz lokalizacjê dla pliku",
                };

                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    await ZapiszDoPliku(result.FullPath);
                    DisplayAlert("Sukces", "Lista uczniów zosta³a eksportowana do pliku.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wyst¹pi³ b³¹d: {ex.Message}");
            }
        }

        private async Task ZapiszDoPliku(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var uczen in Uczniowie)
                    {
                        await writer.WriteLineAsync(uczen);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wyst¹pi³ b³¹d podczas zapisu do pliku: {ex.Message}");
            }
        }
    }
}
